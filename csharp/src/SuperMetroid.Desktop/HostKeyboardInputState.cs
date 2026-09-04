using SuperMetroid.Core.Input;

namespace SuperMetroid.Desktop;

/// <summary>
/// Converts Windows key messages into the single cartridge-format controller word consumed
/// by the emulated NMI.
/// </summary>
/// <remarks>
/// This state belongs above individual child controls. A canvas-only KeyDown handler stops
/// receiving Enter and the arrows as soon as a debugger toolbar item owns focus, allowing
/// WinForms dialog/navigation behavior to mutate the host UI instead of the SNES pad.
/// </remarks>
internal sealed class HostKeyboardInputState
{
    internal const int KeyDownMessage = 0x0100;
    internal const int KeyUpMessage = 0x0101;
    internal const int SystemKeyDownMessage = 0x0104;
    internal const int SystemKeyUpMessage = 0x0105;

    private readonly HashSet<Keys> heldKeys = [];

    /// <summary>
    /// Captures one key message. True means the message belongs exclusively to gameplay and
    /// must not continue into ToolStrip, ComboBox, default-button, or dialog navigation.
    /// </summary>
    public bool ApplyWindowMessage(int message, Keys key)
    {
        Keys keyCode = key & Keys.KeyCode;
        if (!IsGameplayKey(keyCode))
            return false;

        switch (message)
        {
            case KeyDownMessage:
            case SystemKeyDownMessage:
                heldKeys.Add(keyCode);
                return true;

            case KeyUpMessage:
            case SystemKeyUpMessage:
                heldKeys.Remove(keyCode);
                return true;

            default:
                return false;
        }
    }

    public void Clear() => heldKeys.Clear();

    /// <summary>Merges the keyboard producer into an already-polled gamepad word.</summary>
    public ushort BuildControllerWord(SnesButton gamepadInput)
    {
        SnesButton input = gamepadInput;
        if (heldKeys.Contains(Keys.Left)) input |= SnesButton.Left;
        if (heldKeys.Contains(Keys.Right)) input |= SnesButton.Right;
        if (heldKeys.Contains(Keys.Up)) input |= SnesButton.Up;
        if (heldKeys.Contains(Keys.Down)) input |= SnesButton.Down;
        if (heldKeys.Contains(Keys.Z)) input |= SnesButton.B;

        // Space is a discoverable desktop jump alias; X retains the compact four-face-
        // button layout printed below the viewport. Both become the retail SNES A bit.
        if (heldKeys.Contains(Keys.X) || heldKeys.Contains(Keys.Space)) input |= SnesButton.A;
        if (heldKeys.Contains(Keys.A)) input |= SnesButton.Y;
        if (heldKeys.Contains(Keys.S)) input |= SnesButton.X;
        if (heldKeys.Contains(Keys.Q)) input |= SnesButton.L;
        if (heldKeys.Contains(Keys.W)) input |= SnesButton.R;
        if (heldKeys.Contains(Keys.Enter)) input |= SnesButton.Start;
        if (heldKeys.Contains(Keys.ShiftKey)) input |= SnesButton.Select;
        return (ushort)input;
    }

    private static bool IsGameplayKey(Keys key) => key is
        Keys.Left or Keys.Right or Keys.Up or Keys.Down or
        Keys.Z or Keys.X or Keys.Space or Keys.A or Keys.S or Keys.Q or Keys.W or
        Keys.Enter or Keys.ShiftKey;
}

/// <summary>
/// Prevents a controller state sampled before desktop deactivation from becoming a
/// permanent held input when Windows or an RDP session drops the corresponding release.
/// </summary>
/// <remarks>
/// The gate does not guess that a long press is invalid. After deactivation it requires
/// one completely neutral merged keyboard/gamepad sample before accepting input again.
/// This preserves arbitrary legitimate holds while making focus loss an explicit release
/// boundary for both host producers.
/// </remarks>
internal sealed class HostInputActivationGate
{
    private bool suppressedUntilNeutral;

    /// <summary>Suppresses subsequent input until every host control has been released.</summary>
    public void SuppressUntilNeutral() => suppressedUntilNeutral = true;

    /// <summary>Returns a safe controller word for the current activation state.</summary>
    public ushort Filter(ushort input)
    {
        if (!suppressedUntilNeutral)
            return input;

        if (input == 0)
            suppressedUntilNeutral = false;
        return 0;
    }
}

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
        ArgumentNullException.ThrowIfNull(keyboard);
        ArgumentNullException.ThrowIfNull(activation);
        keyboard.Clear();
        activation.SuppressUntilNeutral();
    }
}

/// <summary>Result of the headless host-key routing regression.</summary>
public readonly record struct HostKeyboardInputSmokeTestResult(
    ushort EnterControllerWord,
    ushort ReleasedControllerWord);

/// <summary>Exercises the exact parent-preview message path without opening a window.</summary>
public static class HostKeyboardInputSmokeTest
{
    public static HostKeyboardInputSmokeTestResult Run()
    {
        var keyboard = new HostKeyboardInputState();
        if (!keyboard.ApplyWindowMessage(HostKeyboardInputState.KeyDownMessage, Keys.Enter))
            throw new InvalidDataException("Enter keydown escaped the gameplay key preview.");

        ushort pressed = keyboard.BuildControllerWord(SnesButton.None);
        if (pressed != (ushort)SnesButton.Start)
        {
            throw new InvalidDataException(
                $"Enter produced controller ${pressed:X4}; expected Start-only $1000.");
        }

        if (!keyboard.ApplyWindowMessage(HostKeyboardInputState.KeyUpMessage, Keys.Enter))
            throw new InvalidDataException("Enter keyup escaped the gameplay key preview.");
        ushort released = keyboard.BuildControllerWord(SnesButton.None);
        if (released != 0)
            throw new InvalidDataException($"Released Enter left controller ${released:X4} latched.");

        // WinForms/RDP is allowed to omit a keyboard-up message and WinMM may return the
        // last joystick sample when a remote session disconnects without deactivating the
        // form. Model both stale producers: Right remains in the event-owned keyboard set,
        // while A remains in the polled gamepad word.
        var activation = new HostInputActivationGate();
        keyboard.ApplyWindowMessage(HostKeyboardInputState.KeyDownMessage, Keys.Right);
        ushort jumpAndRight = (ushort)(SnesButton.A | SnesButton.Right);
        if (activation.Filter(keyboard.BuildControllerWord(SnesButton.A)) != jumpAndRight)
            throw new InvalidDataException("Active host input was unexpectedly suppressed.");

        // PlayableGameControl routes both Form.Deactivate and Windows SessionSwitch through
        // this policy. The first neutral sample re-arms input; no invented hold timeout is
        // imposed on uninterrupted gameplay.
        HostInputDiscontinuity.Release(keyboard, activation);
        if (keyboard.BuildControllerWord(SnesButton.None) != 0)
            throw new InvalidDataException("Host discontinuity did not release keyboard state.");
        if (activation.Filter((ushort)SnesButton.A) != 0 ||
            activation.Filter((ushort)SnesButton.Right) != 0)
        {
            throw new InvalidDataException(
                "Deactivated host leaked a latched jump or direction before neutral input.");
        }
        if (activation.Filter(0) != 0 ||
            activation.Filter((ushort)SnesButton.Right) != (ushort)SnesButton.Right)
        {
            throw new InvalidDataException(
                "Host input did not re-arm after a completely neutral sample.");
        }

        return new HostKeyboardInputSmokeTestResult(pressed, released);
    }
}
