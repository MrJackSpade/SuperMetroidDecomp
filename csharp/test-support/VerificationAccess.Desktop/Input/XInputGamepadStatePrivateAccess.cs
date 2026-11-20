using SuperMetroid.Core;

namespace SuperMetroid.Desktop.Input;

/// <summary>
/// The private members of <see cref="SuperMetroid.Desktop.Input.XInputGamepadState"/> checks name (#2570), each an extension member of the
/// same name reaching the private member through <see cref="PrivateState"/>, so checks name them as the owner's own code
/// does and production keeps them private. Written as the architecture audit's <c>--fix-test-only-members</c> writes its
/// classes, for a constant it leaves to a person because a check could name a constant where a constant is required;
/// the checks name this one only as a value.
/// </summary>
internal static class XInputGamepadStatePrivateAccess
{
    /// <summary>The private static members checks name.</summary>
    extension(global::SuperMetroid.Desktop.Input.XInputGamepadState)
    {
        /// <summary>The private constant <c>TriggerThreshold</c>, read by name.</summary>
        internal static int TriggerThreshold =>
            PrivateState.StaticField<int>(typeof(global::SuperMetroid.Desktop.Input.XInputGamepadState), "TriggerThreshold");
    }
}
