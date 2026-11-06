namespace SuperMetroid.Core.Fx;

/// <summary>How rain, spores and fog drift across the screen, in 1/256 pixel a frame (#1415).</summary>
public static class AtmosphereDriftDefinitions
{
    /// <summary>Rain falls six pixels a frame.</summary>
    public const int RainVerticalVelocity = 1536;

    /// <summary>Spores rise a quarter pixel a frame.</summary>
    public const int SporesVerticalVelocity = 64;

    /// <summary>Fog rises a quarter pixel a frame.</summary>
    public const int FogVerticalVelocity = 64;

    /// <summary>Fog drifts right five sixteenths of a pixel a frame.</summary>
    public const int FogHorizontalVelocity = 80;
}
