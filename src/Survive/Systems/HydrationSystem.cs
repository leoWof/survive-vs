using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Survive.Config;

namespace Survive.Systems;

/// <summary>
/// Manages hydration decay and cross-interactions with satiety.
/// Runs on a server-side tick.
/// </summary>
public class HydrationSystem : IDisposable
{
    private readonly ICoreServerAPI _api;
    private readonly SurviveConfig _config;
    private readonly PlayerStatsManager _stats;
    private readonly long _tickListenerId;

    /// <summary>Tick interval in milliseconds.</summary>
    private const int TickIntervalMs = 1000;

    public HydrationSystem(ICoreServerAPI api, SurviveConfig config, PlayerStatsManager stats)
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

            // Decay hydration
            float hydration = _stats.GetHydration(player.Entity);
            float decay = _config.HydrationDecayPerSec * dt;

            // Hungry players lose hydration faster (inefficient sweating)
            float satiety = player.Entity.WatchedAttributes.GetFloat("currentSatiety", 0f);
            float maxSatiety = player.Entity.WatchedAttributes.GetFloat("maxSatiety", 1500f);
            if (maxSatiety > 0 && satiety / maxSatiety < 0.25f)
            {
                decay *= 1.2f;
            }

            hydration -= decay;
            _stats.SetHydration(player.Entity, hydration);

            // Cross-interaction: hydration affects satiety decay
            ApplySatietyCrossEffect(player, dt);

            // Dehydration consequences
            if (_stats.GetHydrationFraction(player.Entity) <= 0f)
            {
                // Critical dehydration: deal damage
                player.Entity.ReceiveDamage(
                    new DamageSource { Source = EnumDamageSource.Internal, Type = EnumDamageType.Hunger },
                    0.5f * dt);
            }
        }
    }

    private void ApplySatietyCrossEffect(IPlayer player, float dt)
    {
        float hydrationFrac = _stats.GetHydrationFraction(player.Entity);

        // Modify vanilla satiety decay based on hydration level
        // Well hydrated → satiety decays slower ; dehydrated → satiety decays faster
        float satietyModifier;
        if (hydrationFrac >= _config.WellHydratedThreshold)
        {
            satietyModifier = _config.HydratedSatietyDecayMultiplier;
        }
        else if (hydrationFrac <= _config.DehydrationThreshold)
        {
            satietyModifier = _config.DehydratedSatietyDecayMultiplier;
        }
        else
        {
            // Linear interpolation between thresholds
            float t = (hydrationFrac - _config.DehydrationThreshold) /
                      (_config.WellHydratedThreshold - _config.DehydrationThreshold);
            satietyModifier = _config.DehydratedSatietyDecayMultiplier +
                              t * (_config.HydratedSatietyDecayMultiplier - _config.DehydratedSatietyDecayMultiplier);
        }

        // Apply modifier via stats (Vintage Story's stat system)
        player.Entity.Stats.Set("hungerrate", "survive-hydration", satietyModifier - 1f);
    }

    public void Dispose()
    {
        _api.Event.UnregisterGameTickListener(_tickListenerId);
    }
}
