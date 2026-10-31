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
/// The brightness stays with its screen owner; this type owns only the timing words.
/// </remarks>
public sealed class ScreenFade
{
    /// <summary>
    /// Dispatches skipped between brightness steps. The slow cinematic fades also count its low byte down beside an
    /// unchanged high byte, so it keeps the exact countdown word.
    /// </summary>
    public WrappingCountdown Delay { get; private set; }

    /// <summary>
    /// Dispatches remaining before the next brightness step: a wrapping countdown whose sign after the decrement
    /// decides whether the dispatch steps brightness.
    /// </summary>
    public WrappingCountdown Counter { get; private set; }

    /// <summary>Stores both fade words, as menu setup code does before a fade.</summary>
    /// <param name="delay">The dispatches each brightness step waits, 0 through 65535.</param>
    /// <param name="counter">The dispatches before the first brightness step, 0 through 65535.</param>
    public void SetTiming(int delay, int counter)
    {
        Delay = WrappingCountdown.FromRaw(delay);
        Counter = WrappingCountdown.FromRaw(counter);
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
    /// AdvanceSlowScreenFadeIn: one cinematic dispatch of fading in. Returns
    /// true when this call reached full brightness.
    /// </summary>
    /// <remarks>
    /// Unlike the gameplay fades, these count down the low byte of the delay and reload it
    /// from the counter, stepping when the decrement reaches zero or wraps negative. Equal seeds
    /// of N therefore step on the Nth call and every N calls after it.
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
    /// AdvanceSlowScreenFadeOut: one cinematic dispatch of fading out. Returns
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

    /// <summary>Decrements the delay's low byte: true while the decremented byte stays positive.</summary>
    private bool ConsumeSlowDelay()
    {
        LowByteCountdownStep step = Delay.DecrementEightBitCount();
        Delay = step.Remaining;
        return !step.IsZeroOrNegative;
    }

    /// <summary>Reloads the delay's low byte from the counter.</summary>
    private void ReloadSlowDelay() => Delay = Delay.WithEightBitCount(Counter.EightBitCount);

    /// <summary>Clears the delay and counter on reaching the fade's end.</summary>
    private void ClearSlowTiming()
    {
        Delay = Delay.WithEightBitCount(0);
        Counter = Counter.WithEightBitCount(0);
    }

    /// <summary>DEC/BMI on the 16-bit counter; returns true when this dispatch only counts down.</summary>
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
