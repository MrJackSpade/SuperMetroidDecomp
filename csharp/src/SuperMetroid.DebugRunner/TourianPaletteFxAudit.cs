using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;

/// <summary>Reproduces #482 and checks the literal cross-slot reads in $8D:F621.</summary>
internal static class TourianPaletteFxAudit
{
    public static int Run(string romPath)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        int[] expectedCounts = [1, 2, 2, 2, 2, 2, 1, 0];
        for (int count = 1; count <= 8; count++)
        {
            var fx = new RoomPaletteFxSystem();
            var cgram = new SnesCgram();
            for (int i = 0; i < count; i++) fx.SpawnDefinition(bus, 0xf7a1, 0);
            fx.Step(bus, cgram, 0, 0, false, false);
            fx.Step(bus, cgram, 0, 0, false, false);
            if (fx.ActiveCount != expectedCounts[count - 1])
                throw new InvalidDataException($"F621 with {count} allocations left {fx.ActiveCount} objects, expected {expectedCounts[count - 1]}.");
        }
        var runtime = new SuperMetroidRuntime(bus, playerInvincibilityEnabled: true);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(0xdaae);
        var colors = new HashSet<ushort>();
        for (int frame = 0; frame < 600; frame++)
        {
            runtime.StepFrame(0);
            colors.Add(runtime.Cgram.Colors[0x00e8 / 2]);
        }
        if (runtime.RoomPaletteFx.ActiveCount == 0)
            throw new InvalidDataException("Tourian's room palette animator was incorrectly removed.");
        if (colors.Count < 4)
            throw new InvalidDataException("Tourian's glowing blocks stopped cycling their cartridge colors.");
        Console.WriteLine($"#482 passed: all eight allocation depths and 600 production frames in $8F:DAAE, {colors.Count} glow colors.");
        return 0;
    }
}
