namespace SuperMetroid.Core.Input;

/// <summary>
/// Named combinations of the player's controller buttons, for the screens and hosts that read buttons; gameplay reads
/// <see cref="GameplayAction"/>s and their combinations in <see cref="GameplayActions"/>.
/// </summary>
public static partial class ControllerButtons
{
    /// <summary>Every bit physically supplied by a standard twelve-button controller.</summary>
    public const ControllerButton All =
        ControllerButton.R | ControllerButton.L | ControllerButton.X | ControllerButton.A |
        ControllerButton.Right | ControllerButton.Left | ControllerButton.Down | ControllerButton.Up |
        ControllerButton.Start | ControllerButton.Select | ControllerButton.Y | ControllerButton.B;

    /// <summary>Tests whether at least one requested button is present.</summary>
    public static bool HasAny(this ControllerButton input, ControllerButton buttons) =>
        (input & buttons) != ControllerButton.None;
}
