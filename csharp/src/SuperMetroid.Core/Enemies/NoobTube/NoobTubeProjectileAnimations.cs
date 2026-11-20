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
    internal static UnpausedAnimationSegments<NoobTubeProjectileSegment, NoobTubeProjectileFrame, NoobTubeProjectileCue> Segments { get; } = new(
        new Dictionary<NoobTubeProjectileSegment, AnimationBeat<NoobTubeProjectileFrame, NoobTubeProjectileCue>.Unpaused[]>
    {
        [P.Crack] =
        [
            Show(F.NoobTubeCrackStarting, 12),
            Show(F.NoobTubeCrackSpreading, 10),
            Show(F.NoobTubeCrackWidening, 8),
            Show(F.NoobTubeCrackReaching, 6),
            Show(F.NoobTubeCrackSpanning, 6),
            Show(F.NoobTubeCrackBranching, 6),
            Cue(new(K.StartCrackFlicker)),
            Show(F.NoobTubeShardsBursting, 1),
            Show(F.NoobTubeShardsFlyingOut, 1),
            Show(F.NoobTubeShardsFlyingApart, 1),
            Show(F.NoobTubeShardsSpreading, 1),
            Show(F.NoobTubeShardsSlowing, 1),
            Show(F.NoobTubeShardsPeaking, 2),
            Show(F.NoobTubeShardsTurning, 3),
            Show(F.NoobTubeShardsCoasting, 6),
            Show(F.NoobTubeShardsTipping, 9),
            Show(F.NoobTubeShardsAtWidest, 8),
            Cue(new(K.StartCrackFall)),
            Show(F.NoobTubeShardsDipping, 7),
            Show(F.NoobTubeShardsDropping, 7),
            Show(F.NoobTubeShardsSinking, 7),
            Show(F.NoobTubeShardsLowering, 7),
            Show(F.NoobTubeShardsDescending, 7),
            Show(F.NoobTubeShardsFalling, 7),
            Show(F.NoobTubeShardsDrifting, 7),
            Show(F.NoobTubeShardsFloating, 7),
            Show(F.NoobTubeShardsWafting, 7),
            Show(F.NoobTubeShardsGliding, 7),
            Show(F.NoobTubeShardsSailing, 7),
            Show(F.NoobTubeShardsFluttering, 7),
            Show(F.NoobTubeShardsSubsiding, 7),
            Show(F.NoobTubeShardsSlipping, 7),
            Show(F.NoobTubeShardsSifting, 7),
            Show(F.NoobTubeShardsSettling, 7),
            Show(F.NoobTubeShardsSettlingLower, 7),
            Show(F.NoobTubeShardsNearlySettled, 7),
            Show(F.NoobTubeShardsComingToRest, 16),
        ],
        [P.CrackFallen] =
        [
            Show(F.NoobTubeShardsLanded, 16),
            Show(F.NoobTubeShardsComingToRest, 16),
        ],
        [P.Deletion] =
        [
            Cue(new(K.Delete)),
        ],
        [P.ShardDownOutwardFlicker] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(F.NoobTubeShardDownOutward, F.NoobTubeShardDownOutwardMirrored),
        ],
        [P.ShardFallStart] =
        [
            Cue(new(K.AssignShardFallingAngle)),
            Cue(new(K.StartShardFall)),
        ],
        [P.ShardUpOutwardFlicker] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(F.NoobTubeShardUpOutward, F.NoobTubeShardUpOutwardMirrored),
        ],
        [P.ShardUpSteepFlicker] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(F.NoobTubeShardUpSteep, F.NoobTubeShardUpSteepMirrored),
        ],
        [P.ShardUpDiagonalInnerFlicker] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(F.NoobTubeShardUpDiagonalInner, F.NoobTubeShardUpDiagonalInnerMirrored),
        ],
        [P.ShardUpDiagonalOuterFlicker] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(F.NoobTubeShardUpDiagonalOuter, F.NoobTubeShardUpDiagonalOuterMirrored),
        ],
        [P.ShardDownDiagonalFlicker] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(F.NoobTubeShardDownDiagonal, F.NoobTubeShardDownDiagonalMirrored),
        ],
        [P.ShardDownOuterFlicker] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(F.NoobTubeShardDownOuter, F.NoobTubeShardDownOuterMirrored),
        ],
        [P.ShardOutwardFlicker] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(F.NoobTubeShardOutward, F.NoobTubeShardOutwardMirrored),
        ],
        [P.ShardUpFlicker] =
        [
            Cue(new(K.FlickerHidden)),
            Flicker(F.NoobTubeShardUp),
        ],
        [P.ShardDownInnerFlicker] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(F.NoobTubeShardDownInner, F.NoobTubeShardDownInnerMirrored),
        ],
        [P.ReleasedAirBubble] =
        [
            Cue(new(K.StartBubbleRise)),
            Show(F.NoobTubeDustTrailForming, 2),
            Show(F.NoobTubeDustTrailGrowing, 2),
            Show(F.NoobTubeDustTrailLengthening, 2),
            Show(F.NoobTubeDustTrailFullLength, 2),
            Cue(new(K.AssignBubbleFallingAngle)),
            Cue(new(K.StartBubbleFall)),
            Show(F.NoobTubeDustTrailFullLength, 2),
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
        [P.Crack] = new([Segments.Whole(P.Crack), Segments.Repeated(P.CrackFallen, CrackFallenPlays), Segments.Whole(P.Deletion)], AnimationEnd.Holds),
        [P.ReleasedAirBubble] = new([Segments.Whole(P.ReleasedAirBubble)], AnimationEnd.Holds),
    };

    /// <summary>
    /// The animation each glass shard's spawn starts (#2044): its flight out of the tube, its own flicker played
    /// <see cref="FlightFlickers"/> times, the start of its fall, then its swaying fall, the same flicker played
    /// <see cref="FallFlickers"/> times (#2376), after which it is deleted; each phase plays its segment whole.
    /// </summary>
    internal static IReadOnlyDictionary<NoobTubeShard, Animation<NoobTubeProjectileSegment>> ShardAnimations { get; } = new Dictionary<NoobTubeShard, Animation<NoobTubeProjectileSegment>>
    {
        [S.DownOutward] = new([Segments.Repeated(P.ShardDownOutwardFlicker, FlightFlickers), Segments.Whole(P.ShardFallStart), Segments.Repeated(P.ShardDownOutwardFlicker, FallFlickers), Segments.Whole(P.Deletion)], AnimationEnd.Holds),
        [S.UpOutward] = new([Segments.Repeated(P.ShardUpOutwardFlicker, FlightFlickers), Segments.Whole(P.ShardFallStart), Segments.Repeated(P.ShardUpOutwardFlicker, FallFlickers), Segments.Whole(P.Deletion)], AnimationEnd.Holds),
        [S.UpSteep] = new([Segments.Repeated(P.ShardUpSteepFlicker, FlightFlickers), Segments.Whole(P.ShardFallStart), Segments.Repeated(P.ShardUpSteepFlicker, FallFlickers), Segments.Whole(P.Deletion)], AnimationEnd.Holds),
        [S.UpDiagonalInner] = new([Segments.Repeated(P.ShardUpDiagonalInnerFlicker, FlightFlickers), Segments.Whole(P.ShardFallStart), Segments.Repeated(P.ShardUpDiagonalInnerFlicker, FallFlickers), Segments.Whole(P.Deletion)], AnimationEnd.Holds),
        [S.UpDiagonalOuter] = new([Segments.Repeated(P.ShardUpDiagonalOuterFlicker, FlightFlickers), Segments.Whole(P.ShardFallStart), Segments.Repeated(P.ShardUpDiagonalOuterFlicker, FallFlickers), Segments.Whole(P.Deletion)], AnimationEnd.Holds),
        [S.DownDiagonal] = new([Segments.Repeated(P.ShardDownDiagonalFlicker, FlightFlickers), Segments.Whole(P.ShardFallStart), Segments.Repeated(P.ShardDownDiagonalFlicker, FallFlickers), Segments.Whole(P.Deletion)], AnimationEnd.Holds),
        [S.DownOuter] = new([Segments.Repeated(P.ShardDownOuterFlicker, FlightFlickers), Segments.Whole(P.ShardFallStart), Segments.Repeated(P.ShardDownOuterFlicker, FallFlickers), Segments.Whole(P.Deletion)], AnimationEnd.Holds),
        [S.Outward] = new([Segments.Repeated(P.ShardOutwardFlicker, FlightFlickers), Segments.Whole(P.ShardFallStart), Segments.Repeated(P.ShardOutwardFlicker, FallFlickers), Segments.Whole(P.Deletion)], AnimationEnd.Holds),
        [S.Up] = new([Segments.Repeated(P.ShardUpFlicker, FlightFlickers), Segments.Whole(P.ShardFallStart), Segments.Repeated(P.ShardUpFlicker, FallFlickers), Segments.Whole(P.Deletion)], AnimationEnd.Holds),
        [S.DownInner] = new([Segments.Repeated(P.ShardDownInnerFlicker, FlightFlickers), Segments.Whole(P.ShardFallStart), Segments.Repeated(P.ShardDownInnerFlicker, FallFlickers), Segments.Whole(P.Deletion)], AnimationEnd.Holds),
    };

    /// <summary>The plays of the fallen crack's last pieces before it is deleted.</summary>
    private const int CrackFallenPlays = 6;

    /// <summary>The flickers of a shard's flight out of the tube.</summary>
    private const int FlightFlickers = 32;

    /// <summary>The flickers of a shard's swaying fall.</summary>
    private const int FallFlickers = 272;

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<NoobTubeProjectileSegment> StartingWith(NoobTubeProjectileSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<NoobTubeProjectileFrame, NoobTubeProjectileCue>.Unpaused Show(EnemyProjectileFrameId frame, int duration) => new AnimationBeat<NoobTubeProjectileFrame, NoobTubeProjectileCue>.Frame(new(frame), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<NoobTubeProjectileFrame, NoobTubeProjectileCue>.Unpaused Cue(NoobTubeProjectileCue cue) => new AnimationBeat<NoobTubeProjectileFrame, NoobTubeProjectileCue>.Event(cue);

    /// <summary>
    /// Shows <paramref name="shard"/>, or on even updates its mirror image <paramref name="mirrored"/> across the tube, for
    /// the one update a flicker lasts.
    /// </summary>
    private static AnimationBeat<NoobTubeProjectileFrame, NoobTubeProjectileCue>.Unpaused Flicker(EnemyProjectileFrameId shard, EnemyProjectileFrameId mirrored) => new AnimationBeat<NoobTubeProjectileFrame, NoobTubeProjectileCue>.Frame(new(shard, mirrored), 1);

    /// <summary>Shows <paramref name="shard"/> for the one update a flicker lasts; on even updates the flicker hides it.</summary>
    private static AnimationBeat<NoobTubeProjectileFrame, NoobTubeProjectileCue>.Unpaused Flicker(EnemyProjectileFrameId shard) => new AnimationBeat<NoobTubeProjectileFrame, NoobTubeProjectileCue>.Frame(new(shard), 1);

    /// <summary>Every frame the pieces show, a shard's mirror images after its own, in the order the animations first show them.</summary>
    internal static IEnumerable<EnemyProjectileFrameId> ShownFrames =>
        Segments.Frames.SelectMany(frame => frame.Mirrored is { } mirrored ? [frame.Shown, mirrored] : new[] { frame.Shown }).Distinct();
}
