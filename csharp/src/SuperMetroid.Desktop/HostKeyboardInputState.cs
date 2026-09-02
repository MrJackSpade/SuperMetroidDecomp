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

        return new HostKeyboardInputSmokeTestResult(pressed, released);
    }
}
