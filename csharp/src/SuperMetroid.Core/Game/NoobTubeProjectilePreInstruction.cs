namespace SuperMetroid.Core.Game;

/// <summary>The six bank-$86 movement callbacks the n00b-tube projectile lists install.</summary>
internal enum NoobTubeProjectilePreInstruction : ushort
{
    /// <summary>$86:D7BF: flicker the n00b-tube crack actor.</summary>
    CrackFlickering = (ushort)EnemyProjectilePreInstruction.NoobTubeCrackFlickering,
    /// <summary>$86:D7DE: move the detached n00b-tube crack downward.</summary>
    CrackFalling = (ushort)EnemyProjectilePreInstruction.NoobTubeCrackFalling,
    /// <summary>$86:D7FD: move a newly emitted n00b-tube shard.</summary>
    ShardFlying = (ushort)EnemyProjectilePreInstruction.NoobTubeShardFlying,
    /// <summary>$86:D83D: rotate and fall after a shard's initial flight.</summary>
    ShardFalling = (ushort)EnemyProjectilePreInstruction.NoobTubeShardFalling,
    /// <summary>$86:D89F: rotate and fall a released-air bubble.</summary>
    BubbleFalling = (ushort)EnemyProjectilePreInstruction.NoobTubeBubbleFalling,
    /// <summary>$86:D8DF: move a released-air bubble vertically.</summary>
    BubbleFlying = (ushort)EnemyProjectilePreInstruction.NoobTubeBubbleFlying,
}
