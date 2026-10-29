using SuperMetroid.Core.Samus.Palettes;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>One charge palette: the suit colours a charge sequence shows for one suit at one of its steps.</summary>
/// <param name="Kind">The charge sequence the palette belongs to.</param>
/// <param name="Suit">The suit whose version of the step it is.</param>
/// <param name="Step">The sequence's step, counted from its first.</param>
public readonly record struct SamusChargePalette(SamusChargeKind Kind, SamusSuit Suit, int Step);
