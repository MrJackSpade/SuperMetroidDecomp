using SuperMetroid.Core.Samus.Colours;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>One full-body cycle shade: the shade a cycle family shows for one suit.</summary>
/// <param name="Family">The cycle the shade belongs to.</param>
/// <param name="Suit">The suit whose version of the shade it is.</param>
/// <param name="Shade">The shade's place among the family's four authored shades.</param>
public readonly partial record struct SamusFullBodyCycleShade(SamusFullBodyCycleFamily Family, SamusSuit Suit, int Shade);
