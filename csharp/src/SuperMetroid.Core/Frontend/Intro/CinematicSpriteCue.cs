namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>One cue of a cinematic sprite animation.</summary>
/// <param name="Kind">What the cue does.</param>
/// <param name="FrameBehavior">The frame behaviour a <see cref="CinematicSpriteCueKind.SetFrameBehavior"/> cue installs.</param>
internal readonly record struct CinematicSpriteCue(CinematicSpriteCueKind Kind, CinematicSpriteFrameBehavior FrameBehavior = default);
