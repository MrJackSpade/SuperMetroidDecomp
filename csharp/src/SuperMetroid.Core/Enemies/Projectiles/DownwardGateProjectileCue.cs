using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>One cue of the downward shot gate's programs, with the operands its native command carries.</summary>
internal abstract record DownwardGateProjectileCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private DownwardGateProjectileCue()
    {
    }

    /// <summary>Installs a movement routine (<c>Instruction_EnemyProjectile_PreInstructionInY</c>, $86:8161).</summary>
    /// <param name="PreInstruction">The movement routine the cue installs.</param>
    internal sealed record SetPreInstruction(EnemyProjectilePreInstruction PreInstruction) : DownwardGateProjectileCue;

    /// <summary>Sets the gate's signed 8.8 Y velocity (<c>Instruction_EnemyProjectile_ShotGate_EnemyVelocityInY</c>, $86:E533).</summary>
    /// <param name="YVelocity">The vertical velocity a <see cref="SetYVelocity"/> cue installs.</param>
    internal sealed record SetYVelocity(SignedEightEight YVelocity) : DownwardGateProjectileCue;

    /// <summary>A cue that carries no operand.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Command(DownwardGateProjectileCueKind Kind) : DownwardGateProjectileCue;
}
