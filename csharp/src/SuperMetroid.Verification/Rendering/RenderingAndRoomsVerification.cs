using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Rooms.Backgrounds.RoomSkyTilemapsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using static SuperMetroid.Verification.Frontend.Map.MapSpritesVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.RoomHeaderImport;
using SuperMetroid.TestSupport.VerificationOracles;
using SuperMetroid.Core.Map;
using SuperMetroid.AssetExtraction.Frontend.Map;
namespace SuperMetroid.Verification.Rendering;
using SuperMetroid.Verification.Harness;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.TestSupport.FrameRendering;
using SuperMetroid.AssetExtraction.Progression;

/// <summary>Verifies room loading, camera/scroll state and PPU output with focused cartridge and synthetic fixtures.</summary>
internal static class RenderingAndRoomsVerification
{

/// <summary>
/// Copies a synthetic explicit scroll table into the compiled scroll-buffer entry point.
/// The native loader copies a fixed 50 bytes, so a shorter <paramref name="source"/> is
/// followed by zero bytes, exactly as the cleared sparse cartridge fixture reads past its
/// seeded bytes. This preserves native padding behavior without a cartridge reader.
/// </summary>
private static RoomScrollGrid LoadExplicitScrollFixture(
    ReadOnlySpan<byte> source, Extent16<RoomScreens> size)
{
    byte[] storage = new byte[RoomScrollGrid.KeptCellCount];
    source[..Math.Min(source.Length, storage.Length)].CopyTo(storage);
    return RoomScrollGrid.LoadNativeTable(storage, size);
}

/// <summary>An explicit scroll table whose 50 copied bytes are all blue (one).</summary>
private static byte[] AllBlueExplicitScrollTable() =>
    Enumerable.Repeat((byte)1, RoomScrollGrid.KeptCellCount).ToArray();

/// <summary>OBJ, HUD, cameras, scrolling, tilemaps, room data, and sky verification.</summary>
internal static void VerifyObjRendering()
{
    var bus = new TestAddressSpace();
    var vram = new SnesVram();
    var cgram = new SnesCgram();
    var oam = new OamBuffer();

    // A single palette-index-1 pixel at the tile's upper-left corner. SNES 4-bpp plane 0
    // uses bit 7 of byte 0 for (0,0); every omitted byte reads as zero in this fixture.
    SeedUpperLeftPixelTile(bus);
    ImportedVramOracle.ExecuteQueued(vram, bus, 0x828000, sizeInBytes: 32, encodedDestination: 0x0000);

    // One small tile-zero OBJ at (10,20), with caller-selected palette 2. Palette index
    // 128 + 2*16 + 1 is loaded with maximum red in native BGR555.
    SeedSingleSmallObjSpritemap(bus);
    cgram.SetColor(128 + 2 * 16 + 1, RgbColor.FromWord(0x001f));
    oam.BeginFrame();
    DrawImportedSpritemap(bus, oam, 0x818000, origin: new(10, 20), paletteBits: 0x0400);
    oam.FinalizeFrame();

    // The frontend capture converts the screen's OAM storage into the sprite draw it stands for;
    // the display renderer then shows that draw over the backdrop.
    RgbaPixel[] backdrop = DisplayRendering.Render(FramePaint.FromNative(cgram.Colors, []).SpritesScene(DisplayBrightness.Full));
    RgbaPixel[] pixels = DisplayRendering.Render(FramePaintConstants.CaptureFrontend(vram, cgram, oam, objectSelection: 0).SpritesScene(DisplayBrightness.Full));
    AssertEqual(new RgbaPixel(255, 0, 0), pixels[20 * 256 + 10], "OBJ colored planar pixel");
    AssertEqual(backdrop[20 * 256 + 11], pixels[20 * 256 + 11], "OBJ color zero shows the backdrop");

    // Within a sprite layer the earlier draw covers the later one, whatever priority its parts carried. Model the Ceres failure
    // directly: an earlier Samus-like OBJ2 must hide a later Ridley-like OBJ3, otherwise
    // the hidden boss is painted over Samus. Distinct palettes make the winner observable
    // without private ROM art; both entries deliberately use the same one-pixel tile and
    // screen coordinate.
    cgram.SetColor(128 + 3 * 16 + 1, RgbColor.FromWord(0x03e0));
    oam.BeginFrame();
    oam.AddRawSmallSprite(position: new Position16<SpritePixels>(10, 20), attributes: 0x2400); // OAM 0: OBJ2, palette 2.
    oam.AddRawSmallSprite(position: new Position16<SpritePixels>(10, 20), attributes: 0x3600); // OAM 1: OBJ3, palette 3.
    oam.FinalizeFrame();
    RgbaPixel[] overlap = DisplayRendering.Render(FramePaintConstants.CaptureFrontend(vram, cgram, oam, objectSelection: 0).SpritesScene(DisplayBrightness.Full));
    AssertEqual(
        new RgbaPixel(255, 0, 0),
        overlap[20 * 256 + 10],
        "lower-OAM OBJ2 wins cross-priority overlap");

    // The production timer's first tile is $1E0 under OBSEL=$03. This assertion fixes
    // the important reverse-engineered relationship: character data starts at word $7E00.
    AssertEqual(0xfc00, ObjCharacterImage.ResolveTileByteAddress(0x1e0, 0x03), "timer tile $1E0 VRAM byte address");
    AssertEqual(0xff00, ObjCharacterImage.ResolveTileByteAddress(0x1f8, 0x03), "timer tile $1F8 VRAM byte address");

    Console.WriteLine("  OBJ: OBSEL addressing, the drawn pixel, its colour, transparency, and cross-priority OAM ownership agree.");
}

/// <summary>
/// Verifies the HUD ROM-template mutation, authentic WRAM queue source, live counters, selection
/// palettes and minimap words without relying on the private ROM.
/// </summary>
internal static void VerifyHudState()
{
    var bus = new TestAddressSpace();

    // Seed the three-row ROM template entirely with the canonical blank HUD tile $2C0F.
    // The synthetic digit table uses conspicuous character names $100-$109.
    SeedBlankHudTemplateAndDigitTables(bus);

    var hud = new HudState();
    SuperMetroid.AssetExtraction.Hud.GameplayHudArtwork hudArtwork = SuperMetroid.AssetExtraction.Hud.GameplayHudPresentationFile.Read(
        new MemoryStream(SuperMetroid.AssetExtraction.Hud.GameplayHudPresentationExtractor.Extract(bus)));
    hud.BindPresentation(hudArtwork.Presentation);
    hud.Initialize(HudSnapshot.CeresDebug);
    AssertEqual(0x2d09, hud.TileWords(hudArtwork)[0x8c / 2], "HUD health tens digit from ROM table");
    AssertEqual(0x2d09, hud.TileWords(hudArtwork)[0x8e / 2], "HUD health ones digit from ROM table");

    var damagedSamus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()) { Health = 94, MaxHealth = 99 };
    var hudAudio = new GameplayAudioRequests();
    hud.UpdateGameplayCounters(damagedSamus, hudAudio);
    AssertEqual(0x2d09, hud.TileWords(hudArtwork)[0x8c / 2], "live HUD damage tens digit");
    AssertEqual(0x2d04, hud.TileWords(hudArtwork)[0x8e / 2], "live HUD damage ones digit");

    // HandleHudTilemap owns selection presentation independently of the input handler.
    // Installing missiles after initialization mirrors a live pickup, then selecting them
    // proves both palette directions and the selection sound queued once, as the HUD routine queues it.
    damagedSamus.MaxMissiles = 5;
    damagedSamus.Missiles = 5;
    damagedSamus.SelectedHudItem = HudItem.Missile;
    hud.UpdateGameplayCounters(damagedSamus, hudAudio);
    AssertEqual(0x1000, hud.TileWords(hudArtwork)[0x14 / 2] & 0x1c00,
        "live HUD selects missile icon with palette four");
    AssertEqual(1, hudAudio.CountOf(SamusSound.HudWeaponSelect),
        "live HUD queues the cartridge selection sound on change");
    hud.UpdateGameplayCounters(damagedSamus, hudAudio);
    AssertEqual(1, hudAudio.CountOf(SamusSound.HudWeaponSelect),
        "stable HUD selection does not repeat the selection sound");
    damagedSamus.SelectedHudItem = HudItem.Beam;
    hud.UpdateGameplayCounters(damagedSamus, hudAudio);
    AssertEqual(0x1400, hud.TileWords(hudArtwork)[0x14 / 2] & 0x1c00,
        "live HUD restores deselected missile icon to palette five");

    // Area zero points to a synthetic two-screen-wide Crateria map. Give every 5x3 window of
    // map tiles fifteen different retail map cells (#1513) and mark every coordinate as
    // existing; the expected HUD words then prove room origin + Samus screen coordinates,
    // 5x3 centering, exploration palette, and the map-station/unexplored palette independently.
    SeedSyntheticCrateriaAreaMap(bus);

    var mapSystem = new SharedGameState(new SharedRandom());
    mapSystem.SetAreaMapAcquired(0);
    hud.UpdateMinimap(mapSystem, new MinimapFrame { AreaIndex = AreaId.Crateria, RoomMap = new(0x17, 0), RoomSize = new(9 * 16, 5 * 16), Samus = new(0x0440, 0x04bb), FrameCounter = 8, PresentationMap = SuperMetroid.AssetExtraction.Frontend.Map.AreaMapImporter.Load(bus, AreaId.Crateria) });
    AssertTrue(mapSystem.IsMapTileExplored(AreaId.Crateria, new(27, 5)), "the minimap update explores the Landing Site's absolute tile Samus stands in");
    AssertEqual(SyntheticHudWord(new(25, 4), explored: false), hud.TileWords(hudArtwork)[26], "minimap top-left unvisited map-station tile");
    AssertEqual(SyntheticHudWord(new(27, 5), explored: true), hud.TileWords(hudArtwork)[60], "minimap explored center tile");

