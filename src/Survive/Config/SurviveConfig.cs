namespace Survive.Config;

/// <summary>
/// Main configuration for the Survive mod loaded from JSON.
/// </summary>
public class SurviveConfig
{
    // --- System toggles ---
    public bool EnableHydration { get; set; } = true;
    public bool EnableFatigue { get; set; } = true;
    public bool EnableInjuries { get; set; } = true;
    public bool EnableMentalHealth { get; set; } = true;
    public bool EnableAlcohol { get; set; } = true;
    public bool EnablePerception { get; set; } = true;

    // --- Hydration ---
    /// <summary>Max hydration value.</summary>
    public float HydrationMax { get; set; } = 1500f;

    /// <summary>Hydration lost per second at rest.</summary>
    public float HydrationDecayPerSec { get; set; } = 0.15f;

    /// <summary>Multiplier applied to satiety decay when dehydrated.</summary>
    public float DehydratedSatietyDecayMultiplier { get; set; } = 1.3f;

    /// <summary>Multiplier applied to satiety decay when well hydrated.</summary>
    public float HydratedSatietyDecayMultiplier { get; set; } = 0.7f;

    /// <summary>Hydration fraction below which the player is considered dehydrated.</summary>
    public float DehydrationThreshold { get; set; } = 0.25f;

    /// <summary>Hydration fraction above which the player is considered well hydrated.</summary>
    public float WellHydratedThreshold { get; set; } = 0.75f;

    // --- Fatigue ---
    /// <summary>Fatigue gained per second while awake.</summary>
    public float FatigueGainPerSec { get; set; } = 0.07f;

    /// <summary>Fatigue thresholds for the three phases [Tired, Exhausted, Collapse].</summary>
    public float[] FatiguePhaseThresholds { get; set; } = [50f, 75f, 90f];

    /// <summary>Base fatigue recovery per second while sleeping.</summary>
    public float FatigueRecoveryPerSec { get; set; } = 0.5f;

    /// <summary>Multiplier when sleeping during daytime.</summary>
    public float DaytimeSleepPenalty { get; set; } = 0.5f;

    // --- Injuries ---
    /// <summary>Seconds before a level-1 wound heals naturally.</summary>
    public float MinorWoundHealTimeSec { get; set; } = 600f;

    /// <summary>Chance per tick of an untreated open wound becoming infected (0-1).</summary>
    public float InfectionChancePerTick { get; set; } = 0.002f;

    // --- Mental Health ---
    public float MentalMax { get; set; } = 100f;

    /// <summary>Passive mental decay per second.</summary>
    public float MentalDecayPerSec { get; set; } = 0.01f;

    /// <summary>Mental thresholds for levels [Morosité, Anxiété, Dépression, Dissociation, Rupture] expressed as descending fractions of MentalMax.</summary>
    public float[] MentalLevelThresholds { get; set; } = [80f, 60f, 40f, 20f, 5f];

    // --- Alcohol ---
    /// <summary>Alcohol counter increase per drink.</summary>
    public float AlcoholPerDrink { get; set; } = 15f;

    /// <summary>Counter passive decay per second.</summary>
    public float AlcoholDecayPerSec { get; set; } = 0.02f;

    /// <summary>Threshold counters for Tolerant and Dependent states.</summary>
    public float AlcoholTolerantThreshold { get; set; } = 50f;
    public float AlcoholDependentThreshold { get; set; } = 80f;

    /// <summary>Mental boost when drinking in Naïf state.</summary>
    public float AlcoholMentalBoostNaive { get; set; } = 8f;

    /// <summary>Mental penalty when drinking in Dependent state.</summary>
    public float AlcoholMentalPenaltyDependent { get; set; } = 5f;

    // --- Perception ---
    /// <summary>Minimum mental level required for expanded perception elements.</summary>
    public int PerceptionMinMentalLevel { get; set; } = 2;
}
