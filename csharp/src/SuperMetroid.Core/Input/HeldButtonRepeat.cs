namespace SuperMetroid.Core.Input;

/// <summary>
/// The pause map and item screens' held-button repeat: buttons kept held, and not newly pressed, without any change for
/// more than a delay's worth of updates repeat on that update and on every update after, until the held buttons change.
/// A fresh press is not held on its own update, so it never repeats there; releasing one button of a chord is a change and
/// restarts the delay.
/// Lifetime: the gameplay runtime, so a button still held when the pause screen opens again carries on from where the
/// last pause left it.
/// </summary>
public sealed partial class HeldButtonRepeat
{
    /// <summary>The held buttons the previous update saw, newly pressed buttons excluded.</summary>
    internal ControllerButton PreviousHeld { get; private set; }

    /// <summary>
    /// The repeat delay, counted on each update the held buttons stay unchanged and lasting the delay and the update after
    /// it; none until such an update and again whenever the held buttons change. Once it is over the held buttons repeat on
    /// every further update.
    /// </summary>
    internal UpdateCountdown? RepeatWait { get; set; }

    /// <summary>The held buttons that repeat on this update; none until the delay has passed.</summary>
    public ControllerButton Repeating { get; internal set; }

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
        if (unchanged)
        {
            RepeatWait ??= UpdateCountdown.Lasting(delay + 1);
            RepeatWait.Step();
        }
        else
            RepeatWait = null;
        Repeating = RepeatWait is { IsOver: true } ? held : ControllerButton.None;
    }
}