    // A room's map position is any map coordinate (#1627): a column past 255 continues across the area map's columns, and
    // a row below the map's rows explores and shows nothing.
    var wideMapSystem = new SharedGameState(new SharedRandom());
    wideMapSystem.SetAreaMapAcquired(0);
    hud.UpdateMinimap(wideMapSystem, new MinimapFrame { AreaIndex = AreaId.Crateria, RoomMap = new(0x17 + 256, 0), RoomSize = new(9 * 16, 5 * 16), Samus = new(0x0440, 0x04bb), FrameCounter = 8, PresentationMap = SuperMetroid.AssetExtraction.Frontend.Map.AreaMapImporter.Load(bus, AreaId.Crateria) });
    AssertTrue(wideMapSystem.IsMapTileExplored(AreaId.Crateria, new(27, 5)), "a map column past 255 continues across the area map's columns: it explores the cell Samus stands in");
    var deepMapSystem = new SharedGameState(new SharedRandom());
    hud.UpdateMinimap(deepMapSystem, new MinimapFrame { AreaIndex = AreaId.Crateria, RoomMap = new(0x17, 300), RoomSize = new(9 * 16, 5 * 16), Samus = new(0x0440, 0x04bb), FrameCounter = 8, PresentationMap = SuperMetroid.AssetExtraction.Frontend.Map.AreaMapImporter.Load(bus, AreaId.Crateria) });
    AssertTrue(!deepMapSystem.IsMapTileExplored(AreaId.Crateria, new(27, 5)), "a map row below the area map explores nothing");

    var noMapSystem = new SharedGameState(new SharedRandom());
    hud.UpdateMinimap(noMapSystem, new MinimapFrame { AreaIndex = AreaId.Crateria, RoomMap = new(0x17, 0), RoomSize = new(9 * 16, 5 * 16), Samus = new(0x0440, 0x04bb), FrameCounter = 0, PresentationMap = SuperMetroid.AssetExtraction.Frontend.Map.AreaMapImporter.Load(bus, AreaId.Crateria) });
    AssertEqual((int)new MapTileWord(SyntheticMapWord(new(27, 5))).ForHud(explored: true).WithLocationBlink().Raw, hud.TileWords(hudArtwork)[60],
        "minimap blinking center palette");
    AssertEqual(0x2c1f, hud.TileWords(hudArtwork)[26], "minimap hides unvisited tile without map station");

    // The sloped hallway's lower floor, rising either way, explores the corner above the
    // current cell. Native renders the upper row before setting this bit, so its palette
    // changes next update.
    foreach (int x in new[] { 31, 32 })
    foreach (ushort slope in new ushort[] { 0x0c28, 0x4c28 })
    {
        var slopeSystem = new SharedGameState(new SharedRandom());
        Position16<MapTiles> currentTile = new(checked((ushort)x), 5);
        Position16<MapTiles> aboveTile = new(checked((ushort)x), 4);
        ushort savedCurrent = ReadFixtureMapWord(bus, currentTile);
        ushort savedAbove = ReadFixtureMapWord(bus, aboveTile);
        SeedFixtureMapWord(bus, currentTile, slope);
        SeedFixtureMapWord(bus, aboveTile, 0x0c29);
        hud.UpdateMinimap(slopeSystem, new MinimapFrame { AreaIndex = AreaId.Crateria, RoomMap = new((byte)x, 4), RoomSize = new(16, 16), Samus = new(128, 128), FrameCounter = 8, PresentationMap = SuperMetroid.AssetExtraction.Frontend.Map.AreaMapImporter.Load(bus, AreaId.Crateria) });
        AssertTrue(slopeSystem.IsMapTileExplored(0, new(checked((ushort)x), 4)), "sloped hallway explores its upper corner on either map page");
        AssertEqual(0x2c1f, hud.TileWords(hudArtwork)[28], "upper corner remains blank on native discovery update");
        hud.UpdateMinimap(slopeSystem, new MinimapFrame { AreaIndex = AreaId.Crateria, RoomMap = new((byte)x, 4), RoomSize = new(16, 16), Samus = new(128, 128), FrameCounter = 8, PresentationMap = SuperMetroid.AssetExtraction.Frontend.Map.AreaMapImporter.Load(bus, AreaId.Crateria) });
        AssertEqual(0x2829, hud.TileWords(hudArtwork)[28], "upper corner displays its explored palette on following update");
        SeedFixtureMapWord(bus, currentTile, savedCurrent);
        SeedFixtureMapWord(bus, aboveTile, savedAbove);
    }
    var ordinaryCornerSystem = new SharedGameState(new SharedRandom());
    // The hallway's lower ceiling, character $029, is adjacent in the graphics catalog but
    // must not get the lower floor's exploration side effect; this also rejects
    // indiscriminate upper-cell revealing.
    Position16<MapTiles> ordinaryTile = new(30, 5);
    ushort ordinarySaved = ReadFixtureMapWord(bus, ordinaryTile);
    SeedFixtureMapWord(bus, ordinaryTile, 0x0c29);
    hud.UpdateMinimap(ordinaryCornerSystem, new MinimapFrame { AreaIndex = AreaId.Crateria, RoomMap = new(30, 4), RoomSize = new(16, 16), Samus = new(128, 128), FrameCounter = 8, PresentationMap = SuperMetroid.AssetExtraction.Frontend.Map.AreaMapImporter.Load(bus, AreaId.Crateria) });
    AssertTrue(!ordinaryCornerSystem.IsMapTileExplored(0, new(30, 4)), "ordinary map character does not explore upper corner");
    SeedFixtureMapWord(bus, ordinaryTile, ordinarySaved);

    // A reveal override combines with display state only. Remove the station-mask bit from
    // the top-left HUD cell while leaving its cartridge tile nonblank, giving Public and
    // Secret modes an observable difference without manufacturing a room approximation.
    const int secretHudX = 25;
    const int secretHudY = 4;
    const int publicHudX = 26;
    ClearFixtureMapStationBit(bus, new(secretHudX, secretHudY));
    var overrideSystem = new SharedGameState(new SharedRandom());
    hud.UpdateMinimap(overrideSystem, new MinimapFrame { AreaIndex = AreaId.Crateria, RoomMap = new(0x17, 0), RoomSize = new(9 * 16, 5 * 16), Samus = new(0x0440, 0x04bb), FrameCounter = 8, MapRevealMode = MapRevealMode.Public, PresentationMap = SuperMetroid.AssetExtraction.Frontend.Map.AreaMapImporter.Load(bus, AreaId.Crateria) });
    AssertEqual(MapTileWords.HudBlank.Raw, hud.TileWords(hudArtwork)[26],
        "Public HUD override excludes cartridge secret-only cell");
    AssertEqual(0x2c00, hud.TileWords(hudArtwork)[27] & 0x3c00,
        "Public HUD override reveals station cell as unentered");
    hud.UpdateMinimap(overrideSystem, new MinimapFrame { AreaIndex = AreaId.Crateria, RoomMap = new(0x17, 0), RoomSize = new(9 * 16, 5 * 16), Samus = new(0x0440, 0x04bb), FrameCounter = 8, MapRevealMode = MapRevealMode.Secret, PresentationMap = SuperMetroid.AssetExtraction.Frontend.Map.AreaMapImporter.Load(bus, AreaId.Crateria) });
    AssertEqual(0x2c00, hud.TileWords(hudArtwork)[26] & 0x3c00,
        "Secret HUD override reveals cartridge secret-only cell as unentered");
    overrideSystem.MarkExploredMapTile(AreaId.Crateria, new(secretHudX, secretHudY));
    hud.UpdateMinimap(overrideSystem, new MinimapFrame { AreaIndex = AreaId.Crateria, RoomMap = new(0x17, 0), RoomSize = new(9 * 16, 5 * 16), Samus = new(0x0440, 0x04bb), FrameCounter = 8, MapRevealMode = MapRevealMode.Secret, PresentationMap = SuperMetroid.AssetExtraction.Frontend.Map.AreaMapImporter.Load(bus, AreaId.Crateria) });
    AssertEqual(0x2800, hud.TileWords(hudArtwork)[26] & 0x3c00,
        "Secret HUD keeps entered secret cell visually distinct");
    AssertEqual(0x2c00, hud.TileWords(hudArtwork)[27] & 0x3c00,
        "Secret HUD leaves override-only public cell unentered");
    hud.UpdateMinimap(overrideSystem, new MinimapFrame { AreaIndex = AreaId.Crateria, RoomMap = new(0x17, 0), RoomSize = new(9 * 16, 5 * 16), Samus = new(0x0440, 0x04bb), FrameCounter = 8, MapRevealMode = MapRevealMode.None, PresentationMap = SuperMetroid.AssetExtraction.Frontend.Map.AreaMapImporter.Load(bus, AreaId.Crateria) });
    AssertEqual(MapTileWords.HudBlank.Raw, hud.TileWords(hudArtwork)[27],
        "returning HUD override to None immediately hides unentered public cell");
    AssertTrue(!overrideSystem.HasAreaMap(AreaId.Crateria),
        "HUD reveal override does not grant the map station");
    AssertTrue(!overrideSystem.IsMapTileExplored(AreaId.Crateria, new(publicHudX, secretHudY)),
        "HUD reveal override does not persist an unentered cell");

