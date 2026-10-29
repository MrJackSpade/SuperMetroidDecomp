namespace SuperMetroid.Core.Samus.Palettes;

/// <summary>The suit and cycle step that select an active Speed Booster palette.</summary>
/// <param name="Suit">The suit Samus shows.</param>
/// <param name="Step">The cycle step, counted from the suit's first entry.</param>
public readonly record struct SpeedBoosterPaletteSelection(SamusSuit Suit, int Step);
