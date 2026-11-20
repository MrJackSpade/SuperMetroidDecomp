using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Verification.Harness;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.World.RoomStates;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

using SuperMetroid.AssetExtraction.World.RoomStates;

namespace SuperMetroid.Verification.Rooms.Artwork;

/// <summary>
/// Checks the turning floors (#1649): only a room state that shows a turning floor has one, installed for its tileset and
/// bound when the room loads; the floor's picture is the whole floor (#2104); and the installed Ceres Ridley floor pictures
/// the cartridge's own floor data.
/// </summary>
internal static class RoomFloorSheetsVerification
{
    /// <summary>Cells along each side of the floor.</summary>
    private const int Side = 128;

    /// <summary>Pixels along a tile's side.</summary>
    private const int TilePixels = 8;

    /// <summary>Pixels of one tile.</summary>
    private const int TileBytes = TilePixels * TilePixels;

    /// <summary>The bytes of the floor's memory: one word per cell, its low byte the cell's tile and its high byte one tile pixel.</summary>
    private const int FloorMemoryBytes = Side * Side * sizeof(ushort);

    /// <summary>
    /// The turning floor each retail state authors (#2287): a state that runs the elevator shaft's room main shows the
    /// shaft's floor, a state whose enemies include Ceres Ridley shows his getaway's floor, and a state that runs neither,
    /// or runs the getaway's room main without him (his arena after his escape), shows none. The retail states that show
    /// one load only the shaft's two tilesets and Ridley's arena's first, so only those install a floor (the disassembly's
    /// tileset table and room states, $8F:DF45 and $8F:E0B5).
    /// </summary>
    internal static void VerifyFloorOnlyWhereShown()
    {
        foreach (RoomStateSettings state in RetailRooms.All.SelectMany(RoomStateLists.GetStates))
        {
            RoomTurningFloor shown = state.MainBehavior == RoomMainBehavior.CeresElevatorShaft ? RoomTurningFloor.ElevatorShaft
                : ReferenceEquals(state.EnemyPopulation, RoomEnemyPopulationDefinitions.CeresRidleyDefault) ? RoomTurningFloor.RidleyGetaway
                : RoomTurningFloor.None;
            AssertEqual(shown, state.TurningFloor,
                $"room state {state.Name} authors the turning floor its main and its enemies show");
        }

        RoomGraphicsSet[] floored = [.. RoomBlockUploads.FloorTilesets().Select(tileset => tileset.GraphicsSet).Order()];
        AssertTrue(floored.SequenceEqual([RoomGraphicsSet.BlueCeresElevator, RoomGraphicsSet.WhiteCeresElevator, RoomGraphicsSet.BlueCeresRidley]),
            $"only the shaft's tilesets and Ridley's arena's first install a turning floor, not {string.Join(", ", floored)}");
        Console.WriteLine("  Turning floors: only the shaft and Ridley's getaway show one, and only their tilesets install one.");
    }

