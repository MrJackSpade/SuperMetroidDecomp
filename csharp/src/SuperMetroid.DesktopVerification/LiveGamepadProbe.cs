using System.Diagnostics;
using SuperMetroid.Core.Input;

namespace SuperMetroid.Desktop;

/// <summary>Read-only live WinMM observations, not an injected controller test.</summary>
internal static class LiveGamepadProbe
{
    public static void VerifyMissingAxes()
    {
        const System.Reflection.BindingFlags hidden = System.Reflection.BindingFlags.NonPublic;
        var type = typeof(WindowsGamepadInput);
        var capsType = type.GetNestedType("JoystickCapabilities", hidden)!;
        var positionType = type.GetNestedType("JoystickPosition", hidden)!;
        object caps = Activator.CreateInstance(capsType)!;
        object position = Activator.CreateInstance(positionType)!;
        // Reproduce the live driver: no axes, zero X/Y, advertised ranges retained.
        capsType.GetField("XMaximum")!.SetValue(caps, (uint)65535);
        capsType.GetField("YMaximum")!.SetValue(caps, (uint)65535);
        var method = type.GetMethod("CreateSnapshot", hidden | System.Reflection.BindingFlags.Static)!;
        var snapshot = (GenericGamepadSnapshot)method.Invoke(null, [position, caps])!;
        if (GenericGamepadInput.Map(snapshot) != SnesButton.None)
            throw new InvalidDataException("Zero-axis controller emitted phantom directional input.");
        capsType.GetField("AxisCount")!.SetValue(caps, (uint)2);
        snapshot = (GenericGamepadSnapshot)method.Invoke(null, [position, caps])!;
        if (GenericGamepadInput.Map(snapshot) != (SnesButton.Up | SnesButton.Left))
            throw new InvalidDataException("Real zero-valued axes must retain their full up-left deflection.");
        Console.WriteLine("Missing-axis controls pass: absent axes neutral, present axes preserved.");
    }

    public static void Run(int seconds)
    {
        if (seconds is < 1 or > 30) throw new ArgumentOutOfRangeException(nameof(seconds));
        var pad = new WindowsGamepadInput();
        var clock = Stopwatch.StartNew();
        (string? Device, GenericGamepadSnapshot Snapshot, SnesButton Mapped)? previous = null;
        Console.WriteLine("Live gamepad probe: milliseconds, device, normalized axes, POV, raw buttons, mapped SNES.");
        while (clock.Elapsed.TotalSeconds < seconds)
        {
            SnesButton mapped = pad.Poll();
            var current = (pad.DeviceName, pad.LastSnapshot, mapped);
            if (previous != current)
            {
                var sample = pad.LastSnapshot;
                Console.WriteLine($"{clock.ElapsedMilliseconds},{pad.DeviceName ?? "disconnected"}," +
                    $"{sample.HorizontalAxis},{sample.VerticalAxis},{sample.PointOfView}," +
                    $"{sample.Buttons:X8},{(ushort)mapped:X4}");
                previous = current;
            }
            Thread.Sleep(10);
        }
        Console.WriteLine("Probe complete. Observations do not prove a gameplay action or physical input that was not performed.");
    }
}
