using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>The ending's page geometry and white (#1416).</summary>
public static class EndingScreenDefinitions
{
    /// <summary>Width of the credits and result pages in cells.</summary>
    public const int TilemapWidth = 32;

    /// <summary>Height of the credits and result pages in cells.</summary>
    public const int TilemapHeight = 32;

    /// <summary>White, with every channel at full intensity.</summary>
    public static RgbColor WhiteColor => RgbColor.White;
}
