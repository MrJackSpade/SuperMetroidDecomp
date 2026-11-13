namespace SuperMetroid.Core.Input;

/// <summary>
/// Reads the controller-1 sample near the end of every
/// frame boundary, after the automatic controller read has completed.
/// </summary>
public sealed class ControllerInputState
{
    /// <summary>
    /// Creates an input latch with the first and later auto-repeat delays. They default to zero
    /// because the retail game's auto-repeat output is unused; tools can provide small values to exercise repeat behavior.
    /// </summary>
    public ControllerInputState(int initialRepeatDelay = 0, int subsequentRepeatDelay = 0)
    {
        InitialRepeatDelay = initialRepeatDelay;
        SubsequentRepeatDelay = subsequentRepeatDelay;
    }

    /// <summary>
    /// Frames loaded into <see cref="RepeatTimer"/> when input is released or changes, up to
    /// <see cref="WrappingCountdown.LongestStart"/>: any value the repeat countdown can start at. The delay is only ever
    /// reloaded, never counted itself; the countdown keeps the wrap.
    /// </summary>
    public int InitialRepeatDelay { get; set => field = Ensure.BetweenInclusive(0, WrappingCountdown.LongestStart, value); }

    /// <summary>
    /// Frames loaded into <see cref="RepeatTimer"/> after a held-input repeat pulse, up to
    /// <see cref="WrappingCountdown.LongestStart"/>: any value the repeat countdown can start at. The delay is only ever
    /// reloaded, never counted itself; the countdown keeps the wrap.
    /// </summary>
    public int SubsequentRepeatDelay { get; set => field = Ensure.BetweenInclusive(0, WrappingCountdown.LongestStart, value); }

    /// <summary>Controller buttons currently down.</summary>
    public ControllerButton Current { get; private set; }

    /// <summary>
    /// Rising-edge buttons. A button is set for exactly the frame boundary
    /// where it changes from released to pressed.
    /// </summary>
    public ControllerButton NewlyPressed { get; private set; }

    /// <summary>
    /// Auto-repeat form: it begins as <see cref="NewlyPressed"/> and periodically emits all held bits.
    /// </summary>
    public ControllerButton NewlyPressedWithRepeat { get; private set; }

    /// <summary>The controller buttons that were down on the previous update.</summary>
    public ControllerButton Previous { get; private set; }

    /// <summary>The current countdown to the next auto-repeat, 0 through 65535.</summary>
    public WrappingCountdown RepeatTimer { get; private set; }

    /// <summary>
    /// Temporarily publishes the demo's explicit held/edge pair, as the demo's
    /// does. Disposing restores the real latched sample for the demo-cancellation check.
    /// This is deliberately not Latch: recorded edges need not equal held-input changes.
    /// </summary>
    internal IDisposable UseDemoInput(ControllerButton held, ControllerButton newlyPressed)
    {
        var restore = new DemoInputRestore(this, Current, NewlyPressed);
        Current = held;
        NewlyPressed = newlyPressed;
        return restore;
    }

    /// <summary>Restores the real latched controller sample after a scoped demo input override.</summary>
    private sealed class DemoInputRestore(ControllerInputState owner, ControllerButton held, ControllerButton newlyPressed) : IDisposable
    {
        /// <summary>Prevents a repeated dispose from replacing a later controller sample.</summary>
        private bool disposed;
        /// <summary>Restores the held and edge buttons captured before the demo override.</summary>
        public void Dispose()
        {
            if (disposed) return;
            owner.Current = held;
            owner.NewlyPressed = newlyPressed;
            disposed = true;
        }
    }

    /// <summary>Latches one completed automatic controller sample.</summary>
    /// <remarks>Raw input is validated where it enters the game (recordings, replays, hosts), not here.</remarks>
    public void Latch(ControllerButton input)
    {
        Current = input;

        // The buttons that changed since the previous read, kept only where they are now held, are the newly pressed
        // ones. Released buttons drop out because they are absent from Current.
        NewlyPressed = Current & (Previous ^ Current);
        NewlyPressedWithRepeat = NewlyPressed;

        if (Current != ControllerButton.None && Current == Previous)
        {
            // The decrement wraps below zero. If RepeatTimer is zero, it becomes 65535 and does
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
