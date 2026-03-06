using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Survive.Config;

namespace Survive.Systems;

/// <summary>
/// Manages fatigue accumulation, sleep quality, and phase effects.
/// </summary>
public class FatigueSystem : IDisposable
{
    private readonly ICoreServerAPI _api;
    private readonly SurviveConfig _config;
    private readonly PlayerStatsManager _stats;
    private readonly long _tickListenerId;

    private const int TickIntervalMs = 1000;

    public FatigueSystem(ICoreServerAPI api, SurviveConfig config, PlayerStatsManager stats)
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

            bool isSleeping = IsPlayerSleeping(player);

            if (isSleeping)
            {
                HandleSleepRecovery(player, dt);
            }
            else
            {
                HandleFatigueAccumulation(player, dt);
            }

            ApplyFatigueEffects(player);
        }
    }

    private void HandleFatigueAccumulation(IPlayer player, float dt)
    {
        float fatigue = _stats.GetFatigue(player.Entity);
        float gain = _config.FatigueGainPerSec * dt;

        // Mental state degradation accelerates fatigue
        int mentalLevel = _stats.GetMentalLevel(player.Entity);
        if (mentalLevel >= 2)
        {
            gain *= 1f + (mentalLevel - 1) * 0.15f;
        }

        _stats.SetFatigue(player.Entity, fatigue + gain);
    }

    private void HandleSleepRecovery(IPlayer player, float dt)
    {
        float fatigue = _stats.GetFatigue(player.Entity);
        float recovery = _config.FatigueRecoveryPerSec * dt;

        // Daytime sleep penalty
        float hourOfDay = _api.World.Calendar.HourOfDay;
        if (hourOfDay >= 6 && hourOfDay <= 18)
        {
            recovery *= _config.DaytimeSleepPenalty;
        }

        // Mental state can cause insomnia
        int mentalLevel = _stats.GetMentalLevel(player.Entity);
        if (mentalLevel >= 2) // Anxiety or worse
        {
            recovery *= Math.Max(0.3f, 1f - mentalLevel * 0.15f);
        }

        // Bed quality is handled by checking block properties (simplified here)
        float bedQuality = GetBedQuality(player);
        recovery *= bedQuality;

        _stats.SetFatigue(player.Entity, fatigue - recovery);
    }

    private void ApplyFatigueEffects(IPlayer player)
    {
        int phase = _stats.GetFatiguePhase(player.Entity);

        switch (phase)
        {
            case 0: // Alert — remove any debuffs
                player.Entity.Stats.Remove("walkspeed", "survive-fatigue");
                player.Entity.Stats.Remove("miningSpeedMul", "survive-fatigue");
                break;

            case 1: // Phase 1 — Tired: reduced stamina
                player.Entity.Stats.Set("walkspeed", "survive-fatigue", -0.05f);
                break;

            case 2: // Phase 2 — Exhausted: tools wear faster
                player.Entity.Stats.Set("walkspeed", "survive-fatigue", -0.1f);
                player.Entity.Stats.Set("miningSpeedMul", "survive-fatigue", -0.15f);
                break;

            case 3: // Phase 3 — Collapse: severe debuffs
                player.Entity.Stats.Set("walkspeed", "survive-fatigue", -0.3f);
                player.Entity.Stats.Set("miningSpeedMul", "survive-fatigue", -0.3f);
                break;
        }
    }

    private bool IsPlayerSleeping(IPlayer player)
    {
        // Check mounted state or sleeping behavior
        return player.Entity.MountedOn != null &&
               player.Entity.MountedOn.SuggestedAnimation == "sleep";
    }

    private float GetBedQuality(IPlayer player)
    {
        if (player.Entity.MountedOn == null) return 0.5f; // sleeping on ground

        // Default quality; bed-specific quality can be read from block attributes
        var block = player.Entity.MountedOn as Vintagestory.API.Common.IMountable;
        // Simplified: return 1.0 for any bed
        return 1.0f;
    }

    public void Dispose()
    {
        _api.Event.UnregisterGameTickListener(_tickListenerId);
    }
}
