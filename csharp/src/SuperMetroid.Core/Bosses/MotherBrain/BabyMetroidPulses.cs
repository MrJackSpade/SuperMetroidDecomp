using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The Baby Metroid's pulses (#2405), which the Tourian Shitroid and the cutscene Baby, one creature, both play: it shows
/// its closed, half-open, wide-open and half-open poses again, each for its own updates.
/// </summary>
internal static class BabyMetroidPulses
{
    /// <summary>The calm pulse: each pose sixteen updates.</summary>
    internal static AnimationBeat<BabyMetroidFrame, TEvent>[] Calm<TEvent>()
        where TEvent : notnull => Pulse<TEvent>(16, 16, 16, 16);

    /// <summary>The quick pulse it drains Mother Brain with: eight, eight, five and two updates.</summary>
    internal static AnimationBeat<BabyMetroidFrame, TEvent>[] Draining<TEvent>()
        where TEvent : notnull => Pulse<TEvent>(8, 8, 5, 2);

    /// <summary>
    /// One pulse: the closed pose for <paramref name="closed"/> updates, the half-open one for <paramref name="opening"/>,
    /// the wide-open one for <paramref name="open"/> and the half-open one again for <paramref name="closing"/>.
    /// </summary>
    internal static AnimationBeat<BabyMetroidFrame, TEvent>[] Pulse<TEvent>(int closed, int opening, int open, int closing)
        where TEvent : notnull =>
    [
        new AnimationBeat<BabyMetroidFrame, TEvent>.Frame(BabyMetroidFrame.Closed, closed),
        new AnimationBeat<BabyMetroidFrame, TEvent>.Frame(BabyMetroidFrame.HalfOpen, opening),
        new AnimationBeat<BabyMetroidFrame, TEvent>.Frame(BabyMetroidFrame.WideOpen, open),
        new AnimationBeat<BabyMetroidFrame, TEvent>.Frame(BabyMetroidFrame.HalfOpen, closing),
    ];
}
