using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Assets;

/// <summary>Checks the current-position cell in actual published gameplay HUD pixels.</summary>
internal static class MinimapBlinkAudit
{
    public static int Run(string romPath, Func<SuperMetroidRuntime, Rgba32[]>? render = null)
    {
        foreach (ushort room in MapCrossViewAuditDefinitions.RepresentativeRooms)
            VerifyRoom(romPath, room, render ?? SuperMetroidRuntimeFrameRenderer.Render);
        Console.WriteLine("Minimap blink: four retail areas, exact eight-frame cadence, and two distinct rendered center-cell palettes pass.");
        return 0;
    }

    private static void VerifyRoom(string romPath, ushort room, Func<SuperMetroidRuntime, Rgba32[]> render)
    {
        var runtime = new SuperMetroidRuntime(SuperMetroidAddressSpace.LoadRetailRom(romPath));
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RunNmi(0, true);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(room, 0, 0);
        runtime.Samus!.XPosition = 128;
        runtime.Samus.YPosition = 128;
        runtime.RunNmi(0, true);
        var images = new Dictionary<bool, string>();
        for (int tick = 0; tick < 40; tick++)
        {
            // Keep this a HUD fixture, not a boss fight or a moving map-cell test.
            runtime.Samus.XPosition = 128;
            runtime.Samus.YPosition = 128;
            runtime.StepFrame(0, allowCeresElevatorDeparture: false);
            var pixels = render(runtime);
            string cell = string.Join(',', Enumerable.Range(0, 64).Select(i => pixels[(16 + i / 8) * 256 + 224 + i % 8].ToString()));
            if (tick < 8) continue; // let the initial HUD publication reach VRAM
            bool lit = ((runtime.NmiFrameCounter8 - 1) & 8) == 0;
            if (images.TryGetValue(lit, out string? previous) && cell != previous)
                throw new InvalidDataException($"Room {room:X4}: rendered blink cadence differs at tick {tick}.");
            images[lit] = cell;
        }
        if (images.Count != 2 || images[true] == images[false])
            throw new InvalidDataException($"Room {room:X4}: minimap center never visibly blinks across two complete cycles.");
    }
}
