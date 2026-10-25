namespace SuperMetroid.Core.Input;

/// <summary>Verification access to <see cref="ControllerButtons"/> members production does not use.</summary>
internal static class ControllerButtonsAccess
{
    /// <summary>Tests whether every requested button is present.</summary>
    internal static bool HasAll(this ControllerButton input, ControllerButton buttons) =>
        (input & buttons) == buttons;
}
