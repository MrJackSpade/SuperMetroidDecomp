namespace SuperMetroid.Core.Input;

/// <summary>Verification access to <see cref="ControllerInputState"/> members production does not use.</summary>
internal static class ControllerInputStateAccess
{
    /// <summary>Provides a typed button view over the controller's raw current-state word.</summary>
    extension(ControllerInputState self)
    {
        /// <summary>Typed view of <see cref="ControllerInputState.Current"/> for gameplay button tests.</summary>
        internal SnesButton CurrentButtons => self.Current;
    }
}
