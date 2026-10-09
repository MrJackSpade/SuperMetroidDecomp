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
    /// <c>Samus_PauseCheck</c> ($90:EA45) reloads both fade words and leaves the brightness
    /// register alone, so the darkening starts from whatever $51 holds; after an unpause
    /// interrupted by reserve refill that is still black, and state $0C ends at once.
    /// $85:8104 (the map-data box) touches neither, so it changes nothing here.
    /// </summary>
    private void BeginGameplayPauseFade()
    {
        pauseFadeDelay = PauseFadeTiming.CounterReload;
        pauseFadeCounter = PauseFadeTiming.CounterReload;
    }

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
