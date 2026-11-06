namespace SuperMetroid.Core.Frontend.Title;

/// <summary>A reveal of a title card (#1513): the card, and the last glyph the reveal shows.</summary>
/// <param name="Card">The phase whose card the reveal belongs to.</param>
/// <param name="Glyph">The place of the glyph the reveal types, from zero: the reveal shows every glyph up to it.</param>
internal readonly record struct TitleTypedStep(TitleSequencePhase Card, int Glyph);
