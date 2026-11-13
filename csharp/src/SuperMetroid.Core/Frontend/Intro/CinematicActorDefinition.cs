namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>One cinematic sprite-object definition.</summary>
/// <param name="FrameBehavior">The frame behaviour the object runs each frame.</param>
/// <param name="Animation">The sprite program it starts.</param>
internal readonly record struct CinematicActorDefinition(CinematicSpriteFrameBehavior FrameBehavior, CinematicSpriteSegment Animation);
