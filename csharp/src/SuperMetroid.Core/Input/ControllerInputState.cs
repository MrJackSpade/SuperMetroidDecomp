namespace SuperMetroid.Core.Input;

/// <summary>
/// Ports the controller-1 portion of <c>$80:9459</c>, which runs near the end of every
/// accepted NMI after the SNES automatic joypad read has completed.
/// </summary>
public sealed class ControllerInputState
{
    /// <summary>
    /// Creates an input latch with the two repeat delays stored at direct-page
    /// <c>$87/$89</c>. They default to zero because the retail game's secondary "fake new"
    /// output is unused; tools can provide small values to exercise repeat behavior.
    /// </summary>
    public ControllerInputState(ushort initialRepeatDelay = 0, ushort subsequentRepeatDelay = 0)
    {
        InitialRepeatDelay = initialRepeatDelay;
        SubsequentRepeatDelay = subsequentRepeatDelay;
    }

    /// <summary>Delay loaded when input is released or changes.</summary>
    public ushort InitialRepeatDelay { get; set; }

    /// <summary>Delay loaded after a held-input repeat pulse.</summary>
    public ushort SubsequentRepeatDelay { get; set; }

    /// <summary>Raw controller bits currently down, corresponding to direct page <c>$8B</c>.</summary>
    public ushort Current { get; private set; }

    /// <summary>Typed view of <see cref="Current"/> for gameplay button tests.</summary>
    public SnesButton CurrentButtons => (SnesButton)Current;

    /// <summary>
    /// Rising-edge button bits at direct page <c>$8F</c>. A bit is set for exactly the NMI
    /// where it changes from released to pressed.
    /// </summary>
    public ushort NewlyPressed { get; private set; }

    /// <summary>Typed rising-edge view of <see cref="NewlyPressed"/>.</summary>
    public SnesButton NewlyPressedButtons => (SnesButton)NewlyPressed;

    /// <summary>
    /// Auto-repeat form at direct page <c>$93</c>. The disassembly calls this "fake new";
    /// it begins as <see cref="NewlyPressed"/> and periodically emits all held bits.
    /// </summary>
    public ushort NewlyPressedWithRepeat { get; private set; }

    /// <summary>Typed auto-repeat view of <see cref="NewlyPressedWithRepeat"/>.</summary>
    public SnesButton NewlyPressedWithRepeatButtons => (SnesButton)NewlyPressedWithRepeat;

    /// <summary>Prior raw input at direct page <c>$97</c>.</summary>
    public ushort Previous { get; private set; }

    /// <summary>Current 16-bit repeat countdown at direct page <c>$9B</c>.</summary>
    public ushort RepeatTimer { get; private set; }

    /// <summary>Latches one completed SNES automatic joypad sample.</summary>
    public void Latch(ushort rawInput)
    {
        // Host, replay, and frontend words all cross the same boundary. Validate once here;
        // downstream code may then use the typed views without repeatedly trusting casts.
        Current = (ushort)SnesButtons.FromRaw(rawInput, "controller-1 latch");

        // EOR previous followed by AND current is a compact rising-edge detector. Released
        // edges disappear because their bits are absent from Current.
        NewlyPressed = (ushort)(Current & (Previous ^ Current));
        NewlyPressedWithRepeat = NewlyPressed;

        if (Current != 0 && Current == Previous)
        {
            // DEC is 16-bit and wraps. If RepeatTimer is zero, it becomes $FFFF and does
            // not pulse—a detail retained because the retail repeat delays remain zero in
            // the known initialization paths and this output is marked unused.
            RepeatTimer = unchecked((ushort)(RepeatTimer - 1));
            if (RepeatTimer == 0)
            {
                NewlyPressedWithRepeat = Current;
                RepeatTimer = SubsequentRepeatDelay;
            }
        }
        else
        {
            // A release, first press, or change in a multi-button chord all reload the
            // longer initial delay. The genuine newly-pressed output remains unaffected.
            RepeatTimer = InitialRepeatDelay;
        }

        Previous = Current;
    }
}

/// <summary>
/// Retail SNES controller bit assignments as returned by registers <c>$4218/$4219</c>.
/// </summary>
[Flags]
public enum SnesButton : ushort
{
    None = 0,
    R = 0x0010,
    L = 0x0020,
    X = 0x0040,
    A = 0x0080,
    Right = 0x0100,
    Left = 0x0200,
    Down = 0x0400,
    Up = 0x0800,
    Start = 0x1000,
    Select = 0x2000,
    Y = 0x4000,
    B = 0x8000,
}
