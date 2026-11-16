using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Rooms.RoomObjects.NoobTubeProjectileCueKind;
using P = SuperMetroid.Core.Rooms.RoomObjects.NoobTubeProjectileSegment;
using S = SuperMetroid.Core.Rooms.RoomObjects.NoobTubeShard;
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
            Show(F.NoobTubeCrackAcross31, 12),
            Show(F.NoobTubeCrackAcross63, 10),
            Show(F.NoobTubeCrackAcross95, 8),
            Show(F.NoobTubeCrackAcross127, 6),
            Show(F.NoobTubeCrackAcross159, 6),
            Show(F.NoobTubeCrackAcross159Branched, 6),
            Cue(new(K.StartCrackFlicker)),
            Show(F.NoobTubeShardsSpread111, 1),
            Show(F.NoobTubeShardsSpread143, 1),
            Show(F.NoobTubeShardsSpread175, 1),
            Show(F.NoobTubeShardsSpread207, 1),
            Show(F.NoobTubeShardsSpread223, 1),
            Show(F.NoobTubeShardsSpread235, 2),
            Show(F.NoobTubeShardsSpread243, 3),
            Show(F.NoobTubeShardsSpread247, 6),
            Show(F.NoobTubeShardsSpread251, 9),
            Show(F.NoobTubeShardsSpread255, 8),
            Cue(new(K.StartCrackFall)),
            Show(F.NoobTubeShardsDropped1, 7),
            Show(F.NoobTubeShardsDropped2, 7),
            Show(F.NoobTubeShardsDropped3, 7),
            Show(F.NoobTubeShardsDropped4, 7),
            Show(F.NoobTubeShardsDropped5, 7),
            Show(F.NoobTubeShardsDropped6, 7),
            Show(F.NoobTubeShardsDropped7, 7),
            Show(F.NoobTubeShardsDropped8, 7),
            Show(F.NoobTubeShardsDropped9, 7),
            Show(F.NoobTubeShardsDropped10, 7),
            Show(F.NoobTubeShardsDropped11, 7),
            Show(F.NoobTubeShardsDropped12, 7),
            Show(F.NoobTubeShardsDropped13, 7),
            Show(F.NoobTubeShardsDropped14, 7),
            Show(F.NoobTubeShardsDropped15, 7),
            Show(F.NoobTubeShardsDropped16, 7),
            Show(F.NoobTubeShardsDropped17, 7),
            Show(F.NoobTubeShardsDropped18, 7),
            Show(F.NoobTubeShardsDropped19, 16),
        ],
        [P.CrackFallen] =
        [
            Show(F.NoobTubeShardsLanded, 16),
            Show(F.NoobTubeShardsDropped19Twin, 16),
            Cue(new(K.Delete)),
        ],
        [P.ShardDownOutwardFlying] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(F.NoobTubeShardDownOutward, F.NoobTubeShardDownOutwardMirrored),
            Cue(new(K.AssignShardFallingAngle)),
            Cue(new(K.StartShardFall)),
        ],
        [P.ShardDownOutwardFalling] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(F.NoobTubeShardDownOutwardTwin, F.NoobTubeShardDownOutwardMirroredTwin),
            Cue(new(K.Delete)),
        ],
        [P.ShardUpOutwardFlying] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(F.NoobTubeShardUpOutward, F.NoobTubeShardUpOutwardMirrored),
            Cue(new(K.AssignShardFallingAngle)),
            Cue(new(K.StartShardFall)),
        ],
        [P.ShardUpOutwardFalling] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(F.NoobTubeShardUpOutwardTwin, F.NoobTubeShardUpOutwardMirroredTwin),
            Cue(new(K.Delete)),
        ],
        [P.ShardUpSteepFlying] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(F.NoobTubeShardUpSteep, F.NoobTubeShardUpSteepMirrored),
            Cue(new(K.AssignShardFallingAngle)),
            Cue(new(K.StartShardFall)),
        ],
        [P.ShardUpSteepFalling] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(F.NoobTubeShardUpSteepTwin, F.NoobTubeShardUpSteepMirroredTwin),
            Cue(new(K.Delete)),
        ],
        [P.ShardUpDiagonalInnerFlying] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(F.NoobTubeShardUpDiagonalInner, F.NoobTubeShardUpDiagonalInnerMirrored),
            Cue(new(K.AssignShardFallingAngle)),
            Cue(new(K.StartShardFall)),
        ],
        [P.ShardUpDiagonalInnerFalling] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(F.NoobTubeShardUpDiagonalInnerTwin, F.NoobTubeShardUpDiagonalInnerMirroredTwin),
            Cue(new(K.Delete)),
        ],
        [P.ShardUpDiagonalOuterFlying] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(F.NoobTubeShardUpDiagonalOuter, F.NoobTubeShardUpDiagonalOuterMirrored),
            Cue(new(K.AssignShardFallingAngle)),
            Cue(new(K.StartShardFall)),
        ],
        [P.ShardUpDiagonalOuterFalling] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(F.NoobTubeShardUpDiagonalOuterTwin, F.NoobTubeShardUpDiagonalOuterMirroredTwin),
            Cue(new(K.Delete)),
        ],
        [P.ShardDownDiagonalFlying] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(F.NoobTubeShardDownDiagonal, F.NoobTubeShardDownDiagonalMirrored),
            Cue(new(K.AssignShardFallingAngle)),
            Cue(new(K.StartShardFall)),
        ],
        [P.ShardDownDiagonalFalling] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(F.NoobTubeShardDownDiagonalTwin, F.NoobTubeShardDownDiagonalMirroredTwin),
            Cue(new(K.Delete)),
        ],
        [P.ShardDownOuterFlying] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(F.NoobTubeShardDownOuter, F.NoobTubeShardDownOuterMirrored),
            Cue(new(K.AssignShardFallingAngle)),
            Cue(new(K.StartShardFall)),
        ],
        [P.ShardDownOuterFalling] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(F.NoobTubeShardDownOuterTwin, F.NoobTubeShardDownOuterMirroredTwin),
            Cue(new(K.Delete)),
        ],
        [P.ShardOutwardFlying] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(F.NoobTubeShardOutward, F.NoobTubeShardOutwardMirrored),
            Cue(new(K.AssignShardFallingAngle)),
            Cue(new(K.StartShardFall)),
        ],
        [P.ShardOutwardFalling] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(F.NoobTubeShardOutwardTwin, F.NoobTubeShardOutwardMirroredTwin),
            Cue(new(K.Delete)),
        ],
        [P.ShardUpFlying] =
        [
            Cue(new(K.FlickerHidden)),
            Flicker(F.NoobTubeShardUp),
            Cue(new(K.AssignShardFallingAngle)),
            Cue(new(K.StartShardFall)),
        ],
        [P.ShardUpFalling] =
        [
            Cue(new(K.FlickerHidden)),
            Flicker(F.NoobTubeShardUpTwin),
            Cue(new(K.Delete)),
        ],
        [P.ShardDownInnerFlying] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(F.NoobTubeShardDownInner, F.NoobTubeShardDownInnerMirrored),
            Cue(new(K.AssignShardFallingAngle)),
            Cue(new(K.StartShardFall)),
        ],
        [P.ShardDownInnerFalling] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(F.NoobTubeShardDownInnerTwin, F.NoobTubeShardDownInnerMirroredTwin),
            Cue(new(K.Delete)),
        ],
        [P.ReleasedAirBubble] =
        [
            Cue(new(K.StartBubbleRise)),
            Show(F.NoobTubeDustTrail16, 2),
            Show(F.NoobTubeDustTrail24, 2),
            Show(F.NoobTubeDustTrail32, 2),
            Show(F.NoobTubeDustTrail40, 2),
            Cue(new(K.AssignBubbleFallingAngle)),
            Cue(new(K.StartBubbleFall)),
            Show(F.NoobTubeDustTrail40Twin, 2),
            Show(F.NoobTubeDustTrailThinning, 2),
            Show(F.NoobTubeDustTrailThin, 2),
            Show(F.NoobTubeDustTrailBreaking, 2),
            Show(F.NoobTubeDustBubblesForming, 2),
            Show(F.NoobTubeDustBubbles, 2),
            Show(F.NoobTubeDustBubblesDrifting, 4),
            Show(F.NoobTubeDustBubblesSparse, 4),
            Show(F.NoobTubeDustBubblesBursting, 4),
            Show(F.NoobTubeDustSpecks, 4),
            Show(F.NoobTubeDustSpecksFaint, 4),
            Cue(new(K.Delete)),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at: the crack's and the bubbles'.</summary>
    internal static IReadOnlyDictionary<NoobTubeProjectileSegment, Animation<NoobTubeProjectileSegment>> StartingAt { get; } = new Dictionary<NoobTubeProjectileSegment, Animation<NoobTubeProjectileSegment>>
    {
        [P.Crack] = new([new(P.Crack, 0, 37, 1), new(P.CrackFallen, 0, 2, 6), new(P.CrackFallen, 2, 1, 1)], AnimationEnd.Holds),
        [P.ReleasedAirBubble] = new([new(P.ReleasedAirBubble, 0, 19, 1)], AnimationEnd.Holds),
    };

    /// <summary>
    /// The animation each glass shard's spawn starts (#2044): its flight out of the tube, flickering for 32 plays, then its
    /// swaying fall, flickering for 272, after which it is deleted.
    /// </summary>
    internal static IReadOnlyDictionary<NoobTubeShard, Animation<NoobTubeProjectileSegment>> ShardAnimations { get; } = new Dictionary<NoobTubeShard, Animation<NoobTubeProjectileSegment>>
    {
        [S.DownOutward] = new([new(P.ShardDownOutwardFlying, 0, 2, 32), new(P.ShardDownOutwardFlying, 2, 2, 1), new(P.ShardDownOutwardFalling, 0, 2, 272), new(P.ShardDownOutwardFalling, 2, 1, 1)], AnimationEnd.Holds),
        [S.UpOutward] = new([new(P.ShardUpOutwardFlying, 0, 2, 32), new(P.ShardUpOutwardFlying, 2, 2, 1), new(P.ShardUpOutwardFalling, 0, 2, 272), new(P.ShardUpOutwardFalling, 2, 1, 1)], AnimationEnd.Holds),
        [S.UpSteep] = new([new(P.ShardUpSteepFlying, 0, 2, 32), new(P.ShardUpSteepFlying, 2, 2, 1), new(P.ShardUpSteepFalling, 0, 2, 272), new(P.ShardUpSteepFalling, 2, 1, 1)], AnimationEnd.Holds),
        [S.UpDiagonalInner] = new([new(P.ShardUpDiagonalInnerFlying, 0, 2, 32), new(P.ShardUpDiagonalInnerFlying, 2, 2, 1), new(P.ShardUpDiagonalInnerFalling, 0, 2, 272), new(P.ShardUpDiagonalInnerFalling, 2, 1, 1)], AnimationEnd.Holds),
        [S.UpDiagonalOuter] = new([new(P.ShardUpDiagonalOuterFlying, 0, 2, 32), new(P.ShardUpDiagonalOuterFlying, 2, 2, 1), new(P.ShardUpDiagonalOuterFalling, 0, 2, 272), new(P.ShardUpDiagonalOuterFalling, 2, 1, 1)], AnimationEnd.Holds),
        [S.DownDiagonal] = new([new(P.ShardDownDiagonalFlying, 0, 2, 32), new(P.ShardDownDiagonalFlying, 2, 2, 1), new(P.ShardDownDiagonalFalling, 0, 2, 272), new(P.ShardDownDiagonalFalling, 2, 1, 1)], AnimationEnd.Holds),
        [S.DownOuter] = new([new(P.ShardDownOuterFlying, 0, 2, 32), new(P.ShardDownOuterFlying, 2, 2, 1), new(P.ShardDownOuterFalling, 0, 2, 272), new(P.ShardDownOuterFalling, 2, 1, 1)], AnimationEnd.Holds),
        [S.Outward] = new([new(P.ShardOutwardFlying, 0, 2, 32), new(P.ShardOutwardFlying, 2, 2, 1), new(P.ShardOutwardFalling, 0, 2, 272), new(P.ShardOutwardFalling, 2, 1, 1)], AnimationEnd.Holds),
        [S.Up] = new([new(P.ShardUpFlying, 0, 2, 32), new(P.ShardUpFlying, 2, 2, 1), new(P.ShardUpFalling, 0, 2, 272), new(P.ShardUpFalling, 2, 1, 1)], AnimationEnd.Holds),
        [S.DownInner] = new([new(P.ShardDownInnerFlying, 0, 2, 32), new(P.ShardDownInnerFlying, 2, 2, 1), new(P.ShardDownInnerFalling, 0, 2, 272), new(P.ShardDownInnerFalling, 2, 1, 1)], AnimationEnd.Holds),
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
