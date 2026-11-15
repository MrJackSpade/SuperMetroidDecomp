using SuperMetroid.Core.Samus.Colours;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>One charge glow: the suit colours a charge sequence shows for one suit at one of its steps.</summary>
/// <param name="Kind">The charge sequence the shade belongs to.</param>
/// <param name="Suit">The suit whose version of the step it is.</param>
/// <param name="Step">The sequence's step, counted from its first.</param>
public readonly record struct SamusChargeShade(SamusChargeKind Kind, SamusSuit Suit, int Step);
