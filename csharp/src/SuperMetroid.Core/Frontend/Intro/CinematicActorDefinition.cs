namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>One six-byte cinematic sprite-object definition.</summary>
/// <param name="FrameBehavior">The frame behaviour the object runs each frame.</param>
/// <param name="Program">The sprite program it starts.</param>
internal readonly record struct CinematicActorDefinition(CinematicSpriteFrameBehavior FrameBehavior, CinematicSpriteProgram Program);
