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
            Show(EnemyFrameId.BlueBrinstarFaceBlockSamusLeftSecond, 16),
            Show(EnemyFrameId.BlueBrinstarFaceBlockSamusLeftThird, 16),
        ],
        [P.SamusRight] =
        [
            Show(EnemyFrameId.FaceBlockNeutral, 48),
            Show(EnemyFrameId.BlueBrinstarFaceBlockSamusRightSecond, 16),
            Show(EnemyFrameId.BlueBrinstarFaceBlockSamusRightThird, 16),
        ],
        [P.Initial] =
        [
            Show(EnemyFrameId.FaceBlockNeutral, 1),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<BlueBrinstarFaceBlockSegment, Animation<BlueBrinstarFaceBlockSegment>> StartingAt { get; } = new Dictionary<BlueBrinstarFaceBlockSegment, Animation<BlueBrinstarFaceBlockSegment>>
    {
        [P.SamusLeft] = new([new(P.SamusLeft, 0, 3, 1)], AnimationEnd.Holds),
        [P.SamusRight] = new([new(P.SamusRight, 0, 3, 1)], AnimationEnd.Holds),
        [P.Initial] = new([new(P.Initial, 0, 1, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<BlueBrinstarFaceBlockSegment> StartingWith(BlueBrinstarFaceBlockSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame(frame, duration);
}
