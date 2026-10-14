using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Bosses.MotherBrain;

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
