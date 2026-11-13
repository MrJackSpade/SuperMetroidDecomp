using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.LavaJumpers.NorfairLavaJumperCueKind;
using P = SuperMetroid.Core.Enemies.LavaJumpers.NorfairLavaJumperProgram;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.LavaJumpers;

/// <summary>
/// The Norfair lava jumper's hidden, jump and follower animations as typed programs (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each callback becomes a <see cref="NorfairLavaJumperCue"/> with its typed operands.
/// </summary>
internal static class NorfairLavaJumperAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<NorfairLavaJumperProgram, EnemyFrameId, NorfairLavaJumperCue> Segments { get; } = new(
        new Dictionary<NorfairLavaJumperProgram, AnimationBeat<EnemyFrameId, NorfairLavaJumperCue>[]>
    {
        [P.Hidden] =
        [
            Show(EnemyFrameId.NorfairLavaJumperHidden, 1),
        ],
        [P.Jump] =
        [
            Show(EnemyFrameId.NorfairLavaJumperHidden, 1),
            Show(EnemyFrameId.NorfairLavaJumperJumpSecond, 5),
            Show(EnemyFrameId.NorfairLavaJumperJumpThird, 9),
            Show(EnemyFrameId.NorfairLavaJumperJumpFourth, 7),
            Show(EnemyFrameId.NorfairLavaJumperJumpFifth, 3),
            Show(EnemyFrameId.NorfairLavaJumperJumpSixth, 10),
            Show(EnemyFrameId.NorfairLavaJumperJumpSeventh, 1),
            Cue(new(K.FinishAnimation)),
        ],
        [P.Follower] =
        [
            Show(EnemyFrameId.NorfairLavaJumperFollowerFirst, 1),
            Show(EnemyFrameId.NorfairLavaJumperFollowerSecond, 1),
        ],
        [P.FollowerLoop] =
        [
            Show(EnemyFrameId.NorfairLavaJumperFollowerLoopFirst, 1),
            Show(EnemyFrameId.NorfairLavaJumperFollowerSecond, 1),
            Show(EnemyFrameId.NorfairLavaJumperFollowerLoopThird, 1),
            Show(EnemyFrameId.NorfairLavaJumperFollowerSecond, 1),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<NorfairLavaJumperProgram, Animation<NorfairLavaJumperProgram>> StartingAt { get; } = new Dictionary<NorfairLavaJumperProgram, Animation<NorfairLavaJumperProgram>>
    {
        [P.Hidden] = new([new(P.Hidden, 0, 1, 1)], AnimationEnd.Holds),
        [P.Jump] = new([new(P.Jump, 0, 8, 1)], AnimationEnd.Holds),
        [P.Follower] = new([new(P.Follower, 0, 2, 1), new(P.FollowerLoop, 0, 4, 1, AnimationPhaseCounting.EveryPlay)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<NorfairLavaJumperProgram> StartingWith(NorfairLavaJumperProgram segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NorfairLavaJumperCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NorfairLavaJumperCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, NorfairLavaJumperCue> Cue(NorfairLavaJumperCue cue) => new AnimationBeat<EnemyFrameId, NorfairLavaJumperCue>.Event(cue);
}
