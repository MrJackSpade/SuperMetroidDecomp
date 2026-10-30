using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Areas.Ceres;

/// <summary>Checks the turning floor a Ceres room publishes (#1349, #1467): its tileset's installed floor and the pictures drawn over it.</summary>
internal static class CeresMode7MapVerification
{
    /// <summary>
    /// A Ceres room publishes its tileset's installed floor, whose pixels are the cartridge's map over the characters the
    /// room's upload carries, and captures it on the floor layer; a picture drawn through the room's enemy services reaches
    /// the next NMI's floor while the published one keeps what it showed.
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
        // The floor's map is the low bytes of the first $4000 words the room's character upload composes, its tiles'
        // pixels their high bytes; the installed floor pictures exactly them.
        FloorMemory memory = AffineFloorConstants.Split(runtime.ComposedVram());
        byte[] map = memory.Map, pixels = memory.Pixels;
        SpriteImage image = first.Image.Sheet;
        int mismatches = 0;
        for (int y = 0; y < AffineFloor.SidePixels; y++)
        {
            for (int x = 0; x < AffineFloor.SidePixels; x++)
            {
                int tile = map[y / 8 * (AffineFloor.SidePixels / 8) + x / 8];
                int expected = pixels[tile * 64 + y % 8 * 8 + x % 8];
                if (image.Inks[(first.Image.Corner.Y + y) * image.Size.X + first.Image.Corner.X + x].PaletteIndex != expected)
                    mismatches++;
            }
        }
        AssertEqual(0, mismatches, "the published floor pictures the tileset map the room's character upload carries");
        AssertEqual(runtime.Room.LoadedScope.Assets.Tileset, (runtime.Room.LoadedScope.Floor ?? throw new InvalidOperationException("Ceres shows no floor.")).Sheet.Tileset,
            "the floor is the room's tileset's");
        AssertTrue(first.DrawnOver.IsEmpty, "nothing is drawn over the floor a room starts with");

        string platform = CeresFloorAnimation.PictureOf(CeresFloorAnimationFrame.ElevatorLight);
        runtime.Enemies.Room.Services.DrawFloor(platform);
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        AffineFloor second = Published(runtime);
        AssertTrue(second.DrawnOver.Length == 1 && second.DrawnOver[0].Equals(
                (runtime.Room.LoadedScope.Floor ?? throw new InvalidOperationException("Ceres shows no floor.")).Sheet.Picture(platform)),
            "a drawn picture reaches the next NMI's floor");
        AssertTrue(first.DrawnOver.IsEmpty, "the earlier published floor keeps what it showed");

        var getaway = runtime.Enemies.RidleyEncounter.CeresRidley ?? throw new InvalidOperationException("The Ceres Ridley room has a getaway state.");
        getaway.GetawayActive = true;
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        LayeredScene captured = GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame());
        var mode7 = (TurningRoomLayer)captured.Layers[0];
        AssertTrue(mode7.Map.Equals(second), "the floor layer draws the published floor");
        Console.WriteLine("  Ceres floor: the tileset floor publishes, drawn pictures reach the next NMI, and the floor layer draws it.");
    }

    /// <summary>The floor the runtime's last NMI published.</summary>
    private static AffineFloor Published(SuperMetroidRuntime runtime) =>
        (runtime.Display.Latched.Rooms ?? throw new InvalidOperationException("A loaded room publishes its backgrounds.")).Floor
            ?? throw new InvalidOperationException("A Ceres room publishes its floor.");

    /// <summary>Runs the Ceres floor checks.</summary>
    [VerificationCommand("--ceres-mode7-map")]
    internal static int CeresMode7MapCommand()
    {
        Suite(nameof(VerifyPublishedCeresMode7Map), () => VerifyPublishedCeresMode7Map());
        return 0;
    }
}
