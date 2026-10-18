namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>One native six-byte intro-Rinka cinematic-object definition.</summary>
internal readonly record struct IntroRinkaActorDefinition(
    CinematicSpritePreInstruction PreInstruction,
    CinematicSpriteProgram Program);
