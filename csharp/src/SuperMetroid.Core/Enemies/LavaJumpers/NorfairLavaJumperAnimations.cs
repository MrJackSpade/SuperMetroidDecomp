using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Enemies.LavaJumpers.NorfairLavaJumperCueKind;
using P = SuperMetroid.Core.Enemies.LavaJumpers.NorfairLavaJumperSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.LavaJumpers;

/// <summary>
/// The Norfair lava jumper's hidden, jump and follower animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="NorfairLavaJumperCue"/> naming what it does.
/// </summary>
internal static class NorfairLavaJumperAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<NorfairLavaJumperSegment, EnemyFrameId, NorfairLavaJumperCue> Segments { get; } = new(
        new Dictionary<NorfairLavaJumperSegment, AnimationBeat<EnemyFrameId, NorfairLavaJumperCue>[]>
    {
        [P.Hidden] =
        [
            Hold(EnemyFrameId.NorfairLavaJumperClawsCurledDown),
        ],
        [P.Jump] =
        [
            Show(EnemyFrameId.NorfairLavaJumperClawsCurledDown, 1),
            Show(EnemyFrameId.NorfairLavaJumperClawsSpreading, 5),
            Show(EnemyFrameId.NorfairLavaJumperClawsSpread, 9),
            Show(EnemyFrameId.NorfairLavaJumperClawsSpreadFlat, 7),
            Show(EnemyFrameId.NorfairLavaJumperClawsRaising, 3),
            Show(EnemyFrameId.NorfairLavaJumperClawsRaised, 10),
            Show(EnemyFrameId.NorfairLavaJumperClawsFolded, 1),
            Cue(new(K.FinishAnimation)),
        ],
        [P.Follower] =
        [
            Show(EnemyFrameId.NorfairLavaJumperFlameFlare, 1),
            Show(EnemyFrameId.Empty, 1),
        ],
        [P.FollowerFlickering] =
        [
            Show(EnemyFrameId.NorfairLavaJumperFlameSmall, 1),
            Show(EnemyFrameId.Empty, 1),
            Show(EnemyFrameId.NorfairLavaJumperFlameTall, 1),
            Show(EnemyFrameId.Empty, 1),
        ],
    });

    /// <summary>
    /// The animation that starts at each segment an owner can start or restart at: the hidden claws are a still picture,
    /// and the trailing flame flares and flickers once, a cycle it repeats from the flare (#2337).
    /// </summary>
    internal static IReadOnlyDictionary<NorfairLavaJumperSegment, Animation<NorfairLavaJumperSegment>> StartingAt { get; } = new Dictionary<NorfairLavaJumperSegment, Animation<NorfairLavaJumperSegment>>
    {
        [P.Hidden] = new([Whole(P.Hidden)], AnimationEnd.Holds),
        [P.Jump] = new([Whole(P.Jump)], AnimationEnd.Holds),
        [P.Follower] = new([Whole(P.Follower), Whole(P.FollowerFlickering)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<NorfairLavaJumperSegment> StartingWith(NorfairLavaJumperSegment segment) => StartingAt[segment];

    /// <summary>One play of every beat of <paramref name="segment"/>.</summary>
    private static AnimationPhase<NorfairLavaJumperSegment> Whole(NorfairLavaJumperSegment segment) => new(segment, 0, Segments.Beats(segment).Count, 1);

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NorfairLavaJumperCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NorfairLavaJumperCue>.Frame(frame, duration);

    /// <summary>Shows <paramref name="frame"/> for good (#2337): a still picture rests on it until its owner plays another.</summary>
    private static AnimationBeat<EnemyFrameId, NorfairLavaJumperCue> Hold(EnemyFrameId frame) => new AnimationBeat<EnemyFrameId, NorfairLavaJumperCue>.HeldFrame(frame);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, NorfairLavaJumperCue> Cue(NorfairLavaJumperCue cue) => new AnimationBeat<EnemyFrameId, NorfairLavaJumperCue>.Event(cue);
}