    // Room $01/$27 lives on the right-hand map page. Give only its eastern neighbor an
    // existence bit. The historical HUD formula counted mapX's page bit twice and read
    // this coordinate from the following row, while pause used the correct byte. A solid
    // map plane cannot expose that disagreement, so retain this deliberately sparse case.
    const int rightPageNeighborX = 35;
    const int rightPageCenterY = 19;
    ResetFixtureMapStationBitsToSingleTile(bus, new(rightPageNeighborX, rightPageCenterY));
    var rightPageSystem = new SharedGameState(new SharedRandom());
    rightPageSystem.SetAreaMapAcquired(AreaId.Crateria);
    hud.UpdateMinimap(rightPageSystem, new MinimapFrame { AreaIndex = AreaId.Crateria, RoomMap = new(34, 18), RoomSize = new(2 * 16, 16), Samus = new(0x0080, 0x0080), FrameCounter = 8, PresentationMap = SuperMetroid.AssetExtraction.Frontend.Map.AreaMapImporter.Load(bus, AreaId.Crateria) });
    AssertTrue(rightPageSystem.IsMapTileExplored(AreaId.Crateria, new(34, rightPageCenterY)), "the right-page minimap update explores its absolute tile");
    AssertEqual(SyntheticHudWord(new(rightPageNeighborX, rightPageCenterY), explored: false), hud.TileWords(hudArtwork)[61],
        "right-page minimap reads the same eastern cell as pause map");
    AssertEqual(0x2c1f, hud.TileWords(hudArtwork)[59],
        "right-page sparse map keeps the western cell blank");

    AssertEqual(HudStateConstants.MutableTileCount, hud.Tiles(hudArtwork).Length, "the HUD's three drawn rows");
    AssertEqual(0x2c0f, hud.Tiles(hudArtwork)[0].PackedWord, "the HUD's first drawn cell is the blank word the latched BG3 plane shows");

    Console.WriteLine("  HUD: inventory, live minimap coordinates/blink and WRAM upload agree.");
}

/// <summary>Reads the synthetic Crateria map tile before a fixture temporarily replaces it.</summary>
static ushort ReadFixtureMapWord(TestAddressSpace bus, Position16<MapTiles> tile) =>
    SuperMetroid.AssetExtraction.Cartridge.RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus),
        0xb58000 + AreaMapPages.GetTilemapWordIndex(tile) * 2);

/// <summary>Overwrites the synthetic Crateria bank-$B5 area-map tilemap word at <paramref name="tile"/>.</summary>
static void SeedFixtureMapWord(TestAddressSpace bus, Position16<MapTiles> tile, ushort word) =>
    WriteTestWord(bus, 0xb58000 + AreaMapPages.GetTilemapWordIndex(tile) * 2, word);

/// <summary>
/// Clears <paramref name="tile"/>'s bit in the synthetic Crateria map-station mask at $82:9000,
/// leaving its cartridge tilemap word nonblank so reveal modes have an observable difference.
/// </summary>
static void ClearFixtureMapStationBit(TestAddressSpace bus, Position16<MapTiles> tile)
{
    int address = 0x829000 + AreaMapPages.GetBitByteIndex(tile);
    WriteTestByte(bus, address, (byte)(bus.ReadByte(address) & ~AreaMapPages.GetBitMask(tile.X)));
}

/// <summary>Clears the whole synthetic Crateria map-station mask at $82:9000, then sets only <paramref name="tile"/>'s bit.</summary>
static void ResetFixtureMapStationBitsToSingleTile(TestAddressSpace bus, Position16<MapTiles> tile)
{
    for (int index = 0; index < 0x100; index++)
        WriteTestByte(bus, 0x829000 + index, 0);
    WriteTestByte(bus, 0x829000 + AreaMapPages.GetBitByteIndex(tile), (byte)AreaMapPages.GetBitMask(tile.X));
}

/// <summary>
/// Seeds area zero's synthetic two-screen-wide Crateria map: the area pointer tables name
/// mask $82:9000 and tilemap $B5:8000, every mask bit is set, and every tilemap word is the
/// <see cref="SyntheticMapWord"/> of its cell.
/// </summary>
static void SeedSyntheticCrateriaAreaMap(TestAddressSpace bus)
{
    WriteTestByte(bus, 0x82964a, 0x00);
    WriteTestByte(bus, 0x82964b, 0x80);
    WriteTestByte(bus, 0x82964c, 0xb5);
    WriteTestWord(bus, 0x829717, 0x9000);
    for (int index = 0; index < 0x100; index++)
        WriteTestByte(bus, 0x829000 + index, 0xff);
    for (int y = 0; y < AreaMapSize.HeightInMapTiles; y++)
    for (int x = 0; x < AreaMapSize.WidthInMapTiles; x++)
    {
        var tile = new Position16<MapTiles>(checked((ushort)x), checked((ushort)y));
        WriteTestWord(bus, 0xb58000 + AreaMapPages.GetTilemapWordIndex(tile) * 2, SyntheticMapWord(tile));
    }
}

/// <summary>
/// The synthetic map's word at <paramref name="tile"/>: one of fifteen retail map cells, none a sloped hallway, each the
/// first map character that pictures its cell in the unexplored row (#1513), chosen by the tile's column modulo five and
/// row modulo three, so every 5x3 window of the map shows fifteen different cells and the HUD's words name the tile read.
/// </summary>
static ushort SyntheticMapWord(Position16<MapTiles> tile)
{
    ushort[] words = [0x0c20, 0x0c21, 0x0c22, 0x0c23, 0x0c24, 0x0c25, 0x0c26, 0x0c27, 0x0c4d, 0x0c4f, 0x0c5e, 0x0c6e, 0x0c6f, 0x0c76, 0x0c77];
    return words[tile.X % 5 + 5 * (tile.Y % 3)];
}

/// <summary>The HUD word the minimap shows the synthetic map's <paramref name="tile"/> as, explored or not.</summary>
static int SyntheticHudWord(Position16<MapTiles> tile, bool explored) => new MapTileWord(SyntheticMapWord(tile)).ForHud(explored).Raw;

/// <summary>
/// Seeds the three-row HUD ROM template entirely with the canonical blank HUD tile $2C0F, and
/// both digit tables with conspicuous character names $100-$109.
/// </summary>
static void SeedBlankHudTemplateAndDigitTables(TestAddressSpace bus)
{
    for (int tile = 0; tile < HudStateConstants.MutableTileCount; tile++)
        WriteTestWord(bus, 0x8098cb + tile * 2, 0x2c0f);
    for (int digit = 0; digit < 10; digit++)
    {
        WriteTestWord(bus, 0x809dbf + digit * 2, (ushort)(0x2c00 | (0x100 + digit)));
        WriteTestWord(bus, 0x809dd3 + digit * 2, (ushort)(0x2c00 | (0x100 + digit)));
    }
}

/// <summary>A 4-bpp tile at $82:8000 whose only set bit is plane 0's upper-left pixel (palette index 1).</summary>
static void SeedUpperLeftPixelTile(TestAddressSpace bus) =>
    WriteTestByte(bus, 0x828000, 0x80);

/// <summary>A one-entry common spritemap at $81:8000: one small tile-zero OBJ at offset (0, 0).</summary>
static void SeedSingleSmallObjSpritemap(TestAddressSpace bus) =>
    bus.WriteBytes(0x818000, [0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00]);

/// <summary>Seeds one bank-$A9 walk instruction stream with retail timing and harmless synthetic spritemap operands.</summary>
internal static void SeedMotherBrainWalkProgram(
    TestAddressSpace bus,
    ushort listAddress,
    ushort duration,
    bool forward)
{
    // All four speed variants share command topology; only each visible frame duration
    // differs. These words are transcribed from `$A9:9730-$9972`, with harmless synthetic
    // spritemap operands because movement verification never draws the extended map.
    ushort[] commands = forward
        ? [0x95fc, 0x960c, 0x961c, 0x9622, 0x9638, 0x9648, 0x9658, 0x9668]
        : [0x96f0, 0x96e0, 0x96d0, 0x96ba, 0x96aa, 0x96a4, 0x9694, 0x967e];
    var words = new List<ushort> { 0x9708, duration, 0x1000 };
    for (int command = 0; command < commands.Length; command++)
    {
        words.Add(commands[command]);
        if (command == commands.Length - 1)
            words.Add(0x9700);
        words.Add(duration);
        words.Add(unchecked((ushort)(0x1001 + command)));
    }
    words.Add(0x812f);
    for (int index = 0; index < words.Count; index++)
        WriteTestWord(bus, 0xa90000 | unchecked((ushort)(listAddress + index * 2)), words[index]);
}

/// <summary>Seeds the fast crouch instruction sequence used by Mother Brain movement checks.</summary>
internal static void SeedMotherBrainCrouchFastProgram(TestAddressSpace bus)
{
    ushort[] words =
    [
        0x9718, 0x0008, 0x1200,
        0x95de, 0x0002, 0x1201,
        0x95e8, 0x0002, 0x1202,
        0x95f2, 0x9710, 0x0008, 0x1203,
        0x812f,
    ];
    WriteTestWords(bus, 0xa99a26, words);
}

