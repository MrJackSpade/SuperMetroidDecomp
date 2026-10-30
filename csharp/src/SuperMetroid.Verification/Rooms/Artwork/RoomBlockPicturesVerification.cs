using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.World;
using SuperMetroid.Core;
using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Escape;
using SuperMetroid.Core.Escape.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.World.RoomStates;
using SuperMetroid.Verification.Harness;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Rooms.Artwork;

/// <summary>
/// Checks rooms draw their blocks from the installed pre-coloured block sheets (#1392, #1466): each block the displayed
/// planes show is a picture that paints exactly what its tiles paint over the characters the cartridge would show, as the
/// room's appearance inputs change under animated tiles and collectibles.
/// </summary>
internal static class RoomBlockPicturesVerification
{
    /// <summary>Displayed frames each room is checked over: enough for every animated tile to change frame.</summary>
    private const int Frames = 40;

    /// <summary>Pixels along a character's side.</summary>
    private const int CharacterPixels = 8;

    /// <summary>VRAM words in one four-bit room tile character.</summary>
    private static readonly int TileCharacterWords = BgCharacterSet.Tiles.CharacterBytes() / sizeof(ushort);

    /// <summary>
    /// What each tileset's rooms show over their characters after loading, as the import pipeline replays it (#1466): the
    /// test's reference for which characters an animation paints and what the room's load leaves.
    /// </summary>
    private static readonly Lazy<IReadOnlyDictionary<TilesetDefinition, RoomBlockUploads.TilesetUploads>> TilesetUploads =
        new(() => RoomBlockUploads.ByTileset(RoomBlockSheetInputs.Load(SuperMetroid.TestSupport.RepositoryInstallation.RepositoryInstallation.Installation.ContentDirectory)));

    /// <summary>
    /// In the Landing Site, whose blocks show its load alone, in the Wasteland, whose spikes animate, in the Morph Ball
    /// room, whose item binds its graphics and writes its blocks, in the East Sand Hall, whose sand animates, and in the
    /// Wrecked Ship main shaft with its treadmills running, every displayed block's picture paints what its tiles paint
    /// over the live characters.
    /// </summary>
    internal static void VerifyRoomBlockPictures()
    {
        Check(RoomHeaderDefinitions.LandingSite, expectUploads: false);
        Check(RoomHeaderDefinitions.Wasteland, expectUploads: true);
        Check(RoomHeaderDefinitions.MorphBall, expectUploads: true);
        Check(RoomHeaderDefinitions.EastSandHall, expectUploads: true);
        // The Ceres Ridley arena's blocks read the OBJ character table's enemy tiles: the page its load composes is the
        // pictures' baseline, and only the getaway's looks, which show later in the encounter, change them (#1466).
        Check(RoomHeaderDefinitions.CeresRidley, expectUploads: false);
        // The treadmills run once Phantoon, the Wrecked Ship's boss, is defeated.
        Check(RoomHeaderDefinitions.WreckedShipMainShaft, expectUploads: true,
            RoomStateSelection.GetStates(RoomHeaderDefinitions.WreckedShipMainShaft)
                .Single(state => state.Fx?.Records.Any(record => record.AnimatedTileSlots != RoomFxEffectSlots.None) == true),
            runtime => runtime.System.SetBossBits((int)AreaId.WreckedShip, BossBits.AreaBoss));
    }

    /// <summary>
    /// In the Parlor, whose rock background shows its room's load, and in the Tourian statue room, whose statues' frames
    /// rebind the characters its background reads, BG2 draws the library background's image, and every pixel of its two
    /// pages paints what BG2's tilemap tiles paint over the live characters, frame after frame. Crateria Power Bombs runs
    /// the sky's main loop without its band HDMA, so BG2 shows its rock background's image, not the sky (#1403).
    /// </summary>
    internal static void VerifyRoomBackgroundImages()
    {
        CheckBackground(RoomHeaderDefinitions.Parlor, expectRebound: false);
        CheckBackground(RoomHeaderDefinitions.CrateriaPowerBombs, expectRebound: false);
        CheckBackground(RoomHeaderDefinitions.Statues, expectRebound: true);
        CheckSky(RoomHeaderDefinitions.LandingSite);
    }

