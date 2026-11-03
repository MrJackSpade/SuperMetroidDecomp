namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>
/// A palette-FX record shown in a colour group (#1512): the group a palette-FX object recolours and the record of its
/// cycle the group shows, whose shade the owner of the group's pictures names.
/// </summary>
/// <param name="Colours">The colour group the record recolours.</param>
/// <param name="Record">The record the group shows.</param>
internal readonly record struct ShownPaletteFxRecord(PaletteFxColours Colours, PaletteFxFrame Record);
