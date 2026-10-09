namespace SuperMetroid.Core.Frontend;

/// <summary>
/// The native screen-fade words $0723 (delay) and $0725 (counter), and the
/// <c>HandleFadingIn</c> ($80:894D) / <c>HandleFadingOut</c> ($80:8924) steps that use
/// them to advance an INIDISP brightness byte ($51).
/// </summary>
/// <remarks>
/// Each call is one native dispatch. A nonnegative decremented counter consumes the call
/// without changing brightness; otherwise the counter reloads from the delay and brightness
/// moves one step. A delay of one therefore changes brightness on every second dispatch.
/// The brightness byte stays with its screen owner; this type owns only the timing words.
/// </remarks>
public sealed class ScreenFade
{
    /// <summary>INIDISP forced-blank bit with zero brightness, written when a fade-out ends.</summary>
    public const int ForcedBlank = 0x80;

    /// <summary>INIDISP's fully lit four-bit brightness value.</summary>
    public const int FullyLit = 0x0f;

    private const int BrightnessMask = 0x0f;

    /// <summary>$0723: dispatches skipped between brightness steps.</summary>
    public ushort Delay { get; private set; }

    /// <summary>$0725: dispatches remaining before the next brightness step.</summary>
    public ushort Counter { get; private set; }

    /// <summary>Displayed brightness nibble of an INIDISP byte, zero while forced blank is set.</summary>
    public static byte Displayed(int inidisp) =>
        (inidisp & ForcedBlank) != 0 ? (byte)0 : (byte)(inidisp & BrightnessMask);

    /// <summary>Completion test documented at $80:8924: forced blank has been set.</summary>
    public static bool IsForcedBlank(int inidisp) => (inidisp & ForcedBlank) != 0;

    /// <summary>Stores both fade words, as menu setup code does before a fade.</summary>
    public void SetTiming(ushort delay, ushort counter)
    {
        Delay = delay;
        Counter = counter;
    }

    /// <summary>$80:894D: one dispatch of fading in. Completion is <c>inidisp == FullyLit</c>.</summary>
    public void FadeIn(ref int inidisp)
    {
        if (ConsumeCounter())
            return;
        int next = (inidisp + 1) & BrightnessMask;
        if (next != 0)
            inidisp = next;
    }

    /// <summary>$80:8924: one dispatch of fading out. Completion is <see cref="IsForcedBlank"/>.</summary>
    public void FadeOut(ref int inidisp)
    {
        if (ConsumeCounter())
            return;
        int current = inidisp & BrightnessMask;
        if (current == 0)
            return;
        inidisp = current == 1 ? ForcedBlank : current - 1;
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
    public bool AdvanceSlowFadeIn(ref int inidisp)
    {
        if (ConsumeSlowDelay())
            return false;
        int next = (inidisp + 1) & 0x1f;
        if ((sbyte)(next - FullyLit) >= 0)
        {
            inidisp = FullyLit;
            ClearSlowTiming();
            return true;
        }
        inidisp = next;
        ReloadSlowDelay();
        return false;
    }

    /// <summary>
    /// $8B:90D5 AdvanceSlowScreenFadeOut: one cinematic dispatch of fading out. Returns the
    /// native carry, set once brightness is zero (forced blank is written on that call).
    /// </summary>
    public bool AdvanceSlowFadeOut(ref int inidisp)
    {
        if (ConsumeSlowDelay())
            return false;
        int current = inidisp & BrightnessMask;
        if (current == 0)
            return true;
        if (current == 1)
        {
            inidisp = ForcedBlank;
            ClearSlowTiming();
            return true;
        }
        inidisp = current - 1;
        ReloadSlowDelay();
        return false;
    }

    /// <summary>8-bit DEC $0723 with BEQ/BPL: true while the decremented byte stays positive.</summary>
    private bool ConsumeSlowDelay()
    {
        byte next = unchecked((byte)(Delay - 1));
        Delay = (ushort)((Delay & 0xff00) | next);
        return next is > 0 and < 0x80;
    }

    /// <summary>8-bit LDA $0725 : STA $0723.</summary>
    private void ReloadSlowDelay() => Delay = (ushort)((Delay & 0xff00) | (Counter & 0xff));

    /// <summary>8-bit STZ $0723 and $0725 on reaching the fade's end.</summary>
    private void ClearSlowTiming()
    {
        Delay &= 0xff00;
        Counter &= 0xff00;
    }

    /// <summary>DEC/BMI on the 16-bit counter; returns true when this dispatch only counts down.</summary>
    private bool ConsumeCounter()
    {
        short next = unchecked((short)(Counter - 1));
        if (next >= 0)
        {
            Counter = (ushort)next;
            return true;
        }
        Counter = Delay;
        return false;
    }
}
