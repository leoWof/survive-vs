using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Server;
using Survive.Config;

namespace Survive.Systems;

/// <summary>
/// Manages the five survival gauges for each player via WatchedAttributes.
/// Provides helpers to read/write pillar values and ensures initialisation on player join.
/// </summary>
public class PlayerStatsManager : IDisposable
{
    public const string AttrHydration = "survive:hydration";
    public const string AttrFatigue = "survive:fatigue";
    public const string AttrMental = "survive:mental";
    public const string AttrAlcohol = "survive:alcohol";
    public const string AttrInjuries = "survive:injuries";

    private readonly ICoreServerAPI _api;
    private readonly SurviveConfig _config;

    public PlayerStatsManager(ICoreServerAPI api, SurviveConfig config)
    {
        _api = api;
        _config = config;

        api.Event.PlayerJoin += OnPlayerJoin;
    }

    private void OnPlayerJoin(IServerPlayer player)
    {
        var attrs = player.Entity.WatchedAttributes;

        if (!attrs.HasAttribute(AttrHydration))
            attrs.SetFloat(AttrHydration, _config.HydrationMax);

        if (!attrs.HasAttribute(AttrFatigue))
            attrs.SetFloat(AttrFatigue, 0f);

        if (!attrs.HasAttribute(AttrMental))
            attrs.SetFloat(AttrMental, _config.MentalMax);

        if (!attrs.HasAttribute(AttrAlcohol))
            attrs.SetFloat(AttrAlcohol, 0f);

        if (!attrs.HasAttribute(AttrInjuries))
            attrs.GetOrAddTreeAttribute(AttrInjuries);

        attrs.MarkPathDirty(AttrHydration);
    }

    // --- Getters ---

    public float GetHydration(Entity entity) =>
        entity.WatchedAttributes.GetFloat(AttrHydration, _config.HydrationMax);

    public float GetFatigue(Entity entity) =>
        entity.WatchedAttributes.GetFloat(AttrFatigue, 0f);

    public float GetMental(Entity entity) =>
        entity.WatchedAttributes.GetFloat(AttrMental, _config.MentalMax);

    public float GetAlcohol(Entity entity) =>
        entity.WatchedAttributes.GetFloat(AttrAlcohol, 0f);

    public ITreeAttribute GetInjuries(Entity entity) =>
        entity.WatchedAttributes.GetOrAddTreeAttribute(AttrInjuries);

    // --- Setters ---

    public void SetHydration(Entity entity, float value)
    {
        entity.WatchedAttributes.SetFloat(AttrHydration, Math.Clamp(value, 0f, _config.HydrationMax));
        entity.WatchedAttributes.MarkPathDirty(AttrHydration);
    }

    public void SetFatigue(Entity entity, float value)
    {
        entity.WatchedAttributes.SetFloat(AttrFatigue, Math.Clamp(value, 0f, 100f));
        entity.WatchedAttributes.MarkPathDirty(AttrFatigue);
    }

    public void SetMental(Entity entity, float value)
    {
        entity.WatchedAttributes.SetFloat(AttrMental, Math.Clamp(value, 0f, _config.MentalMax));
        entity.WatchedAttributes.MarkPathDirty(AttrMental);
    }

    public void SetAlcohol(Entity entity, float value)
    {
        entity.WatchedAttributes.SetFloat(AttrAlcohol, Math.Max(value, 0f));
        entity.WatchedAttributes.MarkPathDirty(AttrAlcohol);
    }

    // --- Helpers ---

    public float GetHydrationFraction(Entity entity) =>
        GetHydration(entity) / _config.HydrationMax;

    public float GetMentalFraction(Entity entity) =>
        GetMental(entity) / _config.MentalMax;

    /// <summary>
    /// Returns the current mental level (0 = healthy, 1-5 = degradation levels).
    /// </summary>
    public int GetMentalLevel(Entity entity)
    {
        float mental = GetMental(entity);
        for (int i = 0; i < _config.MentalLevelThresholds.Length; i++)
        {
            if (mental > _config.MentalLevelThresholds[i])
                return i;
        }
        return _config.MentalLevelThresholds.Length;
    }

    /// <summary>
    /// Returns the current fatigue phase (0 = alert, 1-3 = fatigue phases).
    /// </summary>
    public int GetFatiguePhase(Entity entity)
    {
        float fatigue = GetFatigue(entity);
        for (int i = 0; i < _config.FatiguePhaseThresholds.Length; i++)
        {
            if (fatigue < _config.FatiguePhaseThresholds[i])
                return i;
        }
        return _config.FatiguePhaseThresholds.Length;
    }

    /// <summary>
    /// Returns the alcohol state: 0 = Naïf, 1 = Tolerant, 2 = Dependent.
    /// </summary>
    public int GetAlcoholState(Entity entity)
    {
        float alcohol = GetAlcohol(entity);
        if (alcohol >= _config.AlcoholDependentThreshold) return 2;
        if (alcohol >= _config.AlcoholTolerantThreshold) return 1;
        return 0;
    }

    /// <summary>
    /// Returns the injury stage (0-4) for a given body zone.
    /// </summary>
    public int GetInjuryStage(Entity entity, string zone)
    {
        var injuries = GetInjuries(entity);
        return injuries.GetInt(zone, 0);
    }

    /// <summary>
    /// Sets the injury stage for a given body zone.
    /// </summary>
    public void SetInjuryStage(Entity entity, string zone, int stage)
    {
        var injuries = GetInjuries(entity);
        injuries.SetInt(zone, Math.Clamp(stage, 0, 4));
        entity.WatchedAttributes.MarkPathDirty(AttrInjuries);
    }

    public void Dispose()
    {
        _api.Event.PlayerJoin -= OnPlayerJoin;
    }
}
