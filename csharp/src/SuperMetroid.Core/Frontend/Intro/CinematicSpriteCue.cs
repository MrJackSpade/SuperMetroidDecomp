namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>One cue of a cinematic sprite animation, which the actor's owner carries out (#1903).</summary>
/// <param name="Kind">What the cue does.</param>
internal readonly record struct CinematicSpriteCue(CinematicSpriteCueKind Kind);
