using P = SuperMetroid.Core.Enemies.Nuclear.NuclearWaffleSegment;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Nuclear;

/// <summary>
/// Puromi/Nuclear Waffle's body animation as a typed animation (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and no beat raises a cue for its owner.
/// </summary>
internal static class NuclearWaffleAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<NuclearWaffleSegment, EnemyFrameId, NoAnimationCue> Segments { get; } = new(
        new Dictionary<NuclearWaffleSegment, AnimationBeat<EnemyFrameId, NoAnimationCue>[]>
    {
        [P.BodyLoop] =
        [
            Show(EnemyFrameId.NuclearWaffleBodyTurned0, 3),
            Show(EnemyFrameId.NuclearWaffleBodyTurned45, 3),
            Show(EnemyFrameId.NuclearWaffleBodyTurned90, 3),
            Show(EnemyFrameId.NuclearWaffleBodyTurned135, 3),
            Show(EnemyFrameId.NuclearWaffleBodyTurned180, 3),
            Show(EnemyFrameId.NuclearWaffleBodyTurned225, 3),
            Show(EnemyFrameId.NuclearWaffleBodyTurned0, 3),
            Show(EnemyFrameId.NuclearWaffleBodyTurned45, 3),
            Show(EnemyFrameId.NuclearWaffleBodyTurned270, 3),
            Show(EnemyFrameId.NuclearWaffleBodyTurned135, 3),
            Show(EnemyFrameId.NuclearWaffleBodyTurned180, 3),
            Show(EnemyFrameId.NuclearWaffleBodyTurned315, 3),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<NuclearWaffleSegment, Animation<NuclearWaffleSegment>> StartingAt { get; } = new Dictionary<NuclearWaffleSegment, Animation<NuclearWaffleSegment>>
    {
        [P.BodyLoop] = new([Segments.Whole(P.BodyLoop)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<NuclearWaffleSegment> StartingWith(NuclearWaffleSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame(frame, duration);
}
