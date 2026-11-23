using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;
using P = SuperMetroid.Core.Enemies.SpacePirates.SpacePirateProjectileSegment;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>The Pirate/Mother Brain laser and Ninja Pirate claw animations (#641).</summary>
internal static partial class SpacePirateProjectileAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static UnpausedAnimationSegments<SpacePirateProjectileSegment, EnemyProjectileFrameId, SpacePirateProjectileCue> Segments { get; } = new(
        new Dictionary<SpacePirateProjectileSegment, AnimationBeat<EnemyProjectileFrameId, SpacePirateProjectileCue>.Unpaused[]>
    {
        [P.LaserLeft] = Laser(),
        [P.LaserLeftFlicker] = LaserFlicker,
        [P.LaserRight] = Laser(),
        [P.LaserRightFlicker] = LaserFlicker,
        [P.ClawLeft] = Claw(),
        [P.ClawLeftSpin] = ClawSpin(EnemyProjectileFrameId.SpacePirateClawLeftTurned0, EnemyProjectileFrameId.SpacePirateClawLeftTurned45, EnemyProjectileFrameId.SpacePirateClawLeftTurned90, EnemyProjectileFrameId.SpacePirateClawLeftTurned135, EnemyProjectileFrameId.SpacePirateClawLeftTurned180, EnemyProjectileFrameId.SpacePirateClawLeftTurned225, EnemyProjectileFrameId.SpacePirateClawLeftTurned270, EnemyProjectileFrameId.SpacePirateClawLeftTurned315),
        [P.ClawRight] = Claw(),
        [P.ClawRightSpin] = ClawSpin(EnemyProjectileFrameId.SpacePirateClawRightTurned0, EnemyProjectileFrameId.SpacePirateClawRightTurned45, EnemyProjectileFrameId.SpacePirateClawRightTurned90, EnemyProjectileFrameId.SpacePirateClawRightTurned135, EnemyProjectileFrameId.SpacePirateClawRightTurned180, EnemyProjectileFrameId.SpacePirateClawRightTurned225, EnemyProjectileFrameId.SpacePirateClawRightTurned270, EnemyProjectileFrameId.SpacePirateClawRightTurned315),
    });

    /// <summary>
    /// The animation that starts at each segment an owner can start or restart at, each segment played whole: a laser's
    /// muzzle and flight, then its flicker looping; a claw's throw, then its spin looping (#2276).
    /// </summary>
    internal static IReadOnlyDictionary<SpacePirateProjectileSegment, Animation<SpacePirateProjectileSegment>> StartingAt { get; } = new Dictionary<SpacePirateProjectileSegment, Animation<SpacePirateProjectileSegment>>
    {
        [P.LaserLeft] = Animation<SpacePirateProjectileSegment>.LeadingInto([Segments.Whole(P.LaserLeft)], [Segments.Whole(P.LaserLeftFlicker)]),
        [P.LaserRight] = Animation<SpacePirateProjectileSegment>.LeadingInto([Segments.Whole(P.LaserRight)], [Segments.Whole(P.LaserRightFlicker)]),
        [P.ClawLeft] = Animation<SpacePirateProjectileSegment>.LeadingInto([Segments.Whole(P.ClawLeft)], [Segments.Whole(P.ClawLeftSpin)]),
        [P.ClawRight] = Animation<SpacePirateProjectileSegment>.LeadingInto([Segments.Whole(P.ClawRight)], [Segments.Whole(P.ClawRightSpin)]),
    };

    /// <summary>A laser fired <c>facing</c>: its flare grows, it fires, and the beam extends to its full length, flickering bright and dim.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, SpacePirateProjectileCue>.Unpaused[] Laser() =>
    [
        Show(EnemyProjectileFrameId.SpacePirateLaserFlareGlint, 2),
        Show(EnemyProjectileFrameId.SpacePirateLaserFlareGlow, 2),
        Show(EnemyProjectileFrameId.SpacePirateLaserFlareBall, 2),
        Cue(SpacePirateProjectileCueKind.FireLaser),
        Show(EnemyProjectileFrameId.SpacePirateLaserEmergingBright, 1),
        Show(EnemyProjectileFrameId.SpacePirateLaserEmergingDim, 1),
        Show(EnemyProjectileFrameId.SpacePirateLaserExtendingBright, 1),
        Show(EnemyProjectileFrameId.SpacePirateLaserExtendingDim, 1),
        Show(EnemyProjectileFrameId.SpacePirateLaserLengtheningBright, 1),
        Show(EnemyProjectileFrameId.SpacePirateLaserLengtheningDim, 1),
        .. LaserFlicker,
    ];

    /// <summary>A full-length laser flickering bright and dim, which ends its extension and plays on while it flies either way.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, SpacePirateProjectileCue>.Unpaused[] LaserFlicker =>
    [
        Show(EnemyProjectileFrameId.SpacePirateLaserFullLengthBright, 1),
        Show(EnemyProjectileFrameId.SpacePirateLaserFullLengthDim, 1),
    ];

    /// <summary>A claw thrown <c>facing</c>.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, SpacePirateProjectileCue>.Unpaused[] Claw() => [Cue(SpacePirateProjectileCueKind.ThrowClaw)];

    /// <summary>A thrown claw's spin over its side's <paramref name="turns"/>, turned by 0 degrees through to 315, one update each.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, SpacePirateProjectileCue>.Unpaused[] ClawSpin(params ReadOnlySpan<EnemyProjectileFrameId> turns) =>
        AnimationBeats<EnemyProjectileFrameId, SpacePirateProjectileCue>.Steady(1, turns);

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, SpacePirateProjectileCue>.Unpaused Show(EnemyProjectileFrameId frame, int duration) =>
        new AnimationBeat<EnemyProjectileFrameId, SpacePirateProjectileCue>.Frame(frame, duration);

    /// <summary>Publishes a cue of <paramref name="kind"/> toward <c>direction</c>.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, SpacePirateProjectileCue>.Unpaused Cue(
        SpacePirateProjectileCueKind kind) =>
        new AnimationBeat<EnemyProjectileFrameId, SpacePirateProjectileCue>.Event(new(kind));
}