    /// <summary>
    /// A synthetic floor memory whose cells show one tile with visible pixels at two places composes a floor picture of the
    /// whole floor (#2104), placed at the floor's top-left corner, each pixel the tile pixel its cell shows and colour zero
    /// everywhere else; a floor showing nothing anywhere is refused.
    /// </summary>
    internal static void VerifyFloorPicturesWhatShows()
    {
        const int Shown = 1, Clear = 2;
        var words = new byte[FloorMemoryBytes];
        // Tile 1 shows a diagonal of colour 5; tile 2's pixels are all colour zero.
        for (int pixel = 0; pixel < TilePixels; pixel++)
            words[(Shown * TileBytes + pixel * TilePixels + pixel) * sizeof(ushort) + 1] = 5;
        words[(3 * Side + 5) * sizeof(ushort)] = Shown;
        words[(4 * Side + 7) * sizeof(ushort)] = Shown;
        words[(10 * Side + 20) * sizeof(ushort)] = Clear;
        var colours = new RgbColor[ImageColourGroup.ColourCountOf(SpriteImageColours.FullPalette)];
        colours[5] = RgbColor.FromWord(0x1234);
        ComposedSpriteSheet sheet = RoomFloorSheetComposer.ComposeFloor(words, colours);
        SpriteSheetFrameDocument floor = sheet.Manifest.Frames[RoomFloorSheet.FloorFrameName];
        AssertEqual(new Extent16<ImagePixels>(Side * TilePixels, Side * TilePixels), floor.Size, "the floor's picture is the whole floor");
        AssertEqual(Offset16<ImagePixels>.Zero, floor.Origin, "the floor's picture lies at the floor's top-left corner");
        for (int y = 0; y < floor.Size.Y; y++)
        {
            for (int x = 0; x < floor.Size.X; x++)
            {
                int column = x / TilePixels, row = y / TilePixels;
                bool showsTile = (column, row) is (5, 3) or (7, 4);
                int expected = showsTile && x % TilePixels == y % TilePixels ? 5 : 0;
                int painted = sheet.PaletteIndices[(floor.Corner.Y + y) * sheet.Size.X + floor.Corner.X + x];
                if (painted != expected)
                    AssertTrue(false, $"floor picture pixel ({x}, {y}) paints index {painted} where its cell's tile names {expected}");
            }
        }
        bool refused = false;
        try
        {
            _ = RoomFloorSheetComposer.ComposeFloor(new byte[FloorMemoryBytes], colours);
        }
        catch (InvalidDataException)
        {
            refused = true;
        }
        AssertTrue(refused, "a floor no cell of which shows anything is refused");
        Console.WriteLine("  Turning floors: a floor's picture is the whole floor, each pixel its cell's.");
    }

    /// <summary>
    /// A room whose state shows no turning floor binds none, and its tileset installs none: the Ceres hall before the
    /// elevator has no floor to draw over; the elevator shaft binds its tileset's.
    /// </summary>
    internal static void VerifyRoomBindsNoFloorWhereNoneShows()
    {
        SuperMetroidRuntime hall = LoadRoom(RoomDefinitions.FallingTile);
        AssertTrue(hall.Room.LoadedScope.Floor is null, "the Ceres hall before the elevator binds no turning floor");
        AssertTrue(!hall.Room.LoadedScope.Assets.Pictures.Floors.ContainsKey(hall.Room.LoadedScope.Assets.Tileset),
            "the hall's tileset installs no turning floor");

        SuperMetroidRuntime shaft = LoadRoom(RoomDefinitions.CeresElevator);
        RoomFloor floor = shaft.Room.LoadedScope.Floor ?? throw new InvalidOperationException("The elevator shaft binds no turning floor.");
        AssertEqual(shaft.Room.LoadedScope.Assets.Tileset, floor.Sheet.Tileset, "the shaft binds its tileset's floor");
        Console.WriteLine("  Turning floors: a room that shows none binds none; the shaft binds its tileset's.");
    }

