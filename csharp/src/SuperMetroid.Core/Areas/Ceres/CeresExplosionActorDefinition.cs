namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>One native six-byte bank-$8B cinematic sprite-object definition.</summary>
internal readonly record struct CeresExplosionActorDefinition(
    ushort PreInstruction,
    ushort InstructionList);
