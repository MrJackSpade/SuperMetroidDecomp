namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>The ending's text page geometry, blank glyph, white and sprite-sheet selections (#1416).</summary>
public static class EndingScreenDefinitions
{
    /// <summary>The ending font's blank tilemap word, which clears the credits and post-credits pages before text is drawn.</summary>
    public const ushort BlankTile = 0x007f;

    /// <summary>Width of the credits and result pages in tile cells.</summary>
    public const int TilemapWidth = 32;

    /// <summary>Height of the credits and result pages in tile cells.</summary>
    public const int TilemapHeight = 32;

    /// <summary>The tilemap words in one 32-by-32 credits or result page.</summary>
    public const int TilemapWords = TilemapWidth * TilemapHeight;

    /// <summary>White, with every channel at full intensity.</summary>
    public static RgbColor WhiteColor => RgbColor.White;

    /// <summary>The sprite-sheet selection of the escape and explosion scenes.</summary>
    public const byte EscapeObjectSelection = 2;

    /// <summary>The sprite-sheet selection of the atmospheric cloud scenes.</summary>
    public const byte CloudObjectSelection = 0xa3;

    /// <summary>The sprite-sheet selection of the reward: the contiguous sheet at the start of sprite memory.</summary>
    public const byte RewardObjectSelection = 0;

    /// <summary>The sprite-sheet selection the reward jump installs when Samus passes above the screen and her falling sheet replaces the reward sheet.</summary>
    public const byte FallingRewardObjectSelection = 3;
}
