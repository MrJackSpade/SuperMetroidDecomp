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
