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
    public ControllerInputState(int initialRepeatDelay = 0, int subsequentRepeatDelay = 0)
    {
        InitialRepeatDelay = initialRepeatDelay;
        SubsequentRepeatDelay = subsequentRepeatDelay;
    }

    /// <summary>
    /// Frames loaded into <see cref="RepeatTimer"/> when input is released or changes, 0 through 65,535: any value the
    /// repeat countdown can start at. The delay is only ever reloaded, never counted itself; the countdown keeps the wrap.
    /// </summary>
    public int InitialRepeatDelay { get; set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value); }

    /// <summary>
    /// Frames loaded into <see cref="RepeatTimer"/> after a held-input repeat pulse, 0 through 65,535: any value the
    /// repeat countdown can start at. The delay is only ever reloaded, never counted itself; the countdown keeps the wrap.
    /// </summary>
    public int SubsequentRepeatDelay { get; set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value); }

    /// <summary>Controller buttons currently down, corresponding to direct page <c>$8B</c>.</summary>
    public ControllerButton Current { get; private set; }

    /// <summary>
    /// Rising-edge button bits at direct page <c>$8F</c>. A bit is set for exactly the NMI
    /// where it changes from released to pressed.
    /// </summary>
    public ControllerButton NewlyPressed { get; private set; }

    /// <summary>
    /// Auto-repeat form at direct page <c>$93</c>. The disassembly calls this "fake new";
    /// it begins as <see cref="NewlyPressed"/> and periodically emits all held bits.
    /// </summary>
    public ControllerButton NewlyPressedWithRepeat { get; private set; }

    /// <summary>Prior controller buttons at direct page <c>$97</c>.</summary>
    public ControllerButton Previous { get; private set; }

    /// <summary>Current 16-bit repeat countdown at direct page <c>$9B</c>.</summary>
    public WrappingCountdown RepeatTimer { get; private set; }

    /// <summary>
    /// Temporarily publishes the demo's explicit held/edge pair, as bank-$90 demo alpha
    /// does. Disposing restores the real NMI sample for the demo-cancellation check.
    /// This is deliberately not Latch: recorded edges need not equal held-input changes.
    /// </summary>
    internal IDisposable UseDemoInput(ControllerButton held, ControllerButton newlyPressed)
    {
        var restore = new DemoInputRestore(this, Current, NewlyPressed);
        Current = held;
        NewlyPressed = newlyPressed;
        return restore;
    }

    /// <summary>Restores the real NMI controller sample after a scoped demo input override.</summary>
    private sealed class DemoInputRestore(ControllerInputState owner, ControllerButton held, ControllerButton newlyPressed) : IDisposable
    {
        /// <summary>Prevents a repeated dispose from replacing a later controller sample.</summary>
        private bool disposed;
        /// <summary>Restores the held and edge words captured before the demo override.</summary>
        public void Dispose()
        {
            if (disposed) return;
            owner.Current = held;
            owner.NewlyPressed = newlyPressed;
            disposed = true;
        }
    }

    /// <summary>Latches one completed SNES automatic joypad sample.</summary>
    /// <remarks>Raw words are validated where they enter the game (recordings, replays, hosts), not here.</remarks>
    public void Latch(ControllerButton input)
    {
        Current = input;

        // EOR previous followed by AND current is a compact rising-edge detector. Released
        // edges disappear because their bits are absent from Current.
        NewlyPressed = Current & (Previous ^ Current);
        NewlyPressedWithRepeat = NewlyPressed;

        if (Current != ControllerButton.None && Current == Previous)
        {
            // DEC is 16-bit and wraps. If RepeatTimer is zero, it becomes $FFFF and does
            // not pulse—a detail retained because the retail repeat delays remain zero in
            // the known initialization paths and this output is marked unused.
            RepeatTimer = RepeatTimer.Decrement().Remaining;
            if (RepeatTimer.IsZero)
            {
                NewlyPressedWithRepeat = Current;
                RepeatTimer = WrappingCountdown.Start(SubsequentRepeatDelay);
            }
        }
        else
        {
            // A release, first press, or change in a multi-button chord all reload the
            // longer initial delay. The genuine newly-pressed output remains unaffected.
            RepeatTimer = WrappingCountdown.Start(InitialRepeatDelay);
        }

        Previous = Current;
    }
}
