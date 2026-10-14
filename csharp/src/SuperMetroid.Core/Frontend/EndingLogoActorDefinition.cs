namespace SuperMetroid.Core.Frontend;

/// <summary>One native six-byte final-logo cinematic-object definition.</summary>
internal readonly record struct EndingLogoActorDefinition(
    ushort PreInstruction,
    ushort InstructionList);
