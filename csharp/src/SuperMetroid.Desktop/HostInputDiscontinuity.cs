using SuperMetroid.Core;

namespace SuperMetroid.Desktop;

/// <summary>
/// Applies one host-input discontinuity to every stateful desktop producer. Windows can
/// omit both keyboard-up messages and fresh WinMM joystick samples across a remote-session
/// switch, so the operation must clear event-owned keys and arm the merged neutral gate as
/// one indivisible policy decision.
/// </summary>
internal static class HostInputDiscontinuity
{
    /// <summary>Releases event-owned keys and suppresses stale device samples until neutral.</summary>
    public static void Release(
        HostKeyboardInputState keyboard,
        HostInputActivationGate activation)
    {
        Ensure.NotNull(keyboard);
        Ensure.NotNull(activation);
        keyboard.Clear();
        activation.SuppressUntilNeutral();
    }
}
