using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One cue of Mother Brain's onion rings, bombs, breath, drool, door fragments and subtitle programs, with the operands its native command carries.</summary>
internal abstract record MotherBrainEffectCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private MotherBrainEffectCue()
    {
    }

    /// <summary>Installs a movement routine (<c>Instruction_EnemyProjectile_PreInstructionInY</c>, $86:8161).</summary>
    /// <param name="PreInstruction">The movement routine the cue installs.</param>
    internal sealed record SetPreInstruction(EnemyProjectilePreInstruction PreInstruction) : MotherBrainEffectCue;

    /// <summary>Sets the projectile's packed X and Y collision radii (<c>Instruction_EnemyProjectile_XYRadiusInY</c>, $86:8298).</summary>
    /// <param name="Radius">The collision half-extents <see cref="SetRadius"/> installs.</param>
    internal sealed record SetRadius(Extent16<RoomPixels> Radius) : MotherBrainEffectCue;

    /// <summary>A cue that carries no operand.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Command(MotherBrainEffectCueKind Kind) : MotherBrainEffectCue;
}