    /// <summary>
    /// Loads sky room <paramref name="room"/> and checks BG2 draws the sky's rows from their image: every pixel of every row
    /// the sky shows paints what the row's tiles paint over the live characters.
    /// </summary>
    private static void CheckSky(RoomHeaderDefinition room)
    {
        SuperMetroidRuntime runtime = LoadRoom(room);
        runtime.StepFrame(0);
        var shown = runtime.Display.Latched.Rooms?.Sky
            ?? throw new InvalidOperationException($"{room.Name}: BG2 shows no sky image.");
        // The installed sky pages the import pictured; the runtime holds only their picture (#1466).
        SkyTilemap sky = SuperMetroid.TestSupport.RepositoryInstallation.RepositoryInstallation.Installation.LoadRoomSkyTilemaps()
            .Tilemap((runtime.Room.Scope?.ScrollingSky ?? throw new InvalidOperationException($"{room.Name} has no sky.")).Variant);
        ReadOnlySpan<byte> vram = runtime.ComposedVram();
        int rows = 0;
        for (int row = 0; row < SkyTilemap.Rows; row++)
        {
            if (!sky.RowShown(row))
                continue;
            rows++;
            for (int y = row * CharacterPixels; y < (row + 1) * CharacterPixels; y++)
            for (int x = 0; x < RoomBackgroundImage.PagePixels; x++)
            {
                var word = new SnesBgTilemapWord(sky.Tiles[row * SkyTilemap.Columns + x / CharacterPixels].PackedWord);
                AssertTrue(shown.RowsShown[row], $"{room.Name}: the sky shows row {row}");
                (SpriteSheetFrame picture, Position16<ImagePixels> pixel) = PictureAt(shown.Image, x, y);
                AssertPaintsTile($"{room.Name}'s sky at ({x}, {y})", vram, word, x % CharacterPixels, y % CharacterPixels, picture, pixel);
            }
        }
        AssertTrue(rows > 0, $"{room.Name}'s sky shows rows");
        Console.WriteLine($"Room background images: {room.Name}'s sky painted its {rows} shown rows from their image.");
    }

    /// <summary>
    /// The picture and its pixel a background image shows at (<paramref name="x"/>, <paramref name="y"/>) of its pages or
    /// rows laid one above the next: a rebound 16x16 cell's own picture, otherwise the image's.
    /// </summary>
    private static (SpriteSheetFrame Picture, Position16<ImagePixels> Pixel) PictureAt(RoomBackgroundImagePlane image, int x, int y)
    {
        const int cell = 16;
        return image.Cell(y / cell * (RoomBackgroundImage.PagePixels / cell) + x / cell) is { } picture
            ? (picture, new Position16<ImagePixels>((ushort)(x % cell), (ushort)(y % cell)))
            : (image.Picture, new Position16<ImagePixels>((ushort)x, (ushort)y));
    }

    /// <summary>
    /// Checks the pixel at (<paramref name="x"/>, <paramref name="y"/>) of <paramref name="word"/>'s tile, as <paramref name="vram"/>'s
    /// characters paint it, is <paramref name="picture"/>'s pixel at <paramref name="pixel"/>: its palette index, its tile
    /// priority and its standard colour.
    /// </summary>
    internal static void AssertPaintsTile(string context, ReadOnlySpan<byte> vram, SnesBgTilemapWord word, int x, int y,
        SpriteSheetFrame picture, Position16<ImagePixels> pixel)
    {
        int pixelRow = word.FlipVertically ? CharacterPixels - 1 - y : y;
        int bit = 7 - (word.FlipHorizontally ? CharacterPixels - 1 - x : x);
        int address = word.CharacterIndex * TileCharacterWords * sizeof(ushort) + pixelRow * 2;
        int colour = (vram[address] >> bit & 1) | (vram[address + 1] >> bit & 1) << 1
            | (vram[address + 16] >> bit & 1) << 2 | (vram[address + 17] >> bit & 1) << 3;
        int expected = colour == 0 ? 0 : word.PaletteIndex * 16 + colour;
        SpriteImage sheet = picture.Sheet;
        int at = (picture.Corner.Y + pixel.Y) * sheet.Size.X + picture.Corner.X + pixel.X;
        AssertTrue(sheet.Inks[at].PaletteIndex == expected && (colour == 0 || ((sheet.Inks[at].Priority != 0) == word.HasPriority
                && sheet.Pixels[at] == sheet.StandardColours[expected].ToRgbaPixel())),
            $"{context}: the picture paints index {sheet.Inks[at].PaletteIndex} where tile ${word.Raw:X4} paints {expected}");
    }

    /// <summary>A retail runtime with <paramref name="room"/> loaded and Samus held still.</summary>
    private static SuperMetroidRuntime LoadRoom(RoomHeaderDefinition room) => LoadRoom(room, HudSnapshot.CeresDebug);

    /// <summary>A retail runtime with <paramref name="room"/> loaded, the HUD showing <paramref name="hud"/> and Samus held still.</summary>
    private static SuperMetroidRuntime LoadRoom(RoomHeaderDefinition room, HudSnapshot hud)
    {
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(hud);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(room);
        runtime.Player.IntroducedSamus.Control.LockHandlers();
        return runtime;
    }