/// <summary>
/// Exercises the exact 50-byte room loader plus all four directional bank-$80 handlers at
/// internal red boundaries and physical room edges.
/// </summary>
internal static void VerifyRoomScrollGridAndBoundaryCamera()
{
    byte[] source = AllBlueExplicitScrollTable();

    // The logical 3x2 grid begins fully blue. Give the copied padding bytes values past the
    // three authored states to prove the load keeps every cell after the sixth logical one, read
    // as green, rather than synthesizing red cells there.
    for (int index = 6; index < RoomScrollGrid.KeptCellCount; index++)
        source[index] = (byte)(0x80 + index);

    RoomScrollGrid grid = LoadExplicitScrollFixture(source, new Extent16<RoomScreens>(3, 2));
    AssertEqual(RoomScrollState.Open, grid.KeptStates[6], "scroll loader retains first nonlogical cell");
    AssertEqual(RoomScrollState.Open, grid.KeptStates[49], "scroll loader retains fiftieth cell");

    // Single Chamber's neighboring room uses a four-byte explicit table for a 4x2 room.
    // The native fixed-size copy makes the following room header ($1F,$02,$1D,$04) its
    // logical second row. Those bytes are not corrupt enum values: bank $80 compares them
    // only with zero (blocked) and one (blue alignment), treating every other nonzero byte
    // like the conventional green value, so the import reads them as green cells.
    byte[] shortTableAndFollowingHeader = [2, 2, 2, 2, 0x1f, 0x02, 0x1d, 0x04];
    RoomScrollGrid shortTableGrid = LoadExplicitScrollFixture(
        shortTableAndFollowingHeader,
        new Extent16<RoomScreens>(4, 2));
    AssertEqual(RoomScrollState.Open, shortTableGrid.ReadState(4),
        "short explicit scroll table preserves following room-header byte as a green cell");
    AssertEqual(RoomScrollState.Open, shortTableGrid.ReadState(7),
        "short explicit scroll table preserves complete logical overread row");

    var camera = new ScrollBoundaryCamera(grid);

    // Right: the screen to the right is red, so the attempted +16 is rejected and the
    // routine's deliberate extra two-pixel retreat clamps the signed underflow to zero.
    grid.SetLogicalState(new(1, 0), RoomScrollState.Closed);
    camera.SetPosition(new Position16<RoomPixels>(0, 0));
    camera.MoveRight(16);
    AssertEqual(0, camera.PixelPosition.X, "$80:A641 red boundary moving right");

    // Left: entering a red current cell from its right edge produces the symmetric +2
    // retreat described by $80:A719-$80:A72B.
    camera.SetPosition(new Position16<RoomPixels>(272, 0));
    camera.MoveLeft(16);
    AssertEqual(274, camera.PixelPosition.X, "$80:A6BB red boundary moving left");

    // Down: a blue current cell over a red lower cell rejects the move and retreats two
    // pixels above the pre-move position (224 -> proposed 240 -> result 222).
    grid.SetLogicalState(new(0, 0), RoomScrollState.OpenAlignedToTop);
    grid.SetLogicalState(new(0, 1), RoomScrollState.Closed);
    camera.SetPosition(new Position16<RoomPixels>(0, 224));
    camera.MoveDown(16);
    AssertEqual(222, camera.PixelPosition.Y, "$80:A893 red boundary moving down");

    // Up: starting inside that red lower cell gives the corresponding two-pixel retreat.
    camera.SetPosition(new Position16<RoomPixels>(0, 272));
    camera.MoveUp(16);
    AssertEqual(274, camera.PixelPosition.Y, "$80:A936 red boundary moving up");

    // Physical right edge is (width-1)*$100 regardless of the scroll padding bytes.
    grid.SetLogicalState(new(1, 0), RoomScrollState.OpenAlignedToTop);
    camera.SetPosition(new Position16<RoomPixels>(0x01f8, 0));
    camera.MoveRight(16);
    AssertEqual(0x0200, camera.PixelPosition.X, "scroll camera physical room maximum");

    // Issue #12's slot-one capture is Construction Zone immediately after returning from
    // First Missile. Its room header starts `[blue, red]`; the incoming door points at
    // `$8F:BE25`, which must replace that pair with `[green, blue]` before camera tracking.
    // A door setup names its screens by column and row in the room its doors lead into, so each runs over that room's grid.
    RoomScrollGrid constructionZone = LoadExplicitScrollFixture(source, RoomDefinitions.ConstructionZone.Size);
    constructionZone.SetLogicalState(new(0, 0), RoomScrollState.OpenAlignedToTop);
    constructionZone.SetLogicalState(new(0, 1), RoomScrollState.Closed);
    DoorSetupDefinitions.ConstructionZoneOpensScreen0x0OpensScreen0x1TopAligned.ChangeScreens(constructionZone);
    AssertEqual(RoomScrollState.Open, constructionZone.ReadState(0),
        "$8F:BE25 opens Construction Zone screen zero");
    AssertEqual(RoomScrollState.OpenAlignedToTop, constructionZone.ReadState(1),
        "$8F:BE25 opens Construction Zone screen one, aligned to its top");
    RoomScrollGrid parlor = LoadExplicitScrollFixture(source, RoomDefinitions.Parlor.Size);
    parlor.SetCell(6, RoomScrollState.Closed);
    DoorSetupDefinitions.ParlorOpensScreen1x1.ChangeScreens(parlor);
    AssertEqual(RoomScrollState.Open, parlor.ReadState(6),
        "$8F:B981 opens early-route screen six, the Parlor's (1, 1)");

    // Door $83:AB4C leaves the Ceres Mode-7 shaft through `$8F:E513`. That routine
    // restores ordinary PPU state but performs no writes to the destination scroll array.
    // Its setup changes no screens, so an otherwise valid grid is left as it was;
    // its room event ends the turning floor.
    grid.SetCell(7, RoomScrollState.OpenAlignedToTop);
    DoorSetupDefinitions.FallingTileEndsTurningFloor.ChangeScreens(grid);
    AssertEqual(RoomScrollState.OpenAlignedToTop, grid.ReadState(7),
        "$8F:E513 preserves the destination scroll cells");

    Console.WriteLine("  Scrolls: fifty-cell load and four directional boundary handlers agree.");
}

