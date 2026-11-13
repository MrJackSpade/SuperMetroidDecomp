using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Verification.Harness;
using SuperMetroid.AssetExtraction.Areas.Escape;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core.World;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.AssetExtraction.Bosses;
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
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.World.RoomStates;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.World.RoomStates;
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
    /// <summary>The installation's room block sheet inputs, which the boss rooms' characters are read from.</summary>
    private static readonly Lazy<RoomBlockSheetInputs> BossRoomInputs =
        new(() => RoomBlockSheetInputs.Load(SuperMetroid.TestSupport.RepositoryInstallation.Verification.CartridgeImportContent.Directory));

    private static readonly Lazy<IReadOnlyDictionary<TilesetDefinition, RoomBlockUploads.TilesetUploads>> TilesetUploads =
        new(() => SharedFixtureTiming.Load("Fixture: room block uploads by tileset",
            () => RoomBlockUploads.ByTileset(RoomBlockSheetInputs.Load(SuperMetroid.TestSupport.RepositoryInstallation.Verification.CartridgeImportContent.Directory))));

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
        Check(RoomHeaderDefinitions.MorphBall, expectUploads: false);
        Check(RoomHeaderDefinitions.EastSandHall, expectUploads: true);
        // The Ceres Ridley arena's blocks read the OBJ character table's enemy tiles: the page its load composes is the
        // pictures' baseline, and only the getaway's looks, which show later in the encounter, change them (#1466).
        Check(RoomHeaderDefinitions.CeresRidley, expectUploads: false);
        // The treadmills run once Phantoon, the Wrecked Ship's boss, is defeated.
        Check(RoomHeaderDefinitions.WreckedShipMainShaft, expectUploads: true,
            RoomStateLists.GetStates(RoomHeaderDefinitions.WreckedShipMainShaft)
                .Single(state => state.Fx?.Records.Any(record => !record.AnimatedTiles.IsEmpty) == true),
            runtime => runtime.System.SetBossBits((int)AreaId.WreckedShip, BossBits.AreaBoss));
    }

    /// <summary>
    /// In the Parlor, whose rock background shows its room's load, and in the Tourian statue room, whose statues' frames
    /// rebind the characters its background reads, BG2 draws the library background's image, and every pixel of its two
    /// pages paints what BG2's tilemap tiles paint over the live characters, frame after frame. Crateria Power Bombs runs
    /// the sky's main loop without its band HDMA, so BG2 shows its rock background's image, not the sky (#1403).
    ///  Each room is its own suite, as each loads and steps its own room.</summary>
    internal static void VerifyParlorBackgroundImage() => CheckBackground(RoomHeaderDefinitions.Parlor, expectRebound: false);

    /// <summary>Checks the Crateria Power Bombs room's background image as <see cref="VerifyParlorBackgroundImage"/> checks the Parlor's.</summary>
    internal static void VerifyCrateriaPowerBombsBackgroundImage() => CheckBackground(RoomHeaderDefinitions.CrateriaPowerBombs, expectRebound: false);

    /// <summary>Checks the Statues room's background image, which rebinds, as <see cref="VerifyParlorBackgroundImage"/> checks the Parlor's.</summary>
    internal static void VerifyStatuesBackgroundImage() => CheckBackground(RoomHeaderDefinitions.Statues, expectRebound: true);

    /// <summary>
    /// Phantoon's room, whose library background copies the boss's own layer (#1649), binds no background picture as it
    /// loads, and its first frame draws none: the background shows only what the boss's body draws.
    /// </summary>
    internal static void VerifyPhantoonRoomDrawsNoBackgroundImage()
    {
        SuperMetroidRuntime runtime = LoadRoom(RoomHeaderDefinitions.Phantoon);
        runtime.StepFrame(0);
        AssertEqual<RoomLibraryBackgroundPicture>(new RoomLibraryBackgroundPicture.ShownOtherwise(LibraryBackgroundView.BossBody),
            RoomScopeOf(runtime).BackgroundImage, "Phantoon's room binds no background picture");
        RoomBackgrounds rooms = runtime.Display.Latched.Rooms ?? throw new InvalidOperationException("Phantoon's room shows no room backgrounds.");
        AssertTrue(rooms.BackgroundImage is null, "Phantoon's room draws no background picture");
        Console.WriteLine("Room background images: Phantoon's room binds and draws no background picture.");
    }

    /// <summary>Checks the Landing Site's sky image (<see cref="CheckSky"/>).</summary>
    internal static void VerifyLandingSiteSkyImage() => CheckSky(RoomHeaderDefinitions.LandingSite);

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
            rows++;
            for (int y = row * CharacterPixels; y < (row + 1) * CharacterPixels; y++)
            {
                for (int x = 0; x < RoomBackgroundImageGeometryConstants.PagePixels; x++)
                {
                    var word = new SnesBgTilemapWord(sky.Tiles[row * SkyTilemap.Columns + x / CharacterPixels].PackedWord);
                    (LayeredPicture picture, Position16<ImagePixels> pixel) = PictureAt(shown.Image, x, y);
                    AssertPaintsTile($"{room.Name}'s sky at ({x}, {y})", vram, word, x % CharacterPixels, y % CharacterPixels, picture, pixel);
                }
            }
        }
        AssertTrue(rows > 0, $"{room.Name}'s sky shows rows");
        Console.WriteLine($"Room background images: {room.Name}'s sky painted its {rows} shown rows from their image.");
    }

    /// <summary>
    /// The picture and its pixel a background image shows at (<paramref name="x"/>, <paramref name="y"/>) of its pages or
    /// rows laid one above the next: a rebound 16x16 cell's own picture, otherwise the image's.
    /// </summary>
    private static (LayeredPicture Picture, Position16<ImagePixels> Pixel) PictureAt(RoomBackgroundImagePlane image, int x, int y)
    {
        const int cell = 16;
        return image.Cell(y / cell * (image.Size.X / cell) + x / cell) is { } picture
            ? (picture, new Position16<ImagePixels>((ushort)(x % cell), (ushort)(y % cell)))
            : (image.Picture, new Position16<ImagePixels>((ushort)x, (ushort)y));
    }

    /// <summary>
    /// Checks the pixel at (<paramref name="x"/>, <paramref name="y"/>) of <paramref name="word"/>'s tile, as <paramref name="vram"/>'s
    /// characters paint it, is whole picture <paramref name="picture"/>'s pixel at <paramref name="pixel"/>: its palette index
    /// and its standard colour. A whole picture, such as a boss body's, is drawn at one depth (#1634), so the tile's priority
    /// is not a property of its pixels.
    /// </summary>
    internal static void AssertPaintsTile(string context, ReadOnlySpan<byte> vram, SnesBgTilemapWord word, int x, int y,
        SpriteSheetFrame picture, Position16<ImagePixels> pixel)
    {
        int colour = TileColour(vram, word, x, y);
        int expected = colour == 0 ? 0 : word.PaletteIndex * 16 + colour;
        SpriteImage sheet = picture.Sheet;
        int at = At(picture, pixel);
        // The message is built only for a failing pixel, since every checked pixel comes through here.
        if (!(colour == 0 ? sheet.Pixels[at].A == 0 : ShowsIndex(sheet, at, expected)))
            AssertTrue(false, $"{context}: the picture paints index {Painted(sheet, at)} where tile ${word.Raw:X4} paints {expected}");
    }

    /// <summary>
    /// Checks the pixel at (<paramref name="x"/>, <paramref name="y"/>) of <paramref name="word"/>'s tile, as <paramref name="vram"/>'s
    /// characters paint it, is <paramref name="picture"/>'s pixel at <paramref name="pixel"/> in the part the tile's priority
    /// puts it in (#1634), with its palette index and its standard colour, and that the other part shows nothing there.
    /// </summary>
    internal static void AssertPaintsTile(string context, ReadOnlySpan<byte> vram, SnesBgTilemapWord word, int x, int y,
        LayeredPicture picture, Position16<ImagePixels> pixel)
    {
        int colour = TileColour(vram, word, x, y);
        int expected = colour == 0 ? 0 : word.PaletteIndex * 16 + colour;
        // The message is built only for a failing pixel, since every checked pixel comes through here.
        if (!PaintsInPart(picture, pixel, expected, word.HasPriority))
            AssertTrue(false, $"{context}: the picture's parts paint {Describe(picture, pixel)} where tile ${word.Raw:X4} paints {expected} {PartOf(word)}");
    }

    /// <summary>
    /// Whether <paramref name="picture"/> paints, at <paramref name="pixel"/> of its rectangle, what a tile in front when
    /// <paramref name="inFront"/> and behind otherwise paints there as palette index <paramref name="expected"/> (#1634): the
    /// part of the tile's depth paints the index in its standard colour and the other part shows nothing there; where the
    /// tile paints nothing, neither part does.
    /// </summary>
    private static bool PaintsInPart(LayeredPicture picture, Position16<ImagePixels> pixel, int expected, bool inFront)
    {
        SpriteSheetFrame? shown = inFront ? picture.InFront : picture.Behind;
        SpriteSheetFrame? other = inFront ? picture.Behind : picture.InFront;
        if (expected == 0)
            return PaintsNothing(picture.Behind, pixel) && PaintsNothing(picture.InFront, pixel);
        if (shown is null)
            return false;
        return ShowsIndex(shown.Sheet, At(shown, pixel), expected) && PaintsNothing(other, pixel);
    }

    /// <summary>Whether <paramref name="part"/> shows nothing at <paramref name="pixel"/>: it is not installed, or its pixel there is transparent.</summary>
    private static bool PaintsNothing(SpriteSheetFrame? part, Position16<ImagePixels> pixel) =>
        part is null || part.Sheet.Pixels[At(part, pixel)].A == 0;

    /// <summary>
    /// Whether two-bit <paramref name="picture"/> paints, at <paramref name="pixel"/>, what a tile in front when
    /// <paramref name="inFront"/> and behind otherwise paints there as palette index <paramref name="expected"/>, whose group's
    /// clear colour is <paramref name="clear"/> (#1634): an opaque pixel in the part of the tile's depth, in its standard
    /// colour, with the other part's pixel clear in that group; a clear pixel in every installed part.
    /// </summary>
    private static bool PaintsTwoBitInPart(LayeredPicture picture, Position16<ImagePixels> pixel, int expected, int clear, bool inFront)
    {
        SpriteSheetFrame? shown = inFront ? picture.InFront : picture.Behind;
        SpriteSheetFrame? other = inFront ? picture.Behind : picture.InFront;
        if (expected == clear)
            return picture.Parts.Any() && picture.Parts.All(part => PaintsClear(part, pixel));
        if (shown is null)
            return false;
        return ShowsIndex(shown.Sheet, At(shown, pixel), expected) && (other is null || PaintsClear(other, pixel));
    }

    /// <summary>Whether two-bit <paramref name="part"/>'s pixel at <paramref name="pixel"/> is clear.</summary>
    private static bool PaintsClear(SpriteSheetFrame part, Position16<ImagePixels> pixel) => part.Sheet.Pixels[At(part, pixel)].A == 0;

    /// <summary>The place in <paramref name="part"/>'s sheet of the pixel at <paramref name="pixel"/> of its rectangle.</summary>
    private static int At(SpriteSheetFrame part, Position16<ImagePixels> pixel) =>
        (part.Corner.Y + pixel.Y) * part.Sheet.Size.X + part.Corner.X + pixel.X;

    /// <summary>The palette index each installed part of <paramref name="picture"/> paints at <paramref name="pixel"/>, for a failure's message.</summary>
    private static string Describe(LayeredPicture picture, Position16<ImagePixels> pixel) =>
        $"behind {(picture.Behind is { } behind ? Painted(behind.Sheet, At(behind, pixel)).ToString(System.Globalization.CultureInfo.InvariantCulture) : "none")}, "
        + $"in front {(picture.InFront is { } inFront ? Painted(inFront.Sheet, At(inFront, pixel)).ToString(System.Globalization.CultureInfo.InvariantCulture) : "none")}";

    /// <summary>The part of its picture <paramref name="word"/>'s priority puts its pixels in, for a failure's message.</summary>
    private static string PartOf(SnesBgTilemapWord word) => word.HasPriority ? "in front" : "behind";

    /// <summary>
    /// Whether <paramref name="sheet"/>'s pixel at <paramref name="at"/> is opaque and shows the colour key the import composed
    /// palette index <paramref name="index"/> into (#1633): the index's colour, recoloured by every shade as the index is.
    /// Pictures that differ only in where their colours came from are held once, so a pixel is checked by the colour it
    /// shows, not by the index of the frame that holds its picture.
    /// </summary>
    private static bool ShowsIndex(SpriteImage sheet, int at, int index) =>
        sheet.Pixels[at].A != 0 && InstalledPictureColoursConstants.KeyComposedAs(sheet, index) is { } key && sheet.KeyAt(at) == key;

    /// <summary>The palette index the import composed <paramref name="sheet"/>'s pixel at <paramref name="at"/> as (#1544); zero for a transparent pixel.</summary>
    private static int Painted(SpriteImage sheet, int at) => InstalledPictureColoursConstants.ComposedIndexAt(sheet, at);

    /// <summary>The colour <paramref name="sheet"/>'s pixels composed as palette index <paramref name="index"/> hold (#1633).</summary>
    /// <exception cref="InvalidDataException">The sheet has no pixel composed as that index.</exception>
    private static RgbColor StandardOf(SpriteImage sheet, int index) =>
        (InstalledPictureColoursConstants.KeyComposedAs(sheet, index)
            ?? throw new InvalidDataException($"{sheet} has no colour composed as palette index {index}.")).Colour;

    /// <summary>The colour, within its palette row, that <paramref name="word"/>'s tile paints at (<paramref name="x"/>, <paramref name="y"/>) over <paramref name="vram"/>'s characters.</summary>
    private static int TileColour(ReadOnlySpan<byte> vram, SnesBgTilemapWord word, int x, int y)
    {
        int pixelRow = word.FlipVertically ? CharacterPixels - 1 - y : y;
        int bit = 7 - (word.FlipHorizontally ? CharacterPixels - 1 - x : x);
        int address = word.CharacterIndex * TileCharacterWords * sizeof(ushort) + pixelRow * 2;
        return (vram[address] >> bit & 1) | (vram[address + 1] >> bit & 1) << 1
            | (vram[address + 16] >> bit & 1) << 2 | (vram[address + 17] >> bit & 1) << 3;
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
        WrittenPictureSheet sheet = (runtime.Room.Scope?.BackgroundWrittenPictures ?? throw new InvalidOperationException($"{room.Name} has no written pictures.")).Sheet;
        ReadOnlySpan<byte> vram = runtime.ComposedVram();
        var typewriter = new EscapeTypewriterState((runtime.BossArtwork.EscapeTypewriterPresentation
            ?? throw new InvalidOperationException("No escape typewriter.")).Get(EscapeTypewriterProgramId.Zebes));
        // The typewriter types each glyph's picture by name (#1466); the overlay is checked against the tiles it typed.
        RoomWrittenOverlay overlay = RoomWrittenOverlay.Empty;
        var typedWords = new int[TwoScreenTilemapCells.Count];
        Array.Fill(typedWords, RoomWrittenOverlay.Unwritten);
        while (!typewriter.Step(out EscapeTypewriterGlyph? glyph))
        {
            if (glyph is { } typed)
            {
                overlay = overlay.With(new WrittenPicture(typed.Picture, typed.Corner));
                typedWords[TwoScreenTilemapCells.IndexOf(typed.Corner)] = EscapeTypewriterGlyphTilesConstants.Of(EscapeTypewriterProgramId.Zebes, typed).PackedWord;
            }
        }
        BgTilePicture?[] pictures = TwoScreenTilemapCells.Lay(new WrittenPictureResolver(sheet).Resolve(overlay, AppearanceOf(runtime, RoomScopeOf(runtime).ArtworkFrames)));
        int glyphs = 0;
        for (int index = 0; index < TwoScreenTilemapCells.Count; index++)
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
    /// Loads boss room <paramref name="room"/> and checks every whole frame picture of <paramref name="owner"/>'s body sheet
    /// paints, at each cell its frame writes, the tile word written there over the room's characters as it loads (#1515).
    /// </summary>
    private static void CheckBody(RoomHeaderDefinition room, BossBodyOwner owner)
    {
        SuperMetroidRuntime runtime = LoadRoom(room);
        runtime.StepFrame(0);
        BossBodySheet sheet = (runtime.BossArtwork.BodySheets ?? throw new InvalidOperationException("No boss body sheets are installed.")).Of(owner);
        BossSourceArtwork bosses = InstalledFixtures.BossArtwork;
        byte[] loaded = runtime.ComposedVram();
        // A frame pictured under a room animation's frames (Crocomire's melt states, #1513) shows those frames' characters
        // where they paint, as the body painter composes it, and the room's load elsewhere.
        IReadOnlyList<AnimatedCharacterPlacement> animated = RoomBlockUploads.ShownUnderBoss(BossRoomInputs.Value,
            RoomStateLists.GetStates(room).First(state => !state.EnemyPopulation.Records.IsEmpty)).Animated;
        int frames = 0;
        foreach (BossBodyFrameSource frame in BossBodySheetComposer.Frames(bosses, owner))
        {
            byte[] vram = loaded;
            if (frame.Selected.Count > 0)
            {
                vram = [.. loaded];
                foreach (AnimatedCharacterPlacement placement in animated.Where(placement => placement.Set == BgCharacterSet.Tiles
                    && frame.Selected.TryGetValue(placement.Frame.Animation, out ArtworkFrame selected) && selected == placement.Frame))
                    placement.Placement.Bytes.Span.CopyTo(vram.AsSpan(placement.Placement.FirstCharacter * TileCharacterWords * sizeof(ushort)));
            }
            BossBodyFrame shown = sheet.Frame(frame.Name);
            foreach ((int cell, BackgroundTile tile) in frame.Cells)
            {
                Position16<BackgroundPixels> at = BossBodyPainter.PixelOf(cell);
                var word = new SnesBgTilemapWord(tile.PackedWord);
                bool inks = false;
                for (int y = 0; y < CharacterPixels; y++)
                for (int x = 0; x < CharacterPixels; x++)
                {
                    inks |= TileColour(vram, word, x, y) != 0;
                    // A frame that paints nothing has no picture: the tile it writes there paints nothing (#1513).
                    if (shown.Picture is { } picture)
                        AssertPaintsTile($"{owner} {shown.Name} cell {cell} pixel ({x}, {y})", vram, word, x, y, picture,
                            new Position16<ImagePixels>(at.X - shown.Corner.X + x, at.Y - shown.Corner.Y + y));
                    else
                        AssertTrue(TileColour(vram, word, x, y) == 0,
                            $"{owner} {shown.Name} clears cell {cell}, where tile ${word.Raw:X4} paints pixel ({x}, {y})");
                }
                // The frame is drawn whole at the depth the tier of every word it paints with names (#1634): in front of the
                // sprites for the upper tier, behind them for the lower.
                if (inks)
                    AssertTrue(shown.Depth == (word.HasPriority ? BossBodyDepth.InFrontOfSprites : BossBodyDepth.BehindSprites),
                        $"{owner} {shown.Name} is drawn {shown.Depth}, but its tile ${word.Raw:X4} at cell {cell} is written in the {(word.HasPriority ? "upper" : "lower")} tier");
            }
            frames++;
        }
        AssertTrue(frames > 0, $"{room.Name}'s boss draws frames");
        Console.WriteLine($"Boss body pictures: {owner}'s sheet pictured {frames} whole frames over {room.Name} as it loads.");
    }

    /// <summary>
    /// Confirms BG3 draws from the tileset's BG3 word sheet (#1392): in a room of each effect whose page BG3 shows, and in
    /// Kraid's room, whose library background moves the HUD's characters, the latched BG3 plane draws from pictures, and
    /// every word of the effect's page under a stocked HUD is painted in the page's picture as its two-bit character paints
    /// at the HUD character base BG3 shows, colour zero's row included (#1514); a room showing no page shows none. The words are resolved against the characters as the check
    /// reads them, since an effect can bind its next frame after the latch.
    /// </summary>
    internal static void VerifyBg3Pictures()
    {
        var stocked = new HudSnapshot(1234, 1499, 123, 230, 45, 50, 6, 50,
            Enum.GetValues<SamusEquipmentFlags>().Aggregate((all, item) => all | item), HudItem.Missile, 300, ReserveTankMode.Auto);
        var rooms = new List<RoomHeaderDefinition> { RoomHeaderDefinitions.Kraid };
        foreach (RoomFxType effect in new[] { RoomFxType.Lava, RoomFxType.Acid, RoomFxType.Water, RoomFxType.Rain, RoomFxType.Fog, RoomFxType.Spores })
        {
            rooms.Add(RetailRoomHeaders.All.First(room => RoomStateLists.GetStates(room)[0].Fx?.Records
                .Any(record => record.Type == effect) == true));
        }
        foreach (RoomHeaderDefinition room in rooms)
        {
            SuperMetroidRuntime runtime = LoadRoom(room, stocked);
            runtime.StepFrame(0);
            GameplayDisplayLatch latched = runtime.Display.Latched;
            FxOverlayPlane plane = latched.FxOverlay ?? throw new InvalidOperationException($"{room.Name} latches no BG3 plane.");
            // The plane names its pictures (#1466); the installed pages spell the words the cartridge writes for them.
            ushort[] planeWords = plane.WordsIn(InstalledFixtures.ImportMaps.RoomFxPages);
            AssertTrue(latched.Rooms?.FxOverlay is not null, $"{room.Name}: BG3 draws from its pictures");
            ReadOnlySpan<byte> vram = runtime.ComposedVram();
            BgCharacterSet layer3 = BgCharacterSet.Layer3;
            int characterBase = BgCharacterWordsConstants.WordOf(layer3, 0);
            ArtworkFrameSelections frames = RoomScopeOf(runtime).ArtworkFrames;
            var effects = new RoomAnimatedCharacters(UploadsOf(runtime).Bg3Options, layer3, AnimatedTilesOf(runtime, room.Name));
            // The overlay shows the effect's page picture (#1514): each of its cells paints the word the cartridge writes
            // there, from the row it writes the page to.
            FxOverlayPage page = new WrittenPictureResolver((runtime.Room.Scope
                ?? throw new InvalidOperationException($"{room.Name} has no room scope.")).FxOverlayPictures.Sheet).Resolve(plane, AppearanceOf(runtime, frames));
            // A room whose effect shows no page shows nothing on the overlay.
            if (page.Picture is not { } shownPage)
            {
                AssertTrue(plane.EffectPage is null, $"{room.Name}: BG3 shows its effect's page");
                Console.WriteLine($"BG3 pictures: {room.Name} shows no effect page.");
                continue;
            }
            int firstWord = FxOverlayPlaneConstants.EffectPageFirstRow * FxOverlayPlaneConstants.Columns;
            // The page picture is one period across of what the plane shows, repeated at its width (#1675).
            int pageWidth = (shownPage.Size ?? throw new InvalidOperationException($"{room.Name}: its effect page installs neither part.")).X;
            for (int index = firstWord; index < FxOverlayPlaneConstants.Columns * FxOverlayPlaneConstants.Rows; index++)
            {
                int cellX = (index - firstWord) % FxOverlayPlaneConstants.Columns * CharacterPixels;
                int cellY = (index - firstWord) / FxOverlayPlaneConstants.Columns * CharacterPixels;
                var word = new SnesBgTilemapWord(planeWords[index]);
                // A character an effect's animation paints shows the frame it selected (#1406), which VRAM never holds.
                ReadOnlySpan<byte> bytes = effects.Bytes(frames, word.CharacterIndex, vram[(characterBase * sizeof(ushort))..], out _);
                for (int y = 0; y < CharacterPixels; y++)
                for (int x = 0; x < CharacterPixels; x++)
                {
                    int pixelRow = word.FlipVertically ? CharacterPixels - 1 - y : y;
                    int bit = 7 - (word.FlipHorizontally ? CharacterPixels - 1 - x : x);
                    int address = pixelRow * 2;
                    int colour = (bytes[address] >> bit & 1) | (bytes[address + 1] >> bit & 1) << 1;
                    int expected = word.PaletteIndex * 4 + colour;
                    var at = new Position16<ImagePixels>((cellX + x) % pageWidth, cellY + y);
                    // The message names the character's picture, which costs a lookup, so it is built only for a failing pixel.
                    if (!PaintsTwoBitInPart(shownPage, at, expected, word.PaletteIndex * 4, word.HasPriority))
                    {
                        AssertTrue(false,
                            $"{room.Name}: BG3 word {index} ${word.Raw:X4} pixel ({x}, {y}) paints {Describe(shownPage, at)} {PartOf(word)} where its character, "
                            + $"{runtime.BgArtwork.Showing(layer3, word.CharacterIndex, 1)}, paints {expected}");
                    }
                }
            }
            Console.WriteLine($"BG3 pictures: {room.Name}'s HUD and effect page painted from their pictures at base ${characterBase:X4}.");
        }
    }

    /// <summary>
    /// Confirms a Ceres room's turning floor draws its tileset's installed floor (#1392, #1467): in the Ceres elevator room
    /// the latched floor is the installed picture, and each of its pixels paints the colour its cell's tile's characters
    /// name in the room's composed memory.
    /// </summary>
    internal static void VerifyMode7TilePictures()
    {
        RoomHeaderDefinition room = RoomHeaderDefinitions.CeresElev;
        SuperMetroidRuntime runtime = LoadRoom(room);
        runtime.StepFrame(0);
        AffineFloor shown = runtime.Display.Latched.Rooms?.Floor ?? throw new InvalidOperationException($"{room.Name} shows no floor.");
        RoomFloorSheet floor = (runtime.Room.LoadedScope.Floor ?? throw new InvalidOperationException($"{room.Name} has no floor.")).Sheet;
        AssertTrue(ReferenceEquals(shown.Image, floor.Floor.Image), $"{room.Name} shows its tileset's installed floor");
        FloorMemory memory = AffineFloorConstants.Split(runtime.ComposedVram());
        byte[] map = memory.Map, pixels = memory.Pixels;
        SpriteImage sheet = shown.Image.Sheet;
        // The installed image holds the part of the floor that shows anything, placed by its origin (#1649); the floor
        // beyond it shows nothing.
        int left = -shown.Image.Origin.X, top = -shown.Image.Origin.Y;
        int checkedPixels = 0;
        for (int y = 0; y < AffineFloor.SidePixels; y++)
        {
            for (int x = 0; x < AffineFloor.SidePixels; x++)
            {
                int tile = map[y / 8 * (AffineFloor.SidePixels / 8) + x / 8];
                int expected = pixels[tile * 64 + y % 8 * 8 + x % 8];
                checkedPixels++;
                if (x < left || x >= left + shown.Image.Size.X || y < top || y >= top + shown.Image.Size.Y)
                {
                    if (expected != 0)
                        AssertTrue(false, $"{floor.Name}: floor pixel ({x}, {y}) lies beyond the installed image, where its tile's characters name {expected}");
                    continue;
                }
                int at = (shown.Image.Corner.Y + y - top) * sheet.Size.X + shown.Image.Corner.X + x - left;
                // The message is built only for a failing pixel.
                if (!(Painted(sheet, at) == expected && (expected == 0 || sheet.Pixels[at] == StandardOf(sheet, expected).ToRgbaPixel())))
                    AssertTrue(false, $"{floor.Name}: floor pixel ({x}, {y}) paints index {Painted(sheet, at)} where its tile's characters name {expected}");
            }
        }
        Console.WriteLine($"Floor pictures: {floor.Name} paints all {checkedPixels} of {room.Name}'s floor pixels as its characters name them.");
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
        // The tileset's animations and the characters each paints are the room's, read once for all its frames.
        var animations = new RoomAnimatedCharacters(UploadsOf(runtime).Options, BgCharacterSet.Tiles, AnimatedTilesOf(runtime, room.Name));
        int rebound = 0;
        for (int frame = 0; frame < Frames; frame++)
        {
            ArtworkFrameSelections latched = SelectionsOf(runtime);
            runtime.StepFrame(0);
            rebound += AssertBackgroundPaintsTiles(runtime, animations, latched, $"{room.Name} frame {frame}");
        }
        AssertEqual(expectRebound, rebound > 0, $"{room.Name}'s background shows cells over artwork bound after its load");
        Console.WriteLine($"Room background images: {room.Name} painted its pictured pages for {Frames} frames, {rebound} cell pictures over rebound artwork.");
    }

    /// <summary>
    /// Asserts BG2's library background paints, pixel for pixel over the pages it pictures (one period of what it shows,
    /// #1651), in the part each tile's priority puts it in
    /// (#1634), the palette index and standard colour the image's tilemap tiles paint over the characters the cartridge would show, as <paramref name="animations"/>
    /// paint them in <paramref name="latched"/>'s frames; returns its rebound cells.
    /// </summary>
    private static int AssertBackgroundPaintsTiles(SuperMetroidRuntime runtime, RoomAnimatedCharacters animations,
        ArtworkFrameSelections latched, string where)
    {
        RoomBackgroundImagePlane plane = runtime.Display.Latched.Rooms?.BackgroundImage
            ?? throw new InvalidOperationException($"{where}: BG2 shows no library background image.");
        ReadOnlySpan<byte> vram = runtime.ComposedVram();
        // The room's load selects the image by name (#1406); its tilemap words are the pages the library list builds, as
        // the import pipeline replays them (#1466).
        string name = (runtime.Room.Scope?.BackgroundImage as RoomLibraryBackgroundPicture.Drawn
            ?? throw new InvalidOperationException($"{where}: the room selects no library background image.")).Image.Image.Name;
        RoomBlockUploads.RoomBackgroundMap map = UploadsOf(runtime).Backgrounds.Single(background => background.Name == name);
        ushort[] words = map.Words;
        int width = map.WidthTiles;
        // The image pictures one period of what the background shows (#1651): one page, or two pages laid out as the room shows them (#1617), side by side or one above the
        // other. Each tile's character is read once for the eight rows of eight pixels it paints.
        for (int tileY = 0; tileY < words.Length / width; tileY++)
        for (int tileX = 0; tileX < width; tileX++)
        {
            var word = new SnesBgTilemapWord(words[tileY * width + tileX]);
            // A character an animation paints shows the frame it selected (#1406), which the composed VRAM never holds.
            ReadOnlySpan<byte> bytes = animations.Bytes(latched, word.CharacterIndex, vram, out _);
            AssertBackgroundTilePaints(plane, word, bytes, tileX, tileY, where);
        }
        int rebound = 0;
        for (int cell = 0; cell < words.Length / RoomBackgroundImageGeometry.CellTiles; cell++)
        {
            if (plane.Cell(cell) is not null)
                rebound++;
        }
        return rebound;
    }

    /// <summary>
    /// Asserts <paramref name="plane"/> paints, at each pixel of the tile at (<paramref name="tileX"/>, <paramref name="tileY"/>)
    /// of its pages, in the part its priority puts it in (#1634), the palette index and standard colour <paramref name="word"/>
    /// paints over its character's <paramref name="bytes"/>.
    /// </summary>
    private static void AssertBackgroundTilePaints(RoomBackgroundImagePlane plane, SnesBgTilemapWord word, ReadOnlySpan<byte> bytes,
        int tileX, int tileY, string where)
    {
        for (int y = tileY * CharacterPixels; y < (tileY + 1) * CharacterPixels; y++)
        for (int x = tileX * CharacterPixels; x < (tileX + 1) * CharacterPixels; x++)
        {
            int pixelRow = word.FlipVertically ? CharacterPixels - 1 - y % CharacterPixels : y % CharacterPixels;
            int bit = 7 - (word.FlipHorizontally ? CharacterPixels - 1 - x % CharacterPixels : x % CharacterPixels);
            int address = pixelRow * 2;
            int colour = (bytes[address] >> bit & 1) | (bytes[address + 1] >> bit & 1) << 1
                | (bytes[address + 16] >> bit & 1) << 2 | (bytes[address + 17] >> bit & 1) << 3;
            int expected = colour == 0 ? 0 : word.PaletteIndex * 16 + colour;
            (LayeredPicture picture, Position16<ImagePixels> picturePixel) = PictureAt(plane, x, y);
            // The message is built only for a failing pixel: building it for each of the pages' pixels was the check's cost.
            if (!PaintsInPart(picture, picturePixel, expected, word.HasPriority))
            {
                AssertTrue(false,
                    $"{where}: the background paints {Describe(picture, picturePixel)} at ({x}, {y}) where its tile ${word.Raw:X4} paints {expected} {PartOf(word)}");
            }
        }
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
        // The tileset's animations and the characters each paints are the room's, read once for all its frames.
        var animations = new RoomAnimatedCharacters(UploadsOf(runtime).Options, BgCharacterSet.Tiles, AnimatedTilesOf(runtime, room.Name));
        int checkedPictures = 0, uploaded = 0;
        for (int frame = 0; frame < Frames; frame++)
        {
            // The frame boundary resolves the pictures from the frames the animations selected by the previous update; the
            // update after it selects the next ones (#1406).
            ArtworkFrameSelections latched = SelectionsOf(runtime);
            runtime.StepFrame(0);
            (int pictures, int over) = AssertPicturesPaintLive(runtime, animations, latched, $"{room.Name} frame {frame}");
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
        foreach (AnimationAppearance animation in uploads.Options.Concat(uploads.Bg3Options).Select(option => option.Input)
            .OfType<AnimationAppearance>().Distinct())
        {
            if (scope.ArtworkFrames.TryShown(animation.Animation, out ArtworkFrame frame))
                copy.Select(frame);
        }
        return copy;
    }

    /// <summary>What the loaded room of <paramref name="runtime"/> shows over its load, with <paramref name="frames"/> its animations selected.</summary>
    private static RoomAppearanceView AppearanceOf(SuperMetroidRuntime runtime, ArtworkFrameSelections frames) =>
        new(RoomScopeOf(runtime).Appearance, frames, runtime.EnemySession.Looks);

    /// <summary>What the loaded room's tileset's rooms show over their characters, as the import pipeline replays it.</summary>
    private static RoomBlockUploads.TilesetUploads UploadsOf(SuperMetroidRuntime runtime) => TilesetUploads.Value[RoomScopeOf(runtime).Assets.Tileset];

    /// <summary>The installed characters of the animated-tile frames <paramref name="runtime"/>'s room <paramref name="room"/> can show.</summary>
    private static RoomFxAnimatedTileAtlas AnimatedTilesOf(SuperMetroidRuntime runtime, string room) =>
        runtime.Artwork.MapPresentation is null ? throw new InvalidOperationException($"{room}: the runtime has no installed presentation.")
            // The animated tiles' characters are import data no installation holds: the cartridge-derived extraction's (#1636).
            : SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.ImportMaps.RoomFxAnimatedTiles;

    /// <summary>
    /// Asserts each picture the displayed BG1 resolves by block identity and appearance (#1431, #1466) paints, pixel for
    /// pixel, in the part each tile's priority puts it in (#1634), the palette index and standard colour the block's tiles paint over the characters the cartridge
    /// would show: the tiles the cartridge's block tables compose, or for a collectible's picture, its kind's own graphics in
    /// its palette selectors' colours (#1538). Returns the pictures checked and those shown for inputs that no longer show
    /// the room's load.
    /// </summary>
    private static (int Pictures, int Uploaded) AssertPicturesPaintLive(SuperMetroidRuntime runtime, RoomAnimatedCharacters tileAnimations,
        ArtworkFrameSelections latched, string where)
    {
        RoomBackgrounds rooms = runtime.Display.Latched.Rooms ?? throw new InvalidOperationException($"{where}: no room backgrounds are displayed.");
        RoomBlockPictureTable table = rooms.Foreground.Pictures ?? throw new InvalidOperationException($"{where}: BG1 shows no block pictures.");
        RoomAppearanceView appearance = AppearanceOf(runtime, latched);
        // The Ceres Ridley arena's blocks read the OBJ character page its uploads compose; any other room's, the bound tiles,
        // and the frames its animations paint over them.
        bool sprites = rooms.BlockArtwork == RoomBlockArtwork.Sprites;
        byte[] tiles = sprites
            ? EnemySheetFixtures.LivePage(runtime.Enemies.GraphicsSet, SuperMetroid.Verification.Enemies.Artwork.UploadedPictureChecks.Shown(runtime)).Characters.ToArray()
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
            // A collectible's picture shows its kind's own graphics, whatever the room loaded or showed before it (#1538).
            bool collectible = CollectibleCellPictures.TryPictureOf(block, out CollectiblePicture shownCollectible);
            bool overUpload = !collectible && scope.Assets.BlockSheet.Inputs.Of(block, rooms.BlockArtwork)
                .Any(input => appearance.ValueOf(input) != RoomAppearanceValues.Loaded);
            ushort[] blockWords = collectible ? CollectibleWords(shownCollectible)
                : stock[(block * BlockTiles)..((block + 1) * BlockTiles)];
            for (int tile = 0; tile < BlockTiles; tile++)
            {
                var word = new SnesBgTilemapWord(blockWords[tile]);
                // A character an animation paints shows the frame it selected (#1406), which the composed VRAM never holds.
                bool animated = false;
                ReadOnlySpan<byte> bytes = collectible
                    ? DynamicCollectibleGraphics.Get(shownCollectible.Kind).Tiles.Span[(word.CharacterIndex * BgCharacterSet.Tiles.CharacterBytes())..]
                    : sprites ? tiles.AsSpan(word.CharacterIndex * BgCharacterSet.Tiles.CharacterBytes())
                    : tileAnimations.Bytes(latched, word.CharacterIndex, tiles, out animated);
                for (int y = 0; y < CharacterPixels; y++)
                {
                    int row = word.FlipVertically ? CharacterPixels - 1 - y : y;
                    int address = row * 2;
                    for (int x = 0; x < CharacterPixels; x++)
                    {
                        int bit = 7 - (word.FlipHorizontally ? CharacterPixels - 1 - x : x);
                        int colour = (bytes[address] >> bit & 1) | (bytes[address + 1] >> bit & 1) << 1
                            | (bytes[address + 16] >> bit & 1) << 2 | (bytes[address + 17] >> bit & 1) << 3;
                        var pixel = new Position16<ImagePixels>(tile % 2 * CharacterPixels + x, tile / 2 * CharacterPixels + y);
                        int expected = colour == 0 ? 0 : word.PaletteIndex * 16 + colour;
                        // The message is built only for a failing pixel.
                        if (!PaintsInPart(picture, pixel, expected, word.HasPriority))
                        {
                            AssertTrue(false,
                                $"{where}: block {block}'s picture paints {Describe(picture, pixel)} {PartOf(word)} " +
                                $"at tile {tile} pixel ({x}, {y}), where its tile ${word.Raw:X4} paints index {expected}{(animated ? " in its animation's frame" : "")}");
                        }
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
    /// The tile words of <paramref name="picture"/>'s frame (#1538): its four characters among its kind's own graphics, as
    /// the cartridge's palette selectors colour them.
    /// </summary>
    private static ushort[] CollectibleWords(CollectiblePicture picture)
    {
        ReadOnlySpan<byte> palettes = DynamicCollectibleGraphics.Get(picture.Kind).PaletteOffsets.Span;
        var tiles = new BackgroundTile[BlockTiles];
        for (int tile = 0; tile < BlockTiles; tile++)
        {
            int character = picture.Frame * BlockTiles + tile;
            tiles[tile] = new BackgroundTile(character, palettes[character], highPriority: false);
        }
        return BackgroundTileWordBytes.Words(tiles);
    }

    /// <summary>Runs the room block picture checks.</summary>
    [VerificationCommand("--room-block-pictures")]
    internal static int RoomBlockPicturesCommand()
    {
        Suite(nameof(VerifyRoomBlockPictures), VerifyRoomBlockPictures);
        Suite(nameof(VerifyParlorBackgroundImage), VerifyParlorBackgroundImage);
        Suite(nameof(VerifyCrateriaPowerBombsBackgroundImage), VerifyCrateriaPowerBombsBackgroundImage);
        Suite(nameof(VerifyStatuesBackgroundImage), VerifyStatuesBackgroundImage);
        Suite(nameof(VerifyPhantoonRoomDrawsNoBackgroundImage), VerifyPhantoonRoomDrawsNoBackgroundImage);
        Suite(nameof(VerifyLandingSiteSkyImage), VerifyLandingSiteSkyImage);
        Suite(nameof(VerifyTileWordPictures), VerifyTileWordPictures);
        Suite(nameof(VerifyBg3Pictures), VerifyBg3Pictures);
        Suite(nameof(VerifyMode7TilePictures), VerifyMode7TilePictures);
        return 0;
    }

    /// <summary>The room scope <paramref name="runtime"/> has loaded.</summary>
    private static RoomScope RoomScopeOf(SuperMetroidRuntime runtime) =>
        runtime.Room.Scope ?? throw new InvalidOperationException("No room is loaded.");
}
