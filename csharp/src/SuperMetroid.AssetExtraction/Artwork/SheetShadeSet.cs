namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// The shades a sheet takes for one source (#1512): those that recolour its inks, and those that select another palette
/// row for its draws.
/// </summary>
/// <param name="Inks">The shades that recolour the sheet's inks.</param>
/// <param name="Rows">The shades whose draws select another palette row.</param>
internal sealed record SheetShadeSet(IReadOnlyList<SheetShade> Inks, IReadOnlyList<SheetRowShade> Rows);
