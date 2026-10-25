namespace SuperMetroid.Core.Fx;

/// <summary>How rain, spores and fog drift across the screen, in 1/256 pixel a frame, and what the spores blend with (#1415).</summary>
public static class AtmosphereDriftDefinitions
{
    /// <summary>Rain falls six pixels a frame.</summary>
    public const ushort RainVerticalVelocity = 1536;

    /// <summary>Spores rise a quarter pixel a frame.</summary>
    public const ushort SporesVerticalVelocity = 64;

    /// <summary>Fog rises a quarter pixel a frame.</summary>
    public const ushort FogVerticalVelocity = 64;

    /// <summary>Fog drifts right five sixteenths of a pixel a frame.</summary>
    public const ushort FogHorizontalVelocity = 80;

    /// <summary>
    /// The spores add to the background, the sprites drawn in blending palettes and the backdrop; the foreground
    /// does not blend ($88:80AB).
    /// </summary>
    public const ColorBlend SporesBlendedLayers = ColorBlend.Bg2 | ColorBlend.Sprites | ColorBlend.Backdrop;
}
