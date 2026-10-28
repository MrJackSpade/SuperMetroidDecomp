using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.World;
using SuperMetroid.Core;
using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.Kraid.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Phantoon.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Drawing;
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
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.World.RoomStates;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Rooms.Artwork;

/// <summary>
/// Checks rooms draw their blocks from the installed pre-coloured block sheets (#1392): each block the displayed planes
/// show is a picture that paints exactly what its tiles paint over the characters the room's bindings show, as those
/// bindings change under animated tiles and collectibles.
/// </summary>
internal static class RoomBlockPicturesVerification
{
    /// <summary>Displayed frames each room is checked over: enough for every animated tile to change frame.</summary>
    private const int Frames = 40;

    /// <summary>Pixels along a character's side.</summary>
    private const int CharacterPixels = 8;

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
        // The Ceres Ridley arena's blocks read the OBJ character table's enemy tiles.
        Check(RoomHeaderDefinitions.CeresRidley, expectUploads: true);
        // The treadmills run once Phantoon, the Wrecked Ship's boss, is defeated.
        Check(RoomHeaderDefinitions.WreckedShipMainShaft, expectUploads: true,
            RoomStateSelectionDefinitions.GetStates(RoomHeaderDefinitions.WreckedShipMainShaft)
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
        SkyTilemap sky = (runtime.Artwork.RoomSkyTilemapArt ?? throw new InvalidOperationException("The fixture has no sky rows."))
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
        int address = word.CharacterIndex * RoomBlockPictureNames.CharacterWords * sizeof(ushort) + pixelRow * 2;
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
    /// Confirms words written over BG2 draw from the room's tile word sheet (#1392): in each boss's room as loaded, every
    /// frame the boss writes resolves each tile to a picture painting what the tile paints over the room's characters, and
    /// so does the whole Zebes escape warning typed over Mother Brain's room.
    /// </summary>
    internal static void VerifyTileWordPictures()
    {
        CheckBody(RoomHeaderDefinitions.Kraid, runtime =>
        {
            KraidBackgroundArtwork art = runtime.BossArtwork.Installed.KraidBackground ?? throw new InvalidOperationException("No Kraid artwork.");
            BackgroundTile[] working = KraidBodyTilemap.ComposeWorkingMap(art);
            return art.Heads.Keys.Order().SelectMany(stage => new[]
            {
                ($"head {stage}", KraidBodyTilemap.Resolve(new KraidBodyFrame(KraidBodyPage.Body, KraidHeadFrame.Closed with { MouthStage = stage },
                    KraidBodyPage.Body), working, art).Tiles),
                ($"grown head {stage}", KraidBodyTilemap.Resolve(new KraidBodyFrame(KraidBodyPage.PriorityBody,
                    KraidHeadFrame.Closed with { MouthStage = stage }, KraidBodyPage.PriorityBody), working, art).Tiles),
            }).Append(("blank", KraidBodyTilemap.Resolve(KraidBodyFrame.Blank, working, art).Tiles));
        });
        CheckBody(RoomHeaderDefinitions.Phantoon, runtime => BodyFrames(
            (runtime.BossArtwork.Installed.PhantoonBg2Frames ?? throw new InvalidOperationException("No Phantoon frames.")).Writes,
            PhantoonBg2FrameDefinitions.BlankTile));
        CheckBody(RoomHeaderDefinitions.Draygon, runtime => BodyFrames(
            (runtime.BossArtwork.Installed.DraygonBg2Frames ?? throw new InvalidOperationException("No Draygon frames.")).Writes,
            EnemyBg2SurfaceDefinitions.BlankTile));
        CheckBody(RoomHeaderDefinitions.Crocomire, runtime => BodyFrames(
            (runtime.BossArtwork.Installed.CrocomireBg2Frames ?? throw new InvalidOperationException("No Crocomire frames.")).Writes,
            CrocomireBg2.CrocomireBlankBg2Tile));
        CheckBody(RoomHeaderDefinitions.MotherBrain, runtime => BodyFrames(
            (runtime.BossArtwork.Installed.MotherBrainBodyBg2Frames ?? throw new InvalidOperationException("No Mother Brain frames.")).Writes,
            MotherBrainBg2Definitions.BlankTile));
        CheckTypedWarning(RoomHeaderDefinitions.MotherBrain);
    }

    /// <summary>Loads <paramref name="room"/>, types the whole Zebes escape warning over BG2 and checks each glyph's picture.</summary>
    private static void CheckTypedWarning(RoomHeaderDefinition room)
    {
        SuperMetroidRuntime runtime = LoadRoom(room);
        runtime.StepFrame(0);
        TileWordSheet sheet = (runtime.Room.Scope?.Bg2OverlayPictures ?? throw new InvalidOperationException($"{room.Name} has no tile words.")).Sheet;
        ReadOnlySpan<byte> vram = runtime.ComposedVram();
        var typewriter = new EscapeTypewriterState((runtime.BossArtwork.EscapeTypewriterPresentation
            ?? throw new InvalidOperationException("No escape typewriter.")).Get(EscapeTypewriterProgramId.Zebes), EscapeTypewriterDefinitions.ZebesTileBase);
        RoomTilemapOverlay overlay = RoomTilemapOverlay.Empty;
        while (!typewriter.Step(out EscapeTypewriterGlyph? glyph))
        {
            if (glyph is { } typed)
                overlay = overlay.With(typed.Cell, [typed.Tile]);
        }
        ImmutableArray<BgTilePicture?> pictures = new TileWordPictureResolver(sheet).Resolve(overlay,
            RoomCharacterSource.Of(RoomBackgroundCharacters.Tileset, runtime.BgArtwork, runtime.EnemySession.SpriteCharacters,
                RoomScopeOf(runtime).AnimatedCharacters, RoomScopeOf(runtime).ArtworkFrames));
        int glyphs = 0;
        for (int index = 0; index < RoomTilemapOverlay.WordCount; index++)
        {
            if (overlay.Words[index] == RoomTilemapOverlay.Unwritten)
                continue;
            BgTilePicture picture = pictures[index] ?? throw new InvalidOperationException($"{sheet.Name}: typed word {index} draws from its character.");
            var word = new SnesBgTilemapWord((ushort)overlay.Words[index]);
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

    /// <summary>Each frame of <paramref name="catalog"/> written over an image filled with <paramref name="fill"/>.</summary>
    private static IEnumerable<(string, ImmutableArray<BackgroundTile>)> BodyFrames(EnemyBg2FrameCatalog catalog, BackgroundTile fill) =>
        catalog.Frames.Select(frame =>
        {
            var image = new EnemyBg2Image(fill);
            foreach (EnemyBg2TilemapWrite write in frame.Value)
                image.Write(write.FirstCell, write.Tiles.Span);
            return (frame.Key.ToString(), image.Publish().Tiles);
        });

    /// <summary>Loads boss room <paramref name="room"/> and checks each body tilemap <paramref name="bodies"/> gives resolves to its pictures.</summary>
    private static void CheckBody(RoomHeaderDefinition room, Func<SuperMetroidRuntime, IEnumerable<(string Name, ImmutableArray<BackgroundTile> Tiles)>> bodies)
    {
        SuperMetroidRuntime runtime = LoadRoom(room);
        runtime.StepFrame(0);
        TileWordSheet sheet = (runtime.Room.Scope?.BodyPictures ?? throw new InvalidOperationException($"{room.Name} has no tile words.")).Sheet;
        ReadOnlySpan<byte> vram = runtime.ComposedVram();
        int frames = 0;
        foreach ((string name, ImmutableArray<BackgroundTile> tiles) in bodies(runtime))
        {
            ImmutableArray<BgTilePicture?> pictures = new TileWordPictureResolver(sheet).Resolve(tiles,
                RoomCharacterSource.Of(RoomBackgroundCharacters.Tileset, runtime.BgArtwork, runtime.EnemySession.SpriteCharacters,
                RoomScopeOf(runtime).AnimatedCharacters, RoomScopeOf(runtime).ArtworkFrames));
            for (int tile = 0; tile < tiles.Length; tile++)
            {
                BgTilePicture picture = pictures[tile] ?? throw new InvalidOperationException($"{sheet.Name} {name}: tile {tile} draws from its character.");
                var word = new SnesBgTilemapWord(tiles[tile].PackedWord);
                for (int y = 0; y < CharacterPixels; y++)
                for (int x = 0; x < CharacterPixels; x++)
                {
                    AssertPaintsTile($"{sheet.Name} {name} tile {tile} pixel ({x}, {y})", vram, word, x, y, picture.Picture,
                        new Position16<ImagePixels>((ushort)(picture.Corner.X + x), (ushort)(picture.Corner.Y + y)));
                }
            }
            frames++;
        }
        AssertTrue(frames > 0, $"{room.Name}'s boss writes frames");
        Console.WriteLine($"Tile word pictures: {sheet.Name} pictured {frames} body frames over {room.Name} as it loads.");
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
            rooms.Add(RoomHeaderDefinitions.All.First(room => RoomStateSelectionDefinitions.GetStates(room)[0].Fx?.Records
                .Any(record => record.Type == effect) == true));
        }
        foreach (RoomHeaderDefinition room in rooms)
        {
            SuperMetroidRuntime runtime = LoadRoom(room, stocked);
            runtime.StepFrame(0);
            GameplayDisplayLatch latched = runtime.Display.Latched;
            Layer3Plane plane = latched.Layer3 ?? throw new InvalidOperationException($"{room.Name} latches no BG3 plane.");
            AssertTrue(latched.Rooms?.Bg3 is not null, $"{room.Name}: BG3 draws from its pictures");
            ReadOnlySpan<byte> vram = runtime.ComposedVram();
            BgCharacterSet layer3 = latched.Scene.Layer3Characters;
            int characterBase = BgCharacterWordsConstants.WordOf(layer3, 0);
            var animated = new AnimatedCharacterView(RoomScopeOf(runtime).AnimatedCharacters.Bg3, RoomScopeOf(runtime).ArtworkFrames);
            ImmutableArray<BgTilePicture?> pictures = new TileWordPictureResolver((runtime.Room.Scope
                ?? throw new InvalidOperationException($"{room.Name} has no room scope.")).Bg3Pictures.Sheet).Resolve(plane, runtime.BgArtwork, layer3,
                animated);
            for (int index = 0; index < Layer3Plane.Columns * Layer3Plane.Rows; index++)
            {
                BgTilePicture picture = pictures[index] ?? throw new InvalidOperationException($"{room.Name}: BG3 word {index} draws from its character.");
                var word = new SnesBgTilemapWord(plane.Words[index]);
                SpriteImage sheet = picture.Picture.Sheet;
                for (int y = 0; y < CharacterPixels; y++)
                for (int x = 0; x < CharacterPixels; x++)
                {
                    int pixelRow = word.FlipVertically ? CharacterPixels - 1 - y : y;
                    int bit = 7 - (word.FlipHorizontally ? CharacterPixels - 1 - x : x);
                    // A character an effect's animation paints shows the frame it selected (#1406), which VRAM never holds.
                    ReadOnlySpan<byte> bytes = animated.TryShowing(word.CharacterIndex, 8, out BgArtworkShown frame)
                        ? AnimatedTileFrameArtwork.Bytes(frame.Artwork, runtime.Artwork.MapPresentation?.RoomFxAnimatedTiles
                            ?? throw new InvalidOperationException($"{room.Name}: the runtime has no installed animated tiles.")).Span[(frame.WordOffset * sizeof(ushort))..]
                        : vram[((characterBase + word.CharacterIndex * 8) * sizeof(ushort))..];
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
        AssertTrue(map.NamedTiles.All(tile => tiles.Shown[tile] is not null), $"{room.Name}'s Mode 7 map names only pictured tiles");
        ReadOnlySpan<byte> vram = runtime.ComposedVram();
        SpriteImage sheet = tiles.Picture.Sheet;
        int pictured = 0;
        for (int tile = 0; tile < FloorTileSheet.Tiles; tile++)
        {
            if (tiles.Shown[tile] is null)
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
            ArtworkFrameSelections latched = SelectionsOf(RoomScopeOf(runtime));
            runtime.StepFrame(0);
            rebound += AssertBackgroundPaintsTiles(runtime, latched, $"{room.Name} frame {frame}");
        }
        AssertEqual(expectRebound, rebound > 0, $"{room.Name}'s background shows cells over artwork bound after its load");
        Console.WriteLine($"Room background images: {room.Name} painted its two pages for {Frames} frames, {rebound} cell pictures over rebound artwork.");
    }

    /// <summary>
    /// Asserts BG2's library background paints, pixel for pixel over both pages, the palette index, priority and standard
    /// colour the image's tilemap tiles paint over the characters the bindings compose into VRAM; returns its rebound cells.
    /// </summary>
    private static int AssertBackgroundPaintsTiles(SuperMetroidRuntime runtime, ArtworkFrameSelections latched, string where)
    {
        // A character an animation paints shows the frame it selected (#1406), which the composed VRAM never holds.
        var animated = new AnimatedCharacterView(RoomScopeOf(runtime).AnimatedCharacters.Blocks, latched);
        RoomFxAnimatedTileAtlas animatedTiles = runtime.Artwork.MapPresentation?.RoomFxAnimatedTiles
            ?? throw new InvalidOperationException($"{where}: the runtime has no installed animated tiles.");
        RoomBackgroundImagePlane plane = runtime.Display.Latched.Rooms?.Bg2Image
            ?? throw new InvalidOperationException($"{where}: BG2 shows no library background image.");
        ReadOnlySpan<byte> vram = runtime.ComposedVram();
        // The room's load selects the image by name (#1406); its tilemap words are the pages the library list builds.
        ImmutableArray<ushort> words = [.. BackgroundTileWordBytes.Words((runtime.Room.Scope?.BackgroundImage
            ?? throw new InvalidOperationException($"{where}: the room selects no library background image.")).Image.Tiles.AsSpan())];
        // A 32x64 map lays the two pages out one above the other, as the image does.
        for (int y = 0; y < RoomBackgroundImage.Pages * RoomBackgroundImage.PagePixels; y++)
        for (int x = 0; x < RoomBackgroundImage.PagePixels; x++)
        {
            var word = new SnesBgTilemapWord(words[y / CharacterPixels * RoomBackgroundImage.PageTiles + x / CharacterPixels]);
            int pixelRow = word.FlipVertically ? CharacterPixels - 1 - y % CharacterPixels : y % CharacterPixels;
            int bit = 7 - (word.FlipHorizontally ? CharacterPixels - 1 - x % CharacterPixels : x % CharacterPixels);
            ReadOnlySpan<byte> bytes = animated.TryShowing(word.CharacterIndex, RoomBlockPictureNames.CharacterWords, out BgArtworkShown frame)
                ? AnimatedTileFrameArtwork.Bytes(frame.Artwork, animatedTiles).Span[(frame.WordOffset * sizeof(ushort))..]
                : vram[(word.CharacterIndex * RoomBlockPictureNames.CharacterWords * sizeof(ushort))..];
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
        for (int cell = 0; cell < RoomBackgroundImage.Words / RoomBlockPictureNames.Tiles; cell++)
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
            ArtworkFrameSelections latched = SelectionsOf(RoomScopeOf(runtime));
            runtime.StepFrame(0);
            (int pictures, int over) = AssertPicturesPaintLive(runtime, latched, $"{room.Name} frame {frame}");
            checkedPictures += pictures;
            uploaded += over;
        }
        AssertTrue(checkedPictures > 0, $"{room.Name} shows block pictures");
        AssertEqual(expectUploads, uploaded > 0, $"{room.Name} shows blocks over artwork bound after its load");
        Console.WriteLine($"Room block pictures: {room.Name} showed {checkedPictures} pictures, {uploaded} over artwork bound after its load.");
    }

    /// <summary>A copy of the frames <paramref name="scope"/>'s animations show now, read through every run the room declares.</summary>
    private static ArtworkFrameSelections SelectionsOf(RoomScope scope)
    {
        var copy = new ArtworkFrameSelections();
        RoomAnimatedCharacters animated = scope.AnimatedCharacters;
        foreach (AnimatedCharacterRun run in animated.Blocks.Runs.Concat(animated.ObjectPage.Runs).Concat(animated.Bg3.Runs))
        {
            if (scope.ArtworkFrames.TryShown(run.Animation, out ArtworkFrame frame))
                copy.Select(frame);
        }
        return copy;
    }

    /// <summary>
    /// Asserts each picture the displayed BG1 resolves paints, pixel for pixel, the palette index, priority and standard
    /// colour its tiles paint over the characters the bindings compose into VRAM; returns the pictures checked and those
    /// over artwork bound after the room's load.
    /// </summary>
    private static (int Pictures, int Uploaded) AssertPicturesPaintLive(SuperMetroidRuntime runtime, ArtworkFrameSelections latched, string where)
    {
        static RoomFxAnimatedTileAtlas AnimatedTiles(SuperMetroidRuntime runtime) =>
            runtime.Artwork.MapPresentation?.RoomFxAnimatedTiles ?? throw new InvalidOperationException("The runtime has no installed animated tiles.");
        RoomBackgrounds rooms = runtime.Display.Latched.Rooms ?? throw new InvalidOperationException($"{where}: no room backgrounds are displayed.");
        RoomBlockPictureTable table = rooms.Bg1.Pictures ?? throw new InvalidOperationException($"{where}: BG1 shows no block pictures.");
        RoomCharacterSource characters = RoomCharacterSource.Of(rooms.Characters, runtime.BgArtwork, runtime.EnemySession.SpriteCharacters,
                RoomScopeOf(runtime).AnimatedCharacters, latched);
        // The Ceres Ridley arena's blocks read the OBJ character page its uploads compose; any other room's, the bound tiles.
        byte[] tiles = rooms.Characters == RoomBackgroundCharacters.ObjectCharacterPage
            ? runtime.Artwork.GameplayObjectPage(runtime.EnemySession.SpriteCharacters).Characters.ToArray()
            : runtime.ComposedVram()[(BgCharacterWordsConstants.WordOf(BgCharacterSet.Tiles, 0) * sizeof(ushort))..];
        ReadOnlySpan<byte> definitions = BackgroundTileWordBytes.Of(runtime.Room.LoadedLevelData.ForegroundPlane.Definitions.AsSpan());
        int pictures = 0, uploaded = 0;
        for (int definition = 0; definition < table.Definitions; definition++)
        {
            if (table.Picture(definition) is not { } picture)
                continue;
            pictures++;
            SpriteImage sheet = picture.Sheet;
            bool overUpload = false;
            for (int tile = 0; tile < RoomBlockPictureNames.Tiles; tile++)
            {
                int at = definition * RoomLevelPlaneDefinitionBytes + tile * sizeof(ushort);
                var word = new SnesBgTilemapWord(LittleEndianBytes.Word(definitions[at], definitions[at + 1]));
                // A character an animation paints shows the frame it selected (#1406), which the composed VRAM never holds.
                bool animated = characters.Animated.TryShowing(word.CharacterIndex, RoomBlockPictureNames.CharacterWords, out BgArtworkShown shown);
                if (!animated)
                    shown = characters.Artwork.Showing(word.CharacterIndex, 1);
                overUpload |= animated || shown.Artwork != RoomBgArtwork.CommonRoomElements && !shown.Artwork.Name.StartsWith("tileset characters ", StringComparison.Ordinal);
                ReadOnlySpan<byte> bytes = animated
                    ? AnimatedTileFrameArtwork.Bytes(shown.Artwork, AnimatedTiles(runtime)).Span[(shown.WordOffset * sizeof(ushort))..]
                    : tiles.AsSpan((word.CharacterIndex * RoomBlockPictureNames.CharacterWords * sizeof(ushort))..);
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
                            $"{where}: definition {definition}'s picture paints index {sheet.Inks[pixel].PaletteIndex} priority {sheet.Inks[pixel].Priority} " +
                            $"at tile {tile} pixel ({x}, {y}), where its tile ${word.Raw:X4} paints index {expected} over {shown.Artwork.Name}+{shown.WordOffset:X4}");
                    }
                }
            }
            if (overUpload)
                uploaded++;
        }
        return (pictures, uploaded);
    }

    /// <summary>
    /// Asserts the X-ray reveal the frame displays paints, pixel for pixel, the palette index, priority and standard colour
    /// the reveal buffer's tile words paint over the characters the bindings compose into VRAM (#1392).
    /// </summary>
    internal static void AssertRevealPaintsWords(SuperMetroidRuntime runtime, ReadOnlySpan<BackgroundTile> tiles, string where)
    {
        ushort[] words = BackgroundTileWordBytes.Words(tiles);
        XrayRevealPlane reveal = runtime.Display.Latched.XrayReveal ?? throw new InvalidOperationException($"{where}: no X-ray reveal is displayed.");
        ReadOnlySpan<byte> vram = runtime.ComposedVram();
        for (int row = 0; row < XrayTilemapLayout.MetatileRows; row++)
        for (int column = 0; column < XrayTilemapLayout.MetatileColumns; column++)
        for (int y = 0; y < RoomBlockSheet.BlockPixels; y++)
        for (int x = 0; x < RoomBlockSheet.BlockPixels; x++)
        {
            var word = new SnesBgTilemapWord(words[XrayRevealTilemap.FirstCellOf(column, row) + y / CharacterPixels * 32 + x / CharacterPixels]);
            int pixelRow = word.FlipVertically ? CharacterPixels - 1 - y % CharacterPixels : y % CharacterPixels;
            int bit = 7 - (word.FlipHorizontally ? CharacterPixels - 1 - x % CharacterPixels : x % CharacterPixels);
            int address = word.CharacterIndex * RoomBlockPictureNames.CharacterWords * sizeof(ushort) + pixelRow * 2;
            int colour = (vram[address] >> bit & 1) | (vram[address + 1] >> bit & 1) << 1
                | (vram[address + 16] >> bit & 1) << 2 | (vram[address + 17] >> bit & 1) << 3;
            int expected = colour == 0 ? 0 : word.PaletteIndex * 16 + colour;
            RevealedBlock block = reveal.Blocks[row * XrayTilemapLayout.MetatileColumns + column]
                ?? throw new InvalidOperationException($"{where}: the reveal shows no block at ({column}, {row}).");
            SpriteSheetFrame picture = reveal.Pictures.Picture(block.Definition)
                ?? throw new InvalidOperationException($"{where}: the reveal's block definition {block.Definition} has no picture.");
            Position16<ImagePixels> picturePixel = ArrangedPixel(block.Arrangement, x, y);
            SpriteImage sheet = picture.Sheet;
            int pixel = (picture.Corner.Y + picturePixel.Y) * sheet.Size.X + picture.Corner.X + picturePixel.X;
            AssertTrue(sheet.Inks[pixel].PaletteIndex == expected && (colour == 0 || (sheet.Inks[pixel].Priority != 0) == word.HasPriority)
                && (colour == 0 || sheet.Pixels[pixel] == sheet.StandardColours[expected].ToRgbaPixel()),
                $"{where}: reveal block ({column}, {row}) pixel ({x}, {y}) paints index {sheet.Inks[pixel].PaletteIndex} where its tile ${word.Raw:X4} paints {expected}");
        }
    }

    /// <summary>
    /// The pixel of a block definition's picture a revealed block shows at (<paramref name="x"/>, <paramref name="y"/>) of
    /// its 16x16 square, laid out by <paramref name="arrangement"/>.
    /// </summary>
    private static Position16<ImagePixels> ArrangedPixel(RevealedBlockArrangement arrangement, int x, int y)
    {
        const int side = RoomBlockSheet.BlockPixels;
        if (!Enum.IsDefined(arrangement))
            throw new InvalidDataException($"Unknown revealed block arrangement {arrangement}.");
        if (arrangement is RevealedBlockArrangement.MirroredHorizontally or RevealedBlockArrangement.MirroredBoth)
            x = side - 1 - x;
        if (arrangement is RevealedBlockArrangement.MirroredVertically or RevealedBlockArrangement.MirroredBoth)
            y = side - 1 - y;
        // Swapped rows show the lower tile row on top; neither row is flipped.
        if (arrangement == RevealedBlockArrangement.RowsSwapped)
            y = (y + side / 2) % side;
        return new Position16<ImagePixels>((ushort)x, (ushort)y);
    }

    /// <summary>Bytes per block definition.</summary>
    private const int RoomLevelPlaneDefinitionBytes = SuperMetroid.Core.Rooms.Backgrounds.RoomLevelPlane.DefinitionTiles * sizeof(ushort);

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
