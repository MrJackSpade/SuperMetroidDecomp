namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>One cinematic actor definition: the animation it starts. Its motion belongs to its owner (#1903).</summary>
/// <param name="Animation">The sprite animation it starts.</param>
internal readonly record struct CinematicActorDefinition(CinematicSpriteSegment Animation);
