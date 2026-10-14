using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Game;

/// <summary>A turret's, bullet's or impact smoke's typed animation.</summary>
internal sealed class MotherBrainTurretAnimation : EnemyProjectileTimelineAnimation<MotherBrainTurretFrame, MotherBrainTurretCue>
{
    /// <summary>The timeline played.</summary>
    private readonly AnimationTimeline<MotherBrainTurretFrame, MotherBrainTurretCue> timeline;

    /// <summary>Plays the still pose of <paramref name="program"/> facing <paramref name="direction"/>.</summary>
    internal MotherBrainTurretAnimation(MotherBrainTurretProgram program, MotherBrainTurretDirection direction)
    {
        Program = program;
        timeline = MotherBrainTurretAnimations.Pose(program, direction);
    }

    /// <summary>Plays a bullet's impact smoke.</summary>
    internal MotherBrainTurretAnimation()
    {
        Program = MotherBrainTurretProgram.BulletImpact;
        timeline = MotherBrainTurretAnimations.BulletImpact;
    }

    /// <summary>The program played.</summary>
    internal MotherBrainTurretProgram Program { get; }

    /// <inheritdoc/>
    private protected override AnimationTimeline<MotherBrainTurretFrame, MotherBrainTurretCue> Timeline => timeline;

    /// <inheritdoc/>
    private protected override ushort PresentationKey(MotherBrainTurretFrame frame) => MotherBrainTurretVisualDefinitions.PresentationKey(frame);
}
