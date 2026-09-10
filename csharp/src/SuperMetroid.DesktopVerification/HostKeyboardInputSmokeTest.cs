using SuperMetroid.Core.Input;

namespace SuperMetroid.Desktop;

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
