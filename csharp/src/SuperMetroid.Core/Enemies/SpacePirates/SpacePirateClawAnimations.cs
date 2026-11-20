using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;
using P = SuperMetroid.Core.Enemies.SpacePirates.SpacePirateClawSegment;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>The Ninja Pirate claw's animations (#641, #2554).</summary>
internal static class SpacePirateClawAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<SpacePirateClawSegment, EnemyProjectileFrameId, SpacePirateClawCue> Segments { get; } = new(
        new Dictionary<SpacePirateClawSegment, AnimationBeat<EnemyProjectileFrameId, SpacePirateClawCue>[]>
    {
        [P.ClawLeft] =
        [
            Cue(HorizontalDirection.Left),
        ],
        [P.ClawLeftSpin] =
        [
            Show(EnemyProjectileFrameId.SpacePirateClawLeftTurned0, 1),
            Show(EnemyProjectileFrameId.SpacePirateClawLeftTurned45, 1),
            Show(EnemyProjectileFrameId.SpacePirateClawLeftTurned90, 1),
            Show(EnemyProjectileFrameId.SpacePirateClawLeftTurned135, 1),
            Show(EnemyProjectileFrameId.SpacePirateClawLeftTurned180, 1),
            Show(EnemyProjectileFrameId.SpacePirateClawLeftTurned225, 1),
            Show(EnemyProjectileFrameId.SpacePirateClawLeftTurned270, 1),
            Show(EnemyProjectileFrameId.SpacePirateClawLeftTurned315, 1),
        ],
        [P.ClawRight] =
        [
            Cue(HorizontalDirection.Right),
        ],
        [P.ClawRightSpin] =
        [
            Show(EnemyProjectileFrameId.SpacePirateClawRightTurned0, 1),
            Show(EnemyProjectileFrameId.SpacePirateClawRightTurned45, 1),
            Show(EnemyProjectileFrameId.SpacePirateClawRightTurned90, 1),
            Show(EnemyProjectileFrameId.SpacePirateClawRightTurned135, 1),
            Show(EnemyProjectileFrameId.SpacePirateClawRightTurned180, 1),
            Show(EnemyProjectileFrameId.SpacePirateClawRightTurned225, 1),
            Show(EnemyProjectileFrameId.SpacePirateClawRightTurned270, 1),
            Show(EnemyProjectileFrameId.SpacePirateClawRightTurned315, 1),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start: a claw's throw, played whole, then its spin looping (#2276).</summary>
    internal static IReadOnlyDictionary<SpacePirateClawSegment, Animation<SpacePirateClawSegment>> StartingAt { get; } = new Dictionary<SpacePirateClawSegment, Animation<SpacePirateClawSegment>>
    {
        [P.ClawLeft] = Animation<SpacePirateClawSegment>.LeadingInto([Segments.Whole(P.ClawLeft)], [Segments.Whole(P.ClawLeftSpin)]),
        [P.ClawRight] = Animation<SpacePirateClawSegment>.LeadingInto([Segments.Whole(P.ClawRight)], [Segments.Whole(P.ClawRightSpin)]),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<SpacePirateClawSegment> StartingWith(SpacePirateClawSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, SpacePirateClawCue> Show(EnemyProjectileFrameId frame, int duration) =>
        new AnimationBeat<EnemyProjectileFrameId, SpacePirateClawCue>.Frame(frame, duration);

    /// <summary>Publishes the cue that throws the claw toward <paramref name="direction"/>.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, SpacePirateClawCue> Cue(HorizontalDirection direction) =>
        new AnimationBeat<EnemyProjectileFrameId, SpacePirateClawCue>.Event(new(direction));
}
