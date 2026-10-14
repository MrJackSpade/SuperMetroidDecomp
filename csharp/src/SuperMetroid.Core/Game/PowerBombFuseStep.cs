namespace SuperMetroid.Core.Game;

/// <summary>Writes and caller-owned side effects of one native Power Bomb fuse invocation.</summary>
public readonly record struct PowerBombFuseStep(
    ushort Timer, ushort InstructionPointer, bool SpawnExplosion, bool DeleteProjectile);
