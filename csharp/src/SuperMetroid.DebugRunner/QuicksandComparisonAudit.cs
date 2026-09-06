using System.Globalization;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;

/// <summary>Compares takeoff, rising, release, landing, and sinking to an original-CPU trace.</summary>
internal static class QuicksandComparisonAudit
{
    public static int Run(string romPath, string tracePath)
    {
        var lines = File.ReadLines(tracePath).Where(line => line.StartsWith("SAND "))
            .Select(line => line.Split(' ').Skip(1).Select(field => field.Split('='))
                .ToDictionary(field => field[0], field => field[1])).ToArray();
        if (lines.Length != 2880) throw new InvalidDataException($"Expected 2880 native sand samples, got {lines.Length}.");
        int sample = 0, mismatches = 0;
        for (int deep = 0; deep < 2; deep++)
        for (int gravity = 0; gravity < 2; gravity++)
        for (int hold = 0; hold < 2; hold++)
        for (int delay = 8; delay <= 80; delay += 72)
        {
            var bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
            var runtime = new SuperMetroidRuntime(bus);
            runtime.InitializeHud(HudSnapshot.CeresDebug);
            runtime.RunNmi(0, true);
            runtime.InitializeStartingCeresRoom();
            runtime.InitializeCeresStartSamus();
            runtime.LoadCartridgeRoomForDebug(0xd461, 0, 0);
            // Both fixed banks mirror low WRAM. Supply an explicit empty population/
            // graphics-set terminator instead of allowing retail actors into the fixture.
            bus.WriteByte(0x7e1ff0, 0xff);
            bus.WriteByte(0x7e1ff1, 0xff);
            bus.WriteByte(0x7e1ff2, 0);
            runtime.Enemies.Load(bus, 0x1ff0, 0x1ff0, runtime.Vram, runtime.Cgram, () => 0);
            var level = runtime.LevelData!;
            for (int y = 0; y < 16; y++)
            for (int x = 0; x < 64; x++)
            {
                int index = y * 64 + x;
                level.SetForegroundEntry(index, y < 12 ? (ushort)0 : y < 14 ? (ushort)0x3000 : (ushort)0x8000);
                level.SetBehavior(index, y is 12 or 13 ? (deep != 0 && y == 13 ? (byte)0x83 : (byte)0x82) : (byte)0);
            }
            runtime.InitializeDebugGroundedSamus(128, 166, 14);
            var samus = runtime.Samus!;
            samus.YPosition = 172;
            samus.Kinematics.YSubposition = 0;
            samus.InputLocked = false;
            samus.EquippedItems = gravity != 0 ? (ushort)SamusEquipmentFlags.GravitySuit : (ushort)0;
            for (int frame = 0; frame < 180; frame++)
            {
                ushort input = frame >= delay && frame < delay + (hold != 0 ? 60 : 8) ? (ushort)SnesButton.A : (ushort)0;
                runtime.StepFrame(input);
                var expected = lines[sample++];
                uint Hex(string key) => uint.Parse(expected[key], NumberStyles.HexNumber);
                if (int.Parse(expected["deep"]) != deep || int.Parse(expected["gravity"]) != gravity || int.Parse(expected["hold"]) != hold ||
                    int.Parse(expected["delay"]) != delay || int.Parse(expected["frame"]) != frame)
                    throw new InvalidDataException("Native sand trace order mismatch.");
                var body = samus.Kinematics;
                if (body.YFixed == Hex("y") && samus.Pose == Hex("pose") &&
                    body.VerticalSpeedFixed == Hex("speed") && body.YDirection == int.Parse(expected["dir"]) &&
                    (uint)body.ExtraYFixed == Hex("extra")) continue;
                if (++mismatches <= 25)
                    Console.WriteLine($"MISMATCH deep={deep} gravity={gravity} hold={hold} delay={delay} frame={frame}: " +
                        $"Y={body.YFixed:X8}/{expected["y"]} pose={samus.Pose:X2}/{expected["pose"]} " +
                        $"speed={body.VerticalSpeedFixed:X8}/{expected["speed"]} dir={body.YDirection}/{expected["dir"]} extra={body.ExtraYFixed:X8}/{expected["extra"]}");
            }
        }
        Console.WriteLine($"Quicksand native comparison: {sample} frames, {mismatches} mismatches.");
        return mismatches == 0 ? 0 : 1;
    }
}
