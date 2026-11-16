using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>The ending's page geometry, white and brightness levels (#1416).</summary>
public static class EndingScreenDefinitions
{
    /// <summary>The screen's full brightness.</summary>
    public const int FullBrightness = 15;

    /// <summary>The screen's brightness when it is dark.</summary>
    public const int Dark = 0;

    /// <summary>White, with every channel at full intensity.</summary>
    public static RgbColor WhiteColor => RgbColor.White;
}
