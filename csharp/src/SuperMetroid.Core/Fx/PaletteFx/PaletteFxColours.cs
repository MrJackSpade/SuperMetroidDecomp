namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Where a palette-FX object's next colour record starts (#1405): an owner's colours, from one colour on.</summary>
/// <param name="Owner">Whose colours the records write.</param>
/// <param name="Colour">The first colour written, among the owner's.</param>
public readonly record struct PaletteFxColours(PaletteFxColourOwner Owner, int Colour);
