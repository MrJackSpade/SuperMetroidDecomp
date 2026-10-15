namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>What a cue of the n00b tube's crack, shards and bubbles programs does: one per bank-$86 command its lists use.</summary>
internal enum NoobTubeProjectileCueKind
{
    /// <summary>Gives a bubble a random falling angle (<c>Inststruction_EnemyProjectile_NoobTubeReleasedAirBubbles</c>, $86:D69A).</summary>
    AssignBubbleFallingAngle,
    /// <summary>Gives a shard a random falling angle and its falling speed (<c>Instruction_EnemyProjectile_AssignNoobTubeShardFallingAngle</c>, $86:D5E1).</summary>
    AssignShardFallingAngle,
    /// <summary>Deletes the projectile (<c>Instruction_EnemyProjectile_Delete</c>, $86:8154).</summary>
    Delete,
    /// <summary>Places the shard at its column on odd frames and hides it on even ones (<c>Instruction_EnemyProjectile_NoobTubeShard_FlickerSpritemap</c>, $86:D62A).</summary>
    FlickerHidden,
    /// <summary>Places the shard at its column on odd frames and mirrored across the tube on even ones (<c>Instruction_EnemyProj_FlickerNoobTubeShardLeftRightSpritemap</c>, $86:D5F2).</summary>
    FlickerReflected,
    /// <summary>Installs a movement routine (<c>Instruction_EnemyProjectile_PreInstructionInY</c>, $86:8161).</summary>
    SetPreInstruction,
}
