using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Hardware;
namespace SuperMetroid.TestSupport.RenderFixtures;
using SuperMetroid.TestSupport.RepositoryInstallation;

/// <summary>Checks the current-position cell in actual published gameplay HUD pixels.</summary>
internal static class MinimapBlinkAudit
{
    /// <summary>Checks visible current-cell blinking in representative rooms across four retail areas.</summary>
    public static int Run(string romPath, Func<SuperMetroidRuntime, Rgba32[]>? render = null)
    {
        foreach (ushort room in MapCrossViewAuditDefinitions.RepresentativeRooms)
            VerifyRoom(romPath, room, render ?? (runtime => FrontendScenePixelsAccess.Pixels(runtime.PublishDisplayFrame())));
        Console.WriteLine("Minimap blink: four retail areas, exact eight-frame cadence, and two distinct rendered center-cell palettes pass.");
        return 0;
    }

    /// <summary>Keeps Samus on one map cell and compares the rendered HUD cell through two blink cycles.</summary>
    private static void VerifyRoom(string romPath, ushort room, Func<SuperMetroidRuntime, Rgba32[]> render)
    {
        var runtime = RepositoryInstallation.CreateRuntime(SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath));
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(room, new Position16<RoomPixels>(0, 0));
        runtime.Player.IntroducedSamus.Position = runtime.Player.IntroducedSamus.Position.WithWholeX(128);
        runtime.Player.IntroducedSamus.Position = runtime.Player.IntroducedSamus.Position.WithWholeY(128);
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        var images = new Dictionary<bool, string>();
        for (int tick = 0; tick < 40; tick++)
        {
            // Keep this a HUD fixture, not a boss fight or a moving map-cell test.
            runtime.Player.IntroducedSamus.Position = runtime.Player.IntroducedSamus.Position.WithWhole(new(128, 128));
            runtime.StepFrame(0, new RuntimeFrameRequest { Policy = RuntimeFramePolicy.Gameplay with { AllowCeresElevatorDeparture = false } });
            var pixels = render(runtime);
            string cell = string.Join(',', Enumerable.Range(0, 64).Select(i => pixels[(16 + i / 8) * 256 + 224 + i % 8].ToString()));
            if (tick < 8) continue; // let the initial HUD publication reach VRAM
            bool lit = ((runtime.Clock.Counter8 - 1) & 8) == 0;
            if (images.TryGetValue(lit, out string? previous) && cell != previous)
                throw new InvalidDataException($"Room {room:X4}: rendered blink cadence differs at tick {tick}.");
            images[lit] = cell;
        }
        if (images.Count != 2 || images[true] == images[false])
            throw new InvalidDataException($"Room {room:X4}: minimap center never visibly blinks across two complete cycles.");
    }
}
