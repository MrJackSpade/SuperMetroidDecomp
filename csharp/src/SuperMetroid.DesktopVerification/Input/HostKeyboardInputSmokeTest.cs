using SuperMetroid.Core.Input;
using SuperMetroid.Desktop.Input;

namespace SuperMetroid.DesktopVerification.Input;

/// <summary>Exercises the exact parent-preview message path without opening a window.</summary>
public static class HostKeyboardInputSmokeTest
{
    /// <summary>Checks synthetic gameplay-key routing, shoulder mappings, Enter release, and neutral-input rearming after a host discontinuity without a window.</summary>
    public static HostKeyboardInputSmokeTestResult Run()
    {
        LiveGamepadProbe.VerifyMissingAxes();
        var keyboard = new HostKeyboardInputState();
        // These are synthetic Windows key messages, not a physical-keyboard claim.
        keyboard.ApplyWindowMessage((int)HostKeyboardInputState.KeyboardWindowMessage.KeyDown, Keys.Space);
        keyboard.ApplyWindowMessage((int)HostKeyboardInputState.KeyboardWindowMessage.KeyDown, Keys.W);
        if (keyboard.BuildControllerWord(ControllerButton.None) != (ControllerButton.A | ControllerButton.R))
            throw new InvalidDataException("Space+W must reach gameplay as Jump+AimUp under native defaults.");
        keyboard.ApplyWindowMessage((int)HostKeyboardInputState.KeyboardWindowMessage.KeyUp, Keys.W);
        keyboard.ApplyWindowMessage((int)HostKeyboardInputState.KeyboardWindowMessage.KeyDown, Keys.Q);
        if (keyboard.BuildControllerWord(ControllerButton.None) != (ControllerButton.A | ControllerButton.L))
            throw new InvalidDataException("Space+Q must reach gameplay as Jump+AimDown under native defaults.");
        keyboard.Clear();
        if (!keyboard.ApplyWindowMessage((int)HostKeyboardInputState.KeyboardWindowMessage.KeyDown, Keys.Enter))
            throw new InvalidDataException("Enter keydown escaped the gameplay key preview.");

        ControllerButton pressed = keyboard.BuildControllerWord(ControllerButton.None);
        if (pressed != ControllerButton.Start)
        {
            throw new InvalidDataException(
                $"Enter produced controller ${pressed:X}; expected Start-only $1000.");
        }

        if (!keyboard.ApplyWindowMessage((int)HostKeyboardInputState.KeyboardWindowMessage.KeyUp, Keys.Enter))
            throw new InvalidDataException("Enter keyup escaped the gameplay key preview.");
        ControllerButton released = keyboard.BuildControllerWord(ControllerButton.None);
        if (released != 0)
            throw new InvalidDataException($"Released Enter left controller ${released:X} latched.");

        // WinForms/RDP is allowed to omit a keyboard-up message and WinMM may return the
        // last joystick sample when a remote session disconnects without deactivating the
        // form. Model both stale producers: Right remains in the event-owned keyboard set,
        // while A remains in the polled gamepad word.
        var activation = new HostInputActivationGate();
        keyboard.ApplyWindowMessage((int)HostKeyboardInputState.KeyboardWindowMessage.KeyDown, Keys.Right);
        ControllerButton jumpAndRight = (ControllerButton.A | ControllerButton.Right);
        if (activation.Filter(keyboard.BuildControllerWord(ControllerButton.A)) != jumpAndRight)
            throw new InvalidDataException("Active host input was unexpectedly suppressed.");

        // PlayableGameControl routes both Form.Deactivate and Windows SessionSwitch through
        // this policy. The first neutral sample re-arms input; no invented hold timeout is
        // imposed on uninterrupted gameplay.
        HostInputDiscontinuity.Release(keyboard, activation);
        if (keyboard.BuildControllerWord(ControllerButton.None) != 0)
            throw new InvalidDataException("Host discontinuity did not release keyboard state.");
        if (activation.Filter(ControllerButton.A) != 0 ||
            activation.Filter(ControllerButton.Right) != 0)
        {
            throw new InvalidDataException(
                "Deactivated host leaked a latched jump or direction before neutral input.");
        }
        if (activation.Filter(0) != 0 ||
            activation.Filter(ControllerButton.Right) != ControllerButton.Right)
        {
            throw new InvalidDataException(
                "Host input did not re-arm after a completely neutral sample.");
        }

        return new HostKeyboardInputSmokeTestResult(pressed, released);
    }
}