    /// <summary>
    /// Confirms each boss's body sheet (#1466) and the room's tile word sheet (#1392) picture what they draw: in each boss's
    /// room as loaded, every part of every frame the boss draws paints, at each cell it covers, what the frame's tile word
    /// paints over the room's characters; and the whole Zebes escape warning typed over Mother Brain's room draws from the
    /// tile word sheet the same way.
    /// </summary>
    internal static void VerifyTileWordPictures()
    {
        CheckBody(RoomHeaderDefinitions.Kraid, BossBodyOwner.Kraid);
        CheckBody(RoomHeaderDefinitions.Phantoon, BossBodyOwner.Phantoon);
        CheckBody(RoomHeaderDefinitions.Draygon, BossBodyOwner.Draygon);
        CheckBody(RoomHeaderDefinitions.Crocomire, BossBodyOwner.Crocomire);
        CheckBody(RoomHeaderDefinitions.MotherBrain, BossBodyOwner.MotherBrain);
        CheckTypedWarning(RoomHeaderDefinitions.MotherBrain);
    }

    /// <summary>Loads <paramref name="room"/>, types the whole Zebes escape warning over BG2 and checks each glyph's picture.</summary>
    private static void CheckTypedWarning(RoomHeaderDefinition room)
    {
        SuperMetroidRuntime runtime = LoadRoom(room);
        runtime.StepFrame(0);
        WrittenPictureSheet sheet = (runtime.Room.Scope?.Bg2OverlayPictures ?? throw new InvalidOperationException($"{room.Name} has no written pictures.")).Sheet;
        ReadOnlySpan<byte> vram = runtime.ComposedVram();
        var typewriter = new EscapeTypewriterState((runtime.BossArtwork.EscapeTypewriterPresentation
            ?? throw new InvalidOperationException("No escape typewriter.")).Get(EscapeTypewriterProgramId.Zebes), EscapeTypewriterDefinitions.ZebesTileBase);
        // The typewriter types each glyph's picture by name (#1466); the overlay is checked against the tiles it typed.
        RoomWrittenOverlay overlay = RoomWrittenOverlay.Empty;
        var typedWords = new int[RoomWrittenOverlay.CellCount];
        Array.Fill(typedWords, RoomWrittenOverlay.Unwritten);
        while (!typewriter.Step(out EscapeTypewriterGlyph? glyph))
        {
            if (glyph is { } typed)
            {
                overlay = overlay.With(new WrittenPicture(typed.Picture, typed.Cell));
                typedWords[typed.Cell] = typed.Tile.PackedWord;
            }
        }
        ImmutableArray<BgTilePicture?> pictures = new WrittenPictureResolver(sheet).Resolve(overlay, AppearanceOf(runtime, RoomScopeOf(runtime).ArtworkFrames));
        int glyphs = 0;
        for (int index = 0; index < RoomWrittenOverlay.CellCount; index++)
        {
            if (typedWords[index] == RoomWrittenOverlay.Unwritten)
            {
                AssertTrue(pictures[index] is null, $"{sheet.Name}: nothing typed at word {index} shows nothing");
                continue;
            }
            BgTilePicture picture = pictures[index] ?? throw new InvalidOperationException($"{sheet.Name}: typed word {index} draws from its character.");
            var word = new SnesBgTilemapWord((ushort)typedWords[index]);
            for (int y = 0; y < CharacterPixels; y++)
            for (int x = 0; x < CharacterPixels; x++)
            {
                AssertPaintsTile($"{sheet.Name} typed word {index} pixel ({x}, {y})", vram, word, x, y, picture.Picture,
                    new Position16<ImagePixels>((ushort)(picture.Corner.X + x), (ushort)(picture.Corner.Y + y)));
            }
            glyphs++;
        }
        AssertTrue(glyphs > 0, "The Zebes escape warning types glyphs");
        Console.WriteLine($"Tile word pictures: {sheet.Name} pictured the {glyphs} glyphs of the Zebes escape warning.");
    }

    /// <summary>
    /// Loads boss room <paramref name="room"/> and checks every still picture of <paramref name="owner"/>'s body sheet paints,
    /// at each cell its part covers, the tile word its frame writes there over the room's characters as it loads.
    /// </summary>
    private static void CheckBody(RoomHeaderDefinition room, BossBodyOwner owner)
    {
        SuperMetroidRuntime runtime = LoadRoom(room);
        runtime.StepFrame(0);
        BossBodySheet sheet = (runtime.BossArtwork.BodySheets ?? throw new InvalidOperationException("No boss body sheets are installed.")).Of(owner);
        BossArtworkCatalog bosses = runtime.BossArtwork.Artwork ?? throw new InvalidOperationException("No boss artwork is installed.");
        ReadOnlySpan<byte> vram = runtime.ComposedVram();
        int parts = 0;
        foreach (BossBodyFrameSource frame in BossBodySheetComposer.Frames(bosses.Sources, owner))
        {
            foreach (BossBodyPart part in sheet.Frame(frame.Name).Parts)
            {
                foreach ((int cell, BackgroundTile tile) in frame.Cells)
                {
                    int row = cell / BossBodyDefinitions.ColumnCount, column = cell % BossBodyDefinitions.ColumnCount;
                    if (!part.Footprint.Covers(row, column))
                        continue;
                    var word = new SnesBgTilemapWord(tile.PackedWord);
                    for (int y = 0; y < CharacterPixels; y++)
                    for (int x = 0; x < CharacterPixels; x++)
                    {
                        AssertPaintsTile($"{owner} {part.Name} cell {cell} pixel ({x}, {y})", vram, word, x, y, part.Picture,
                            new Position16<ImagePixels>((ushort)(column * CharacterPixels + x), (ushort)((row - part.Top) * CharacterPixels + y)));
                    }
                }
                parts++;
            }
        }
        AssertTrue(parts > 0, $"{room.Name}'s boss draws frames");
        Console.WriteLine($"Boss body pictures: {owner}'s sheet pictured {parts} parts over {room.Name} as it loads.");
    }

