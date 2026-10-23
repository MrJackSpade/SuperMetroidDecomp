using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Areas.Ceres;

/// <summary>Checks the Mode 7 map a Ceres room publishes instead of writing it into VRAM (#1349).</summary>
internal static class CeresMode7MapVerification
{
    /// <summary>
    /// A Ceres room publishes its tileset's map, bound to that tileset, and captures it on the Mode 7 layer; a map write
    /// through the room's enemy services reaches the next NMI's map while the published one keeps its tiles.
    /// </summary>
    internal static void VerifyPublishedCeresMode7Map()
    {
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.CeresRidley, new Position16<RoomPixels>(0, 0));
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        AffineFloor first = Published(runtime);
        // The room's tileset characters are bound (#1350); Mode 7 reads its map from the low bytes of the first $4000
        // words of the image they compose.
        AssertTrue(first.Cells.SequenceEqual(AffineFloor.Capture(runtime.ComposedVram(), first.Tiles).Cells),
            "the published map is the tileset map the room's character upload carries");
        AssertEqual(runtime.Room.LoadedScope.Assets.Tileset, (runtime.Room.LoadedScope.FloorMap ?? throw new InvalidOperationException("Ceres shows no Mode 7 map.")).Tileset,
            "the map is bound to the room's tileset");

        byte[] frame = [0x11, 0x22, 0x33, 0x44];
        runtime.Enemies.Room.Services.DrawMode7Map(CeresDoorVisualRomData.Mode7DestinationWord, frame);
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        AffineFloor second = Published(runtime);
        AssertTrue(second.Cells.Slice(CeresDoorVisualRomData.Mode7DestinationWord, frame.Length).Select(cell => (byte)cell.Tile).SequenceEqual(frame),
            "a map write reaches the next NMI's map");
        AssertTrue(!first.Cells.Slice(CeresDoorVisualRomData.Mode7DestinationWord, frame.Length).Select(cell => (byte)cell.Tile).SequenceEqual(frame),
            "the earlier published map keeps its tiles");

        var getaway = runtime.Enemies.RidleyEncounter.CeresRidley ?? throw new InvalidOperationException("The Ceres Ridley room has a getaway state.");
        getaway.Mode7Active = true;
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        LayeredScene captured = GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame());
        var mode7 = (Mode7GameplayRenderLayer)captured.Layers[0];
        AssertTrue(mode7.Map.Cells.SequenceEqual(second.Cells), "the Mode 7 layer draws the published map");
        Console.WriteLine("  Ceres Mode 7: the tileset map publishes, map writes reach the next NMI, and the Mode 7 layer draws it.");
    }

    /// <summary>The Mode 7 map the runtime's last NMI published.</summary>
    private static AffineFloor Published(SuperMetroidRuntime runtime) =>
        (runtime.Display.Latched.Rooms ?? throw new InvalidOperationException("A loaded room publishes its backgrounds.")).Mode7
            ?? throw new InvalidOperationException("A Ceres room publishes its Mode 7 map.");

    /// <summary>Runs the Ceres Mode 7 map checks.</summary>
    [VerificationCommand("--ceres-mode7-map")]
    internal static int CeresMode7MapCommand()
    {
        Suite(nameof(VerifyPublishedCeresMode7Map), () => VerifyPublishedCeresMode7Map());
        return 0;
    }
}
