using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Survive.Config;
using Survive.Systems;

namespace Survive.Client;

/// <summary>
/// Renders the five survival gauges on the player's HUD.
/// Uses EnumRenderStage.Ortho for 2D overlay rendering.
/// </summary>
public class SurviveHudRenderer : IRenderer, IDisposable
{
    private readonly ICoreClientAPI _api;
    private readonly SurviveConfig _config;

    public double RenderOrder => 0.91;
    public int RenderRange => 1;

    // Bar colours (RGBA)
    private static readonly Vec4f ColorHydration = new(0.2f, 0.6f, 1.0f, 0.85f);
    private static readonly Vec4f ColorFatigue = new(0.6f, 0.4f, 0.8f, 0.85f);
    private static readonly Vec4f ColorMental = new(0.3f, 0.9f, 0.5f, 0.85f);
    private static readonly Vec4f ColorInjury = new(0.9f, 0.3f, 0.3f, 0.85f);
    private static readonly Vec4f ColorBackground = new(0.1f, 0.1f, 0.1f, 0.5f);

    // Layout
    private const float BarWidth = 120f;
    private const float BarHeight = 8f;
    private const float BarSpacing = 12f;
    private const float MarginRight = 10f;
    private const float MarginBottom = 60f;

    public SurviveHudRenderer(ICoreClientAPI api, SurviveConfig config)
    {
        _api = api;
        _config = config;

        api.Event.RegisterRenderer(this, EnumRenderStage.Ortho);
    }

    public void OnRenderFrame(float deltaTime, EnumRenderStage stage)
    {
        var player = _api.World.Player;
        if (player?.Entity == null) return;

        var attrs = player.Entity.WatchedAttributes;

        float screenW = _api.Render.FrameWidth;
        float screenH = _api.Render.FrameHeight;

        float x = screenW - BarWidth - MarginRight;
        float y = screenH - MarginBottom;

        // Draw bars bottom-to-top
        DrawBar(x, y, GetFraction(attrs, PlayerStatsManager.AttrMental, _config.MentalMax), ColorMental);
        y -= BarSpacing;

        // Injury — average of all zones (0-4 scale → 0-1)
        float injuryAvg = GetInjuryAverage(attrs);
        DrawBar(x, y, 1f - injuryAvg / 4f, ColorInjury);
        y -= BarSpacing;

        DrawBar(x, y, 1f - GetFraction(attrs, PlayerStatsManager.AttrFatigue, 100f), ColorFatigue);
        y -= BarSpacing;

        DrawBar(x, y, GetFraction(attrs, PlayerStatsManager.AttrHydration, _config.HydrationMax), ColorHydration);
    }

    private void DrawBar(float x, float y, float fraction, Vec4f color)
    {
        fraction = GameMath.Clamp(fraction, 0f, 1f);

        // Background
        _api.Render.Render2DTexture(0, (int)x, (int)y, (int)BarWidth, (int)BarHeight,
            ColorBackground.R, ColorBackground.G, ColorBackground.B, ColorBackground.A);

        // Filled portion
        int filledWidth = (int)(BarWidth * fraction);
        if (filledWidth > 0)
        {
            _api.Render.Render2DTexture(0, (int)x, (int)y, filledWidth, (int)BarHeight,
                color.R, color.G, color.B, color.A);
        }
    }

    private float GetFraction(Vintagestory.API.Datastructures.ITreeAttribute attrs, string key, float max)
    {
        float val = attrs.GetFloat(key, max);
        return max > 0 ? val / max : 0f;
    }

    private float GetInjuryAverage(Vintagestory.API.Datastructures.ITreeAttribute attrs)
    {
        var injuries = attrs.GetTreeAttribute(PlayerStatsManager.AttrInjuries);
        if (injuries == null) return 0f;

        float total = 0f;
        foreach (var zone in InjurySystem.AllZones)
        {
            total += injuries.GetInt(zone, 0);
        }
        return total / InjurySystem.AllZones.Length;
    }

    public void Dispose()
    {
        _api.Event.UnregisterRenderer(this, EnumRenderStage.Ortho);
    }
}
