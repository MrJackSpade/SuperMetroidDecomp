namespace SuperMetroid.Core.Frontend;

public sealed partial class SuperMetroidGame
{
    private int pauseFadeCounter = PauseFadeTiming.CounterReload;

    private void BeginPauseFade(byte initialBrightness)
    {
        pauseBrightness = initialBrightness;
        pauseFadeCounter = PauseFadeTiming.CounterReload;
    }

    /// <summary>
    /// Advances the bank-$80 fade counter once per accepted frontend frame. Gameplay
    /// still runs on counter-only frames during pause entry and gameplay restoration;
    /// skipping those frames changes the airborne pause/soft-morph timing window.
    /// </summary>
    private void AdvancePauseFade(bool brightening)
    {
        if (pauseFadeCounter-- > 0) return;
        pauseFadeCounter = PauseFadeTiming.CounterReload;
        pauseBrightness = (byte)Math.Clamp(pauseBrightness + (brightening ? 1 : -1), 0, PauseFadeTiming.FullyLit);
    }
}
