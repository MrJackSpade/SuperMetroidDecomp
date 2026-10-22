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
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rendering;
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
using static SuperMetroid.Verification.Harness.VerificationAssert;

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
                .Single(state => state.Fx?.Records.Any(record => record.AnimatedTileBitset != 0) == true),
            runtime => runtime.System.SetBossBits((int)AreaId.WreckedShip, BossBits.AreaBoss));
    }

    /// <summary>
    /// In the Parlor, whose rock background shows its room's load, and in the Tourian statue room, whose statues' frames
    /// rebind the characters its background reads, BG2 draws the library background's image, and every pixel of its two
    /// pages paints what BG2's tilemap tiles paint over the live characters, frame after frame.
    /// </summary>
    internal static void VerifyRoomBackgroundImages()
    {
        CheckBackground(RoomHeaderDefinitions.Parlor, expectRebound: false);
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
        RoomBackgroundImagePlane plane = runtime.Display.Latched.Rooms?.SkyImage
            ?? throw new InvalidOperationException($"{room.Name}: BG2 shows no sky image.");
        SkyTilemap sky = (runtime.Artwork.RoomSkyTilemapArt ?? throw new InvalidOperationException("The fixture has no sky rows."))
            .Tilemap((runtime.Room.Scope?.ScrollingSky ?? throw new InvalidOperationException($"{room.Name} has no sky.")).Variant);
        PpuMemorySnapshot memory = PpuMemorySnapshot.Capture(runtime.Vram, runtime.Cgram, runtime.DisplayedSprites, runtime.BgArtwork);
        ReadOnlySpan<byte> vram = memory.Vram;
        IBgSource shown = plane.AsSky(sky);
        int rows = 0;
        for (int row = 0; row < SkyTilemap.Rows; row++)
        {
            if (!sky.RowShown(row))
                continue;
            rows++;
            for (int y = row * CharacterPixels; y < (row + 1) * CharacterPixels; y++)
            for (int x = 0; x < RoomBackgroundImage.PagePixels; x++)
            {
                var word = new SnesBgTilemapWord(sky.Words[row * SkyTilemap.Columns + x / CharacterPixels]);
                AssertTrue(shown.TrySample(new Position16<BgPlanePixels>((ushort)x, (ushort)y), out BgSample sample),
                    $"{room.Name}: the sky shows row {row}");
                SpriteSheetFrame picture = sample.Picture ?? throw new InvalidOperationException($"{room.Name}: the sky shows a tile at ({x}, {y}).");
                AssertPaintsTile($"{room.Name}'s sky at ({x}, {y})", vram, word, x % CharacterPixels, y % CharacterPixels, picture, sample.PicturePixel);
            }
        }
        AssertTrue(rows > 0, $"{room.Name}'s sky shows rows");
        Console.WriteLine($"Room background images: {room.Name}'s sky painted its {rows} shown rows from their image.");
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
        AssertTrue(sheet.PaletteIndices[at] == expected && (colour == 0 || ((sheet.Priorities[at] != 0) == word.HasPriority
                && sheet.Pixels[at] == sheet.StandardColours[expected].ToRgba32())),
            $"{context}: the picture paints index {sheet.PaletteIndices[at]} where tile ${word.Raw:X4} paints {expected}");
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
            ushort[] working = KraidBodyTilemap.ComposeWorkingMap(art);
            return art.Heads.Keys.Order().SelectMany(stage => new[]
            {
                ($"head {stage}", KraidBodyTilemap.Resolve(new KraidBodyFrame(KraidBodyPage.Body, KraidHeadFrame.Closed with { MouthStage = stage },
                    KraidBodyPage.Body), working, art).Words),
                ($"grown head {stage}", KraidBodyTilemap.Resolve(new KraidBodyFrame(KraidBodyPage.PriorityBody,
                    KraidHeadFrame.Closed with { MouthStage = stage }, KraidBodyPage.PriorityBody), working, art).Words),
            }).Append(("blank", KraidBodyTilemap.Resolve(KraidBodyFrame.Blank, working, art).Words));
        });
        CheckBody(RoomHeaderDefinitions.Phantoon, runtime => BodyFrames(
            (runtime.BossArtwork.Installed.PhantoonBg2Frames ?? throw new InvalidOperationException("No Phantoon frames.")).Writes,
            PhantoonBg2FrameDefinitions.BlankTile));
        CheckBody(RoomHeaderDefinitions.Draygon, runtime => BodyFrames(
            (runtime.BossArtwork.Installed.DraygonBg2Frames ?? throw new InvalidOperationException("No Draygon frames.")).Writes,
            EnemyBg2SurfaceDefinitions.BlankTile.Raw));
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
        ReadOnlySpan<byte> vram = PpuMemorySnapshot.Capture(runtime.Vram, runtime.Cgram, runtime.DisplayedSprites, runtime.BgArtwork).Vram;
        var typewriter = new EscapeTypewriterState((runtime.BossArtwork.EscapeTypewriterPresentation
            ?? throw new InvalidOperationException("No escape typewriter.")).Get(EscapeTypewriterProgramId.Zebes), EscapeTypewriterRomData.ZebesTileBase);
        RoomTilemapOverlay overlay = RoomTilemapOverlay.Empty;
        while (!typewriter.Step(out EscapeTypewriterGlyph? glyph))
        {
            if (glyph is { } typed)
                overlay = overlay.With(typed.Destination - GameplayVramLayout.Bg2TilemapWord, [typed.Tile.Raw]);
        }
        ImmutableArray<BgTilePicture?> pictures = new TileWordPictureResolver(sheet).Resolve(overlay,
            RoomCharacterSource.Of(RoomBackgroundCharacters.Tileset, runtime.BgArtwork, runtime.EnemySession.ObjCharacters));
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
    private static IEnumerable<(string, ImmutableArray<ushort>)> BodyFrames(EnemyBg2FrameCatalog catalog, ushort fill) =>
        catalog.Frames.Select(frame =>
        {
            var image = new EnemyBg2Image(fill);
            foreach (EnemyBg2TilemapWrite write in frame.Value)
                image.Write(write.DestinationWord, write.Tiles.Span);
            return (frame.Key.ToString(), image.Publish().Words);
        });

    /// <summary>Loads boss room <paramref name="room"/> and checks each body tilemap <paramref name="bodies"/> gives resolves to its pictures.</summary>
    private static void CheckBody(RoomHeaderDefinition room, Func<SuperMetroidRuntime, IEnumerable<(string Name, ImmutableArray<ushort> Words)>> bodies)
    {
        SuperMetroidRuntime runtime = LoadRoom(room);
        runtime.StepFrame(0);
        TileWordSheet sheet = (runtime.Room.Scope?.BodyPictures ?? throw new InvalidOperationException($"{room.Name} has no tile words.")).Sheet;
        ReadOnlySpan<byte> vram = PpuMemorySnapshot.Capture(runtime.Vram, runtime.Cgram, runtime.DisplayedSprites, runtime.BgArtwork).Vram;
        int frames = 0;
        foreach ((string name, ImmutableArray<ushort> words) in bodies(runtime))
        {
            ImmutableArray<BgTilePicture?> pictures = new TileWordPictureResolver(sheet).Resolve(words,
                RoomCharacterSource.Of(RoomBackgroundCharacters.Tileset, runtime.BgArtwork, runtime.EnemySession.ObjCharacters));
            for (int tile = 0; tile < words.Length; tile++)
            {
                BgTilePicture picture = pictures[tile] ?? throw new InvalidOperationException($"{sheet.Name} {name}: tile {tile} draws from its character.");
                var word = new SnesBgTilemapWord(words[tile]);
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
                .Any(record => RoomFxTypes.FromCartridge(record.Type, room.Name) == effect) == true));
        }
        foreach (RoomHeaderDefinition room in rooms)
        {
            SuperMetroidRuntime runtime = LoadRoom(room, stocked);
            runtime.StepFrame(0);
            GameplayNmiDisplayLatch latched = runtime.Display.Latched;
            Layer3Plane plane = (latched.Rooms ?? throw new InvalidOperationException($"{room.Name} shows no room backgrounds.")).Bg3;
            AssertTrue(!plane.Pictures.IsDefault, $"{room.Name}: BG3 draws from its pictures");
            ReadOnlySpan<byte> vram = PpuMemorySnapshot.Capture(runtime.Vram, runtime.Cgram, runtime.DisplayedSprites, runtime.BgArtwork).Vram;
            int characterBase = latched.Scene.HudCharacterWord;
            ImmutableArray<BgTilePicture?> pictures = new TileWordPictureResolver((runtime.Room.Scope
                ?? throw new InvalidOperationException($"{room.Name} has no room scope.")).Bg3Pictures.Sheet).Resolve(plane, runtime.BgArtwork, characterBase);
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
                    int address = (characterBase + word.CharacterIndex * 8) * sizeof(ushort) + pixelRow * 2;
                    int colour = (vram[address] >> bit & 1) | (vram[address + 1] >> bit & 1) << 1;
                    int expected = word.PaletteIndex * 4 + colour;
                    int at = (picture.Picture.Corner.Y + picture.Corner.Y + y) * sheet.Size.X + picture.Picture.Corner.X + picture.Corner.X + x;
                    AssertTrue(sheet.PaletteIndices[at] == expected && (sheet.Priorities[at] != 0) == word.HasPriority
                            && (colour == 0 ? sheet.Pixels[at].A == 0 : sheet.Pixels[at] == sheet.StandardColours[expected].ToRgba32()),
                        $"{room.Name}: BG3 word {index} ${word.Raw:X4} pixel ({x}, {y}) paints index {sheet.PaletteIndices[at]} where its character, "
                        + $"{runtime.BgArtwork.Showing(characterBase + word.CharacterIndex * 8, 8)}, paints {expected}");
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
        Mode7MapSnapshot map = runtime.Display.Latched.Rooms?.Mode7 ?? throw new InvalidOperationException($"{room.Name} shows no Mode 7 map.");
        Mode7TileSheet tiles = map.Tiles ?? throw new InvalidOperationException($"{room.Name}'s Mode 7 map draws from no tile pictures.");
        AssertTrue(map.NamedTiles.All(tile => tiles.Shown[tile] is not null), $"{room.Name}'s Mode 7 map names only pictured tiles");
        ReadOnlySpan<byte> vram = PpuMemorySnapshot.Capture(runtime.Vram, runtime.Cgram, runtime.DisplayedSprites, runtime.BgArtwork).Vram;
        SpriteImage sheet = tiles.Picture.Sheet;
        int pictured = 0;
        for (int tile = 0; tile < Mode7TileSheet.Tiles; tile++)
        {
            if (tiles.Shown[tile] is null)
                continue;
            pictured++;
            Position16<ImagePixels> corner = Mode7TileSheet.Corner(tile);
            for (int pixel = 0; pixel < Mode7TileSheet.TileWords; pixel++)
            {
                int expected = vram[(tile * Mode7TileSheet.TileWords + pixel) * 2 + 1];
                int at = (tiles.Picture.Corner.Y + corner.Y + pixel / 8) * sheet.Size.X + tiles.Picture.Corner.X + corner.X + pixel % 8;
                AssertTrue(sheet.PaletteIndices[at] == expected && (expected == 0 || sheet.Pixels[at] == sheet.StandardColours[expected].ToRgba32()),
                    $"{tiles.Name}: tile {tile} pixel {pixel} paints index {sheet.PaletteIndices[at]} where its characters name {expected}");
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
            runtime.StepFrame(0);
            rebound += AssertBackgroundPaintsTiles(runtime, $"{room.Name} frame {frame}");
        }
        AssertEqual(expectRebound, rebound > 0, $"{room.Name}'s background shows cells over artwork bound after its load");
        Console.WriteLine($"Room background images: {room.Name} painted its two pages for {Frames} frames, {rebound} cell pictures over rebound artwork.");
    }

    /// <summary>
    /// Asserts BG2's library background paints, pixel for pixel over both pages, the palette index, priority and standard
    /// colour BG2's tilemap tiles paint over the characters the bindings compose into VRAM; returns its rebound cells.
    /// </summary>
    private static int AssertBackgroundPaintsTiles(SuperMetroidRuntime runtime, string where)
    {
        RoomBackgroundImagePlane plane = runtime.Display.Latched.Rooms?.Bg2Image
            ?? throw new InvalidOperationException($"{where}: BG2 shows no library background image.");
        PpuMemorySnapshot memory = PpuMemorySnapshot.Capture(runtime.Vram, runtime.Cgram, runtime.DisplayedSprites, runtime.BgArtwork);
        ReadOnlySpan<byte> vram = memory.Vram;
        // A 32x64 map lays the two pages out one above the other, as the image does.
        IBgSource shown = plane.At(new Extent16<TilemapCells>(RoomBackgroundImage.PageTiles, RoomBackgroundImage.Pages * RoomBackgroundImage.PageTiles));
        for (int y = 0; y < RoomBackgroundImage.Pages * RoomBackgroundImage.PagePixels; y++)
        for (int x = 0; x < RoomBackgroundImage.PagePixels; x++)
        {
            int at = GameplayVramLayout.Bg2TilemapWord * 2 + (y / CharacterPixels * RoomBackgroundImage.PageTiles + x / CharacterPixels) * 2;
            var word = new SnesBgTilemapWord((ushort)(vram[at] | vram[at + 1] << 8));
            int pixelRow = word.FlipVertically ? CharacterPixels - 1 - y % CharacterPixels : y % CharacterPixels;
            int bit = 7 - (word.FlipHorizontally ? CharacterPixels - 1 - x % CharacterPixels : x % CharacterPixels);
            int address = word.CharacterIndex * RoomBlockPictureNames.CharacterWords * sizeof(ushort) + pixelRow * 2;
            int colour = (vram[address] >> bit & 1) | (vram[address + 1] >> bit & 1) << 1
                | (vram[address + 16] >> bit & 1) << 2 | (vram[address + 17] >> bit & 1) << 3;
            int expected = colour == 0 ? 0 : word.PaletteIndex * 16 + colour;
            AssertTrue(shown.TrySample(new Position16<BgPlanePixels>((ushort)x, (ushort)y), out BgSample sample),
                $"{where}: the background shows ({x}, {y})");
            SpriteSheetFrame picture = sample.Picture ?? throw new InvalidOperationException($"{where}: the background shows a tile at ({x}, {y}).");
            SpriteImage sheet = picture.Sheet;
            int pixel = (picture.Corner.Y + sample.PicturePixel.Y) * sheet.Size.X + picture.Corner.X + sample.PicturePixel.X;
            AssertTrue(sheet.PaletteIndices[pixel] == expected && (colour == 0 || ((sheet.Priorities[pixel] != 0) == word.HasPriority
                    && sheet.Pixels[pixel] == sheet.StandardColours[expected].ToRgba32())),
                $"{where}: the background paints index {sheet.PaletteIndices[pixel]} at ({x}, {y}) where its tile ${word.Raw:X4} paints {expected}");
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
    private static void Check(RoomHeaderDefinition room, bool expectUploads, CartridgeRoomState? state = null,
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
            runtime.StepFrame(0);
            (int pictures, int over) = AssertPicturesPaintLive(runtime, $"{room.Name} frame {frame}");
            checkedPictures += pictures;
            uploaded += over;
        }
        AssertTrue(checkedPictures > 0, $"{room.Name} shows block pictures");
        AssertEqual(expectUploads, uploaded > 0, $"{room.Name} shows blocks over artwork bound after its load");
        Console.WriteLine($"Room block pictures: {room.Name} showed {checkedPictures} pictures, {uploaded} over artwork bound after its load.");
    }

    /// <summary>
    /// Asserts each picture the displayed BG1 resolves paints, pixel for pixel, the palette index, priority and standard
    /// colour its tiles paint over the characters the bindings compose into VRAM; returns the pictures checked and those
    /// over artwork bound after the room's load.
    /// </summary>
    private static (int Pictures, int Uploaded) AssertPicturesPaintLive(SuperMetroidRuntime runtime, string where)
    {
        RoomBackgrounds rooms = runtime.Display.Latched.Rooms ?? throw new InvalidOperationException($"{where}: no room backgrounds are displayed.");
        RoomBlockPictureTable table = rooms.Bg1.Pictures ?? throw new InvalidOperationException($"{where}: BG1 shows no block pictures.");
        RoomCharacterSource characters = RoomCharacterSource.Of(rooms.Characters, runtime.BgArtwork, runtime.EnemySession.ObjCharacters);
        PpuMemorySnapshot memory = PpuMemorySnapshot.Capture(runtime.Vram, runtime.Cgram, runtime.DisplayedSprites, runtime.BgArtwork);
        ReadOnlySpan<byte> vram = memory.Vram;
        ReadOnlySpan<byte> definitions = rooms.Bg1.Plane.Definitions.AsSpan();
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
                var word = new SnesBgTilemapWord(SnesLittleEndian.Word(definitions[at], definitions[at + 1]));
                BgArtworkShown shown = characters.Artwork.Showing(characters.CharacterBase + word.CharacterIndex * RoomBlockPictureNames.CharacterWords,
                    RoomBlockPictureNames.CharacterWords);
                overUpload |= shown.Artwork != RoomBgArtwork.CommonRoomElements && !shown.Artwork.Name.StartsWith("tileset characters ", StringComparison.Ordinal);
                for (int y = 0; y < CharacterPixels; y++)
                {
                    int row = word.FlipVertically ? CharacterPixels - 1 - y : y;
                    int address = (characters.CharacterBase + word.CharacterIndex * RoomBlockPictureNames.CharacterWords) * sizeof(ushort) + row * 2;
                    for (int x = 0; x < CharacterPixels; x++)
                    {
                        int bit = 7 - (word.FlipHorizontally ? CharacterPixels - 1 - x : x);
                        int colour = (vram[address] >> bit & 1) | (vram[address + 1] >> bit & 1) << 1
                            | (vram[address + 16] >> bit & 1) << 2 | (vram[address + 17] >> bit & 1) << 3;
                        int pixel = (picture.Corner.Y + tile / 2 * CharacterPixels + y) * sheet.Size.X + picture.Corner.X + tile % 2 * CharacterPixels + x;
                        int expected = colour == 0 ? 0 : word.PaletteIndex * 16 + colour;
                        AssertTrue(sheet.PaletteIndices[pixel] == expected && (colour == 0 || (sheet.Priorities[pixel] != 0) == word.HasPriority)
                            && (colour == 0 || sheet.Pixels[pixel] == sheet.StandardColours[expected].ToRgba32()),
                            $"{where}: definition {definition}'s picture paints index {sheet.PaletteIndices[pixel]} priority {sheet.Priorities[pixel]} " +
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
    internal static void AssertRevealPaintsWords(SuperMetroidRuntime runtime, ReadOnlySpan<ushort> words, string where)
    {
        XrayRevealPlane reveal = runtime.Display.Latched.XrayReveal ?? throw new InvalidOperationException($"{where}: no X-ray reveal is displayed.");
        PpuMemorySnapshot memory = PpuMemorySnapshot.Capture(runtime.Vram, runtime.Cgram, runtime.DisplayedSprites, runtime.BgArtwork);
        ReadOnlySpan<byte> vram = memory.Vram;
        for (int row = 0; row < XrayTilemapLayout.MetatileRows; row++)
        for (int column = 0; column < XrayTilemapLayout.MetatileColumns; column++)
        for (int y = 0; y < RoomBlockSheet.BlockPixels; y++)
        for (int x = 0; x < RoomBlockSheet.BlockPixels; x++)
        {
            var word = new SnesBgTilemapWord(words[XrayRevealTilemap.Destination(column, row) + y / CharacterPixels * 32 + x / CharacterPixels]);
            int pixelRow = word.FlipVertically ? CharacterPixels - 1 - y % CharacterPixels : y % CharacterPixels;
            int bit = 7 - (word.FlipHorizontally ? CharacterPixels - 1 - x % CharacterPixels : x % CharacterPixels);
            int address = word.CharacterIndex * RoomBlockPictureNames.CharacterWords * sizeof(ushort) + pixelRow * 2;
            int colour = (vram[address] >> bit & 1) | (vram[address + 1] >> bit & 1) << 1
                | (vram[address + 16] >> bit & 1) << 2 | (vram[address + 17] >> bit & 1) << 3;
            int expected = colour == 0 ? 0 : word.PaletteIndex * 16 + colour;
            AssertTrue(reveal.TrySample(new Position16<BgPlanePixels>((ushort)(column * 16 + x), (ushort)(row * 16 + y)), out BgSample sample),
                $"{where}: the reveal shows block ({column}, {row})");
            SpriteSheetFrame picture = sample.Picture ?? throw new InvalidOperationException($"{where}: the reveal shows a tile, not a picture.");
            SpriteImage sheet = picture.Sheet;
            int pixel = (picture.Corner.Y + sample.PicturePixel.Y) * sheet.Size.X + picture.Corner.X + sample.PicturePixel.X;
            AssertTrue(sheet.PaletteIndices[pixel] == expected && (colour == 0 || (sheet.Priorities[pixel] != 0) == word.HasPriority)
                && (colour == 0 || sheet.Pixels[pixel] == sheet.StandardColours[expected].ToRgba32()),
                $"{where}: reveal block ({column}, {row}) pixel ({x}, {y}) paints index {sheet.PaletteIndices[pixel]} where its tile ${word.Raw:X4} paints {expected}");
        }
    }

    /// <summary>Bytes per block definition.</summary>
    private const int RoomLevelPlaneDefinitionBytes = SuperMetroid.Core.Rooms.Backgrounds.RoomLevelPlane.DefinitionBytes;

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
}
