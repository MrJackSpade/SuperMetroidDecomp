namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>One native six-byte bank-$8B cinematic sprite-object definition.</summary>
/// <param name="FrameBehavior">The pre-instruction the object runs each frame.</param>
/// <param name="Program">The sprite program it starts.</param>
internal readonly record struct CinematicActorDefinition(CinematicSpriteFrameBehavior FrameBehavior, CinematicSpriteProgram Program);
