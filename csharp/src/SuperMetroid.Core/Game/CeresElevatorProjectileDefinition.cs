namespace SuperMetroid.Core.Game;

/// <summary>One immutable bank-$86 enemy-projectile header.</summary>
internal readonly record struct CeresElevatorProjectileDefinition(
    ushort DefinitionPointer,
    ushort InitialInstruction);
