namespace SuperMetroid.Core.Frontend;

/// <summary>
/// The native screen-fade words $0723 (delay) and $0725 (counter), and the
/// <c>HandleFadingIn</c> ($80:894D) / <c>HandleFadingOut</c> ($80:8924) steps that use
/// them to step a <see cref="ScreenBrightness"/>.
/// </summary>
/// <remarks>
/// Each call is one native dispatch. A nonnegative decremented counter consumes the call
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
    /// $80:894D: one dispatch of fading in, one level brighter up to fully lit; a blanked screen comes back at level
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
    /// $80:8924: one dispatch of fading out. The step down from level one blanks the screen, which is the fade's
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
    /// $8B:911B AdvanceSlowScreenFadeIn: one cinematic dispatch of fading in. Returns the
    /// native carry, set when this call reached full brightness.
    /// </summary>
    /// <remarks>
    /// Unlike the bank $80 routines, these count down the low byte of $0723 and reload it
    /// from $0725, stepping when the decrement reaches zero or wraps negative. Equal seeds
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
    /// $8B:90D5 AdvanceSlowScreenFadeOut: one cinematic dispatch of fading out. Returns the
    /// native carry, set once brightness is zero (the screen is blanked on that call).
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

    /// <summary>8-bit DEC $0723 with BEQ/BPL: true while the decremented byte stays positive.</summary>
    private bool ConsumeSlowDelay()
    {
        LowByteCountdownStep step = WrappingCounter.DecrementLowByte(Delay.RawValue);
        Delay = WrappingCountdown.FromRaw(step.Remaining);
        return !step.IsZeroOrNegative;
    }

    /// <summary>8-bit LDA $0725 : STA $0723.</summary>
    private void ReloadSlowDelay() => Delay = WrappingCountdown.FromRaw(
        (ushort)(SnesLittleEndian.HighByteInPlace(Delay.RawValue) | SnesLittleEndian.LowByte(Counter.RawValue)));

    /// <summary>8-bit STZ $0723 and $0725 on reaching the fade's end.</summary>
    private void ClearSlowTiming()
    {
        Delay = WrappingCountdown.FromRaw((ushort)SnesLittleEndian.HighByteInPlace(Delay.RawValue));
        Counter = WrappingCountdown.FromRaw((ushort)SnesLittleEndian.HighByteInPlace(Counter.RawValue));
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
