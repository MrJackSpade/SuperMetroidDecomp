using SuperMetroid.Core.Animation;
using F = SuperMetroid.Core.Frontend.GameOver.GameOverBabyFrame;
using C = SuperMetroid.Core.Frontend.GameOver.GameOverBabyColouring;

namespace SuperMetroid.Core.Frontend.GameOver;

/// <summary>
/// The game-over Baby Metroid's animation (#1825), played by the shared animation playback: it idles twice and cries, idles
/// four times and cries, idles three times and cries, then starts again. Each cry is heard as its first pose ends.
/// </summary>
internal static class GameOverBabyAnimation
{
    /// <summary>The look the Baby rests in before its animation starts, which is also its first.</summary>
    internal static GameOverBabyLook Resting { get; } = new(F.Closed, C.Idle);

    /// <summary>The animation's poses and cries.</summary>
    internal static AnimationSegments<GameOverBabySegment, GameOverBabyLook, GameOverBabySound> Segments { get; } = new(
        new Dictionary<GameOverBabySegment, AnimationBeat<GameOverBabyLook, GameOverBabySound>[]>
        {
            [GameOverBabySegment.Crying] =
            [
                .. Idling(), .. Idling(), .. Crying(GameOverBabySound.FirstCry),
                .. Idling(), .. Idling(), .. Idling(), .. Idling(), .. Crying(GameOverBabySound.SecondCry),
                .. Idling(), .. Idling(), .. Idling(), .. Crying(GameOverBabySound.ThirdCry),
            ],
        });

    /// <summary>The animation the Baby plays for as long as the game-over screen shows: every pose and cry, again and again.</summary>
    internal static Animation<GameOverBabySegment> Playing { get; } =
        new([new(GameOverBabySegment.Crying, 0, Segments.Beats(GameOverBabySegment.Crying).Count, 1)], AnimationEnd.LoopsFrom(0));

    /// <summary>The Baby idling once: closed, middle, open and middle again, ten updates each, in its idle colours.</summary>
    private static AnimationBeat<GameOverBabyLook, GameOverBabySound>[] Idling() =>
    [
        Show(F.Closed, C.Idle, 10),
        Show(F.Middle, C.Idle, 10),
        Show(F.Open, C.Idle, 10),
        Show(F.Middle, C.Idle, 10),
    ];

    /// <summary>
    /// The Baby crying <paramref name="cry"/>: its poses twice through in their cry colours, slowing from six updates to two
    /// and back to five, the cry heard as the first pose ends.
    /// </summary>
    private static AnimationBeat<GameOverBabyLook, GameOverBabySound>[] Crying(GameOverBabySound cry) =>
    [
        Show(F.Closed, C.ClosedCry, 6),
        new AnimationBeat<GameOverBabyLook, GameOverBabySound>.Event(cry),
        Show(F.Middle, C.MiddleCry, 5),
        Show(F.Open, C.OpenCry, 4),
        Show(F.Middle, C.MiddleCry, 3),
        Show(F.Closed, C.ClosedCry, 2),
        Show(F.Middle, C.MiddleCry, 3),
        Show(F.Open, C.OpenCry, 4),
        Show(F.Middle, C.MiddleCry, 5),
    ];

    /// <summary>Shows <paramref name="frame"/> in <paramref name="colours"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<GameOverBabyLook, GameOverBabySound> Show(F frame, C colours, int duration) =>
        new AnimationBeat<GameOverBabyLook, GameOverBabySound>.Frame(new(frame, colours), duration);
}