    /// <summary>
    /// Confirms BG3 draws from the tileset's BG3 word sheet (#1392): in a room of each effect whose page BG3 shows, and in
    /// Kraid's room, whose library background moves the HUD's characters, the latched BG3 plane draws from pictures, and
    /// every word of it under a stocked HUD resolves to a picture painting what its two-bit character paints at the HUD
    /// character base BG3 shows, colour zero's row included. The words are resolved against the characters as the check
    /// reads them, since an effect can bind its next frame after the latch.
    /// </summary>
    internal static void VerifyBg3Pictures()
    {
        var stocked = new HudSnapshot(1234, 1499, 123, 230, 45, 50, 6, 50,
            Enum.GetValues<SamusEquipmentFlags>().Aggregate((all, item) => all | item), HudItem.Missile, 300, ReserveTankMode.Auto);
        var rooms = new List<RoomHeaderDefinition> { RoomHeaderDefinitions.Kraid };
        foreach (RoomFxType effect in new[] { RoomFxType.Lava, RoomFxType.Acid, RoomFxType.Water, RoomFxType.Rain, RoomFxType.Fog, RoomFxType.Spores })
        {
            rooms.Add(RoomHeaderDefinitions.All.First(room => RoomStateSelection.GetStates(room)[0].Fx?.Records
                .Any(record => record.Type == effect) == true));
        }
        foreach (RoomHeaderDefinition room in rooms)
        {
            SuperMetroidRuntime runtime = LoadRoom(room, stocked);
            runtime.StepFrame(0);
            GameplayDisplayLatch latched = runtime.Display.Latched;
            Layer3Plane plane = latched.Layer3 ?? throw new InvalidOperationException($"{room.Name} latches no BG3 plane.");
            // The plane names its pictures (#1466); the installed pages spell the words the cartridge writes for them.
            ushort[] planeWords = plane.WordsIn(InstalledFixtures.Maps.RoomFxLayer3Tilemaps);
            AssertTrue(latched.Rooms?.Bg3 is not null, $"{room.Name}: BG3 draws from its pictures");
            ReadOnlySpan<byte> vram = runtime.ComposedVram();
            BgCharacterSet layer3 = BgCharacterSet.Layer3;
            int characterBase = BgCharacterWordsConstants.WordOf(layer3, 0);
            ArtworkFrameSelections frames = RoomScopeOf(runtime).ArtworkFrames;
            IReadOnlyList<AppearanceOption> effects = UploadsOf(runtime).Bg3Options;
            RoomFxAnimatedTileAtlas animatedTiles = runtime.Artwork.MapPresentation?.RoomFxAnimatedTiles
                ?? throw new InvalidOperationException($"{room.Name}: the runtime has no installed animated tiles.");
            ImmutableArray<BgTilePicture?> pictures = new WrittenPictureResolver((runtime.Room.Scope
                ?? throw new InvalidOperationException($"{room.Name} has no room scope.")).Bg3Pictures.Sheet).Resolve(plane, AppearanceOf(runtime, frames));
            for (int index = 0; index < Layer3Plane.Columns * Layer3Plane.Rows; index++)
            {
                BgTilePicture picture = pictures[index] ?? throw new InvalidOperationException($"{room.Name}: BG3 word {index} draws from its character.");
                var word = new SnesBgTilemapWord(planeWords[index]);
                SpriteImage sheet = picture.Picture.Sheet;
                for (int y = 0; y < CharacterPixels; y++)
                for (int x = 0; x < CharacterPixels; x++)
                {
                    int pixelRow = word.FlipVertically ? CharacterPixels - 1 - y : y;
                    int bit = 7 - (word.FlipHorizontally ? CharacterPixels - 1 - x : x);
                    // A character an effect's animation paints shows the frame it selected (#1406), which VRAM never holds.
                    ReadOnlySpan<byte> bytes = CharacterBytes(effects, frames, layer3, word.CharacterIndex, animatedTiles,
                        vram[(characterBase * sizeof(ushort))..], out _);
                    int address = pixelRow * 2;
                    int colour = (bytes[address] >> bit & 1) | (bytes[address + 1] >> bit & 1) << 1;
                    int expected = word.PaletteIndex * 4 + colour;
                    int at = (picture.Picture.Corner.Y + picture.Corner.Y + y) * sheet.Size.X + picture.Picture.Corner.X + picture.Corner.X + x;
                    AssertTrue(sheet.Inks[at].PaletteIndex == expected && (sheet.Inks[at].Priority != 0) == word.HasPriority
                            && (colour == 0 ? sheet.Pixels[at].A == 0 : sheet.Pixels[at] == sheet.StandardColours[expected].ToRgbaPixel()),
                        $"{room.Name}: BG3 word {index} ${word.Raw:X4} pixel ({x}, {y}) paints index {sheet.Inks[at].PaletteIndex} where its character, "
                        + $"{runtime.BgArtwork.Showing(layer3, word.CharacterIndex, 1)}, paints {expected}");
                }
            }
            Console.WriteLine($"BG3 pictures: {room.Name}'s HUD and effect page painted from their pictures at base ${characterBase:X4}.");
        }
    }

