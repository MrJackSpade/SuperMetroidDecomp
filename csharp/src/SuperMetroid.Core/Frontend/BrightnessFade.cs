namespace SuperMetroid.Core.Frontend;

/// <summary>
/// A screen brightness, 0 (black) through 15, that steps once each time its <see cref="ScreenFadeTimer"/> runs out: the
/// post-Ceres fade-in, the death black-out and the Zebes escape fade-out all count this way.
/// </summary>
internal sealed class BrightnessFade
{
    /// <summary>The fade's counter and its reload delay.</summary>
    private readonly ScreenFadeTimer timer = new();

    /// <summary>Creates a fade at <paramref name="brightness"/> whose counter reloads with <paramref name="delay"/>.</summary>
    internal BrightnessFade(int brightness, int delay) => Start(brightness, delay);

    /// <summary>The current brightness, 0 (black) through 15.</summary>
    [IntegerQuantity("screen brightness level the fade steps")]
    internal int Brightness { get; private set; }

    /// <summary>Restarts the fade at <paramref name="brightness"/>, with both counter words loaded with <paramref name="delay"/>.</summary>
    internal void Start(int brightness, int delay)
    {
        Brightness = Ensure.BetweenInclusive(0, Pause.PauseFadeTiming.FullyLit, brightness);
        timer.Delay = delay;
        timer.Counter = delay;
    }

    /// <summary>Counts one frame and, when the counter runs out, steps one level darker.</summary>
    internal int Darken() => Brightness = timer.Darken(Brightness);

    /// <summary>Counts one frame and, when the counter runs out, steps one level brighter.</summary>
    internal int Brighten() => Brightness = timer.Brighten(Brightness);
}