    /// <summary>
    /// The installed Ceres Ridley floor pictures the cartridge's floor data (#1649): the tileset's characters as the cartridge
    /// stores them, which its rooms upload as the floor's memory, give every cell's tile and every tile's pixels; the floor's
    /// picture is the whole floor (#2104), and each of its pixels is the pixel the cartridge's cell shows there.
    /// </summary>
    internal static void VerifyRidleyFloorMatchesCartridge()
    {
        SuperMetroidRuntime runtime = LoadRoom(RoomDefinitions.CeresRidley);
        TilesetDefinition tileset = runtime.Room.LoadedScope.Assets.Tileset;
        SpriteSheetFrame image = (runtime.Room.LoadedScope.Floor ?? throw new InvalidOperationException("Ceres Ridley's arena binds no turning floor."))
            .Sheet.Floor.Image;
        IImportCartridgeSource cartridge = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        byte[] memory = RomDataReader.Decompress(cartridge, RoomTilesetSources.Read(cartridge).Characters[tileset.Characters]);
        AssertTrue(memory.Length >= FloorMemoryBytes, $"the cartridge's {tileset} characters fill the floor's memory ({memory.Length} bytes)");

        AssertEqual(Offset16<ImagePixels>.Zero, image.Origin, "the installed floor lies at the floor's top-left corner");
        AssertEqual(new Extent16<ImagePixels>(Side * TilePixels, Side * TilePixels), image.Size, "the installed floor is the whole floor");

        SpriteImage sheet = image.Sheet;
        int mismatches = 0;
        for (int y = 0; y < image.Size.Y; y++)
        {
            for (int x = 0; x < image.Size.X; x++)
            {
                int tile = memory[(y / TilePixels * Side + x / TilePixels) * sizeof(ushort)];
                int expected = memory[(tile * TileBytes + y % TilePixels * TilePixels + x % TilePixels) * sizeof(ushort) + 1];
                if (InstalledPictureColoursConstants.ComposedIndexAt(sheet, (image.Corner.Y + y) * sheet.Size.X + image.Corner.X + x) != expected)
                    mismatches++;
            }
        }
        AssertEqual(0, mismatches, "every pixel of the installed Ceres Ridley floor is the pixel the cartridge's cell shows there");
        Console.WriteLine($"  Turning floors: {RoomFloorSheet.NameOf(tileset)} pictures the cartridge's whole {image.Size.X}x{image.Size.Y} floor.");
    }

    /// <summary>A retail runtime with <paramref name="room"/> loaded.</summary>
    private static SuperMetroidRuntime LoadRoom(RoomDefinition room)
    {
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.NewGame);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus(runtime.Room.Loaded);
        runtime.LoadCartridgeRoomForDebug(room);
        return runtime;
    }

    /// <summary>Runs the turning floor checks.</summary>
    /// <summary>
    /// The elevator shaft's turning floor repeats beyond its edges, as the cartridge's Mode 7 settings 0 (never written by
    /// the shaft's door, `$8F:E4E0`) do, so a tilted shaft shows the far wall's columns where the floor runs out; Ceres
    /// Ridley's getaway floor shows nothing beyond its edges, as its settings `$80` (`$A6:AA54`) do (#1653).
    /// </summary>
    internal static void VerifyFloorEdges()
    {
        PerspectiveFloor shaft = CeresPerspectiveFloors.ElevatorShaft(new TurningFloorTransform(0.25, default), default);
        AssertEqual(FloorOverflowMode.Wrap, shaft.Transform.Outside, "the elevator shaft's floor repeats beyond its edges");
        PerspectiveFloor getaway = CeresPerspectiveFloors.RidleyGetaway(FloorView.Turned(default, FloorZoom.OwnSize, default, default), []);
        AssertEqual(FloorOverflowMode.Transparent, getaway.Transform.Outside, "Ceres Ridley's getaway floor shows nothing beyond its edges");
        Console.WriteLine("Ceres turning floors: the shaft's repeats beyond its edges; Ridley's getaway floor shows nothing there.");
    }

    [VerificationCommand("--room-floors")]
    internal static int RoomFloorsCommand()
    {
        Suite(nameof(VerifyFloorOnlyWhereShown), VerifyFloorOnlyWhereShown);
        Suite(nameof(VerifyFloorPicturesWhatShows), VerifyFloorPicturesWhatShows);
        Suite(nameof(VerifyRoomBindsNoFloorWhereNoneShows), VerifyRoomBindsNoFloorWhereNoneShows);
        Suite(nameof(VerifyRidleyFloorMatchesCartridge), VerifyRidleyFloorMatchesCartridge);
        Suite(nameof(VerifyFloorEdges), VerifyFloorEdges);
        return 0;
    }
}
