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
        if (GenericGamepadInput.Map(snapshot) != SnesButton.None)
            throw new InvalidDataException("Zero-axis controller emitted phantom directional input.");
        caps.AxisCount = 2;
        snapshot = WindowsGamepadInput.CreateSnapshot(position, caps);
        if (GenericGamepadInput.Map(snapshot) != (SnesButton.Up | SnesButton.Left))
            throw new InvalidDataException("Real zero-valued axes must retain their full up-left deflection.");
        Console.WriteLine("Missing-axis controls pass: absent axes neutral, present axes preserved.");
    }

    /// <summary>Prints changing WinMM controller snapshots and mapped SNES buttons for the requested duration.</summary>
    public static void Run(int seconds)
    {
        if (seconds is < 1 or > 30) throw new ArgumentOutOfRangeException(nameof(seconds));
        var pad = new WindowsGamepadInput();
        var clock = Stopwatch.StartNew();
        GamepadReading? previous = null;
        Console.WriteLine("Live gamepad probe: milliseconds, device, normalized axes, POV, raw buttons, mapped SNES.");
        while (clock.Elapsed.TotalSeconds < seconds)
        {
            SnesButton mapped = pad.Poll();
            var current = new GamepadReading(pad.DeviceName, pad.LastSnapshot, mapped);
            if (previous != current)
            {
                var sample = pad.LastSnapshot;
                Console.WriteLine($"{clock.ElapsedMilliseconds},{pad.DeviceName ?? "disconnected"}," +
                    $"{sample.Stick.X},{sample.Stick.Y},{sample.PointOfView}," +
                    $"{sample.Buttons:X8},{(ushort)mapped:X4}");
                previous = current;
            }
            Thread.Sleep(10);
        }
        Console.WriteLine("Probe complete. Observations do not prove a gameplay action or physical input that was not performed.");
    }
}