/// <summary>
/// Checks bank-$90 moved-axis target selection and 16.16 "distance + 1" arithmetic before
/// those results enter the already-verified bank-$80 boundary handlers.
/// </summary>
internal static void VerifyMovedSamusCameraTracking()
{
    var bus = new TestAddressSpace();
    byte[] source = AllBlueExplicitScrollTable();
    RoomScrollGrid grid = LoadExplicitScrollFixture(source, new Extent16<RoomScreens>(3, 2));
    var camera = new ScrollBoundaryCamera(grid);

    // Facing right, normal forward movement, distance slot zero targets Samus X-$60.
    // Samus moved four pixels, and $90:96C0 intentionally adds 1.0, so camera advances 5.
    camera.SetPosition(new Position16<RoomPixels>(100, 0));
    var previous = FixedPosition<RoomPixels>.FromStoredParts(200, 0, 100, 0);
    var current = FixedPosition<RoomPixels>.FromStoredParts(204, 0, 100, 0);
    camera.TrackMovedSamusHorizontally(
        previous,
        current,
        new HorizontalCameraContext(KnockedBack: false, SamusMovementType.Standing, TurningAgainstMomentum: false, FacesLeft: false, CameraDistanceMode.NormalTracking));
    AssertEqual(108, camera.IdealPosition.X, "camera facing-right ideal X");
    AssertEqual(5, camera.Speed.X.UnsignedWholePixels, "camera X distance includes one pixel");
    AssertEqual(105, camera.PixelPosition.X, "camera X follows by calculated speed");

    // The complete fixed-point difference matters: 200.8000 -> 201.4000 is +0.C000;
    // adding 1.0 yields a camera delta of 1.C000.
    camera.SetPosition(new Position16<RoomPixels>(100, 0));
    previous = previous.WithX(new FixedCoordinate(200, 0x8000));
    current = current.WithX(new FixedCoordinate(201, 0x4000));
    camera.TrackMovedSamusHorizontally(
        previous,
        current,
        new HorizontalCameraContext(KnockedBack: false, SamusMovementType.Standing, TurningAgainstMomentum: false, FacesLeft: false, CameraDistanceMode.NormalTracking));
    AssertEqual(1, camera.Speed.X.UnsignedWholePixels, "camera fixed X speed integer");
    AssertEqual(0xc000, camera.Speed.X.Fraction, "camera fixed X speed fraction");
    AssertEqual(101, camera.PixelPosition.X, "camera fixed X position integer");
    AssertEqual(0xc000, camera.Position.X.Fraction, "camera fixed X position fraction");

    // Downward Samus movement uses up_scroller. 210-$64 gives ideal 110; a two-pixel move
    // becomes camera speed three and advances layer Y from 100 to 103 without overshoot.
    camera.SetPosition(new Position16<RoomPixels>(0, 100));
    previous = FixedPosition<RoomPixels>.FromStoredParts(0, 0, 208, 0);
    current = FixedPosition<RoomPixels>.FromStoredParts(0, 0, 210, 0);
    camera.TrackMovedSamusVertically(
        previous,
        current,
        new VerticalCameraContext(YDirection: SamusVerticalDirection.Down, UpScroller: 100, DownScroller: 112));
    AssertEqual(110, camera.IdealPosition.Y, "camera downward ideal Y");
    AssertEqual(3, camera.Speed.Y.UnsignedWholePixels, "camera Y distance includes one pixel");
    AssertEqual(103, camera.PixelPosition.Y, "camera Y follows by calculated speed");

    // Equal integer coordinates take $80:A528/$80:A731. With identical fixed-point
    // samples, the bank-$90 distance routine still produces speed 1; autoscroll then adds
    // its own two pixels. A red current cell with a blue neighbor therefore drifts +3.
    grid.SetLogicalState(new(0, 0), RoomScrollState.Closed);
    grid.SetLogicalState(new(1, 0), RoomScrollState.OpenAlignedToTop);
    camera.SetPosition(new Position16<RoomPixels>(0x0020, 0));
    var stationary = FixedPosition<RoomPixels>.FromStoredParts(200, 0, 200, 0);
    camera.TrackMovedSamusHorizontally(
        stationary,
        stationary,
        new HorizontalCameraContext(KnockedBack: false, SamusMovementType.Standing, TurningAgainstMomentum: false, FacesLeft: false, CameraDistanceMode.NormalTracking));
    AssertEqual(1, camera.Speed.X.UnsignedWholePixels, "stationary camera X speed still includes one");
    AssertEqual(0x0023, camera.PixelPosition.X, "$80:A528 red-cell rightward drift");

    // Red on both sides cancels that drift by rounding back to the current screen edge.
    grid.SetLogicalState(new(1, 0), RoomScrollState.Closed);
    camera.SetPosition(new Position16<RoomPixels>(0x0020, 0));
    camera.TrackMovedSamusHorizontally(
        stationary,
        stationary,
        new HorizontalCameraContext(KnockedBack: false, SamusMovementType.Standing, TurningAgainstMomentum: false, FacesLeft: false, CameraDistanceMode.NormalTracking));
    AssertEqual(0, camera.PixelPosition.X, "$80:A528 adjacent-red horizontal rounding");

    // The time-frozen flag short-circuits autoscrolling after bank $90 has calculated the
    // speed. It is intentionally not a blanket prohibition on moved-axis handling.
    grid.SetLogicalState(new(1, 0), RoomScrollState.OpenAlignedToTop);
    camera.SetPosition(new Position16<RoomPixels>(0x0020, 0));
    camera.TrackMovedSamusHorizontally(
        stationary,
        stationary,
        new HorizontalCameraContext(KnockedBack: false, SamusMovementType.Standing, TurningAgainstMomentum: false, FacesLeft: false, CameraDistanceMode.NormalTracking),
        timeIsFrozen: true);
    AssertEqual(0x0020, camera.PixelPosition.X, "$80:A528 time-frozen return");

    // A 9x5 room at its padded bottom can select scroll index 50 after centered camera X
    // advances into screen five. Native WRAM continues into ExploredMapTiles at $CD52;
    // a zero byte there makes the blue bottom row clamp to Y=$0400 without an array error.
    var bottomEdgeBus = new TestAddressSpace();
    RoomScrollGrid bottomEdgeGrid = LoadExplicitScrollFixture(
        source,
        new Extent16<RoomScreens>(9, 5));
    var bottomEdgeCamera = new ScrollBoundaryCamera(bottomEdgeGrid);
    bottomEdgeCamera.SetPosition(new Position16<RoomPixels>(0x0492, 0x0415));
    bottomEdgeCamera.TrackMovedSamusVertically(
        stationary,
        stationary,
        new VerticalCameraContext(YDirection: SamusVerticalDirection.None, UpScroller: 0x70, DownScroller: 0xa0));
    AssertEqual(0x0400, bottomEdgeCamera.PixelPosition.Y, "$80:A731 index-$32 adjacent-WRAM bottom clamp");

    // Vertical autoscrolling uses the centered X cell and the same speed+2 drift. This
    // three-row grid leaves the cell below blue so the candidate remains unrounded.
    var verticalBus = new TestAddressSpace();
    RoomScrollGrid verticalGrid = LoadExplicitScrollFixture(
        source,
        new Extent16<RoomScreens>(3, 3));
    verticalGrid.SetLogicalState(new(0, 0), RoomScrollState.Closed);
    verticalGrid.SetLogicalState(new(0, 1), RoomScrollState.OpenAlignedToTop);
    var verticalCamera = new ScrollBoundaryCamera(verticalGrid);
    verticalCamera.SetPosition(new Position16<RoomPixels>(0, 0x0020));
    verticalCamera.TrackMovedSamusVertically(
        stationary,
        stationary,
        new VerticalCameraContext(YDirection: SamusVerticalDirection.None, UpScroller: 0, DownScroller: 0));
    AssertEqual(1, verticalCamera.Speed.Y.UnsignedWholePixels, "stationary camera Y speed still includes one");
    AssertEqual(0x0023, verticalCamera.PixelPosition.Y, "$80:A731 red-cell downward drift");

    // Whether Samus is knocked back, her acceleration mode and her pose's facing build the context the camera reads:
    // a knockback reverses the target, only the turn mode keeps the old momentum, and only a left-facing pose faces left.
    AssertTrue(new HorizontalCameraContext(true, SamusMovementType.Standing, SamusAccelerationMode.Accelerating, SamusFacingDirection.Right, CameraDistanceMode.NormalTracking).KnockedBack,
        "a knockback reverses the camera target");
    AssertTrue(new HorizontalCameraContext(false, SamusMovementType.Standing, SamusAccelerationMode.Turning, SamusFacingDirection.Right, CameraDistanceMode.NormalTracking).TurningAgainstMomentum,
        "the turning acceleration mode is the momentum-keeping turn");
    AssertTrue(!new HorizontalCameraContext(false, SamusMovementType.Standing, SamusAccelerationMode.Decelerating, SamusFacingDirection.Right,
        CameraDistanceMode.NormalTracking).TurningAgainstMomentum, "plain deceleration does not reverse the camera target");
    AssertTrue(new HorizontalCameraContext(false, SamusMovementType.Standing, SamusAccelerationMode.Accelerating, SamusFacingDirection.Left, CameraDistanceMode.NormalTracking).FacesLeft,
        "a left-facing pose faces left");
    AssertTrue(!new HorizontalCameraContext(false, SamusMovementType.Standing, SamusAccelerationMode.Accelerating, SamusFacingDirection.ForwardOrSpecial, CameraDistanceMode.NormalTracking).FacesLeft,
        "a front-view pose takes the right-facing target");

    // The scrollers are distances: a negative one is rejected.
    AssertThrows<ArgumentOutOfRangeException>(() => _ = new VerticalCameraContext(SamusVerticalDirection.None, -1, 0),
        "a negative up-scroller is rejected");
    AssertThrows<ArgumentOutOfRangeException>(() => _ = new VerticalCameraContext(SamusVerticalDirection.None, 0, -1),
        "a negative down-scroller is rejected");

    Console.WriteLine("  Camera: bank $90 tracking and bank $80 stationary autoscroll agree.");
}

/// <summary>
/// Checks the exact parallax multiply, the views following their positions, frozen frames and the door's opening views
/// (#1555): the layers' views carry no offset of their own.
/// </summary>
internal static void VerifyBackgroundScrollState()
{
    var state = new BackgroundScrollState
    {
        ForegroundPosition = new(0x1234, 0x0200),
        BackgroundScrollModes = new(BackgroundAxisScroll.Parallax(128), BackgroundAxisScroll.FollowsForeground, RoomBackgroundSource.Level),
    };

    state.StepScrolling();
    AssertEqual(0x1234, state.ForegroundScroll.X, "the foreground's view follows the camera across");
    AssertEqual(0x0200, state.ForegroundScroll.Y, "the foreground's view follows the camera down");
    AssertEqual(0x091a, state.BackgroundPosition.X, "half-speed X parallax");
    AssertEqual(0x0200, state.BackgroundPosition.Y, "zero mode copies the foreground");
    AssertEqual(0x091a, state.BackgroundScroll.X, "the background's view follows its parallax position");

    // A fixed axis preserves both the background's position and its view.
    ushort oldLayer2X = state.BackgroundPosition.X;
    ushort oldBg2X = state.BackgroundScroll.X;
    state.BackgroundScrollModes = new(BackgroundAxisScroll.Fixed, BackgroundAxisScroll.Fixed, RoomBackgroundSource.Placed);
    state.ForegroundPosition = new(0x1300, 0x0300);
    state.StepScrolling();
    AssertEqual(oldLayer2X, state.BackgroundPosition.X, "fixed BG2 X position remains unchanged");
    AssertEqual(oldBg2X, state.BackgroundScroll.X, "fixed BG2 X view remains unchanged");

    ushort frozenBg1X = state.ForegroundScroll.X;
    state.ForegroundPosition = state.ForegroundPosition.WithX(0x4444);
    state.StepScrolling(timeIsFrozen: true);
    AssertEqual(frozenBg1X, state.ForegroundScroll.X, "a frozen frame keeps the views");

    var destinationParallax = new BackgroundScrollState
    {
        BackgroundScrollModes = new(BackgroundAxisScroll.Parallax(128), BackgroundAxisScroll.Parallax(64), RoomBackgroundSource.Level),
    };
    destinationParallax.PrepareDoorOpeningDestination(new(0x0400, 0x0200), new(0x0400, 0x0200));
    AssertEqual((ushort)0x0200, destinationParallax.BackgroundPosition.X,
        "door setup calculates destination half-speed BG2 X");
    AssertEqual((ushort)0x0080, destinationParallax.BackgroundPosition.Y,
        "door setup calculates destination quarter-speed BG2 Y");
    AssertEqual(new Position16<RoomPixels>(0x0400, 0x0200), destinationParallax.ForegroundPosition,
        "door setup publishes the whole destination layer-one position");

    // Upward doors derive BG2 from destination+$1F while layer one keeps the destination.
    var upwardParallax = new BackgroundScrollState { BackgroundScrollModes = new(BackgroundAxisScroll.Parallax(128), BackgroundAxisScroll.Parallax(64), RoomBackgroundSource.Level) };
    upwardParallax.PrepareDoorOpeningDestination(new(0x0400, 0x0220), new(0x0400, 0x021f));
    AssertEqual(new Position16<BackgroundPixels>(0x0200, 0x0087), upwardParallax.BackgroundPosition,
        "upward door setup calculates BG2 from its separate parallax basis");
    AssertEqual(new Position16<RoomPixels>(0x0400, 0x0220), upwardParallax.ForegroundPosition,
        "upward door setup keeps the destination layer-one position");

    // A door's opening scroll begins with both views where the source screen lies on the destination, the destination's
    // staged origin; the setup's first four-pixel step then moves the views with the layers by exactly four pixels.
    var rightDoor = new BackgroundScrollState
    {
        ForegroundPosition = new(0xff04, 0), // destination $0000 - $00FC
        BackgroundPosition = new(0xff04, 0),
    };
    rightDoor.BeginDoorOpeningViews(stagedForeground: new(0xff00, 0x0000));
    AssertEqual(new Position16<RoomPixels>(0xff00, 0), rightDoor.ForegroundScroll,
        "the door's foreground view begins at the staged destination origin");
    AssertEqual(new Position16<BackgroundPixels>(0xff00, 0), rightDoor.BackgroundScroll,
        "the door's background view begins at the staged destination origin");
    rightDoor.CalculateLayerScrolls();
    AssertEqual((ushort)0xff04, rightDoor.ForegroundScroll.X,
        "right-door foreground view moves four pixels with its first step");
    AssertEqual((ushort)0xff04, rightDoor.BackgroundScroll.X,
        "right-door background view moves four pixels with its first step");

    var leftDoor = new BackgroundScrollState
    {
        ForegroundPosition = new(0x00fc, 0), // destination $0000 + $00FC
        BackgroundPosition = new(0x00fc, 0),
    };
    leftDoor.BeginDoorOpeningViews(stagedForeground: new(0x0100, 0x0000));
    leftDoor.CalculateLayerScrolls();
    AssertEqual((ushort)0x00fc, leftDoor.ForegroundScroll.X,
        "left-door foreground view retreats four pixels with its first step");
    AssertEqual((ushort)0x00fc, leftDoor.BackgroundScroll.X,
        "left-door background view retreats four pixels with its first step");

    // Upward setup shows the destination five pixels above its staged origin after the built-in first moving call.
    var upwardDestination = new BackgroundScrollState
    {
        ForegroundPosition = new(0, 0x02fb),
        BackgroundPosition = new(0, 0x01dc),
    };
    upwardDestination.BeginDoorOpeningViews(stagedForeground: new(0, 0x0300));
    AssertEqual((ushort)0x0300, upwardDestination.ForegroundScroll.Y,
        "upward setup's view begins at the staged origin");
    upwardDestination.CalculateLayerScrolls();
    AssertEqual((ushort)0x02fb, upwardDestination.ForegroundScroll.Y,
        "upward setup's first moving call shows the source screen five pixels lower");

    Console.WriteLine("  BG scroll: parallax, frozen frames and the door's opening views agree.");
}

