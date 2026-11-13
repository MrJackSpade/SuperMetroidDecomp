namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// What the equipment toggle does to play when it lands on the first reserve row (#1505): moving into the reserve
/// column with A pressed on the same frame toggles through the reserve category's empty equipment tables, which repaint
/// a run of projectile values as if they were label cells. Of those values only the last bomb slot's whole vertical
/// speed is read by play: a bomb from a charged bomb spread in that slot resumes, after the pause, with its whole
/// speed's palette field set to the disabled-label palette. No equipment changes.
/// </summary>
/// <remarks>
/// Consulted by the pause screen's equipment toggle when the reserve column's first row is selected. Evidence: the
/// toggle at <c>$82:B568</c> reads the reserve category's equipment word and mask through null table entries, so both
/// are the scratch word holding the row's label offset; the word tests as set, is cleared, and the disabled-label
/// palette loop at <c>$82:A29D</c> then writes through that cleared word. Its first write points the loop at the
/// projectile area, so it repaints the last bomb slot's vertical speed (read whole by the spread bomb's update at
/// <c>$90:D919</c>) and the low bytes' neighbours: projectile directions, which every reader masks to their low byte,
/// and bomb directions, which nothing reads. The other reserve rows take their tables from scratch other code left,
/// which only full memory emulation could reproduce, and stay refused under #1500.
/// </remarks>
[CompatibilityService("A same-frame equipment toggle into the first reserve row repaints the last spread bomb's whole vertical speed with the disabled-label palette")]
[EncodingOwner("a spread bomb's whole vertical speed repainted as a label cell")]
public static class ReserveRowToggleSpreadBombSpeed
{
    /// <summary>The first bit of a label cell's palette field.</summary>
    private const int PaletteShift = 10;

    /// <summary>The three bits of a label cell that hold its palette.</summary>
    private const int LabelPaletteField = 0b111 << PaletteShift;

    /// <summary>The disabled-label palette, palette 3, in that field.</summary>
    private const int DisabledLabelPalette = 3 << PaletteShift;

    /// <summary>
    /// The last bomb slot's whole vertical speed after the toggle repaints it; the speed's fraction is not touched. The
    /// pause screen applies it to the bomb frozen in that slot.
    /// </summary>
    /// <param name="wholeSpeed">The bomb's whole vertical speed before the toggle.</param>
    public static int Repaint(int wholeSpeed) => unchecked((short)((wholeSpeed & ~LabelPaletteField) | DisabledLabelPalette));
}
