using SuperMetroid.Core.Frontend.Pause;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// The screen fade timing values: the counter and its reload delay. The counter advances once per
/// accepted frontend frame; each time it runs out, the brightness steps once toward the fade's target.
/// </summary>
internal sealed class ScreenFadeTimer
{
    /// <summary>Screen fade counter.</summary>
    internal int Counter { get; set; } = PauseFadeTiming.CounterReload;

    /// <summary>
    /// Screen fade delay, the counter's reload value. Every return to main gameplay
    /// leaves both zero, which a map-station pause then inherits.
    /// </summary>
    internal int Delay { get; set; }

    /// <summary>Reloads both counters, as a pause fade's start does.</summary>
    internal void Reload()
    {
        Delay = PauseFadeTiming.CounterReload;
        Counter = PauseFadeTiming.CounterReload;
    }

    /// <summary>Clears both fade counters on a fade's final frame.</summary>
    internal void Clear()
    {
        Delay = 0;
        Counter = 0;
    }

    /// <summary>
    /// Advances the counter once and returns the brightness after this frame. Gameplay still runs on
    /// counter-only frames during pause entry and gameplay restoration; skipping those frames changes the
    /// airborne pause/soft-morph timing window.
    /// </summary>
    internal int Brighten(int brightness) => Advance(brightness, 1);

    /// <summary>Advances the counter once and returns the brightness after this frame, one step darker when it expires.</summary>
    internal int Darken(int brightness) => Advance(brightness, -1);

    /// <summary>Counts down the delay, then moves the brightness one <paramref name="step"/> within the fade's range.</summary>
    private int Advance(int brightness, int step)
    {
        if (Counter-- > 0) return brightness;
        Counter = Delay;
        return Math.Clamp(brightness + step, 0, PauseFadeTiming.FullyLit);
    }
}
