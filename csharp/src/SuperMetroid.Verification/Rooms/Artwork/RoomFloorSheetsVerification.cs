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

namespace SuperMetroid.Verification.Rooms.Artwork;

/// <summary>
/// Checks the turning floors (#1649): only a room state that shows a turning floor has one, installed for its tileset and
/// bound when the room loads; the floor's picture holds exactly the part of the floor that shows anything, placed where it
/// lies on the whole floor; and the installed Ceres Ridley floor pictures the cartridge's own floor data.
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
    /// Synthetic room states: a state that runs the elevator shaft's room main shows the shaft's floor, a state whose enemies
    /// include Ceres Ridley shows his getaway's floor, and a state that runs neither, or runs the getaway's room main
    /// without him (his arena after his escape), shows none. The retail states that show one load only the shaft's two
    /// tilesets and Ridley's arena's first, so only those install a floor (the disassembly's tileset table and room states,
    /// $8F:DF45 and $8F:E0B5).
    /// </summary>
    internal static void VerifyFloorOnlyWhereShown()
    {
        RoomStateSettings plain = CeresRoomStateDefinitions.FallingTileDefault;
        AssertEqual(RoomTurningFloor.None, RoomTurningFloors.Of(plain), "a state that runs neither the shaft nor Ridley shows no turning floor");
        AssertEqual(RoomTurningFloor.ElevatorShaft, RoomTurningFloors.Of(plain with { MainCallback = RoomMainCallback.CeresElevatorShaft }),
            "a state that runs the elevator shaft's room main shows the shaft's floor");
        AssertEqual(RoomTurningFloor.RidleyGetaway, RoomTurningFloors.Of(plain with { EnemyPopulation = RoomEnemyPopulationDefinitions.CeresRidleyDefault }),
            "a state whose enemies include Ceres Ridley shows his getaway's floor");
        AssertEqual(RoomTurningFloor.None, RoomTurningFloors.Of(plain with { MainCallback = RoomMainCallback.HandleCeresRidleyGetawayCutscene }),
            "the getaway's room main without Ridley shows no turning floor");

        RoomGraphicsSet[] floored = [.. RoomBlockUploads.FloorTilesets().Select(tileset => tileset.GraphicsSet).Order()];
        AssertTrue(floored.SequenceEqual([RoomGraphicsSet.BlueCeresElevator, RoomGraphicsSet.WhiteCeresElevator, RoomGraphicsSet.BlueCeresRidley]),
            $"only the shaft's tilesets and Ridley's arena's first install a turning floor, not {string.Join(", ", floored)}");
        Console.WriteLine("  Turning floors: only the shaft and Ridley's getaway show one, and only their tilesets install one.");
    }

    /// <summary>
    /// A synthetic floor memory whose cells show one tile with visible pixels at two places composes a floor picture of
    /// exactly the rectangle spanning them, placed by its origin where it lies on the whole floor, each pixel the tile pixel
    /// its cell shows; a cell showing a tile of colour zero alone does not widen it, and a floor showing nothing anywhere
    /// is refused.
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
        AssertEqual(new Extent16<ImagePixels>(3 * TilePixels, 2 * TilePixels), floor.Size, "the floor's picture spans exactly the cells that show anything");
        AssertEqual(Offset16<ImagePixels>.Of(-5 * TilePixels, -3 * TilePixels), floor.Origin, "the floor's picture lies where its cells lie on the whole floor");
        for (int y = 0; y < floor.Size.Y; y++)
        {
            for (int x = 0; x < floor.Size.X; x++)
            {
                int column = 5 + x / TilePixels, row = 3 + y / TilePixels;
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
        Console.WriteLine("  Turning floors: a floor's picture holds exactly the part of it that shows anything, in place.");
    }

    /// <summary>
    /// A room whose state shows no turning floor binds none, and its tileset installs none: the Ceres hall before the
    /// elevator has no floor to draw over; the elevator shaft binds its tileset's.
    /// </summary>
    internal static void VerifyRoomBindsNoFloorWhereNoneShows()
    {
        SuperMetroidRuntime hall = LoadRoom(RoomHeaderDefinitions.FallingTile);
        AssertTrue(hall.Room.LoadedScope.Floor is null, "the Ceres hall before the elevator binds no turning floor");
        AssertTrue(!hall.Room.LoadedScope.Assets.Pictures.Floors.ContainsKey(hall.Room.LoadedScope.Assets.Tileset),
            "the hall's tileset installs no turning floor");
        bool refused = false;
        try
        {
            hall.Enemies.Room.Services.ShowFloorFrame(CeresFloorAnimationFrame.ElevatorLight);
        }
        catch (InvalidOperationException)
        {
            refused = true;
        }
        AssertTrue(refused, "drawing over the floor of a room that shows none is refused");

        SuperMetroidRuntime shaft = LoadRoom(RoomHeaderDefinitions.CeresElev);
        RoomFloor floor = shaft.Room.LoadedScope.Floor ?? throw new InvalidOperationException("The elevator shaft binds no turning floor.");
        AssertEqual(shaft.Room.LoadedScope.Assets.Tileset, floor.Sheet.Tileset, "the shaft binds its tileset's floor");
        Console.WriteLine("  Turning floors: a room that shows none binds none; the shaft binds its tileset's.");
    }

    /// <summary>
    /// The installed Ceres Ridley floor pictures the cartridge's floor data (#1649): the tileset's characters as the cartridge
    /// stores them, which its rooms upload as the floor's memory, give every cell's tile and every tile's pixels; the floor's
    /// picture is exactly the rectangle of the cells whose tiles show a pixel, and each of its pixels is the pixel the
    /// cartridge's cell shows there.
    /// </summary>
    internal static void VerifyRidleyFloorMatchesCartridge()
    {
        SuperMetroidRuntime runtime = LoadRoom(RoomHeaderDefinitions.CeresRidley);
        TilesetDefinition tileset = runtime.Room.LoadedScope.Assets.Tileset;
        SpriteSheetFrame image = (runtime.Room.LoadedScope.Floor ?? throw new InvalidOperationException("Ceres Ridley's arena binds no turning floor."))
            .Sheet.Floor.Image;
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(
            CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
        byte[] memory = RomDataReader.Decompress(cartridge, RoomTilesetSources.Read(cartridge).Characters[tileset.Characters]);
        AssertTrue(memory.Length >= FloorMemoryBytes, $"the cartridge's {tileset} characters fill the floor's memory ({memory.Length} bytes)");

        // The rectangle of the cells whose tiles show a pixel, from the cartridge's data alone.
        int left = Side, top = Side, right = -1, bottom = -1;
        for (int cell = 0; cell < Side * Side; cell++)
        {
            int tile = memory[cell * sizeof(ushort)];
            bool shows = false;
            for (int pixel = 0; pixel < TileBytes && !shows; pixel++)
                shows = memory[(tile * TileBytes + pixel) * sizeof(ushort) + 1] != 0;
            if (!shows)
                continue;
            left = Math.Min(left, cell % Side);
            right = Math.Max(right, cell % Side);
            top = Math.Min(top, cell / Side);
            bottom = Math.Max(bottom, cell / Side);
        }
        AssertEqual(Offset16<ImagePixels>.Of(-left * TilePixels, -top * TilePixels), image.Origin,
            "the installed floor lies where the cartridge's first showing cells lie");
        AssertEqual(new Extent16<ImagePixels>(checked((ushort)((right - left + 1) * TilePixels)), checked((ushort)((bottom - top + 1) * TilePixels))), image.Size,
            "the installed floor spans exactly the cartridge's cells that show anything");

        SpriteImage sheet = image.Sheet;
        int mismatches = 0;
        for (int y = 0; y < image.Size.Y; y++)
        {
            for (int x = 0; x < image.Size.X; x++)
            {
                int floorX = left * TilePixels + x, floorY = top * TilePixels + y;
                int tile = memory[(floorY / TilePixels * Side + floorX / TilePixels) * sizeof(ushort)];
                int expected = memory[(tile * TileBytes + floorY % TilePixels * TilePixels + floorX % TilePixels) * sizeof(ushort) + 1];
                if (InstalledPictureColoursConstants.ComposedIndexAt(sheet, (image.Corner.Y + y) * sheet.Size.X + image.Corner.X + x) != expected)
                    mismatches++;
            }
        }
        AssertEqual(0, mismatches, "every pixel of the installed Ceres Ridley floor is the pixel the cartridge's cell shows there");
        Console.WriteLine($"  Turning floors: {RoomFloorSheet.NameOf(tileset)} pictures the cartridge's {image.Size.X}x{image.Size.Y} showing part of its floor.");
    }

    /// <summary>A retail runtime with <paramref name="room"/> loaded.</summary>
    private static SuperMetroidRuntime LoadRoom(RoomHeaderDefinition room)
    {
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
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
        PerspectiveFloor getaway = CeresPerspectiveFloors.RidleyGetaway(FloorView.Turned(default, 256, default, default), []);
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
