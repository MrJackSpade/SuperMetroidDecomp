namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>One native six-byte final-logo cinematic-object definition.</summary>
internal readonly record struct EndingLogoActorDefinition(
    ushort PreInstruction,
    ushort InstructionList);
