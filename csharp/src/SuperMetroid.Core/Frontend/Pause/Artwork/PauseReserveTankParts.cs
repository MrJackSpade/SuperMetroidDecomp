using static SuperMetroid.Core.Frontend.Pause.Artwork.PauseReserveTankDefinitions;

namespace SuperMetroid.Core.Frontend.Pause.Artwork;

/// <summary>
/// Draws the pause screen's reserve tanks (<see cref="PauseReserveTankDefinitions"/>): each tank's anchor, frame and fill level.
/// </summary>
public static class PauseReserveTankParts
{
    /// <summary>The pause sheet's frame for the reserve-strip frame <paramref name="frame"/>.</summary>
    public static string SheetFrameName(string frame) => $"ReserveTank.{frame}";
    /// <summary>$82:C1D6..C1E2: six eight-pixel columns from the first anchor, on one shared row.</summary>
    /// <param name="index">Zero-based tank/cap anchor, 0..5, not a reserve-energy quantity.</param>
    /// <returns>The stock screen-pixel origin before editable anchor deviations are applied.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The anchor is outside the six-entry strip.</exception>
    public static Position16<SpritePixels> StockAnchor(int index)
    {
        if ((uint)index >= AnchorCount) throw new ArgumentOutOfRangeException(nameof(index));
        return new(FirstAnchorX + index * AnchorStride, AnchorY);
    }
    /// <summary>$82:B305 full tank; $82:B396 trailing cap; $82:B37D empty tank and $82:B3D9 seven fill levels.</summary>
    /// <returns>Ten stable frame-name/native-spritemap pairs in full, end-cap, empty, then Fill1..Fill7 order; caller-owned reserve quantities select the frame.</returns>
    public static IEnumerable<PauseReserveTankFrame> Frames() => PauseReserveTankDefinitions.Frames;

    /// <summary>The partial-tank spritemap identity for a fill-step count.</summary>
    /// <param name="steps">Fill steps, 0 (empty) through 7.</param>
    /// <exception cref="ArgumentOutOfRangeException">The count is outside the eight fill levels.</exception>
    internal static PauseReserveTankVisual FillLevel(int steps) => Ensure.Element(FillLevels, steps);

    /// <summary>$82:B3D9 maps each of eight fill-step indices to $20..$27; both native tables agree.</summary>
    /// <param name="index">Entry index, 0..15, covering both identical eight-entry native tables.</param>
    /// <returns>The Empty-through-Fill7 spritemap identity selected by index modulo eight; gameplay computes the index using fourteen energy per step.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside the sixteen native entries.</exception>
    public static PauseReserveTankVisual PartialMap(int index) => (uint)index < PartialMapEntries
        ? FillLevel(index % FillLevels.Length) : throw new ArgumentOutOfRangeException(nameof(index));
}
