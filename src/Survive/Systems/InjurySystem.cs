using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Server;
using Survive.Config;

namespace Survive.Systems;

/// <summary>
/// Manages localized body injuries across four zones: legs, arms, torso, head.
/// Handles wound progression, infection, and healing.
/// </summary>
public class InjurySystem : IDisposable
{
    public const string ZoneLegs = "legs";
    public const string ZoneArms = "arms";
    public const string ZoneTorso = "torso";
    public const string ZoneHead = "head";

    public static readonly string[] AllZones = [ZoneLegs, ZoneArms, ZoneTorso, ZoneHead];

    private readonly ICoreServerAPI _api;
    private readonly SurviveConfig _config;
    private readonly PlayerStatsManager _stats;
    private readonly long _tickListenerId;

    private const int TickIntervalMs = 2000;

    public InjurySystem(ICoreServerAPI api, SurviveConfig config, PlayerStatsManager stats)
    {
        _api = api;
        _config = config;
        _stats = stats;

        _tickListenerId = api.Event.RegisterGameTickListener(OnTick, TickIntervalMs);
        api.Event.PlayerDeath += OnPlayerDeath;
    }

    private void OnTick(float dt)
    {
        foreach (var player in _api.World.AllOnlinePlayers)
        {
            if (player.Entity == null || !player.Entity.Alive) continue;

            foreach (var zone in AllZones)
            {
                int stage = _stats.GetInjuryStage(player.Entity, zone);
                if (stage == 0) continue;

                ProcessWound(player, zone, stage, dt);
            }

            ApplyInjuryEffects(player);
        }
    }

    private void ProcessWound(IPlayer player, string zone, int stage, float dt)
    {
        switch (stage)
        {
            case 1: // Minor — heals naturally
                // Track healing time via a secondary attribute
                string healKey = $"survive:heal_{zone}";
                float healProgress = player.Entity.WatchedAttributes.GetFloat(healKey, 0f);
                healProgress += dt;
                if (healProgress >= _config.MinorWoundHealTimeSec)
                {
                    _stats.SetInjuryStage(player.Entity, zone, 0);
                    player.Entity.WatchedAttributes.RemoveAttribute(healKey);
                }
                else
                {
                    player.Entity.WatchedAttributes.SetFloat(healKey, healProgress);
                }
                break;

            case 2: // Open — bleeding, can become infected
                // Slow HP loss from bleeding
                player.Entity.ReceiveDamage(
                    new DamageSource { Source = EnumDamageSource.Internal, Type = EnumDamageType.Injury },
                    0.1f * dt);

                // Infection chance
                if (_api.World.Rand.NextDouble() < _config.InfectionChancePerTick)
                {
                    _stats.SetInjuryStage(player.Entity, zone, 3);
                }
                break;

            case 3: // Infected — fever effects
                // Increased thirst and fatigue
                player.Entity.Stats.Set("hungerrate", $"survive-infection-{zone}", 0.5f);
                break;

            case 4: // Severe — partial incapacitation handled by ApplyInjuryEffects
                break;
        }
    }

    private void ApplyInjuryEffects(IPlayer player)
    {
        int legs = _stats.GetInjuryStage(player.Entity, ZoneLegs);
        int arms = _stats.GetInjuryStage(player.Entity, ZoneArms);
        int torso = _stats.GetInjuryStage(player.Entity, ZoneTorso);
        int head = _stats.GetInjuryStage(player.Entity, ZoneHead);

        // Legs → movement speed
        if (legs >= 2)
            player.Entity.Stats.Set("walkspeed", "survive-injury-legs", -0.1f * legs);
        else
            player.Entity.Stats.Remove("walkspeed", "survive-injury-legs");

        // Arms → mining speed & melee damage
        if (arms >= 2)
        {
            player.Entity.Stats.Set("miningSpeedMul", "survive-injury-arms", -0.1f * arms);
            player.Entity.Stats.Set("meleeWeaponsDamage", "survive-injury-arms", -0.1f * arms);
        }
        else
        {
            player.Entity.Stats.Remove("miningSpeedMul", "survive-injury-arms");
            player.Entity.Stats.Remove("meleeWeaponsDamage", "survive-injury-arms");
        }

        // Torso → damage resistance (applied as negative armor)
        if (torso >= 2)
            player.Entity.Stats.Set("armorDurabilityLoss", "survive-injury-torso", 0.15f * torso);
        else
            player.Entity.Stats.Remove("armorDurabilityLoss", "survive-injury-torso");

        // Head → craft/visual clarity effects (handled client-side)
        // Server stores the value; client reads it for rendering
    }

    /// <summary>
    /// Called externally when damage is dealt to a player to potentially create a wound.
    /// </summary>
    public void OnPlayerDamaged(IPlayer player, DamageSource source, float damage)
    {
        if (damage < 1f) return;

        // Determine affected zone based on damage type/direction
        string zone = DetermineHitZone(source);
        int currentStage = _stats.GetInjuryStage(player.Entity, zone);

        // Escalate wound if enough damage
        int newStage = Math.Min(4, currentStage + (damage >= 4f ? 2 : 1));
        _stats.SetInjuryStage(player.Entity, zone, newStage);
    }

    private string DetermineHitZone(DamageSource source)
    {
        // Simplified zone determination — can be refined with projectile direction
        return source.Type switch
        {
            EnumDamageType.SlashAttack => ZoneTorso,
            EnumDamageType.PiercingAttack => ZoneTorso,
            EnumDamageType.BluntAttack => ZoneHead,
            EnumDamageType.Gravity => ZoneLegs,
            _ => AllZones[new Random().Next(AllZones.Length)]
        };
    }

    private void OnPlayerDeath(IServerPlayer player, DamageSource? damageSource)
    {
        // Reset all injuries on death
        foreach (var zone in AllZones)
        {
            _stats.SetInjuryStage(player.Entity, zone, 0);
        }
    }

    public void Dispose()
    {
        _api.Event.UnregisterGameTickListener(_tickListenerId);
        _api.Event.PlayerDeath -= OnPlayerDeath;
    }
}