/// <summary>Checks all four branches shared by $80:AA95 and $80:AC57.</summary>
internal static void VerifyLevelBlockTilemapExpansion()
{
    // Put the definition at index one so the test also exercises the level entry's ten-bit
    // lookup. Existing child flip/palette bits are retained and XORed, never reconstructed.
    BackgroundTile[] definitions = BackgroundTileWordBytes.Tiles(new ushort[] { 0, 0, 0, 0, 0x0123, 0x4567, 0x89ab, 0xcdef });

    RoomBlockAppearance unflipped = RoomBlockAppearance.Of(1);
    RoomBlockAppearance horizontalSlope = RoomBlockAppearance.Of(1, BlockMirroring.Horizontal);
    RoomBlockAppearance verticalExtension = RoomBlockAppearance.Of(1, BlockMirroring.Vertical);
    RoomBlockAppearance doubleFlipHorizontalExtension = RoomBlockAppearance.Of(
        1, BlockMirroring.Horizontal | BlockMirroring.Vertical);

    AssertEqual(
        Expanded(0x0123, 0x4567, 0x89ab, 0xcdef),
        LevelBlockTilemapExpander.Expand(unflipped, definitions),
        "unflipped block expansion");
    AssertEqual(
        Expanded(0x0567, 0x4123, 0x8def, 0xc9ab),
        LevelBlockTilemapExpander.Expand(horizontalSlope, definitions),
        "horizontally mirrored block expansion");
    AssertEqual(
        Expanded(0x09ab, 0x4def, 0x8123, 0xc567),
        LevelBlockTilemapExpander.Expand(verticalExtension, definitions),
        "vertically mirrored block expansion");
    AssertEqual(
        Expanded(0x0def, 0x49ab, 0x8567, 0xc123),
        LevelBlockTilemapExpander.Expand(doubleFlipHorizontalExtension, definitions),
        "doubly mirrored block expansion");

    AssertThrows<InvalidDataException>(
        () => LevelBlockTilemapExpander.Expand(RoomBlockAppearance.Of(2), definitions),
        "block definition bounds check");

    Console.WriteLine("  BG stream: all 16x16 block flip expansions agree.");

    // The four tiles the cartridge's four expanded tilemap words place.
    static ExpandedBlockTiles Expanded(ushort topLeft, ushort topRight, ushort bottomLeft, ushort bottomRight) =>
        new(BackgroundTileWordBytes.Tile(topLeft), BackgroundTileWordBytes.Tile(topRight),
            BackgroundTileWordBytes.Tile(bottomLeft), BackgroundTileWordBytes.Tile(bottomRight));
}

/// <summary>
/// Verifies the shared row-major grid of terrain blocks and behaviours collision reads, and the planes the
/// display shows from the same grid.
/// </summary>
internal static void VerifyRoomLevelData()
{
    const int width = 3;
    const int height = 2;
    RoomTerrainBlock[] foreground = [
        default, RoomTerrainBlock.Of(291, BlockMirroring.None, RoomCollisionType.Slope),
        RoomTerrainBlock.Of(359, BlockMirroring.Horizontal, RoomCollisionType.SolidBlock),
        RoomTerrainBlock.Of(1, BlockMirroring.None, RoomCollisionType.ShootableBlock),
        RoomTerrainBlock.Of(2, BlockMirroring.None, RoomCollisionType.GrappleBlock),
        RoomTerrainBlock.Of(3, BlockMirroring.None, RoomCollisionType.BombableBlock),
    ];
    RoomBlockBehavior[] behavior = [RoomBlockBehaviorCodec.Decode(0x00), RoomBlockBehaviorCodec.Decode(0x45), RoomBlockBehaviorCodec.Decode(0x80), RoomBlockBehaviorCodec.Decode(0x11), RoomBlockBehaviorCodec.Decode(0x22), RoomBlockBehaviorCodec.Decode(0x33)];
    RoomBlockAppearance[] background = [.. Enumerable.Range(0, width * height).Select(block => RoomBlockAppearance.Of(block))];
    var level = new RoomLevelData(new Extent16<RoomBlocks>(width, height), new RoomLevelPlanes(foreground, behavior, background), RoomLevelData.NothingBeyondGrid());

    RoomCollisionBlock block = level.GetCollisionBlock(new(1, 1));
    AssertEqual(Position16<RoomBlocks>.Of(1, 1), block.Position, "room collision block position");
    AssertEqual(foreground[4], block.Terrain, "room collision reads the block's terrain");
    AssertEqual(RoomBlockBehaviorCodec.Decode(0x22), block.Behavior, "room collision reads the block's behaviour");
    AssertEqual(RoomCollisionType.GrappleBlock, block.CollisionType, "room collision kind");
    AssertEqual(2, block.VisualBlockIndex, "room collision block shown");
    AssertTrue(level.ForegroundPlane.Blocks.SequenceEqual(foreground.Select(terrain => terrain.Appearance).ToArray()),
        "the foreground shows its terrain's appearances");
    AssertTrue(level.BackgroundPlane.Blocks.SequenceEqual(background), "the background shows its appearances");

    // Pixel (31,17) is block (1,1); shifts must occur before multiplication/indexing.
    AssertEqual(block, level.GetCollisionBlockAtPixel(new(31, 17)), "room pixel-to-block conversion");
    AssertThrows<ArgumentOutOfRangeException>(
        () => level.GetCollisionBlock(new(width, 0)),
        "room collision rejects X beyond header width");
    AssertThrows<ArgumentOutOfRangeException>(
        () => level.GetCollisionBlock(new(0, height)),
        "room collision rejects Y beyond header height");

    Console.WriteLine("  Room level: shared terrain and behaviour indexing, collision kind, and pixel conversion agree.");
}

/// <summary>
/// Seeds a synthetic bank-$8F room at <paramref name="roomPointer"/>: the fixed eleven-byte
/// header (only area and door-list values are semantically interesting; zero dimensions are
/// legal because the test stops before asset construction), then the ordered selector program
/// event, boss bit, Morph Ball+missiles, power bombs, finish. Finish's following byte is the
/// inline default state header, so its expected pointer is derived from this exact command
/// layout instead of fabricated by the production code.
/// </summary>
static void SeedOrderedRoomStateSelectorRoom(TestAddressSpace bus, ushort roomPointer)
{
    int roomAddress = 0x8f0000 | roomPointer;
    bus.WriteBytes(roomAddress,
    [
        0x12, 0x03, 0x00, 0x00, 0x00, 0x00, 0x70, 0xa0, 0x00, 0x00, 0x88,
    ]);
    int selector = roomAddress + 11;
    WriteTestWord(bus, selector, (ushort)RoomStateSelectorCode.EventHasBeenSet);
    WriteTestByte(bus, selector + 2, 0x00);
    WriteTestWord(bus, selector + 3, 0x9100);
    WriteTestWord(bus, selector + 5, (ushort)RoomStateSelectorCode.BossIsDead);
    WriteTestByte(bus, selector + 7, 0x04);
    WriteTestWord(bus, selector + 8, 0x9120);
    WriteTestWord(bus, selector + 10, (ushort)RoomStateSelectorCode.MorphBallAndMissiles);
    WriteTestWord(bus, selector + 12, 0x9140);
    WriteTestWord(bus, selector + 14, (ushort)RoomStateSelectorCode.PowerBombs);
    WriteTestWord(bus, selector + 16, 0x9160);
    WriteTestWord(bus, selector + 18, (ushort)RoomStateSelectorCode.Finish);
}

/// <summary>Overwrites the start of the room's selector program (after its eleven-byte header) with <paramref name="words"/>.</summary>
static void SeedRoomStateSelectorWords(TestAddressSpace bus, ushort roomPointer, params ReadOnlySpan<ushort> words)
{
    int selector = (0x8f0000 | roomPointer) + 11;
    for (int index = 0; index < words.Length; index++)
        WriteTestWord(bus, selector + index * 2, words[index]);
}

