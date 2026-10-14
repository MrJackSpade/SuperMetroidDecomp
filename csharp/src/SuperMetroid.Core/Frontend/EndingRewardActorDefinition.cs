namespace SuperMetroid.Core.Frontend;

/// <summary>One native six-byte ending reward cinematic-object definition.</summary>
internal readonly record struct EndingRewardActorDefinition(
    EndingRewardInitialization Initialization,
    ushort PreInstruction,
    ushort InstructionList);
