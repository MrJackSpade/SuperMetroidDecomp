namespace SuperMetroid.Core.Frontend;

/// <summary>One native six-byte intro-Rinka cinematic-object definition.</summary>
internal readonly record struct IntroRinkaActorDefinition(
    ushort PreInstruction,
    ushort InstructionList);
