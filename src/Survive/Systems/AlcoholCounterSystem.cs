using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Survive.Config;

namespace Survive.Systems;

/// <summary>
/// Invisible alcohol counter with three states: Naïf, Tolérant, Dépendant.
/// Transitions between states generate discreet journal logs.
/// </summary>
public class AlcoholCounterSystem : IDisposable
{
    private readonly ICoreServerAPI _api;
    private readonly SurviveConfig _config;
    private readonly PlayerStatsManager _stats;
    private readonly long _tickListenerId;

    /// <summary>Tracks previous alcohol state per player UID for transition detection.</summary>
    private readonly Dictionary<string, int> _previousStates = new();

    private const int TickIntervalMs = 2000;

    public AlcoholCounterSystem(ICoreServerAPI api, SurviveConfig config, PlayerStatsManager stats)
    {
        _api = api;
        _config = config;
        _stats = stats;

        _tickListenerId = api.Event.RegisterGameTickListener(OnTick, TickIntervalMs);
    }

    private void OnTick(float dt)
    {
        foreach (var player in _api.World.AllOnlinePlayers)
        {
            if (player.Entity == null || !player.Entity.Alive) continue;

            // Passive alcohol counter decay
            float alcohol = _stats.GetAlcohol(player.Entity);
            if (alcohol > 0)
            {
                alcohol -= _config.AlcoholDecayPerSec * dt;
                _stats.SetAlcohol(player.Entity, alcohol);
            }

            // Detect state transitions
            DetectStateTransition(player);
        }
    }

    /// <summary>
    /// Called when a player drinks alcohol. Increases the counter and applies
    /// mental health effects based on the current alcohol state.
    /// </summary>
    public void OnDrinkAlcohol(IPlayer player, float alcoholAmount)
    {
        float alcohol = _stats.GetAlcohol(player.Entity);

        // Counter increases faster with rapid consumption
        float recentDrinking = player.Entity.WatchedAttributes.GetFloat("survive:recentDrinking", 0f);
        float multiplier = 1f + recentDrinking * 0.5f;

        alcohol += alcoholAmount * multiplier;
        _stats.SetAlcohol(player.Entity, alcohol);

        // Track recent drinking (decays over time)
        player.Entity.WatchedAttributes.SetFloat("survive:recentDrinking", recentDrinking + 1f);

        // Apply mental health effects based on current state
        int state = _stats.GetAlcoholState(player.Entity);
        switch (state)
        {
            case 0: // Naïf — alcohol boosts mental health
                float mental = _stats.GetMental(player.Entity);
                _stats.SetMental(player.Entity, mental + _config.AlcoholMentalBoostNaive);
                break;

            case 1: // Tolérant — no effect
                break;

            case 2: // Dépendant — alcohol harms mental health
                float mentalD = _stats.GetMental(player.Entity);
                _stats.SetMental(player.Entity, mentalD - _config.AlcoholMentalPenaltyDependent);
                break;
        }
    }

    private void DetectStateTransition(IPlayer player)
    {
        string uid = player.PlayerUID;
        int currentState = _stats.GetAlcoholState(player.Entity);

        if (_previousStates.TryGetValue(uid, out int prevState) && prevState != currentState)
        {
            // Discreet notification — no explicit alert
            _api.SendMessage(player, 0,
                $"survive:alcohol_state_changed:{currentState}",
                EnumChatType.Notification);
        }

        _previousStates[uid] = currentState;

        // Decay recent drinking counter
        float recent = player.Entity.WatchedAttributes.GetFloat("survive:recentDrinking", 0f);
        if (recent > 0)
        {
            recent -= 0.01f;
            player.Entity.WatchedAttributes.SetFloat("survive:recentDrinking", Math.Max(0f, recent));
        }
    }

    public void Dispose()
    {
        _api.Event.UnregisterGameTickListener(_tickListenerId);
    }
}
