namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>One full-body cycle palette: the shade a cycle family shows for one suit.</summary>
/// <param name="Family">The cycle the palette belongs to.</param>
/// <param name="Suit">The suit's place in Power, Varia, Gravity order.</param>
/// <param name="Shade">The shade's place among the family's four authored shades.</param>
public readonly record struct SamusFullBodyCyclePalette(SamusFullBodyCycleFamily Family, int Suit, int Shade);
