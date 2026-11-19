namespace SuperMetroid.Core.Input;

/// <summary>
/// The installed key of each <see cref="ControllerButton"/> (#2520), written out so renaming a member never changes a
/// name the installation, an installed document or a save spells it by.
/// </summary>
internal static class ControllerButtonKeys
{
    /// <summary>Each ControllerButton member's installed key.</summary>
    internal static InstalledKeys<ControllerButton> Installed { get; } = new(new Dictionary<ControllerButton, string>
    {
        [ControllerButton.None] = "None",
        [ControllerButton.Up] = "Up",
        [ControllerButton.Down] = "Down",
        [ControllerButton.Left] = "Left",
        [ControllerButton.Right] = "Right",
        [ControllerButton.Start] = "Start",
        [ControllerButton.Select] = "Select",
        [ControllerButton.A] = "A",
        [ControllerButton.B] = "B",
        [ControllerButton.X] = "X",
        [ControllerButton.Y] = "Y",
        [ControllerButton.L] = "L",
        [ControllerButton.R] = "R",
    });
}
