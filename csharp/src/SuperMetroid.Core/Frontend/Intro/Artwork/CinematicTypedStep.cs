namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>A step of a typed cinematic text (#1513): the animation that types the text, and the last glyph the step shows.</summary>
/// <param name="Text">The animation that types the text.</param>
/// <param name="Glyph">The place of the glyph the step types, from zero: the step shows every glyph up to it.</param>
internal readonly record struct CinematicTypedStep(CinematicSpriteSegment Text, int Glyph);
