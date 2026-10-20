namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>One native six-byte bank-$8B cinematic sprite-object definition.</summary>
/// <param name="PreInstruction">The pre-instruction the object runs each frame.</param>
/// <param name="Program">The sprite program it starts.</param>
internal readonly record struct CinematicActorDefinition(CinematicSpritePreInstruction PreInstruction, CinematicSpriteProgram Program);
