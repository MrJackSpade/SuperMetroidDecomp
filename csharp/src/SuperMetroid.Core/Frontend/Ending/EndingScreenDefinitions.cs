using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>The ending's text page geometry, blank glyph and white (#1416).</summary>
public static class EndingScreenDefinitions
{
    /// <summary>The ending font's blank tile, which clears the credits and post-credits pages before text is drawn.</summary>
    public static BackgroundTile BlankTile { get; } = new(0x07f, 0, highPriority: false);

    /// <summary>Width of the credits and result pages in tile cells.</summary>
    public const int TilemapWidth = 32;

    /// <summary>Height of the credits and result pages in tile cells.</summary>
    public const int TilemapHeight = 32;

    /// <summary>The cells in one 32-by-32 credits or result page.</summary>
    public const int TilemapWords = TilemapWidth * TilemapHeight;

    /// <summary>White, with every channel at full intensity.</summary>
    public static RgbColor WhiteColor => RgbColor.White;
}
