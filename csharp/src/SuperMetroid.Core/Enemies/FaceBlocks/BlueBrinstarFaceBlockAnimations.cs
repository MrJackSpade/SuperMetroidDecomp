using P = SuperMetroid.Core.Enemies.FaceBlocks.BlueBrinstarFaceBlockSegment;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.FaceBlocks;

/// <summary>
/// The Blue Brinstar face block's neutral and turning animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and no beat raises a cue for its owner.
/// </summary>
internal static class BlueBrinstarFaceBlockAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<BlueBrinstarFaceBlockSegment, EnemyFrameId, NoAnimationCue> Segments { get; } = new(
        new Dictionary<BlueBrinstarFaceBlockSegment, AnimationBeat<EnemyFrameId, NoAnimationCue>[]>
    {
        [P.SamusLeft] =
        [
            Show(EnemyFrameId.FaceBlockNeutral, 48),
            Show(EnemyFrameId.BlueBrinstarFaceBlockGlancingLeft, 16),
            Show(EnemyFrameId.BlueBrinstarFaceBlockLookingLeft, 16),
        ],
        [P.SamusRight] =
        [
            Show(EnemyFrameId.FaceBlockNeutral, 48),
            Show(EnemyFrameId.BlueBrinstarFaceBlockGlancingRight, 16),
            Show(EnemyFrameId.BlueBrinstarFaceBlockLookingRight, 16),
        ],
        [P.LookingAhead] = [Hold(EnemyFrameId.FaceBlockNeutral)],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    private static IReadOnlyDictionary<BlueBrinstarFaceBlockSegment, Animation<BlueBrinstarFaceBlockSegment>> StartingAt { get; } = new Dictionary<BlueBrinstarFaceBlockSegment, Animation<BlueBrinstarFaceBlockSegment>>
    {
        [P.SamusLeft] = new([Segments.Whole(P.SamusLeft)], AnimationEnd.Holds),
        [P.SamusRight] = new([Segments.Whole(P.SamusRight)], AnimationEnd.Holds),
        [P.LookingAhead] = new([Segments.Whole(P.LookingAhead)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<BlueBrinstarFaceBlockSegment> StartingWith(BlueBrinstarFaceBlockSegment segment) => StartingAt[segment];

    /// <summary>A still picture (#2275): <paramref name="frame"/>, held from the update it shows.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Hold(EnemyFrameId frame) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.HeldFrame(frame);

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame(frame, duration);
}
