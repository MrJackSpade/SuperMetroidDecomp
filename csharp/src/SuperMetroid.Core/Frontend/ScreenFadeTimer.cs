using SuperMetroid.Core.Frontend.Pause;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// The bank-$80 screen fade timing words: counter $0725 and its reload delay $0723. The counter advances once per
/// accepted frontend frame; each time it runs out, the brightness steps once toward the fade's target.
/// </summary>
internal sealed class ScreenFadeTimer
{
    /// <summary>Bank-$80 screen fade counter $0725.</summary>
    internal int Counter { get; set; } = PauseFadeTiming.CounterReload;

    /// <summary>
    /// Bank-$80 screen fade delay $0723, the counter's reload value. Every return to state
    /// eight leaves both words zero, which a map-station pause then inherits.
    /// </summary>
    internal int Delay { get; set; }

    /// <summary>Reloads both words, as a pause fade's start does.</summary>
    internal void Reload()
    {
        Delay = PauseFadeTiming.CounterReload;
        Counter = PauseFadeTiming.CounterReload;
    }

    /// <summary>$82:8B34, $82:8CE4 and $82:93B5 clear both fade words on a fade's final frame.</summary>
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
    internal byte Advance(byte brightness, bool brightening)
    {
        if (Counter-- > 0) return brightness;
        Counter = Delay;
        return (byte)Math.Clamp(brightness + (brightening ? 1 : -1), 0, PauseFadeTiming.FullyLit);
    }
}
