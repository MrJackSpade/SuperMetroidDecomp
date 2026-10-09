using SuperMetroid.Core.Input;

/// <summary>Verification access to <see cref="ControllerInputState"/> members production does not use.</summary>
internal static class ControllerInputStateAccess
{
    extension(ControllerInputState self)
    {
        /// <summary>Typed view of <see cref="ControllerInputState.Current"/> for gameplay button tests.</summary>
        internal SnesButton CurrentButtons => (SnesButton)self.Current;
    }
}

/// <summary>Verification access to <see cref="SnesButtons"/> members production does not use.</summary>
internal static class SnesButtonsAccess
{
    /// <summary>Tests whether every requested button is present.</summary>
    internal static bool HasAll(this SnesButton input, SnesButton buttons) =>
        (input & buttons) == buttons;
}
