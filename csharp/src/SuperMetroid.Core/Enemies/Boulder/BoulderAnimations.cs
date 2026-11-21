using P = SuperMetroid.Core.Enemies.Boulder.BoulderSegment;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Artwork;
using Beats = SuperMetroid.Core.Animation.AnimationBeats<SuperMetroid.Core.Enemies.Artwork.EnemyFrameId, SuperMetroid.Core.Animation.NoAnimationCue>;

namespace SuperMetroid.Core.Enemies.Boulder;

/// <summary>
/// The Boulder's mirrored rolling loops (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and no beat raises a cue for its owner.
/// </summary>
internal static partial class BoulderAnimations
{
    /// <summary>The updates each of the Boulder's turned frames shows for.</summary>
    private const int TurnUpdates = 8;

    /// <summary>The Boulder rolling left, an eighth of a turn a frame.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue>[] RollingLeft { get; } = Beats.Steady(TurnUpdates,
        EnemyFrameId.BoulderTurned0, EnemyFrameId.BoulderTurned45, EnemyFrameId.BoulderTurned90, EnemyFrameId.BoulderTurned135,
        EnemyFrameId.BoulderTurned180, EnemyFrameId.BoulderTurned225, EnemyFrameId.BoulderTurned270, EnemyFrameId.BoulderTurned315);

    /// <summary>Every segment's beats: rolling right turns the left roll the other way round.</summary>
    internal static AnimationSegments<BoulderSegment, EnemyFrameId, NoAnimationCue> Segments { get; } = new(
        new Dictionary<BoulderSegment, AnimationBeat<EnemyFrameId, NoAnimationCue>[]>
    {
        [P.Left] = RollingLeft,
        [P.Right] = Beats.OtherWayRound(RollingLeft),
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static Dictionary<BoulderSegment, Animation<BoulderSegment>> StartingAt { get; } = Segments.EachLooping(P.Left, P.Right);

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<BoulderSegment> StartingWith(BoulderSegment segment) => StartingAt[segment];
}
