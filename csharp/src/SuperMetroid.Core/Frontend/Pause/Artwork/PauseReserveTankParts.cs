using static SuperMetroid.Core.Frontend.Pause.Artwork.PauseReserveTankDefinitions;

namespace SuperMetroid.Core.Frontend.Pause.Artwork;

/// <summary>
/// Draws the pause screen's reserve tanks (<see cref="PauseReserveTankDefinitions"/>): each tank's anchor, frame and fill level.
/// </summary>
public static class PauseReserveTankParts
{
    /// <summary>The pause sheet's frame for the reserve-strip frame <paramref name="frame"/>.</summary>
    public static string SheetFrameName(string frame) => $"ReserveTank.{frame}";
    /// <summary>Full tank, trailing cap, empty tank and seven fill levels.</summary>
    /// <returns>Ten stable frame-name/picture pairs in full, end-cap, empty, then Fill1..Fill7 order; caller-owned reserve quantities select the frame.</returns>
    public static IEnumerable<PauseReserveTankFrame> Frames() => PauseReserveTankDefinitions.Frames;

    /// <summary>The partial-tank picture identity for a fill-step count.</summary>
    /// <param name="steps">Fill steps, 0 (empty) through 7.</param>
    /// <exception cref="ArgumentOutOfRangeException">The count is outside the eight fill levels.</exception>
    internal static PauseReserveTankVisual FillLevel(int steps) => Ensure.Element(FillLevels, steps);
}
