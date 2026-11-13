namespace SuperMetroid.Core.Input;

/// <summary>
/// Reads the controller-1 sample near the end of every
/// frame boundary, after the automatic controller read has completed.
/// </summary>
public sealed class ControllerInputState
{
    /// <summary>Controller buttons currently down.</summary>
    public ControllerButton Current { get; private set; }

    /// <summary>
    /// Rising-edge buttons. A button is set for exactly the frame boundary
    /// where it changes from released to pressed.
    /// </summary>
    public ControllerButton NewlyPressed { get; private set; }

    /// <summary>The controller buttons that were down on the previous update.</summary>
    public ControllerButton Previous { get; private set; }

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
        Previous = Current;
    }
}
