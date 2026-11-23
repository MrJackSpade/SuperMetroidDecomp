using SuperMetroid.Diagnostics.Input;
using System.Diagnostics;
using SuperMetroid.Core.Input;
using SuperMetroid.Desktop.Input;

namespace SuperMetroid.DesktopVerification.Input;

/// <summary>Read-only live WinMM observations, not an injected controller test.</summary>
internal static class LiveGamepadProbe
{
    /// <summary>Checks that a controller with no reported axes stays neutral while real zero axes still mean up and left.</summary>
    public static void VerifyMissingAxes()
    {
        // Reproduce the live driver: no axes, zero X/Y, advertised ranges retained.
        var caps = new WindowsGamepadInput.JoystickCapabilities { XMaximum = 65535, YMaximum = 65535 };
        var position = new WindowsGamepadInput.JoystickPosition();
        var snapshot = WindowsGamepadInput.CreateSnapshot(position, caps);
        if (GenericGamepadInput.Map(snapshot) != ControllerButton.None)
            throw new InvalidDataException("Zero-axis controller emitted phantom directional input.");
        caps.AxisCount = 2;
        snapshot = WindowsGamepadInput.CreateSnapshot(position, caps);
        if (GenericGamepadInput.Map(snapshot) != (ControllerButton.Up | ControllerButton.Left))
            throw new InvalidDataException("Real zero-valued axes must retain their full up-left deflection.");
        Console.WriteLine("Missing-axis controls pass: absent axes neutral, present axes preserved.");

        // Reproduce the reported slot: WinMM joystick 0 answered with no buttons and no axes,
        // and selecting it hid every usable controller behind it.
        var empty = new WindowsGamepadInput.JoystickCapabilities { XMaximum = 65535, YMaximum = 65535 };
        if (WindowsGamepadInput.CanProduceInput(empty))
            throw new InvalidDataException("A joystick slot with no buttons and no axes was selectable.");
        if (!WindowsGamepadInput.CanProduceInput(empty with { ButtonCount = 10 }))
            throw new InvalidDataException("A joystick with buttons but no axes was rejected.");
        if (!WindowsGamepadInput.CanProduceInput(empty with { AxisCount = 2 }))
            throw new InvalidDataException("A joystick with axes but no buttons was rejected.");
        Console.WriteLine("Empty joystick slots pass: a slot with no buttons and no axes is skipped.");
    }

    /// <summary>Prints changing WinMM controller snapshots and mapped SNES buttons for the requested duration.</summary>
    public static void Run(int seconds)
    {
        if (seconds is < 1 or > 30) throw new ArgumentOutOfRangeException(nameof(seconds));
        var pad = new WindowsGamepadInput();
        var xinput = new XInputGamepadInput();
        var clock = Stopwatch.StartNew();
        GamepadReading? previous = null;
        Console.WriteLine("Live gamepad probe: milliseconds, WinMM device, normalized axes, POV, raw buttons, mapped SNES (WinMM and XInput).");
        while (clock.Elapsed.TotalSeconds < seconds)
        {
            ControllerButton mapped = pad.Poll() | xinput.Poll();
            var current = new GamepadReading(pad.DeviceName, pad.LastSnapshot, mapped);
            if (previous != current)
            {
                var sample = pad.LastSnapshot;
                Console.WriteLine($"{clock.ElapsedMilliseconds},{pad.DeviceName ?? "disconnected"}," +
                    $"{sample.Stick.X},{sample.Stick.Y},{sample.PointOfView}," +
                    $"{sample.Buttons:X8},{ControllerWordCodec.ToJoypadWord(mapped):X4}");
                previous = current;
            }
            Thread.Sleep(10);
        }
        Console.WriteLine("Probe complete. Observations do not prove a gameplay action or physical input that was not performed.");
    }
}
