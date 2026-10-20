using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>A turret's, bullet's or impact smoke's typed animation.</summary>
internal sealed class MotherBrainTurretAnimation
    : EnemyProjectileProgramAnimation<MotherBrainTurretPose, MotherBrainTurretFrame, MotherBrainTurretCue>
{
    /// <summary>Plays the still pose of <paramref name="program"/> facing <paramref name="direction"/>.</summary>
    internal MotherBrainTurretAnimation(MotherBrainTurretProgram program, MotherBrainTurretDirection direction)
        : base(program == MotherBrainTurretProgram.BulletImpact
            ? throw new ArgumentOutOfRangeException(nameof(program), program, "Only turrets and bullets have direction poses.")
            : new MotherBrainTurretPose(program, direction))
    {
    }

    /// <summary>Plays a bullet's impact smoke.</summary>
    internal MotherBrainTurretAnimation()
        : base(MotherBrainTurretPose.BulletImpact)
    {
    }

    /// <inheritdoc/>
    private protected override AnimationProgramSet<MotherBrainTurretPose, MotherBrainTurretFrame, MotherBrainTurretCue> Programs =>
        MotherBrainTurretAnimations.Programs;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(MotherBrainTurretFrame frame) =>
        MotherBrainTurretVisualDefinitions.ShownProjectileFrames[frame];
}
