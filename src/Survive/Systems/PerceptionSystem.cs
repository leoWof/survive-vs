using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Survive.Config;

namespace Survive.Systems;

/// <summary>
/// Manages the Expanded Perception system. Based on the player's mental degradation level,
/// hidden world elements become accessible — at a mental cost.
/// </summary>
public class PerceptionSystem : IDisposable
{
    private readonly ICoreServerAPI _api;
    private readonly SurviveConfig _config;
    private readonly PlayerStatsManager _stats;
    private readonly long _tickListenerId;

    private const int TickIntervalMs = 5000;

    public PerceptionSystem(ICoreServerAPI api, SurviveConfig config, PlayerStatsManager stats)
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

            int mentalLevel = _stats.GetMentalLevel(player.Entity);

            // Store perception state for client-side rendering
            player.Entity.WatchedAttributes.SetInt("survive:perceptionLevel", mentalLevel);
            player.Entity.WatchedAttributes.MarkPathDirty("survive:perceptionLevel");

            // Server-side perception effects
            if (mentalLevel >= _config.PerceptionMinMentalLevel)
            {
                HandleExpandedPerception(player, mentalLevel, dt);
            }
        }
    }

    private void HandleExpandedPerception(IPlayer player, int mentalLevel, float dt)
    {
        // N2+ : Journal pages become findable, animal reactions change
        if (mentalLevel >= 2)
        {
            player.Entity.WatchedAttributes.SetBool("survive:canSeePages", true);
            player.Entity.WatchedAttributes.SetBool("survive:animalReactions", true);
        }

        // N3+ : Wall inscriptions, neutral entities, albino animals, merchant dialogue
        if (mentalLevel >= 3)
        {
            player.Entity.WatchedAttributes.SetBool("survive:canSeeInscriptions", true);
            player.Entity.WatchedAttributes.SetBool("survive:canSeeEntities", true);
            player.Entity.WatchedAttributes.SetBool("survive:merchantAwakened", true);
        }

        // N4 : Ruin doors accessible
        if (mentalLevel >= 4)
        {
            player.Entity.WatchedAttributes.SetBool("survive:canOpenRuinDoors", true);
        }

        // Reset flags when mental state improves
        if (mentalLevel < 2)
        {
            player.Entity.WatchedAttributes.SetBool("survive:canSeePages", false);
            player.Entity.WatchedAttributes.SetBool("survive:animalReactions", false);
        }
        if (mentalLevel < 3)
        {
            player.Entity.WatchedAttributes.SetBool("survive:canSeeInscriptions", false);
            player.Entity.WatchedAttributes.SetBool("survive:canSeeEntities", false);
            player.Entity.WatchedAttributes.SetBool("survive:merchantAwakened", false);
        }
        if (mentalLevel < 4)
        {
            player.Entity.WatchedAttributes.SetBool("survive:canOpenRuinDoors", false);
        }
    }

    public void Dispose()
    {
        _api.Event.UnregisterGameTickListener(_tickListenerId);
    }
}
