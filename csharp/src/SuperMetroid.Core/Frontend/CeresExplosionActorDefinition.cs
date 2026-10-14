namespace SuperMetroid.Core.Frontend;

/// <summary>One native six-byte bank-$8B cinematic sprite-object definition.</summary>
internal readonly record struct CeresExplosionActorDefinition(
    ushort PreInstruction,
    ushort InstructionList);