    /// <summary>
    /// Confirms a Ceres room's Mode 7 map draws from its tileset's tile pictures (#1392): in the Ceres elevator room the
    /// latched map carries the pictures, every tile it names is pictured, and each pictured tile's pixels are the colours
    /// its characters' high bytes name.
    /// </summary>
    internal static void VerifyMode7TilePictures()
    {
        RoomHeaderDefinition room = RoomHeaderDefinitions.CeresElev;
        SuperMetroidRuntime runtime = LoadRoom(room);
        runtime.StepFrame(0);
        AssertTrue(runtime.Display.Latched.Rooms?.Floor is not null, $"{room.Name} shows its Mode 7 map");
        FloorMap map = runtime.Room.LoadedScope.FloorMap ?? throw new InvalidOperationException($"{room.Name} has no Mode 7 map.");
        FloorTileSheet tiles = map.Tiles;
        AssertTrue(map.NamedTiles.All(tile => tiles.Pictured[tile]), $"{room.Name}'s Mode 7 map names only pictured tiles");
        ReadOnlySpan<byte> vram = runtime.ComposedVram();
        SpriteImage sheet = tiles.Picture.Sheet;
        int pictured = 0;
        for (int tile = 0; tile < FloorTileSheet.Tiles; tile++)
        {
            if (!tiles.Pictured[tile])
                continue;
            pictured++;
            Position16<ImagePixels> corner = FloorTileSheet.Corner(tile);
            for (int pixel = 0; pixel < FloorTileSheet.TileWords; pixel++)
            {
                int expected = vram[(tile * FloorTileSheet.TileWords + pixel) * 2 + 1];
                int at = (tiles.Picture.Corner.Y + corner.Y + pixel / 8) * sheet.Size.X + tiles.Picture.Corner.X + corner.X + pixel % 8;
                AssertTrue(sheet.Inks[at].PaletteIndex == expected && (expected == 0 || sheet.Pixels[at] == sheet.StandardColours[expected].ToRgbaPixel()),
                    $"{tiles.Name}: tile {tile} pixel {pixel} paints index {sheet.Inks[at].PaletteIndex} where its characters name {expected}");
            }
        }
        Console.WriteLine($"Mode 7 tile pictures: {tiles.Name} pictured {pictured} tiles, the {map.NamedTiles.Count} {room.Name}'s map names among them.");
    }

    /// <summary>Loads <paramref name="room"/> and checks its background image over <see cref="Frames"/> frames.</summary>
    private static void CheckBackground(RoomHeaderDefinition room, bool expectRebound)
    {
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(room);
        runtime.Player.IntroducedSamus.Control.LockHandlers();
        int rebound = 0;
        for (int frame = 0; frame < Frames; frame++)
        {
            ArtworkFrameSelections latched = SelectionsOf(runtime);
            runtime.StepFrame(0);
            rebound += AssertBackgroundPaintsTiles(runtime, latched, $"{room.Name} frame {frame}");
        }
        AssertEqual(expectRebound, rebound > 0, $"{room.Name}'s background shows cells over artwork bound after its load");
        Console.WriteLine($"Room background images: {room.Name} painted its two pages for {Frames} frames, {rebound} cell pictures over rebound artwork.");
    }