/// <summary>
/// Exercises bank-$8F's inline room-state selector bytecode without the private cartridge.
/// </summary>
internal static void VerifyCartridgeRoomStateSelection()
{
    var bus = new TestAddressSpace();
    const ushort roomPointer = 0x9000;
    SeedOrderedRoomStateSelectorRoom(bus, roomPointer);
    const ushort defaultStatePointer = 0x901f;

    // All five state headers may remain zero-filled: State.Pointer alone proves which
    // branch won, and TestAddressSpace returns zero for every unwritten payload byte.
    AssertEqual(defaultStatePointer,
        SuperMetroid.TestSupport.RoomHeaderImport.CartridgeRoomHeaderImporter.SelectStatePointer(bus, roomPointer),
        "room selector default inline state");

    ProgressSet<StoryEvent> eventBytes = new PersistentProgress().Events;
    eventBytes.Add(StoryEvent.ZebesAwake);
    AssertEqual((ushort)0x9100,
        SuperMetroid.TestSupport.RoomHeaderImport.CartridgeRoomHeaderImporter.SelectStatePointer(bus, roomPointer,
            new RoomStateSelectionContext(eventBytes, DefeatedBosses.None, false, false)),
        "room selector event branch");
    AssertEqual((ushort)0x9120,
        SuperMetroid.TestSupport.RoomHeaderImport.CartridgeRoomHeaderImporter.SelectStatePointer(bus, roomPointer,
            new RoomStateSelectionContext(null, DefeatedBosses.AreaTorizo, false, false)),
        "room selector boss branch");
    AssertEqual((ushort)0x9140,
        SuperMetroid.TestSupport.RoomHeaderImport.CartridgeRoomHeaderImporter.SelectStatePointer(bus, roomPointer,
            new RoomStateSelectionContext(null, DefeatedBosses.None, true, false)),
        "room selector Morph Ball and missiles branch");
    AssertEqual((ushort)0x9160,
        SuperMetroid.TestSupport.RoomHeaderImport.CartridgeRoomHeaderImporter.SelectStatePointer(bus, roomPointer,
            new RoomStateSelectionContext(null, DefeatedBosses.None, false, true)),
        "room selector power-bomb branch");

    // The interpreter must return at the first successful command; later facts do not
    // override the event-selected pointer merely because they are also true.
    AssertEqual((ushort)0x9100,
        SuperMetroid.TestSupport.RoomHeaderImport.CartridgeRoomHeaderImporter.SelectStatePointer(bus, roomPointer,
            new RoomStateSelectionContext(eventBytes, DefeatedBosses.AreaTorizo, true, true)),
        "room selector preserves cartridge priority");

    // The Tourian-specific routine owns its boss-bit operand in executable code rather
    // than the room stream. Exercise it separately so the catalogued implicit mask cannot
    // accidentally be replaced with the operand-reading behavior of the generic routine.
    SeedRoomStateSelectorWords(bus, roomPointer,
        (ushort)RoomStateSelectorCode.MainAreaBossIsDead, 0x9180, (ushort)RoomStateSelectorCode.Finish);
    AssertEqual((ushort)0x9180,
        SuperMetroid.TestSupport.RoomHeaderImport.CartridgeRoomHeaderImporter.SelectStatePointer(bus, roomPointer,
            new RoomStateSelectionContext(
                null,
                RoomStateSelectorConditions.MainAreaBoss,
                false,
                false)),
        "room selector main-area boss implicit operand");

    // Known unused callbacks remain deliberately unsupported, while an arbitrary word is
    // diagnosed as unknown. Both must stop at the cartridge boundary instead of falling
    // through to a fabricated default state.
    SeedRoomStateSelectorWords(bus, roomPointer, (ushort)RoomStateSelectorCode.UnusedDoor);
    AssertThrows<NotSupportedException>(
        () => SuperMetroid.TestSupport.RoomHeaderImport.CartridgeRoomHeaderImporter.SelectStatePointer(bus, roomPointer),
        "known unused room selector fails loudly");
    SeedRoomStateSelectorWords(bus, roomPointer, 0xdead);
    AssertThrows<InvalidDataException>(
        () => SuperMetroid.TestSupport.RoomHeaderImport.CartridgeRoomHeaderImporter.SelectStatePointer(bus, roomPointer),
        "unknown room selector fails loudly");

    Console.WriteLine(
        "  Room states: complete selector catalog, operands, priority, and failures agree.");
}

/// <summary>Checks that the background shows its content across the area it covers: a library background's pages in one column and Kraid's four pages.</summary>
internal static void VerifyBg2ScreenLayout()
{
    // BG2SC=$4A means a 32x64-tile map rooted at word $4800. Once VOFS bit eight
    // is set, the PPU selects its second $400-word screen at $4C00 rather than wrapping
    // into the first screen. Landing Site relies on this exact vertical arrangement.
    // The gameplay layer's first output row is 32, physical scanline 33, so VOFS=$E0 selects
    // source Y=$101. A per-line HOFS of eight selects tile one in the lower screen.
    // Put its only pixel on character row one: zero-based BG sampling fails.
    var gameplayVram = new SnesVram();
    var gameplayCgram = new SnesCgram();
    var redCharacter = new byte[32];
    var blueCharacter = new byte[32];
    redCharacter[2] = 0x80;
    blueCharacter[2] = 0x80;
    // Leave character zero transparent because empty BG1 map words select it above BG2.
    gameplayVram.LoadBytes(0x0020, redCharacter);
    gameplayVram.LoadBytes(0x0040, blueCharacter);
    gameplayCgram.SetColor(2 * 16 + 1, RgbColor.FromWord(0x001f));
    gameplayCgram.SetColor(3 * 16 + 1, RgbColor.FromWord(0x7c00));
    gameplayVram.ExecuteWordTransfer([0x0801], destinationWord: 0x4800, wordIncrement: 1);
    gameplayVram.ExecuteWordTransfer([0x0801, 0x0c02], destinationWord: 0x4c00, wordIncrement: 1);
    RgbaPixel[] gameplayFrame = RenderOrdinaryGameplay(gameplayCgram,
        new GameplaySceneLayer(
            // The view names the background pixel the screen's top shows: the row below a camera at row $E0 (#1514).
            new GameplaySceneView(new Position16<RoomPixels>(0, 0), GameplaySceneView.LayerPixelAtScreenTop(new Position16<BackgroundPixels>(0, 0x00e0)),
                ScreenLayers.Foreground | ScreenLayers.Background | ScreenLayers.Sprites),
            // BG2 shows the tilemap just loaded, as its library background's image (#1403).
            RoomBackgroundsFixtures.Blank() with
            {
                BackgroundImage = RoomBlockFixtures.LoadedBackground(gameplayVram.Bytes, gameplayCgram.Colors, GameplayVramLayout.Bg2TilemapWord,
                    pagesInOneColumn: true),
            },
            // The sky's first gameplay line scrolls eight pixels; the rest keep BG2's own scroll (#1468).
            new BackgroundLineScrolls { Horizontal = new ScrollBands(0, [new ScrollBand(DisplayScreen.HudHeight, 8), new ScrollBand(DisplayScreen.HudHeight + 1, 0)]) }));
    AssertEqual(new RgbaPixel(0, 0, 255),
        gameplayFrame[DisplayScreen.HudHeight * DisplayScreen.Width],
        "ordinary compositor consumes sky HOFS and vertical BGSC screen");

    // Kraid selects BG2SC=$43: a 64x64 map rooted at word $4000 instead of the normal
    // room BG2 map at $4800. Verify the general compositor consumes that register-derived
    // layout; accepting a 64x64 argument without using its base still leaves only Kraid's
    // ordinary OAM arm visible.
    const ushort kraidBg2BaseWord = 0x4000;
    int firstGameplaySourceRow = DisplayScreen.HudHeight / 8;
    gameplayVram.ExecuteWordTransfer(
        [0x0801],
        destinationWord: unchecked((ushort)(kraidBg2BaseWord + firstGameplaySourceRow * 32)),
        wordIncrement: 1);
    gameplayFrame = RenderOrdinaryGameplay(gameplayCgram,
        new GameplaySceneLayer(
            // The view names the background pixel the screen's top shows: the row below a camera at row zero (#1514).
            new GameplaySceneView(new Position16<RoomPixels>(0, 0), GameplaySceneView.LayerPixelAtScreenTop(new Position16<BackgroundPixels>(0, 0)),
                ScreenLayers.Foreground | ScreenLayers.Background | ScreenLayers.Sprites),
            // Kraid's body is the 64x64 map he loads at $4000, 512 pixels square, drawn from its pictures (#1403).
            RoomBackgroundsFixtures.Blank() with
            {
                BossBody = RoomBackgroundsFixtures.BodyFromTilemap(gameplayVram.Bytes, gameplayCgram.Colors, kraidBg2BaseWord,
                    new Extent16<BackgroundPixels>(512, 512)),
            }));
    AssertEqual(new RgbaPixel(255, 0, 0),
        gameplayFrame[DisplayScreen.HudHeight * DisplayScreen.Width],
        "ordinary compositor consumes Kraid's 64x64 BG2 map at $4000");

    Console.WriteLine("  BG render: BGSC geometry agrees.");

    // The frame the display renderer draws of the gameplay layer alone, with no sprites.
    static RgbaPixel[] RenderOrdinaryGameplay(SnesCgram cgram, GameplaySceneLayer layer) =>
        DisplayRendering.Render(new LayeredScene(FramePaint.FromNativeGameplay(cgram.Colors, []), new DisplayLayer[] { layer }, DisplayBrightness.Full));
}

