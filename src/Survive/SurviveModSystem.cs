using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Survive.Config;
using Survive.Systems;
using Survive.Client;

namespace Survive;

/// <summary>
/// Main entry point for the Survive mod.
/// Registers all server and client subsystems.
/// </summary>
public class SurviveModSystem : ModSystem
{
    public const string ModId = "survive";

    public SurviveConfig Config { get; private set; } = new();

    // Server-side systems
    private PlayerStatsManager? _statsManager;
    private HydrationSystem? _hydrationSystem;
    private FatigueSystem? _fatigueSystem;
    private InjurySystem? _injurySystem;
    private MentalStateSystem? _mentalStateSystem;
    private AlcoholCounterSystem? _alcoholSystem;
    private PerceptionSystem? _perceptionSystem;

    // Client-side systems
    private SurviveHudRenderer? _hudRenderer;
    private MentalEffectsRenderer? _mentalEffectsRenderer;
    private JournalSystem? _journalSystem;

    public override void StartPre(ICoreAPI api)
    {
        Config = api.LoadModConfig<SurviveConfig>($"{ModId}.json") ?? new SurviveConfig();
        api.StoreModConfig(Config, $"{ModId}.json");
    }

    public override void StartServerSide(ICoreServerAPI api)
    {
        _statsManager = new PlayerStatsManager(api, Config);

        if (Config.EnableHydration)
            _hydrationSystem = new HydrationSystem(api, Config, _statsManager);

        if (Config.EnableFatigue)
            _fatigueSystem = new FatigueSystem(api, Config, _statsManager);

        if (Config.EnableInjuries)
            _injurySystem = new InjurySystem(api, Config, _statsManager);

        if (Config.EnableMentalHealth)
            _mentalStateSystem = new MentalStateSystem(api, Config, _statsManager);

        if (Config.EnableAlcohol)
            _alcoholSystem = new AlcoholCounterSystem(api, Config, _statsManager);

        if (Config.EnablePerception)
            _perceptionSystem = new PerceptionSystem(api, Config, _statsManager);
    }

    public override void StartClientSide(ICoreClientAPI api)
    {
        _hudRenderer = new SurviveHudRenderer(api, Config);
        _mentalEffectsRenderer = new MentalEffectsRenderer(api, Config);
        _journalSystem = new JournalSystem(api, Config);
    }

    public override void Dispose()
    {
        _statsManager?.Dispose();
        _hydrationSystem?.Dispose();
        _fatigueSystem?.Dispose();
        _injurySystem?.Dispose();
        _mentalStateSystem?.Dispose();
        _alcoholSystem?.Dispose();
        _perceptionSystem?.Dispose();
        _hudRenderer?.Dispose();
        _mentalEffectsRenderer?.Dispose();
        _journalSystem?.Dispose();
    }
}