    /// <summary>
    /// Asserts BG2's library background paints, pixel for pixel over both pages, the palette index, priority and standard
    /// colour the image's tilemap tiles paint over the characters the cartridge would show; returns its rebound cells.
    /// </summary>
    private static int AssertBackgroundPaintsTiles(SuperMetroidRuntime runtime, ArtworkFrameSelections latched, string where)
    {
        IReadOnlyList<AppearanceOption> options = UploadsOf(runtime).AllOptions;
        RoomFxAnimatedTileAtlas animatedTiles = runtime.Artwork.MapPresentation?.RoomFxAnimatedTiles
            ?? throw new InvalidOperationException($"{where}: the runtime has no installed animated tiles.");
        RoomBackgroundImagePlane plane = runtime.Display.Latched.Rooms?.Bg2Image
            ?? throw new InvalidOperationException($"{where}: BG2 shows no library background image.");
        ReadOnlySpan<byte> vram = runtime.ComposedVram();
        // The room's load selects the image by name (#1406); its tilemap words are the pages the library list builds, as
        // the import pipeline replays them (#1466).
        string name = (runtime.Room.Scope?.BackgroundImage
            ?? throw new InvalidOperationException($"{where}: the room selects no library background image.")).Image.Name;
        ImmutableArray<ushort> words = [.. UploadsOf(runtime).Backgrounds.Single(background => background.Name == name).Words];
        // A 32x64 map lays the two pages out one above the other, as the image does.
        for (int y = 0; y < RoomBackgroundImage.Pages * RoomBackgroundImage.PagePixels; y++)
        for (int x = 0; x < RoomBackgroundImage.PagePixels; x++)
        {
            var word = new SnesBgTilemapWord(words[y / CharacterPixels * RoomBackgroundImage.PageTiles + x / CharacterPixels]);
            int pixelRow = word.FlipVertically ? CharacterPixels - 1 - y % CharacterPixels : y % CharacterPixels;
            int bit = 7 - (word.FlipHorizontally ? CharacterPixels - 1 - x % CharacterPixels : x % CharacterPixels);
            // A character an animation paints shows the frame it selected (#1406), which the composed VRAM never holds.
            ReadOnlySpan<byte> bytes = CharacterBytes(options, latched, BgCharacterSet.Tiles, word.CharacterIndex, animatedTiles, vram, out _);
            int address = pixelRow * 2;
            int colour = (bytes[address] >> bit & 1) | (bytes[address + 1] >> bit & 1) << 1
                | (bytes[address + 16] >> bit & 1) << 2 | (bytes[address + 17] >> bit & 1) << 3;
            int expected = colour == 0 ? 0 : word.PaletteIndex * 16 + colour;
            (SpriteSheetFrame picture, Position16<ImagePixels> picturePixel) = PictureAt(plane, x, y);
            SpriteImage sheet = picture.Sheet;
            int pixel = (picture.Corner.Y + picturePixel.Y) * sheet.Size.X + picture.Corner.X + picturePixel.X;
            AssertTrue(sheet.Inks[pixel].PaletteIndex == expected && (colour == 0 || ((sheet.Inks[pixel].Priority != 0) == word.HasPriority
                    && sheet.Pixels[pixel] == sheet.StandardColours[expected].ToRgbaPixel())),
                $"{where}: the background paints index {sheet.Inks[pixel].PaletteIndex} at ({x}, {y}) where its tile ${word.Raw:X4} paints {expected}");
        }
        int rebound = 0;
        for (int cell = 0; cell < RoomBackgroundImage.Words / RoomBackgroundImage.CellTiles; cell++)
        {
            if (plane.Cell(cell) is not null)
                rebound++;
        }
        return rebound;
    }

    /// <summary>
    /// Loads <paramref name="room"/>, in <paramref name="state"/> when given and after <paramref name="progress"/> sets the
    /// session's progress, and checks its displayed blocks over <see cref="Frames"/> frames.
    /// </summary>
    private static void Check(RoomHeaderDefinition room, bool expectUploads, RoomStateSettings? state = null,
        Action<SuperMetroidRuntime>? progress = null)
    {
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        progress?.Invoke(runtime);
        runtime.LoadCartridgeRoomForDebug(room, state: state);
        runtime.Player.IntroducedSamus.Control.LockHandlers();
        int checkedPictures = 0, uploaded = 0;
        for (int frame = 0; frame < Frames; frame++)
        {
            // The frame boundary resolves the pictures from the frames the animations selected by the previous update; the
            // update after it selects the next ones (#1406).
            ArtworkFrameSelections latched = SelectionsOf(runtime);
            runtime.StepFrame(0);
            (int pictures, int over) = AssertPicturesPaintLive(runtime, latched, $"{room.Name} frame {frame}");
            checkedPictures += pictures;
            uploaded += over;
        }
        AssertTrue(checkedPictures > 0, $"{room.Name} shows block pictures");
        AssertEqual(expectUploads, uploaded > 0, $"{room.Name} shows blocks over artwork bound after its load");
        Console.WriteLine($"Room block pictures: {room.Name} showed {checkedPictures} pictures, {uploaded} over artwork bound after its load.");
    }

