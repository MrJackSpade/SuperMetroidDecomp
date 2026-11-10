namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>
/// A colour effect's frame shown in a colour group (#1512, #1598): the group the effect recolours and the frame of its
/// animation the group shows, whose shade the owner of the group's pictures names.
/// </summary>
/// <param name="Colours">The colour group the frame recolours.</param>
/// <param name="Frame">The frame the group shows.</param>
internal readonly record struct ShownPaletteFxFrame(PaletteFxColours Colours, PaletteFxFrame Frame);
