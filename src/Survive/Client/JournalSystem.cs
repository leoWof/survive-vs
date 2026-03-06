using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Survive.Config;

namespace Survive.Client;

/// <summary>
/// Client-side journal system that generates automatic entries and discreet logs
/// based on the player's mental state, alcohol state, and events.
/// </summary>
public class JournalSystem : IDisposable
{
    private readonly ICoreClientAPI _api;
    private readonly SurviveConfig _config;

    private int _lastMentalLevel = -1;
    private int _lastAlcoholState = -1;

    // Discreet journal entries per mental level (French, matching the mod's tone)
    private static readonly Dictionary<int, string[]> MentalLevelEntries = new()
    {
        [1] = [
            "Les journées semblent plus longues qu'avant.",
            "Tout paraît un peu terne aujourd'hui.",
            "Difficile de se concentrer sur quoi que ce soit."
        ],
        [2] = [
            "Des bruits étranges, au loin. Sûrement le vent.",
            "Impossible de fermer l'œil. Chaque ombre ressemble à autre chose.",
            "Mes mains tremblent. Le froid, probablement."
        ],
        [3] = [
            "Plus faim. Plus soif. Plus rien.",
            "J'ai trouvé des objets que je ne reconnais pas dans mon sac.",
            "Les murs murmurent. Non. C'est le vent. C'est toujours le vent."
        ],
        [4] = [
            "Ils sont là, à la lisière. Ils attendent.",
            "La carte ne veut plus rien dire. Les chemins changent.",
            "J'ai rêvé d'un endroit que je n'ai jamais visité. Ou peut-être que si."
        ],
        [5] = [
            "...",
            "Je ne sais plus où je suis. Je ne sais plus qui je suis.",
            "Les mots s'effacent à mesure que je les écris."
        ]
    };

    private static readonly Dictionary<int, string> AlcoholStateEntries = new()
    {
        [1] = "Le goût n'est plus le même. Il en faut plus pour sentir quelque chose.",
        [2] = "Sans une gorgée, le monde devient trop réel."
    };

    public JournalSystem(ICoreClientAPI api, SurviveConfig config)
    {
        _api = api;
        _config = config;

        api.Event.RegisterGameTickListener(OnTick, 3000);
    }

    private void OnTick(float dt)
    {
        var player = _api.World.Player;
        if (player?.Entity == null) return;

        var attrs = player.Entity.WatchedAttributes;

        // Detect mental level changes
        int mentalLevel = attrs.GetInt("survive:perceptionLevel", 0);
        if (mentalLevel != _lastMentalLevel && _lastMentalLevel >= 0)
        {
            OnMentalLevelChanged(mentalLevel);
        }
        _lastMentalLevel = mentalLevel;

        // Detect alcohol state changes
        // The alcohol counter is server-side invisible, but state transitions
        // are communicated via chat messages that we intercept
    }

    private void OnMentalLevelChanged(int newLevel)
    {
        if (newLevel <= 0) return;

        if (MentalLevelEntries.TryGetValue(newLevel, out var entries))
        {
            var entry = entries[_api.World.Rand.Next(entries.Length)];
            AddJournalEntry(entry);
        }
    }

    /// <summary>
    /// Adds a discreet journal entry. These are never shown as alerts —
    /// the player discovers them when they open their journal.
    /// </summary>
    private void AddJournalEntry(string text)
    {
        // Use the VS journal/lore discovery system
        _api.SendChatMessage($"/lore add survive {text}");

        // Also store locally for the mod's own journal UI (future)
        _api.Logger.Notification($"[Survive Journal] {text}");
    }

    public void OnAlcoholStateChanged(int newState)
    {
        if (AlcoholStateEntries.TryGetValue(newState, out var entry))
        {
            AddJournalEntry(entry);
        }
    }

    public void Dispose()
    {
        // Tick listener cleaned up automatically by API
    }
}
