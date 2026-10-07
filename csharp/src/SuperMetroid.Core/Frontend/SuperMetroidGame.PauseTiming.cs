namespace SuperMetroid.Core.Frontend;

public sealed partial class SuperMetroidGame
{
    /// <summary>Bank-$80 screen fade counter $0725.</summary>
    private int pauseFadeCounter = PauseFadeTiming.CounterReload;

    /// <summary>
    /// Bank-$80 screen fade delay $0723, the counter's reload value. Every return to state
    /// eight leaves both words zero, which a map-station pause then inherits.
    /// </summary>
    private int pauseFadeDelay;

    private void BeginPauseFade(byte initialBrightness)
    {
        pauseBrightness = initialBrightness;
        pauseFadeDelay = PauseFadeTiming.CounterReload;
        pauseFadeCounter = PauseFadeTiming.CounterReload;
    }

    /// <summary>
    /// $85:8104 publishes state $0C after the map-data box without touching either fade
    /// word, so the darkening proceeds at whatever cadence the last fade left behind.
    /// </summary>
    private void BeginMapStationPauseFade() => pauseBrightness = PauseFadeTiming.FullyLit;

    /// <summary>$82:8B34, $82:8CE4 and $82:93B5 clear both fade words on a fade's final frame.</summary>
    private void ClearScreenFadeTiming()
    {
        pauseFadeDelay = 0;
        pauseFadeCounter = 0;
    }

    /// <summary>
    /// Advances the bank-$80 fade counter once per accepted frontend frame. Gameplay
    /// still runs on counter-only frames during pause entry and gameplay restoration;
    /// skipping those frames changes the airborne pause/soft-morph timing window.
    /// </summary>
    private void AdvancePauseFade(bool brightening)
    {
        if (pauseFadeCounter-- > 0) return;
        pauseFadeCounter = pauseFadeDelay;
        pauseBrightness = (byte)Math.Clamp(pauseBrightness + (brightening ? 1 : -1), 0, PauseFadeTiming.FullyLit);
    }
}
