namespace SuperMetroid.Core.Input;

/// <summary>
/// The player's controller buttons, latched once per update by the screens that read buttons rather than gameplay
/// actions (menus, the message box's save choice, the pause screen): the buttons held, newly pressed and previously held.
/// Gameplay reads <see cref="GameplayActionInput"/> instead.
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
