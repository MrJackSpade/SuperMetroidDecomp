using SuperMetroid.Core.Gameplay;
using SuperMetroid.Verification.Harness;
using SuperMetroid.AssetExtraction.Artwork;
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
        runtime.HudSetup.InitializeHud(HudSnapshot.NewGame);
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.CeresRidley, new Position16<RoomPixels>(0, 0));
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        AffineFloor first = Published(runtime);
        // The floor's map is the low bytes of the first $4000 words the room's character upload composes, its tiles'
        // pixels their high bytes; the installed floor pictures exactly them.
        FloorMemory memory = AffineFloorConstants.Split(runtime.ComposedVram());
        byte[] map = memory.Map, pixels = memory.Pixels;
        SpriteImage image = first.Image.Sheet;
        // The installed image is the whole floor (#2104), and the floor's extent is the image's.
        AssertEqual(new Extent16<ImagePixels>(FloorSheetComposer.SidePixels, FloorSheetComposer.SidePixels), first.Extent,
            "Ceres Ridley's floor is its whole floor, its image's extent");
        int left = -first.Image.Origin.X, top = -first.Image.Origin.Y;
        int mismatches = 0;
        for (int y = 0; y < FloorSheetComposer.SidePixels; y++)
        {
            for (int x = 0; x < FloorSheetComposer.SidePixels; x++)
            {
                int tile = map[y / 8 * (FloorSheetComposer.SidePixels / 8) + x / 8];
                int expected = pixels[tile * 64 + y % 8 * 8 + x % 8];
                bool within = x >= left && x < left + first.Image.Size.X && y >= top && y < top + first.Image.Size.Y;
                int painted = within
                    ? InstalledPictureColoursConstants.ComposedIndexAt(image, (first.Image.Corner.Y + y - top) * image.Size.X + first.Image.Corner.X + x - left)
                    : 0;
                if (painted != expected)
                    mismatches++;
            }
        }
        AssertEqual(0, mismatches, "the published floor pictures the tileset map the room's character upload carries");
        AssertEqual(runtime.Room.LoadedScope.Assets.Tileset, (runtime.Room.LoadedScope.Floor ?? throw new InvalidOperationException("Ceres shows no floor.")).Sheet.Tileset,
            "the floor is the room's tileset's");
        AssertTrue(first.DrawnOver.IsEmpty, "nothing is drawn over the floor a room starts with");

        CeresFloorAnimationFrame platform = CeresFloorAnimationFrame.ElevatorLight;
        (runtime.Room.LoadedScope.Floor ?? throw new InvalidOperationException("Ceres shows no floor.")).Show(platform);
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        AffineFloor second = Published(runtime);
        AssertTrue(second.DrawnOver.Length == 1 && second.DrawnOver[0].Equals(
                (runtime.Room.LoadedScope.Floor ?? throw new InvalidOperationException("Ceres shows no floor.")).Sheet.Picture(platform)),
            "a drawn picture reaches the next NMI's floor");
        AssertTrue(first.DrawnOver.IsEmpty, "the earlier published floor keeps what it showed");

        // The station's escape owns the getaway's floor (#2228).
        runtime.Enemies.CeresEscape.Floor.Active = true;
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        LayeredScene captured = GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame());
        var mode7 = (TurningRoomLayer)captured.Layers[0];
        AssertTrue(mode7.Map.Equals(second), "the floor layer draws the published floor");
        Console.WriteLine("  Ceres floor: the tileset floor publishes, drawn pictures reach the next NMI, and the floor layer draws it.");
    }

    /// <summary>The floor the runtime's last NMI published.</summary>
    private static AffineFloor Published(SuperMetroidRuntime runtime) =>
        runtime.Display.LatchedOrdinaryRoom.Rooms.Floor ?? throw new InvalidOperationException("A Ceres room publishes its floor.");

    /// <summary>Runs the Ceres floor checks.</summary>
    [VerificationCommand("--ceres-mode7-map")]
    internal static int CeresMode7MapCommand()
    {
        Suite(nameof(VerifyPublishedCeresMode7Map), () => VerifyPublishedCeresMode7Map());
        return 0;
    }
}
