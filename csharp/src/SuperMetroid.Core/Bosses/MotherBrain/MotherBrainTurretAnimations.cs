using SuperMetroid.Core.Animation;
using Beat = SuperMetroid.Core.Animation.AnimationBeat<SuperMetroid.Core.Bosses.MotherBrain.MotherBrainTurretFrame, SuperMetroid.Core.Bosses.MotherBrain.MotherBrainTurretCue>;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The turrets' and bullets' typed animations (#641, #1600).</summary>
internal static class MotherBrainTurretAnimations
{
    /// <summary>Frames in the impact smoke.</summary>
    internal const int ImpactPhaseCount = 5;

    /// <summary>Every turret segment: each turret and bullet pose by direction, and the impact smoke.</summary>
    private static readonly MotherBrainTurretPose[] Poses =
    [
        .. new[] { MotherBrainTurretSegment.TurretPose, MotherBrainTurretSegment.BulletPose }
            .SelectMany(segment => Enum.GetValues<MotherBrainTurretDirection>().Select(direction => new MotherBrainTurretPose(segment, direction))),
        MotherBrainTurretPose.BulletImpact,
    ];

    /// <summary>
    /// Every turret segment: each turret and bullet pose shows for one update; the impact smoke takes its first colouring, stops
    /// flying, shows four eight-update puffs and a 32-update fade, then deletes itself.
    /// </summary>
    internal static AnimationSegments<MotherBrainTurretPose, MotherBrainTurretFrame, MotherBrainTurretCue> Segments { get; } = new(
        Poses.ToDictionary(pose => pose, pose => pose == MotherBrainTurretPose.BulletImpact ? BulletImpact() : StillPose(pose)), Poses);

    /// <summary>Each segment's animation, played once: a pose then sleeps on its frame, and the deleted smoke rests.</summary>
    internal static IReadOnlyDictionary<MotherBrainTurretPose, Animation<MotherBrainTurretPose>> StartingAt { get; } =
        Poses.ToDictionary(pose => pose, pose => new Animation<MotherBrainTurretPose>([new(pose, 0, Segments.Beats(pose).Count, 1)], AnimationEnd.Holds));

    /// <summary>Shows the pose for one update.</summary>
    private static Beat[] StillPose(MotherBrainTurretPose pose) =>
        [new Beat.Frame(new(pose.Animation, MotherBrainTurretDefinitions.Ring.PositionOf(pose.Direction)), 1)];

    /// <summary>The impact smoke's beats.</summary>
    private static Beat[] BulletImpact() =>
    [
        new Beat.Event(MotherBrainTurretCue.DrawFromSharedSheet),
        new Beat.Event(MotherBrainTurretCue.ClearFrameBehavior),
        .. Enumerable.Range(0, ImpactPhaseCount).Select(phase =>
            new Beat.Frame(new(MotherBrainTurretSegment.BulletImpact, phase), phase == ImpactPhaseCount - 1 ? 32 : 8)),
        new Beat.Event(MotherBrainTurretCue.Delete),
    ];
}
