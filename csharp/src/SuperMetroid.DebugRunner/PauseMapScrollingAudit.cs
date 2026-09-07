using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

internal static class PauseMapScrollingAudit
{
    public static int Run(string romPath)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        var system = new Bank80SystemState();
        system.SetAreaMapAcquired(0);
        var pause = new PauseMenuState(bus, new SamusState(), system, AreaId.Crateria, 30, 16);
        var before = pause.Render();
        ushort initial = pause.MapHorizontalScroll;
        ushort marker = pause.LastIndicatorOriginX;
        for (int tick = 1; tick <= 8; tick++)
        {
            // One short press must complete the accepted native eight-frame pulse.
            pause.Step(0, 0, tick == 1 ? (ushort)SnesButton.Left : (ushort)0);
            ushort expected = unchecked((ushort)(initial - (tick >= 4 ? 8 : 0)));
            if (pause.MapHorizontalScroll != expected)
                throw new InvalidDataException($"Pause map scroll tick {tick}: expected {expected:X4}, got {pause.MapHorizontalScroll:X4}.");
        }
        var after = pause.Render();
        if (pause.LastIndicatorOriginX != marker + 8)
            throw new InvalidDataException("Pause-map rendered marker did not follow the eight-pixel BG scroll.");
        int translatedDetailPixels = 0;
        for (int y = 64; y < 176; y++)
        for (int x = 32; x < 216; x++)
        {
            int source = y * 256 + x;
            if (before[source] != before[source + 8] && after[source + 8] == before[source])
                translatedDetailPixels++;
        }
        if (translatedDetailPixels < 32)
            throw new InvalidDataException("Pause map did not visibly translate its nonuniform terrain pixels.");
        foreach (var sample in new[] { (Button: SnesButton.Left, Dx: -8, Dy: 0),
            (Button: SnesButton.Right, Dx: 8, Dy: 0), (Button: SnesButton.Up, Dx: 0, Dy: -8),
            (Button: SnesButton.Down, Dx: 0, Dy: 8) })
        {
            var scroll = new PauseMapScroll(0, 504, 0, 248);
            ushort x = 128, y = 16;
            for (int tick = 1; tick <= 8; tick++)
            {
                bool sound = scroll.Step(tick == 1 ? (ushort)sample.Button : (ushort)0, ref x, ref y);
                if (x != 128 + (tick >= 4 ? sample.Dx : 0) || y != 16 + (tick >= 4 ? sample.Dy : 0) || sound != (tick == 8))
                    throw new InvalidDataException($"Wrong {sample.Button} scroll displacement/sound at tick {tick}.");
            }
        }
        var limits = new PauseMapScroll(0, 504, 0, 248);
        ushort limitX = unchecked((ushort)-24), limitY = unchecked((ushort)-56);
        for (int tick = 0; tick < 16; tick++)
            if (limits.Step((ushort)(SnesButton.Left | SnesButton.Up), ref limitX, ref limitY) ||
                limitX != unchecked((ushort)-24) || limitY != unchecked((ushort)-56))
                throw new InvalidDataException("Pause map escaped its upper/left scroll boundaries.");
        limitX = 128;
        limitY = 16;
        for (int tick = 0; tick < 512; tick++)
            limits.Step((ushort)SnesButton.Right, ref limitX, ref limitY);
        for (int tick = 0; tick < 512; tick++)
            limits.Step((ushort)SnesButton.Down, ref limitX, ref limitY);
        // Right/down use inclusive native comparisons; one last pulse takes the
        // register just beyond the cutoff rather than clamping it to that cutoff.
        if (limitX != 280 || limitY != 72)
            throw new InvalidDataException($"Wrong lower/right limits: {limitX},{limitY}.");
        Console.WriteLine("Pause map scrolling: short held pulse, exact fourth-tick displacement, and rendered marker movement pass.");
        return 0;
    }
}
