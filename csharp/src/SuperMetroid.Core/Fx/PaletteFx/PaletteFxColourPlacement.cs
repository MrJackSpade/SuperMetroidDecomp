namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>
/// Where a palette-FX colour group's first colour is held until colour sets are keyed by owner (#1464): an owner's
/// colours, from one colour on. Only <see cref="PaletteFxColourPlacementDefinitions"/> names placements.
/// </summary>
/// <param name="Owner">Whose colours the records write.</param>
/// <param name="Colour">The first colour written, among the owner's.</param>
internal readonly record struct PaletteFxColourPlacement(PaletteFxColourOwner Owner, int Colour);