/// <summary>
/// Synthetic cartridge with the literal `$88:A266-$88:A2A5` horizontal samples and vertical
/// band boundaries used by `$88:8CC6/$8D04/$8D46` (kept independent of the production loop so
/// a transposed table or rounded product cannot self-validate), and all sixteen pre-explosion
/// fixed-color entries set to one unmistakable color.
/// </summary>
static TestAddressSpace CreatePowerBombColorMathWindowCartridge()
{
    var bus = new TestAddressSpace();
    bus.WriteBytes(0x88a266, [
        0x00, 0x0c, 0x19, 0x25, 0x31, 0x3e, 0x4a, 0x56,
        0x61, 0x6d, 0x78, 0x83, 0x8e, 0x98, 0xa2, 0xab,
        0xb5, 0xbd, 0xc5, 0xcd, 0xd4, 0xdb, 0xe1, 0xe7,
        0xec, 0xf1, 0xf4, 0xf8, 0xfb, 0xfd, 0xfe, 0xff,
    ]);
    bus.WriteBytes(0x88a286, [
        0xbf, 0xbf, 0xbe, 0xbd, 0xba, 0xb8, 0xb6, 0xb2,
        0xaf, 0xab, 0xa6, 0xa2, 0x9c, 0x96, 0x90, 0x8a,
        0x84, 0x7d, 0x75, 0x6e, 0x66, 0x5e, 0x56, 0x4d,
        0x45, 0x3c, 0x33, 0x2a, 0x20, 0x17, 0x0d, 0x04,
    ]);
    for (int color = 0; color < 16; color++)
        bus.WriteBytes(0x889079 + color * 3, [0x01, 0x02, 0x03]);
    return bus;
}

/// <summary>Proves that power-bomb composition uses the drawn radius frame and an oval centred on the blast.</summary>
internal static void VerifyPowerBombColorMathWindow()
{
    TestAddressSpace bus = CreatePowerBombColorMathWindowCartridge();

    const ushort centerX = 100;
    const ushort centerY = 100;
    var explosion = new SamusPowerBombExplosionState { PresentationColors = PowerBombGlowColorFile.Read(new MemoryStream(SuperMetroid.AssetExtraction.Samus.PowerBombGlowColorExtractor.Extract(bus))) };
    explosion.Arm();
    explosion.Spawn(new(centerX, centerY));

    // Spawning happens after this frame's oval was chosen, so the spawn frame shows nothing.
    AssertEqual(ScreenWindow.Closed, CaptureWindow().Window, "power-bomb spawn frame has no premature oval");

    explosion.StepFrame();
    AssertEqual(PowerBombExplosionPhase.Inactive, explosion.RenderedPhase,
        "first HDMA pass initializes the list without generating a window");
    // The first frame draws radius 4, then advances the live state to radius 52. The oval is that drawn radius wide
    // and three quarters of it tall (#1468): using the already-updated radius would be far wider.
    explosion.StepFrame();
    AssertEqual(PowerBombRadius.FromWholePixels(4), explosion.RenderedPreExplosionRadius,
        "power-bomb renderer retains pre-update radius");
    AssertEqual(PowerBombRadius.FromWholePixels(52), explosion.PreExplosionRadius,
        "power-bomb logic advances next-frame radius");
    ColourAddWindowLayer scaled = CaptureWindow();
    AssertEqual(ScreenWindow.Of(WindowOval.Ellipse(Offset16<ScreenPixels>.Of(centerX, centerY), new Extent16<ScreenPixels>(4, 3)), DisplayScreen.HudHeight),
        scaled.Window, "the first flash is an oval of the drawn radius");
    AssertEqual(new RgbColor(8, 16, 24), scaled.Colour, "the first flash adds the installed fixed colour");

    // The walked yellow ovals are centred on the blast, not on a screen midpoint.
    while (explosion.Phase == PowerBombExplosionPhase.PreExplosionWhite)
        explosion.StepFrame();
    explosion.StepFrame();
    AssertEqual(PowerBombExplosionPhase.PreExplosionYellow, explosion.RenderedPhase,
        "first pre-scaled yellow frame is retained for composition");
    AssertEqual<PowerBombShapeCursor?>(new PowerBombShapeCursor(PowerBombShapeStream.Yellow, 0), explosion.RenderedShape,
        "first yellow frame selects the first yellow oval");
    AssertEqual(ScreenWindow.Of(WindowOval.Ellipse(Offset16<ScreenPixels>.Of(centerX, centerY), new Extent16<ScreenPixels>(146, 109)), DisplayScreen.HudHeight),
        CaptureWindow().Window, "the first yellow oval is centred on the blast");
    Console.WriteLine("  Power bomb: drawn-radius timing, the oval's size and its centre agree.");

    // The colour the displayed frame adds for the explosion, as the gameplay capture takes it.
    ColourAddWindowLayer CaptureWindow() =>
        GameplayEffectCapture.CapturePowerBombGlow(PowerBombRenderSnapshot.Capture(explosion), new Position16<RoomPixels>(0, 0))
            ?? throw new InvalidOperationException("An active Power Bomb captures its window.");
}

/// <summary>Checks bank-$88 sky fixed-point bands and four queued circular-map rows.</summary>
internal static void VerifyScrollingSkyState()
{
    AssertTrue(
        ScrollingSkyState.IsLandRoomMain(RoomMainBehavior.ScrollingSkyLand),
        "$8F:C116 selects land scrolling sky");
    AssertTrue(
        ScrollingSkyState.IsLandRoomMain(RoomMainBehavior.ScrollingSkyLandZebesTimebombSet),
        "$8F:C120 selects land scrolling sky before quake work");
    AssertTrue(
        !ScrollingSkyState.IsLandRoomMain(RoomMainBehavior.ScrollingSkyOcean),
        "$8F:C11B ocean wrapper is not silently treated as land sky");

    AssertEqual(ScrollingSkyChunkTable.Land, ScrollingSkyState.VariantOf(RoomMainBehavior.ScrollingSkyLandZebesTimebombSet),
        "land sky mains read the land chunk table");
    AssertEqual(ScrollingSkyChunkTable.Ocean, ScrollingSkyState.VariantOf(RoomMainBehavior.ScrollingSkyOcean),
        "the ocean main reads the ocean chunk table");
    AssertThrows<ArgumentOutOfRangeException>(() => ScrollingSkyState.VariantOf(RoomMainBehavior.None),
        "a room main without a sky has no chunk table");

    var sky = new ScrollingSkyState(ScrollingSkyChunkTable.Land);
    sky.ProcessFrame(cameraY: 0x041f, timeIsFrozen: false);
    AssertEqual(new Position16<BackgroundPixels>(0, 0x041f), sky.BackgroundScroll, "$88:AFB2 BG2 scroll copies layer-one Y and leaves X zero");

    // The land sky's five chunks repeat from its first: chunk five shows page zero.
    AssertEqual(0, ScrollingSkyChunks.PageOf(ScrollingSkyChunkTable.Land, 5), "land chunk five wraps to the first sky page");

    // The $02E0 band shows data slot eight, which receives both the $0238 row's +0.8000 and its
    // own +0.C000: the shared fastest drift, a pixel and a quarter a frame, reaches pixel 1 after one frame.
    AssertEqual(1, sky.OffsetOf(ScrollingSkyBandDefinitions.Fastest), "the bands at rows 568 and 736 share the fastest drift");
    ScrollingSkyLayer layer = sky.Publish(0x02c0) ?? throw new InvalidOperationException("The configured sky publishes its layer.");
    AssertEqual(1, ScrollShapeLines.BandScroll(SkyBandScrolls.Of(layer, 0), DisplayScreen.HudHeight), "the first gameplay line shows row 736's shared fastest drift");

    sky.ProcessFrame(cameraY: 0x041f, timeIsFrozen: true);
    AssertEqual(false, sky.ShowsDrift, "a frozen frame shows the bands unshifted");
    ScrollingSkyLayer frozen = sky.Publish(0x02c0) ?? throw new InvalidOperationException("The configured sky publishes its layer.");
    AssertTrue(!frozen.ShowsDrift && ScrollShapeLines.GameplayScrolls(SkyBandScrolls.Of(frozen, 0)).All(scroll => scroll == 0), "a frozen frame shows every band unshifted");
    AssertEqual<int?>(1, frozen.BandAt(0x02e0)?.Offset, "a frozen frame keeps the band offsets");
    AssertTrue(new ScrollingSkyState(ScrollingSkyChunkTable.Land, hasBands: false).Publish(0) is null,
        "a sky main without drifting bands publishes no sky layer");

    Console.WriteLine("  Sky: drifting bands, chunk wrap and the published layer agree.");
}

    /// <summary>Runs the HUD template and map reveal checks.</summary>
    [VerificationCommand("--hud-state")]
    internal static int HudStateCommand()
    {
        Suite(nameof(VerifyHudState), () => VerifyHudState());
        return 0;
    }

    /// <summary>Runs the ordered bank-$8F room-state selector checks.</summary>
    [VerificationCommand("--cartridge-room-state-selection")]
    internal static int CartridgeRoomStateSelectionCommand()
    {
        Suite(nameof(VerifyCartridgeRoomStateSelection), () => VerifyCartridgeRoomStateSelection());
        return 0;
    }

    /// <summary>Runs the background scroll-state checks.</summary>
    [VerificationCommand("--background-scroll-state")]
    internal static int BackgroundScrollStateCommand()
    {
        Suite(nameof(VerifyBackgroundScrollState), () => VerifyBackgroundScrollState());
        return 0;
    }

    /// <summary>Runs room tilemap streaming and scrolling-sky checks.</summary>
    [VerificationCommand("--room-sky-tilemaps")]
    internal static int RoomSkyTilemapsCommand()
    {
        Suite(nameof(VerifyScrollingSkyState), () => VerifyScrollingSkyState());
        Suite(nameof(VerifyRoomSkyTilemaps), () => VerifyRoomSkyTilemaps());
        return 0;
    }

    /// <summary>Runs the moved-Samus camera tracking and camera-context checks.</summary>
    [VerificationCommand("--moved-samus-camera-tracking")]
    internal static int MovedSamusCameraTrackingCommand()
    {
        Suite(nameof(VerifyMovedSamusCameraTracking), () => VerifyMovedSamusCameraTracking());
        return 0;
    }
}
