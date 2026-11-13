using P = SuperMetroid.Core.Enemies.Nuclear.NuclearWaffleProgram;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Nuclear;

/// <summary>
/// Puromi/Nuclear Waffle's body animation as a typed program (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and its lists use no owner callbacks.
/// </summary>
internal static class NuclearWaffleAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<NuclearWaffleProgram, EnemyFrameId, NoAnimationCue> Segments { get; } = new(
        new Dictionary<NuclearWaffleProgram, AnimationBeat<EnemyFrameId, NoAnimationCue>[]>
    {
        [P.BodyLoop] =
        [
            Show(EnemyFrameId.NuclearWaffle00, 3),
            Show(EnemyFrameId.NuclearWaffle01, 3),
            Show(EnemyFrameId.NuclearWaffle02, 3),
            Show(EnemyFrameId.NuclearWaffle03, 3),
            Show(EnemyFrameId.NuclearWaffle04, 3),
            Show(EnemyFrameId.NuclearWaffle05, 3),
            Show(EnemyFrameId.NuclearWaffle00, 3),
            Show(EnemyFrameId.NuclearWaffle01, 3),
            Show(EnemyFrameId.NuclearWaffle06, 3),
            Show(EnemyFrameId.NuclearWaffle03, 3),
            Show(EnemyFrameId.NuclearWaffle04, 3),
            Show(EnemyFrameId.NuclearWaffle07, 3),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<NuclearWaffleProgram, Animation<NuclearWaffleProgram>> StartingAt { get; } = new Dictionary<NuclearWaffleProgram, Animation<NuclearWaffleProgram>>
    {
        [P.BodyLoop] = new([new(P.BodyLoop, 0, 12, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<NuclearWaffleProgram> StartingWith(NuclearWaffleProgram segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame(frame, duration);
}
