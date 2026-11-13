namespace SuperMetroid.Core.Frontend;

/// <summary>
/// The screen-fade delay and counter, and the
/// fade-in and fade-out steps that use
/// them to step a <see cref="ScreenBrightness"/>.
/// </summary>
/// <remarks>
/// Each call is one dispatch. A nonnegative decremented counter consumes the call
/// without changing brightness; otherwise the counter reloads from the delay and brightness
/// moves one step. A delay of one therefore changes brightness on every second dispatch.
/// The brightness stays with its screen owner; this type owns only the timing values.
/// </remarks>
public sealed class ScreenFade
{
    /// <summary>
    /// Dispatches skipped between brightness steps. The slow cinematic fades count it down themselves, from at most
    /// <see cref="LongestSlowDelay"/>.
    /// </summary>
    public WrappingCountdown Delay { get; private set; }

    /// <summary>
    /// The longest delay a slow cinematic fade counts. The game counts a slow fade's delay as a small count that runs
    /// past zero below zero; every slow fade starts from a delay and counter of two, well inside the range in which that
    /// count and the countdown agree.
    /// </summary>
    public const int LongestSlowDelay = 127;

    /// <summary>
    /// Dispatches remaining before the next brightness step: a wrapping countdown whose sign after the decrement
    /// decides whether the dispatch steps brightness.
    /// </summary>
    public WrappingCountdown Counter { get; private set; }

    /// <summary>Stores both fade counters, as menu setup does before a fade.</summary>
    /// <param name="delay">The dispatches each brightness step waits, 0 through 65535.</param>
    /// <param name="counter">The dispatches before the first brightness step, 0 through 65535.</param>
    public void SetTiming(int delay, int counter)
    {
        Delay = WrappingCountdown.Start(delay);
        Counter = WrappingCountdown.Start(counter);
    }

    /// <summary>
    /// One dispatch of fading in, one level brighter up to fully lit; a blanked screen comes back at level
    /// one. Completion is reaching <see cref="ScreenBrightness.FullyLit"/>.
    /// </summary>
    public void FadeIn(ref ScreenBrightness brightness)
    {
        if (ConsumeCounter())
            return;
        if (brightness.Level < ScreenBrightness.FullLevel)
            brightness = ScreenBrightness.Lit(brightness.Level + 1);
    }

    /// <summary>
    /// One dispatch of fading out. The step down from level one blanks the screen, which is the fade's
    /// completion (<see cref="ScreenBrightness.IsBlanked"/>); an unblanked level zero stays where it is.
    /// </summary>
    public void FadeOut(ref ScreenBrightness brightness)
    {
        if (ConsumeCounter())
            return;
        int current = brightness.Level;
        if (current == 0)
            return;
        brightness = current == 1 ? ScreenBrightness.Blanked : ScreenBrightness.Lit(current - 1);
    }

    /// <summary>
    /// One cinematic dispatch of fading in slowly. Returns
    /// true when this call reached full brightness.
    /// </summary>
    /// <remarks>
    /// Unlike the gameplay fades, these count the delay down and reload it from the counter, stepping when the
    /// decrement reaches zero or runs past it. Equal seeds of N therefore step on the Nth call and every N calls after it.
    /// </remarks>
    public bool AdvanceSlowFadeIn(ref ScreenBrightness brightness)
    {
        if (ConsumeSlowDelay())
            return false;
        int next = brightness.Level + 1;
        if (next >= ScreenBrightness.FullLevel)
        {
            brightness = ScreenBrightness.FullyLit;
            ClearSlowTiming();
            return true;
        }
        brightness = ScreenBrightness.Lit(next);
        ReloadSlowDelay();
        return false;
    }

    /// <summary>
    /// One cinematic dispatch of fading out slowly. Returns
    /// true once brightness is zero (the screen is blanked on that call).
    /// </summary>
    public bool AdvanceSlowFadeOut(ref ScreenBrightness brightness)
    {
        if (ConsumeSlowDelay())
            return false;
        int current = brightness.Level;
        if (current == 0)
            return true;
        if (current == 1)
        {
            brightness = ScreenBrightness.Blanked;
            ClearSlowTiming();
            return true;
        }
        brightness = ScreenBrightness.Lit(current - 1);
        ReloadSlowDelay();
        return false;
    }

    /// <summary>Counts the slow delay down: true while the decremented delay stays positive.</summary>
    /// <exception cref="InvalidOperationException">The delay is longer than <see cref="LongestSlowDelay"/>.</exception>
    private bool ConsumeSlowDelay()
    {
        if (!Delay.IsNegative && Delay.IsAtLeast(LongestSlowDelay + 1))
            throw new InvalidOperationException($"A slow fade counts a delay of at most {LongestSlowDelay} dispatches, not {Delay}.");
        CountdownStep step = Delay.Decrement();
        Delay = step.Remaining;
        return !step.IsZeroOrNegative;
    }

    /// <summary>Reloads the slow delay from the counter.</summary>
    private void ReloadSlowDelay() => Delay = Counter;

    /// <summary>Clears the delay and counter on reaching the fade's end.</summary>
    private void ClearSlowTiming()
    {
        Delay = WrappingCountdown.Start(0);
        Counter = WrappingCountdown.Start(0);
    }

    /// <summary>Counts the delay down, wrapping below zero; returns true when this dispatch only counts down.</summary>
    private bool ConsumeCounter()
    {
        CountdownStep step = Counter.Decrement();
        if (step.IsNonNegative)
        {
            Counter = step.Remaining;
            return true;
        }
        Counter = Delay;
        return false;
    }
}
