namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>One cue of a cinematic sprite program.</summary>
/// <param name="Kind">What the cue does.</param>
/// <param name="PreInstruction">The pre-instruction a <see cref="CinematicSpriteCueKind.SetPreInstruction"/> cue installs.</param>
internal readonly record struct CinematicSpriteCue(CinematicSpriteCueKind Kind, CinematicSpritePreInstruction PreInstruction = default);
