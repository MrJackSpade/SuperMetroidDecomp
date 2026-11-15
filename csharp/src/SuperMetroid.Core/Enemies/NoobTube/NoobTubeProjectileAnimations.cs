using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Rooms.RoomObjects.NoobTubeProjectileCueKind;
using P = SuperMetroid.Core.Rooms.RoomObjects.NoobTubeProjectileSegment;
using F = SuperMetroid.Core.Enemies.Projectiles.EnemyProjectileFrameId;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.NoobTube;

/// <summary>
/// The n00b tube's crack, glass shard and released-air bubble animations (#641). Each frame shows a
/// <see cref="NoobTubeProjectileFrame"/>, and a shard's its mirror image on alternate updates; each cue is a
/// <see cref="NoobTubeProjectileCue"/>.
/// </summary>
internal static class NoobTubeProjectileAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<NoobTubeProjectileSegment, NoobTubeProjectileFrame, NoobTubeProjectileCue> Segments { get; } = new(
        new Dictionary<NoobTubeProjectileSegment, AnimationBeat<NoobTubeProjectileFrame, NoobTubeProjectileCue>[]>
    {
        [P.Crack] =
        [
            Show(F.NoobTubeCrackSpreadingFirst, 12),
            Show(F.NoobTubeCrackSpreadingSecond, 10),
            Show(F.NoobTubeCrackSpreadingThird, 8),
            Show(F.NoobTubeCrackSpreadingFourth, 6),
            Show(F.NoobTubeCrackSpreadingFifth, 6),
            Show(F.NoobTubeCrackSpreadingSixth, 6),
            Cue(new(K.StartCrackFlicker)),
            Show(F.NoobTubeCrackShatteringFirst, 1),
            Show(F.NoobTubeCrackShatteringSecond, 1),
            Show(F.NoobTubeCrackShatteringThird, 1),
            Show(F.NoobTubeCrackShatteringFourth, 1),
            Show(F.NoobTubeCrackShatteringFifth, 1),
            Show(F.NoobTubeCrackShatteringSixth, 2),
            Show(F.NoobTubeCrackShatteringSeventh, 3),
            Show(F.NoobTubeCrackShatteringEighth, 6),
            Show(F.NoobTubeCrackShatteringNinth, 9),
            Show(F.NoobTubeCrackShatteringTenth, 8),
            Cue(new(K.StartCrackFall)),
            Show(F.NoobTubeCrackFallingFirst, 7),
            Show(F.NoobTubeCrackFallingSecond, 7),
            Show(F.NoobTubeCrackFallingThird, 7),
            Show(F.NoobTubeCrackFallingFourth, 7),
            Show(F.NoobTubeCrackFallingFifth, 7),
            Show(F.NoobTubeCrackFallingSixth, 7),
            Show(F.NoobTubeCrackFallingSeventh, 7),
            Show(F.NoobTubeCrackFallingEighth, 7),
            Show(F.NoobTubeCrackFallingNinth, 7),
            Show(F.NoobTubeCrackFallingTenth, 7),
            Show(F.NoobTubeCrackFallingEleventh, 7),
            Show(F.NoobTubeCrackFallingTwelfth, 7),
            Show(F.NoobTubeCrackFallingThirteenth, 7),
            Show(F.NoobTubeCrackFallingFourteenth, 7),
            Show(F.NoobTubeCrackFallingFifteenth, 7),
            Show(F.NoobTubeCrackFallingSixteenth, 7),
            Show(F.NoobTubeCrackFallingSeventeenth, 7),
            Show(F.NoobTubeCrackFallingEighteenth, 7),
            Show(F.NoobTubeCrackFallingNineteenth, 16),
        ],
        [P.CrackFallen] =
        [
            Show(F.NoobTubeCrackFallenFirst, 16),
            Show(F.NoobTubeCrackFallenSecond, 16),
            Cue(new(K.Delete)),
        ],
        [P.ShardDownOutward] =
        [
        ],
        [P.ShardDownOutwardFlying] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(F.NoobTubeShardDownOutwardFlying, F.NoobTubeShardDownOutwardFlyingMirrored),
            Cue(new(K.AssignShardFallingAngle)),
            Cue(new(K.StartShardFall)),
        ],
        [P.ShardDownOutwardFalling] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(F.NoobTubeShardDownOutwardFalling, F.NoobTubeShardDownOutwardFallingMirrored),
            Cue(new(K.Delete)),
        ],
        [P.ShardUpOutward] =
        [
        ],
        [P.ShardUpOutwardFlying] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(F.NoobTubeShardUpOutwardFlying, F.NoobTubeShardUpOutwardFlyingMirrored),
            Cue(new(K.AssignShardFallingAngle)),
            Cue(new(K.StartShardFall)),
        ],
        [P.ShardUpOutwardFalling] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(F.NoobTubeShardUpOutwardFalling, F.NoobTubeShardUpOutwardFallingMirrored),
            Cue(new(K.Delete)),
        ],
        [P.ShardUpSteep] =
        [
        ],
        [P.ShardUpSteepFlying] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(F.NoobTubeShardUpSteepFlying, F.NoobTubeShardUpSteepFlyingMirrored),
            Cue(new(K.AssignShardFallingAngle)),
            Cue(new(K.StartShardFall)),
        ],
        [P.ShardUpSteepFalling] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(F.NoobTubeShardUpSteepFalling, F.NoobTubeShardUpSteepFallingMirrored),
            Cue(new(K.Delete)),
        ],
        [P.ShardUpDiagonalInner] =
        [
        ],
        [P.ShardUpDiagonalInnerFlying] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(F.NoobTubeShardUpDiagonalInnerFlying, F.NoobTubeShardUpDiagonalInnerFlyingMirrored),
            Cue(new(K.AssignShardFallingAngle)),
            Cue(new(K.StartShardFall)),
        ],
        [P.ShardUpDiagonalInnerFalling] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(F.NoobTubeShardUpDiagonalInnerFalling, F.NoobTubeShardUpDiagonalInnerFallingMirrored),
            Cue(new(K.Delete)),
        ],
        [P.ShardUpDiagonalOuter] =
        [
        ],
        [P.ShardUpDiagonalOuterFlying] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(F.NoobTubeShardUpDiagonalOuterFlying, F.NoobTubeShardUpDiagonalOuterFlyingMirrored),
            Cue(new(K.AssignShardFallingAngle)),
            Cue(new(K.StartShardFall)),
        ],
        [P.ShardUpDiagonalOuterFalling] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(F.NoobTubeShardUpDiagonalOuterFalling, F.NoobTubeShardUpDiagonalOuterFallingMirrored),
            Cue(new(K.Delete)),
        ],
        [P.ShardDownDiagonal] =
        [
        ],
        [P.ShardDownDiagonalFlying] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(F.NoobTubeShardDownDiagonalFlying, F.NoobTubeShardDownDiagonalFlyingMirrored),
            Cue(new(K.AssignShardFallingAngle)),
            Cue(new(K.StartShardFall)),
        ],
        [P.ShardDownDiagonalFalling] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(F.NoobTubeShardDownDiagonalFalling, F.NoobTubeShardDownDiagonalFallingMirrored),
            Cue(new(K.Delete)),
        ],
        [P.ShardDownOuter] =
        [
        ],
        [P.ShardDownOuterFlying] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(F.NoobTubeShardDownOuterFlying, F.NoobTubeShardDownOuterFlyingMirrored),
            Cue(new(K.AssignShardFallingAngle)),
            Cue(new(K.StartShardFall)),
        ],
        [P.ShardDownOuterFalling] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(F.NoobTubeShardDownOuterFalling, F.NoobTubeShardDownOuterFallingMirrored),
            Cue(new(K.Delete)),
        ],
        [P.ShardOutward] =
        [
        ],
        [P.ShardOutwardFlying] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(F.NoobTubeShardOutwardFlying, F.NoobTubeShardOutwardFlyingMirrored),
            Cue(new(K.AssignShardFallingAngle)),
            Cue(new(K.StartShardFall)),
        ],
        [P.ShardOutwardFalling] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(F.NoobTubeShardOutwardFalling, F.NoobTubeShardOutwardFallingMirrored),
            Cue(new(K.Delete)),
        ],
        [P.ShardUp] =
        [
        ],
        [P.ShardUpFlying] =
        [
            Cue(new(K.FlickerHidden)),
            Flicker(F.NoobTubeShardUpFlying),
            Cue(new(K.AssignShardFallingAngle)),
            Cue(new(K.StartShardFall)),
        ],
        [P.ShardUpFalling] =
        [
            Cue(new(K.FlickerHidden)),
            Flicker(F.NoobTubeShardUpFalling),
            Cue(new(K.Delete)),
        ],
        [P.ShardDownInner] =
        [
        ],
        [P.ShardDownInnerFlying] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(F.NoobTubeShardDownInnerFlying, F.NoobTubeShardDownInnerFlyingMirrored),
            Cue(new(K.AssignShardFallingAngle)),
            Cue(new(K.StartShardFall)),
        ],
        [P.ShardDownInnerFalling] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(F.NoobTubeShardDownInnerFalling, F.NoobTubeShardDownInnerFallingMirrored),
            Cue(new(K.Delete)),
        ],
        [P.ReleasedAirBubble] =
        [
            Cue(new(K.StartBubbleRise)),
            Show(F.NoobTubeBubbleRisingFirst, 2),
            Show(F.NoobTubeBubbleRisingSecond, 2),
            Show(F.NoobTubeBubbleRisingThird, 2),
            Show(F.NoobTubeBubbleRisingFourth, 2),
            Cue(new(K.AssignBubbleFallingAngle)),
            Cue(new(K.StartBubbleFall)),
            Show(F.NoobTubeBubbleFallingFirst, 2),
            Show(F.NoobTubeBubbleFallingSecond, 2),
            Show(F.NoobTubeBubbleFallingThird, 2),
            Show(F.NoobTubeBubbleFallingFourth, 2),
            Show(F.NoobTubeBubbleFallingFifth, 2),
            Show(F.NoobTubeBubbleFallingSixth, 2),
            Show(F.NoobTubeBubbleFallingSeventh, 4),
            Show(F.NoobTubeBubbleFallingEighth, 4),
            Show(F.NoobTubeBubbleFallingNinth, 4),
            Show(F.NoobTubeBubbleFallingTenth, 4),
            Show(F.NoobTubeBubbleFallingEleventh, 4),
            Cue(new(K.Delete)),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<NoobTubeProjectileSegment, Animation<NoobTubeProjectileSegment>> StartingAt { get; } = new Dictionary<NoobTubeProjectileSegment, Animation<NoobTubeProjectileSegment>>
    {
        [P.Crack] = new([new(P.Crack, 0, 37, 1), new(P.CrackFallen, 0, 2, 6), new(P.CrackFallen, 2, 1, 1)], AnimationEnd.Holds),
        [P.ShardDownOutward] = new([new(P.ShardDownOutwardFlying, 0, 2, 32), new(P.ShardDownOutwardFlying, 2, 2, 1), new(P.ShardDownOutwardFalling, 0, 2, 272), new(P.ShardDownOutwardFalling, 2, 1, 1)], AnimationEnd.Holds),
        [P.ShardUpOutward] = new([new(P.ShardUpOutwardFlying, 0, 2, 32), new(P.ShardUpOutwardFlying, 2, 2, 1), new(P.ShardUpOutwardFalling, 0, 2, 272), new(P.ShardUpOutwardFalling, 2, 1, 1)], AnimationEnd.Holds),
        [P.ShardUpSteep] = new([new(P.ShardUpSteepFlying, 0, 2, 32), new(P.ShardUpSteepFlying, 2, 2, 1), new(P.ShardUpSteepFalling, 0, 2, 272), new(P.ShardUpSteepFalling, 2, 1, 1)], AnimationEnd.Holds),
        [P.ShardUpDiagonalInner] = new([new(P.ShardUpDiagonalInnerFlying, 0, 2, 32), new(P.ShardUpDiagonalInnerFlying, 2, 2, 1), new(P.ShardUpDiagonalInnerFalling, 0, 2, 272), new(P.ShardUpDiagonalInnerFalling, 2, 1, 1)], AnimationEnd.Holds),
        [P.ShardUpDiagonalOuter] = new([new(P.ShardUpDiagonalOuterFlying, 0, 2, 32), new(P.ShardUpDiagonalOuterFlying, 2, 2, 1), new(P.ShardUpDiagonalOuterFalling, 0, 2, 272), new(P.ShardUpDiagonalOuterFalling, 2, 1, 1)], AnimationEnd.Holds),
        [P.ShardDownDiagonal] = new([new(P.ShardDownDiagonalFlying, 0, 2, 32), new(P.ShardDownDiagonalFlying, 2, 2, 1), new(P.ShardDownDiagonalFalling, 0, 2, 272), new(P.ShardDownDiagonalFalling, 2, 1, 1)], AnimationEnd.Holds),
        [P.ShardDownOuter] = new([new(P.ShardDownOuterFlying, 0, 2, 32), new(P.ShardDownOuterFlying, 2, 2, 1), new(P.ShardDownOuterFalling, 0, 2, 272), new(P.ShardDownOuterFalling, 2, 1, 1)], AnimationEnd.Holds),
        [P.ShardOutward] = new([new(P.ShardOutwardFlying, 0, 2, 32), new(P.ShardOutwardFlying, 2, 2, 1), new(P.ShardOutwardFalling, 0, 2, 272), new(P.ShardOutwardFalling, 2, 1, 1)], AnimationEnd.Holds),
        [P.ShardUp] = new([new(P.ShardUpFlying, 0, 2, 32), new(P.ShardUpFlying, 2, 2, 1), new(P.ShardUpFalling, 0, 2, 272), new(P.ShardUpFalling, 2, 1, 1)], AnimationEnd.Holds),
        [P.ShardDownInner] = new([new(P.ShardDownInnerFlying, 0, 2, 32), new(P.ShardDownInnerFlying, 2, 2, 1), new(P.ShardDownInnerFalling, 0, 2, 272), new(P.ShardDownInnerFalling, 2, 1, 1)], AnimationEnd.Holds),
        [P.ReleasedAirBubble] = new([new(P.ReleasedAirBubble, 0, 19, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<NoobTubeProjectileSegment> StartingWith(NoobTubeProjectileSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<NoobTubeProjectileFrame, NoobTubeProjectileCue> Show(EnemyProjectileFrameId frame, int duration) => new AnimationBeat<NoobTubeProjectileFrame, NoobTubeProjectileCue>.Frame(new(frame), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<NoobTubeProjectileFrame, NoobTubeProjectileCue> Cue(NoobTubeProjectileCue cue) => new AnimationBeat<NoobTubeProjectileFrame, NoobTubeProjectileCue>.Event(cue);

    /// <summary>
    /// Shows <paramref name="shard"/>, or on even updates its mirror image <paramref name="mirrored"/> across the tube, for
    /// the one update a flicker lasts.
    /// </summary>
    private static AnimationBeat<NoobTubeProjectileFrame, NoobTubeProjectileCue> Flicker(EnemyProjectileFrameId shard, EnemyProjectileFrameId mirrored) => new AnimationBeat<NoobTubeProjectileFrame, NoobTubeProjectileCue>.Frame(new(shard, mirrored), 1);

    /// <summary>Shows <paramref name="shard"/> for the one update a flicker lasts; on even updates the flicker hides it.</summary>
    private static AnimationBeat<NoobTubeProjectileFrame, NoobTubeProjectileCue> Flicker(EnemyProjectileFrameId shard) => new AnimationBeat<NoobTubeProjectileFrame, NoobTubeProjectileCue>.Frame(new(shard), 1);

    /// <summary>Every frame the pieces show, a shard's mirror images after its own, in the order the animations first show them.</summary>
    internal static IEnumerable<EnemyProjectileFrameId> ShownFrames =>
        Segments.Frames.SelectMany(frame => frame.Mirrored is { } mirrored ? [frame.Shown, mirrored] : new[] { frame.Shown }).Distinct();
}
