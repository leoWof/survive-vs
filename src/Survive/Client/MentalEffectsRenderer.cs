using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Survive.Config;

namespace Survive.Client;

/// <summary>
/// Client-side renderer for mental state visual effects.
/// Applies progressive visual distortions based on mental degradation level.
/// </summary>
public class MentalEffectsRenderer : IRenderer, IDisposable
{
    private readonly ICoreClientAPI _api;
    private readonly SurviveConfig _config;

    public double RenderOrder => 0.95;
    public int RenderRange => 1;

    private float _effectIntensity;
    private float _trembleTimer;

    public MentalEffectsRenderer(ICoreClientAPI api, SurviveConfig config)
    {
        _api = api;
        _config = config;

        api.Event.RegisterRenderer(this, EnumRenderStage.Ortho);
    }

    public void OnRenderFrame(float deltaTime, EnumRenderStage stage)
    {
        var player = _api.World.Player;
        if (player?.Entity == null) return;

        int mentalLevel = player.Entity.WatchedAttributes.GetInt("survive:perceptionLevel", 0);
        if (mentalLevel <= 0) return;

        _trembleTimer += deltaTime;

        // Smooth intensity transition
        float targetIntensity = mentalLevel / 5f;
        _effectIntensity += (targetIntensity - _effectIntensity) * deltaTime * 2f;

        switch (mentalLevel)
        {
            case 1: // Morosité — subtle desaturation effect
                ApplyVignette(0.15f);
                break;

            case 2: // Anxiété — slight screen tremor
                ApplyVignette(0.3f);
                ApplyTremor(0.002f);
                break;

            case 3: // Dépression — heavier vignette, occasional flickers
                ApplyVignette(0.5f);
                ApplyTremor(0.004f);
                break;

            case 4: // Dissociation — world distortion
                ApplyVignette(0.65f);
                ApplyTremor(0.008f);
                ApplyDistortion(0.3f);
                break;

            case 5: // Rupture — severe effects
                ApplyVignette(0.8f);
                ApplyTremor(0.015f);
                ApplyDistortion(0.6f);
                break;
        }
    }

    private void ApplyVignette(float intensity)
    {
        // Darken screen edges via overlay rendering
        float screenW = _api.Render.FrameWidth;
        float screenH = _api.Render.FrameHeight;
        float alpha = intensity * _effectIntensity;

        // Top/bottom bars for vignette effect
        int barH = (int)(screenH * 0.1f * alpha);
        _api.Render.Render2DTexture(0, 0, 0, (int)screenW, barH, 0f, 0f, 0f, alpha * 0.5f);
        _api.Render.Render2DTexture(0, 0, (int)screenH - barH, (int)screenW, barH, 0f, 0f, 0f, alpha * 0.5f);
    }

    private void ApplyTremor(float magnitude)
    {
        // Offset camera slightly for trembling effect
        float offsetX = (float)Math.Sin(_trembleTimer * 15f) * magnitude * _effectIntensity;
        float offsetY = (float)Math.Cos(_trembleTimer * 12f) * magnitude * _effectIntensity * 0.5f;

        _api.Render.ShaderUniforms.ExtraGodray += offsetX;
    }

    private void ApplyDistortion(float intensity)
    {
        // Advanced shader effects will be implemented in future versions
        // For now, use color shifting via overlay
        float alpha = intensity * _effectIntensity * 0.1f;
        float pulse = (float)Math.Sin(_trembleTimer * 0.5f) * 0.5f + 0.5f;

        _api.Render.Render2DTexture(0, 0, 0,
            (int)_api.Render.FrameWidth, (int)_api.Render.FrameHeight,
            0.1f * pulse, 0f, 0.15f * (1f - pulse), alpha);
    }

    public void Dispose()
    {
        _api.Event.UnregisterRenderer(this, EnumRenderStage.Ortho);
    }
}
