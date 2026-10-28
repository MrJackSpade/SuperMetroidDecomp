namespace SuperMetroid.Core.Frontend.GameOver.Artwork;

/// <summary>
/// Names the game-over document's Baby frames, cursor frames and Baby palettes (<see cref="GameOverPresentationDefinitions"/>).
/// </summary>
public static class GameOverPresentationNames
{
    /// <summary>Maps a compiled Baby animation frame to its authored sprite composition identity.</summary>
    /// <param name="frame">Closed, middle, or open mouth frame.</param>
    /// <returns>The corresponding <c>Baby.</c> sprite dictionary key.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The frame is unsupported.</exception>
    public static string BabyFrameName(GameOverBabyFrame frame) =>
        Ensure.Listed(GameOverPresentationDefinitions.BabyFrameNames, frame);

    /// <summary>Returns the authored composition identity for a missile-cursor frame.</summary>
    /// <param name="frame">Zero-based animation frame 0-3.</param>
    /// <returns><c>Cursor.</c> followed by the frame number.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The frame is outside 0-3.</exception>
    public static string CursorFrameName(int frame) =>
        Ensure.Listed(GameOverPresentationDefinitions.CursorFrameNames, frame);

    /// <summary>Maps a compiled Baby palette phase to its authored color-array identity.</summary>
    /// <param name="palette">Idle, closed-cry, middle-cry, or open-cry palette phase.</param>
    /// <returns>The corresponding <c>Baby.</c> palette dictionary key.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The palette identity is unsupported.</exception>
    public static string BabyPaletteName(GameOverBabyPalette palette) =>
        Ensure.Listed(GameOverPresentationDefinitions.BabyPaletteNames, palette);
}
