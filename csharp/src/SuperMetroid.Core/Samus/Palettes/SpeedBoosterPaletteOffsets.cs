namespace SuperMetroid.Core.Samus.Palettes;

/// <summary>The suit and phase offsets that select a speed booster palette.</summary>
/// <param name="Suit">The suit.</param>
/// <param name="Phase">The phase.</param>
public readonly record struct SpeedBoosterPaletteOffsets(int Suit, int Phase);
