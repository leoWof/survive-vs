using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Survive.Config;

namespace Survive.Systems;

/// <summary>
/// Core mental health system. Calculates degradation from multiple sources
/// and applies gameplay consequences per mental level.
/// </summary>
public class MentalStateSystem : IDisposable
{
    private readonly ICoreServerAPI _api;
    private readonly SurviveConfig _config;
    private readonly PlayerStatsManager _stats;
    private readonly long _tickListenerId;

    /// <summary>Tracks the previous mental level per player UID to detect transitions.</summary>
    private readonly Dictionary<string, int> _previousLevels = new();

    private const int TickIntervalMs = 2000;

    public MentalStateSystem(ICoreServerAPI api, SurviveConfig config, PlayerStatsManager stats)
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

            float decay = CalculateDecay(player, dt);
            float boost = CalculateBoost(player, dt);

            float mental = _stats.GetMental(player.Entity);
            mental = mental - decay + boost;
            _stats.SetMental(player.Entity, mental);

            int currentLevel = _stats.GetMentalLevel(player.Entity);
            ApplyMentalEffects(player, currentLevel);
            DetectLevelTransition(player, currentLevel);
        }
    }

    private float CalculateDecay(IPlayer player, float dt)
    {
        float decay = _config.MentalDecayPerSec * dt;

        // Isolation — no other players nearby
        bool isAlone = true;
        foreach (var other in _api.World.AllOnlinePlayers)
        {
            if (other.Entity == null || other == player) continue;
            if (player.Entity.Pos.DistanceTo(other.Entity.Pos) < 32)
            {
                isAlone = false;
                break;
            }
        }
        if (isAlone) decay *= 1.5f;

        // Darkness — low light level at player position
        int lightLevel = _api.World.BlockAccessor.GetLightLevel(
            player.Entity.Pos.AsBlockPos, EnumLightLevelType.TimeOfDaySunLight);
        if (lightLevel <= 3) decay *= 2f;

        // Injuries accelerate mental decay
        foreach (var zone in InjurySystem.AllZones)
        {
            int stage = _stats.GetInjuryStage(player.Entity, zone);
            if (stage >= 3) decay *= 1.3f;
        }

        // High fatigue accelerates mental decay
        int fatiguePhase = _stats.GetFatiguePhase(player.Entity);
        if (fatiguePhase >= 2) decay *= 1.4f;

        // Dehydration accelerates mental decay
        if (_stats.GetHydrationFraction(player.Entity) < _config.DehydrationThreshold)
        {
            decay *= 1.3f;
        }

        return decay;
    }

    private float CalculateBoost(IPlayer player, float dt)
    {
        float boost = 0f;

        // Environment — shelter and light
        int lightLevel = _api.World.BlockAccessor.GetLightLevel(
            player.Entity.Pos.AsBlockPos, EnumLightLevelType.TimeOfDaySunLight);
        bool hasShelter = !_api.World.BlockAccessor.GetBlock(
            player.Entity.Pos.AsBlockPos.UpCopy(3)).IsLiquid(); // simplified shelter check

        if (lightLevel >= 12 && hasShelter)
        {
            boost += 0.02f * dt; // Comfortable environment
        }

        return boost;
    }

    private void ApplyMentalEffects(IPlayer player, int level)
    {
        // Reset all mental debuffs first
        player.Entity.Stats.Remove("walkspeed", "survive-mental");
        player.Entity.Stats.Remove("miningSpeedMul", "survive-mental");

        switch (level)
        {
            case 0: // Healthy — no effects
                break;

            case 1: // Morosité — craft speed -15%, stamina recovery slower
                player.Entity.Stats.Set("miningSpeedMul", "survive-mental", -0.15f);
                break;

            case 2: // Anxiété — trembling, false alerts (client-side), insomnia
                player.Entity.Stats.Set("miningSpeedMul", "survive-mental", -0.2f);
                break;

            case 3: // Dépression — hunger/thirst x2, can't run long
                player.Entity.Stats.Set("hungerrate", "survive-mental", 1.0f); // double hunger
                player.Entity.Stats.Set("walkspeed", "survive-mental", -0.15f);
                break;

            case 4: // Dissociation — severe effects, mostly client-side
                player.Entity.Stats.Set("hungerrate", "survive-mental", 1.0f);
                player.Entity.Stats.Set("walkspeed", "survive-mental", -0.2f);
                break;

            case 5: // Rupture — near total incapacitation
                player.Entity.Stats.Set("hungerrate", "survive-mental", 1.5f);
                player.Entity.Stats.Set("walkspeed", "survive-mental", -0.3f);
                player.Entity.Stats.Set("miningSpeedMul", "survive-mental", -0.5f);
                break;
        }
    }

    private void DetectLevelTransition(IPlayer player, int currentLevel)
    {
        string uid = player.PlayerUID;
        if (_previousLevels.TryGetValue(uid, out int prevLevel) && prevLevel != currentLevel)
        {
            // Level changed — notify the client for journal entry
            _api.SendMessage(player, 0,
                $"survive:mental_level_changed:{currentLevel}",
                EnumChatType.Notification);
        }
        _previousLevels[uid] = currentLevel;
    }

    /// <summary>
    /// Apply a mental health boost from an external source (comfort food, music, etc).
    /// </summary>
    public void ApplyBoost(IPlayer player, float amount)
    {
        float mental = _stats.GetMental(player.Entity);
        _stats.SetMental(player.Entity, mental + amount);
    }

    /// <summary>
    /// Apply a mental health penalty from an external event (trauma, companion death, etc).
    /// </summary>
    public void ApplyPenalty(IPlayer player, float amount)
    {
        float mental = _stats.GetMental(player.Entity);
        _stats.SetMental(player.Entity, mental - amount);
    }

    public void Dispose()
    {
        _api.Event.UnregisterGameTickListener(_tickListenerId);
    }
}