    /// <summary>
    /// A copy of the frames the loaded room's animations show now, read through every animation its tileset's rooms run
    /// over their tile and BG3 characters.
    /// </summary>
    private static ArtworkFrameSelections SelectionsOf(SuperMetroidRuntime runtime)
    {
        RoomScope scope = RoomScopeOf(runtime);
        RoomBlockUploads.TilesetUploads uploads = UploadsOf(runtime);
        var copy = new ArtworkFrameSelections();
        foreach (AnimationAppearance animation in uploads.AllOptions.Concat(uploads.Bg3Options).Select(option => option.Input)
            .OfType<AnimationAppearance>().Distinct())
        {
            if (scope.ArtworkFrames.TryShown(animation.Animation, out ArtworkFrame frame))
                copy.Select(frame);
        }
        return copy;
    }

    /// <summary>What the loaded room of <paramref name="runtime"/> shows over its load, with <paramref name="frames"/> its animations selected.</summary>
    private static RoomAppearanceView AppearanceOf(SuperMetroidRuntime runtime, ArtworkFrameSelections frames) =>
        new(RoomScopeOf(runtime).Appearance, frames, runtime.EnemySession);

    /// <summary>What the loaded room's tileset's rooms show over their characters, as the import pipeline replays it.</summary>
    private static RoomBlockUploads.TilesetUploads UploadsOf(SuperMetroidRuntime runtime) => TilesetUploads.Value[RoomScopeOf(runtime).Assets.Tileset];

    /// <summary>
    /// The bytes of <paramref name="set"/>'s character <paramref name="character"/> as the room shows it: the frame an
    /// animation of <paramref name="options"/> that paints it selected in <paramref name="frames"/>, which no VRAM image
    /// holds, or else its bytes in <paramref name="characters"/>, the set's characters from its first.
    /// </summary>
    private static ReadOnlySpan<byte> CharacterBytes(IReadOnlyList<AppearanceOption> options, ArtworkFrameSelections frames, BgCharacterSet set,
        int character, RoomFxAnimatedTileAtlas animatedTiles, ReadOnlySpan<byte> characters, out bool animated)
    {
        int characterWords = set.CharacterBytes() / sizeof(ushort);
        foreach (AppearanceOption option in options.Where(option => option.Input is AnimationAppearance && option.Upload.Covers(set, character))
            .OrderBy(option => option.Input.Token, StringComparer.Ordinal))
        {
            if (!frames.TryShown(((AnimationAppearance)option.Input).Animation, out ArtworkFrame frame))
                continue;
            animated = true;
            int offset = (character - option.Upload.FirstCharacter) * characterWords;
            return AnimatedTileFrameArtwork.Bytes(BgArtworkIdentity.Frame(frame), animatedTiles).Span[(offset * sizeof(ushort))..];
        }
        animated = false;
        return characters[(character * characterWords * sizeof(ushort))..];
    }

