using SuperMetroid.Core.Animation;
using P = SuperMetroid.Core.Enemies.YappingMaws.YappingMawBodyProjectileSegment;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>
/// The Yapping Maw's body-link animations (#641). Each
/// frame shows a <see cref="EnemyProjectileFrameId"/> and each cue is a <see cref="EnemyProjectileCommonCue"/> with the values it acts on.
/// </summary>
internal static partial class YappingMawBodyProjectileAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static UnpausedAnimationSegments<YappingMawBodyProjectileSegment, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments { get; } = new(
        new Dictionary<YappingMawBodyProjectileSegment, AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Unpaused[]>
    {
        [P.FacingDown] =
        [
            Hold(EnemyProjectileFrameId.YappingMawFacingDown),
        ],
        [P.FacingUp] =
        [
            Hold(EnemyProjectileFrameId.YappingMawFacingUp),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<YappingMawBodyProjectileSegment, Animation<YappingMawBodyProjectileSegment>> StartingAt { get; } = new Dictionary<YappingMawBodyProjectileSegment, Animation<YappingMawBodyProjectileSegment>>
    {
        [P.FacingDown] = new([Segments.Whole(P.FacingDown)], AnimationEnd.Holds),
        [P.FacingUp] = new([Segments.Whole(P.FacingUp)], AnimationEnd.Holds),
    };

    /// <summary>Shows <paramref name="frame"/> for good (#2276): a still picture rests on it until its owner plays another.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Unpaused Hold(EnemyProjectileFrameId frame) => new AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.HeldFrame(frame);
}
