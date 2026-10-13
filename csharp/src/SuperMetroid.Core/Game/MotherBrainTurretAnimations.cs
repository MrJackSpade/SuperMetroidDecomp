using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Game;

/// <summary>The programs of Mother Brain's room turrets and their bullets.</summary>
public enum MotherBrainTurretProgram
{
    /// <summary>A turret's pose for one of the eight directions ($86:C101, six bytes a direction).</summary>
    TurretPose,
    /// <summary>A bullet's pose for one of the eight directions ($86:C143, six bytes a direction).</summary>
    BulletPose,
    /// <summary>The smoke of a bullet that touches Samus or is shot ($86:C19A).</summary>
    BulletImpact,
}

/// <summary>One turret frame: its program and its direction or smoke phase.</summary>
/// <param name="Program">The program the frame belongs to.</param>
/// <param name="Index">The direction of a pose, or the phase of the smoke.</param>
public readonly record struct MotherBrainTurretFrame(MotherBrainTurretProgram Program, int Index);

/// <summary>Gameplay cues of the turret programs.</summary>
internal enum MotherBrainTurretCue
{
    /// <summary>Draws the smoke with palette zero.</summary>
    UsePaletteZero,
    /// <summary>Stops the bullet's flight pre-instruction.</summary>
    ClearPreInstruction,
    /// <summary>Deletes the finished smoke.</summary>
    Delete,
}

/// <summary>The turrets' and bullets' typed programs (#641), in place of their bank-$86 instruction lists.</summary>
internal static class MotherBrainTurretAnimations
{
    /// <summary>Frames in the impact smoke.</summary>
    internal const int ImpactPhaseCount = 5;

    /// <summary>The still pose facing <paramref name="direction"/>: one update, then the native sleep keeps it.</summary>
    internal static AnimationTimeline<MotherBrainTurretFrame, MotherBrainTurretCue> Pose(MotherBrainTurretProgram program, MotherBrainTurretDirection direction) =>
        program switch
        {
            MotherBrainTurretProgram.TurretPose => TurretPoses[(int)direction],
            MotherBrainTurretProgram.BulletPose => BulletPoses[(int)direction],
            _ => throw new ArgumentOutOfRangeException(nameof(program), program, "Only turrets and bullets have direction poses."),
        };

    /// <summary>The turret poses, by direction.</summary>
    private static readonly AnimationTimeline<MotherBrainTurretFrame, MotherBrainTurretCue>[] TurretPoses =
        [.. Enum.GetValues<MotherBrainTurretDirection>().Select(direction => StillPose(MotherBrainTurretProgram.TurretPose, direction))];

    /// <summary>The bullet poses, by direction.</summary>
    private static readonly AnimationTimeline<MotherBrainTurretFrame, MotherBrainTurretCue>[] BulletPoses =
        [.. Enum.GetValues<MotherBrainTurretDirection>().Select(direction => StillPose(MotherBrainTurretProgram.BulletPose, direction))];

    /// <summary>Shows the pose for one update and sleeps.</summary>
    private static AnimationTimeline<MotherBrainTurretFrame, MotherBrainTurretCue> StillPose(MotherBrainTurretProgram program, MotherBrainTurretDirection direction) => new(
        AnimationStep<MotherBrainTurretFrame, MotherBrainTurretCue>.Show(new(program, (int)direction), 1),
        AnimationStep<MotherBrainTurretFrame, MotherBrainTurretCue>.Hold);

    /// <summary>The impact smoke: palette zero, no more flight, four eight-update puffs and a 32-update fade, then deletion.</summary>
    internal static AnimationTimeline<MotherBrainTurretFrame, MotherBrainTurretCue> BulletImpact { get; } = new(
    [
        AnimationStep<MotherBrainTurretFrame, MotherBrainTurretCue>.Publish(MotherBrainTurretCue.UsePaletteZero),
        AnimationStep<MotherBrainTurretFrame, MotherBrainTurretCue>.Publish(MotherBrainTurretCue.ClearPreInstruction),
        .. Enumerable.Range(0, ImpactPhaseCount).Select(phase => AnimationStep<MotherBrainTurretFrame, MotherBrainTurretCue>.Show(
            new(MotherBrainTurretProgram.BulletImpact, phase), phase == ImpactPhaseCount - 1 ? 32 : 8)),
        AnimationStep<MotherBrainTurretFrame, MotherBrainTurretCue>.Publish(MotherBrainTurretCue.Delete),
        AnimationStep<MotherBrainTurretFrame, MotherBrainTurretCue>.Hold,
    ]);
}

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
