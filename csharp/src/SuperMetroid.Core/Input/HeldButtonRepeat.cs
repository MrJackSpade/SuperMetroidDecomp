namespace SuperMetroid.Core.Input;

/// <summary>
/// The pause map and item screens' held-button repeat: buttons kept held, and not newly pressed, without any change for
/// more than a delay's worth of updates repeat on that update and on every update after, until the held buttons change.
/// A fresh press is not held on its own update, so it never repeats there; releasing one button of a chord is a change and
/// restarts the delay.
/// Lifetime: the gameplay runtime, so a button still held when the pause screen opens again carries on from where the
/// last pause left it.
/// </summary>
public sealed class HeldButtonRepeat
{
    /// <summary>The held buttons the previous update saw, newly pressed buttons excluded.</summary>
    public ControllerButton PreviousHeld { get; private set; }

    /// <summary>
    /// The updates in a row the held buttons have stayed unchanged, counting stops one past the delay, where the repeat
    /// fires on every further update.
    /// </summary>
    public int UnchangedUpdates { get; private set; }

    /// <summary>The held buttons that repeat on this update; none until the delay has passed.</summary>
    public ControllerButton Repeating { get; private set; }

    /// <summary>Takes one update's buttons and decides which of them repeat.</summary>
    /// <param name="repeatDelay">The unchanged updates a held button waits through; it repeats on the next one.</param>
    /// <param name="buttonsDown">All buttons currently down.</param>
    /// <param name="newlyPressed">Buttons newly pressed this update.</param>
    public void Update(int repeatDelay, ControllerButton buttonsDown, ControllerButton newlyPressed)
    {
        int delay = Ensure.AtLeastZero(repeatDelay);
        ControllerButton held = buttonsDown & ~newlyPressed;
        bool unchanged = held == PreviousHeld;
        PreviousHeld = held;
        UnchangedUpdates = unchanged ? Math.Min(UnchangedUpdates + 1, delay + 1) : 0;
        Repeating = UnchangedUpdates > delay ? held : ControllerButton.None;
    }
}
