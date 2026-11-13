using P = SuperMetroid.Core.Enemies.Fireflea.FirefleaProgram;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Fireflea;

/// <summary>
/// Fireflea's 52-frame glow loop as a typed program (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and its lists use no owner callbacks.
/// </summary>
internal static class FirefleaAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<FirefleaProgram, EnemyFrameId, NoAnimationCue> Segments { get; } = new(
        new Dictionary<FirefleaProgram, AnimationBeat<EnemyFrameId, NoAnimationCue>[]>
    {
        [P.Loop] =
        [
            Show(EnemyFrameId.FirefleaCycle0, 2),
            Show(EnemyFrameId.FirefleaCycle1, 1),
            Show(EnemyFrameId.FirefleaCycle2, 2),
            Show(EnemyFrameId.FirefleaCycle1, 1),
            Show(EnemyFrameId.FirefleaCycle0, 2),
            Show(EnemyFrameId.FirefleaCycle1, 1),
            Show(EnemyFrameId.FirefleaCycle2, 2),
            Show(EnemyFrameId.FirefleaCycle1, 1),
            Show(EnemyFrameId.FirefleaCycle3, 2),
            Show(EnemyFrameId.FirefleaCycle4, 1),
            Show(EnemyFrameId.FirefleaCycle5, 2),
            Show(EnemyFrameId.FirefleaCycle4, 1),
            Show(EnemyFrameId.FirefleaCycle6, 2),
            Show(EnemyFrameId.FirefleaCycle7, 1),
            Show(EnemyFrameId.FirefleaCycle8, 2),
            Show(EnemyFrameId.FirefleaCycle7, 1),
            Show(EnemyFrameId.FirefleaCycle9, 2),
            Show(EnemyFrameId.FirefleaCycle10, 1),
            Show(EnemyFrameId.FirefleaCycle11, 2),
            Show(EnemyFrameId.FirefleaCycle10, 1),
            Show(EnemyFrameId.FirefleaCycle12, 2),
            Show(EnemyFrameId.FirefleaCycle13, 1),
            Show(EnemyFrameId.FirefleaCycle14, 2),
            Show(EnemyFrameId.FirefleaCycle13, 1),
            Show(EnemyFrameId.FirefleaCycle15, 2),
            Show(EnemyFrameId.FirefleaCycle16, 1),
            Show(EnemyFrameId.FirefleaCycle17, 2),
            Show(EnemyFrameId.FirefleaCycle16, 1),
            Show(EnemyFrameId.FirefleaCycle18, 2),
            Show(EnemyFrameId.FirefleaCycle19, 1),
            Show(EnemyFrameId.FirefleaCycle20, 2),
            Show(EnemyFrameId.FirefleaCycle19, 1),
            Show(EnemyFrameId.FirefleaCycle15, 2),
            Show(EnemyFrameId.FirefleaCycle16, 1),
            Show(EnemyFrameId.FirefleaCycle17, 2),
            Show(EnemyFrameId.FirefleaCycle16, 1),
            Show(EnemyFrameId.FirefleaCycle12, 2),
            Show(EnemyFrameId.FirefleaCycle13, 1),
            Show(EnemyFrameId.FirefleaCycle14, 2),
            Show(EnemyFrameId.FirefleaCycle13, 1),
            Show(EnemyFrameId.FirefleaCycle9, 2),
            Show(EnemyFrameId.FirefleaCycle10, 1),
            Show(EnemyFrameId.FirefleaCycle11, 2),
            Show(EnemyFrameId.FirefleaCycle10, 1),
            Show(EnemyFrameId.FirefleaCycle6, 2),
            Show(EnemyFrameId.FirefleaCycle7, 1),
            Show(EnemyFrameId.FirefleaCycle8, 2),
            Show(EnemyFrameId.FirefleaCycle7, 1),
            Show(EnemyFrameId.FirefleaCycle3, 2),
            Show(EnemyFrameId.FirefleaCycle4, 1),
            Show(EnemyFrameId.FirefleaCycle5, 2),
            Show(EnemyFrameId.FirefleaCycle4, 1),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<FirefleaProgram, Animation<FirefleaProgram>> StartingAt { get; } = new Dictionary<FirefleaProgram, Animation<FirefleaProgram>>
    {
        [P.Loop] = new([new(P.Loop, 0, 52, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<FirefleaProgram> StartingWith(FirefleaProgram segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame(frame, duration);
}