    /// <summary>
    /// Asserts each picture the displayed BG1 resolves by block identity and appearance (#1431, #1466) paints, pixel for
    /// pixel, the palette index, priority and standard colour the block's tiles paint over the characters the cartridge
    /// would show: the tiles the cartridge's block tables compose, or for a collectible slot's block, those the collectible
    /// shown in the slot writes. Returns the pictures checked and those shown for inputs that no longer show the room's load.
    /// </summary>
    private static (int Pictures, int Uploaded) AssertPicturesPaintLive(SuperMetroidRuntime runtime, ArtworkFrameSelections latched, string where)
    {
        RoomFxAnimatedTileAtlas animatedTiles = runtime.Artwork.MapPresentation?.RoomFxAnimatedTiles
            ?? throw new InvalidOperationException("The runtime has no installed animated tiles.");
        RoomBackgrounds rooms = runtime.Display.Latched.Rooms ?? throw new InvalidOperationException($"{where}: no room backgrounds are displayed.");
        RoomBlockPictureTable table = rooms.Bg1.Pictures ?? throw new InvalidOperationException($"{where}: BG1 shows no block pictures.");
        RoomAppearanceView appearance = AppearanceOf(runtime, latched);
        // The Ceres Ridley arena's blocks read the OBJ character page its uploads compose; any other room's, the bound tiles,
        // and the frames its animations paint over them.
        bool sprites = rooms.BlockArtwork == RoomBlockArtwork.Sprites;
        IReadOnlyList<AppearanceOption> options = sprites ? [] : UploadsOf(runtime).AllOptions;
        byte[] tiles = sprites
            ? RuntimeArtworkContent.Of(runtime).GameplayObjectPage(runtime.EnemySession.SpriteLooks).Characters.ToArray()
            : runtime.ComposedVram()[(BgCharacterWordsConstants.WordOf(BgCharacterSet.Tiles, 0) * sizeof(ushort))..];
        RoomScope scope = RoomScopeOf(runtime);
        bool ceres = scope.Room.AreaIndex == AreaId.Ceres;
        ushort[] stock = BackgroundTileWordBytes.Words(InstalledFixtures.StockBlockTiles(scope.Assets.Tileset, ceres));
        int pictures = 0, uploaded = 0;
        for (int block = 0; block < table.Blocks; block++)
        {
            if (table.Picture(block) is not { } picture)
                continue;
            pictures++;
            SpriteImage sheet = picture.Sheet;
            bool overUpload = scope.Assets.BlockSheet.Inputs.Of(block, rooms.BlockArtwork)
                .Any(input => appearance.ValueOf(input) != RoomAppearanceValues.Loaded);
            ushort[] blockWords = !ceres && CollectibleSlotBlocks.SlotOf(block, CollectiblePlms.GraphicsSlots) is { } slot
                ? CollectibleSlotWords(scope.Appearance, slot, block)
                : stock[(block * BlockTiles)..((block + 1) * BlockTiles)];
            for (int tile = 0; tile < BlockTiles; tile++)
            {
                var word = new SnesBgTilemapWord(blockWords[tile]);
                // A character an animation paints shows the frame it selected (#1406), which the composed VRAM never holds.
                ReadOnlySpan<byte> bytes = CharacterBytes(options, latched, BgCharacterSet.Tiles, word.CharacterIndex, animatedTiles, tiles, out bool animated);
                for (int y = 0; y < CharacterPixels; y++)
                {
                    int row = word.FlipVertically ? CharacterPixels - 1 - y : y;
                    int address = row * 2;
                    for (int x = 0; x < CharacterPixels; x++)
                    {
                        int bit = 7 - (word.FlipHorizontally ? CharacterPixels - 1 - x : x);
                        int colour = (bytes[address] >> bit & 1) | (bytes[address + 1] >> bit & 1) << 1
                            | (bytes[address + 16] >> bit & 1) << 2 | (bytes[address + 17] >> bit & 1) << 3;
                        int pixel = (picture.Corner.Y + tile / 2 * CharacterPixels + y) * sheet.Size.X + picture.Corner.X + tile % 2 * CharacterPixels + x;
                        int expected = colour == 0 ? 0 : word.PaletteIndex * 16 + colour;
                        AssertTrue(sheet.Inks[pixel].PaletteIndex == expected && (colour == 0 || (sheet.Inks[pixel].Priority != 0) == word.HasPriority)
                            && (colour == 0 || sheet.Pixels[pixel] == sheet.StandardColours[expected].ToRgbaPixel()),
                            $"{where}: block {block}'s picture paints index {sheet.Inks[pixel].PaletteIndex} priority {sheet.Inks[pixel].Priority} " +
                            $"at tile {tile} pixel ({x}, {y}), where its tile ${word.Raw:X4} paints index {expected}{(animated ? " in its animation's frame" : "")}");
                    }
                }
            }
            if (overUpload)
                uploaded++;
        }
        return (pictures, uploaded);
    }

    /// <summary>Tiles a block's picture paints: top left, top right, bottom left, bottom right.</summary>
    private const int BlockTiles = 4;

    /// <summary>
    /// The tile words of collectible slot <paramref name="slot"/>'s block <paramref name="block"/>, as the collectible
    /// <paramref name="appearance"/> shows in the slot writes them (#1431, #1466).
    /// </summary>
    private static ushort[] CollectibleSlotWords(RoomAppearance appearance, int slot, int block)
    {
        InWorldCollectibleKind kind = appearance.CollectibleIn(slot)
            ?? throw new InvalidOperationException($"BG1 shows collectible slot {slot}'s block with no collectible in the slot.");
        // The collectible's cartridge palette selectors colour the slot's blocks.
        BackgroundTile[] tiles = CollectibleSlotBlocks.Tiles(slot, DynamicCollectibleGraphics.Get(kind).PaletteOffsets.Span);
        int first = (block - CollectibleSlotBlocks.FirstBlockOf(slot)) * BlockTiles;
        return BackgroundTileWordBytes.Words(tiles.AsSpan(first, BlockTiles));
    }

    /// <summary>Runs the room block picture checks.</summary>
    [VerificationCommand("--room-block-pictures")]
    internal static int RoomBlockPicturesCommand()
    {
        Suite(nameof(VerifyRoomBlockPictures), VerifyRoomBlockPictures);
        Suite(nameof(VerifyRoomBackgroundImages), VerifyRoomBackgroundImages);
        Suite(nameof(VerifyTileWordPictures), VerifyTileWordPictures);
        Suite(nameof(VerifyBg3Pictures), VerifyBg3Pictures);
        Suite(nameof(VerifyMode7TilePictures), VerifyMode7TilePictures);
        return 0;
    }

    /// <summary>The room scope <paramref name="runtime"/> has loaded.</summary>
    private static RoomScope RoomScopeOf(SuperMetroidRuntime runtime) =>
        runtime.Room.Scope ?? throw new InvalidOperationException("No room is loaded.");
}
