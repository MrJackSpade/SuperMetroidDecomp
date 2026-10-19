using SuperMetroid.Core.Assets;
using static SuperMetroid.Verification.Rooms.Backgrounds.RoomSkyTilemapsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Buffers.Binary;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using static SuperMetroid.Verification.Frontend.Map.MapSpritesVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
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
using SuperMetroid.Core.Hud.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.RoomHeaderImport;
using SuperMetroid.TestSupport.VerificationOracles;
using SuperMetroid.Core.Map;
namespace SuperMetroid.Verification.Rendering;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms.Artwork;

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
    byte[] storage = new byte[RoomScrollGrid.StorageByteCount];
    source[..Math.Min(source.Length, storage.Length)].CopyTo(storage);
    return RoomScrollGrid.LoadCompiled(new RoomScrollStorage(), storage, size);
}

/// <summary>An explicit scroll table whose 50 copied bytes are all blue (one).</summary>
private static byte[] AllBlueExplicitScrollTable() =>
    Enumerable.Repeat((byte)1, RoomScrollGrid.StorageByteCount).ToArray();

/// <summary>OBJ, HUD, cameras, scrolling, tilemaps, room data, and sky verification.</summary>
[PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
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
    cgram.SetColor(128 + 2 * 16 + 1, Bgr555.FromWord(0x001f));
    oam.BeginFrame();
    DrawImportedSpritemap(bus, oam, 0x818000, origin: new(10, 20), paletteBits: 0x0400);
    oam.FinalizeFrame();

    var pixels = SnesObjRenderer.Render(oam, vram, cgram, obsel: 0);
    AssertEqual(new SuperMetroid.Core.Hardware.Rgba32(255, 0, 0), pixels[20 * 256 + 10], "OBJ colored planar pixel");
    AssertEqual(new SuperMetroid.Core.Hardware.Rgba32(0, 0, 0, 0), pixels[20 * 256 + 11], "OBJ color zero transparency");

    // OBJ priority controls only the winning sprite pixel's position relative to BGs.
    // It does not override OAM order when two sprites overlap. Model the Ceres failure
    // directly: an earlier Samus-like OBJ2 must exclude a later Ridley-like OBJ3 from the
    // OBJ3 plane, otherwise the compositor paints the hidden boss over Samus at the end of
    // its BG/OBJ ladder. Distinct palettes make the winner observable without private ROM
    // art; both entries deliberately use the same one-pixel tile and screen coordinate.
    cgram.SetColor(128 + 3 * 16 + 1, Bgr555.FromWord(0x03e0));
    oam.BeginFrame();
    oam.AddRawSmallSprite(position: new Position16<OamPixels>(10, 20), attributes: 0x2400); // OAM 0: OBJ2, palette 2.
    oam.AddRawSmallSprite(position: new Position16<OamPixels>(10, 20), attributes: 0x3600); // OAM 1: OBJ3, palette 3.
    oam.FinalizeFrame();
    Rgba32[] obj2Plane = SnesObjRenderer.Render(oam, vram, cgram, obsel: 0, priority: 2);
    Rgba32[] obj3Plane = SnesObjRenderer.Render(oam, vram, cgram, obsel: 0, priority: 3);
    ResolvedObjFrame resolvedObjects = SnesObjRenderer.RenderResolved(oam, vram, cgram, obsel: 0);
    AssertEqual(
        new SuperMetroid.Core.Hardware.Rgba32(255, 0, 0),
        obj2Plane[20 * 256 + 10],
        "lower-OAM OBJ2 wins cross-priority overlap");
    AssertEqual(
        new SuperMetroid.Core.Hardware.Rgba32(0, 0, 0, 0),
        obj3Plane[20 * 256 + 10],
        "higher-OAM OBJ3 excluded by cross-priority overlap");
    AssertEqual(
        new SuperMetroid.Core.Hardware.Rgba32(255, 0, 0),
        resolvedObjects.Pixels[20 * 256 + 10],
        "single-pass resolved OBJ color");
    AssertEqual((byte)2, resolvedObjects.Priorities[20 * 256 + 10], "single-pass resolved OBJ priority");
    AssertEqual(
        SnesObjRenderer.TransparentPriority,
        resolvedObjects.Priorities[20 * 256 + 11],
        "single-pass transparent OBJ priority sentinel");

    // The production timer's first tile is $1E0 under OBSEL=$03. This assertion fixes
    // the important reverse-engineered relationship: character data starts at word $7E00.
    AssertEqual(0xfc00, FrontendOamDraws.ResolveTileByteAddress(0x1e0, 0x03), "timer tile $1E0 VRAM byte address");
    AssertEqual(0xff00, FrontendOamDraws.ResolveTileByteAddress(0x1f8, 0x03), "timer tile $1F8 VRAM byte address");

    Console.WriteLine("  OBJ: OBSEL addressing, planar pixels, CGRAM, transparency, and cross-priority OAM ownership agree.");
}

/// <summary>
/// Verifies the HUD ROM-template mutation, authentic WRAM queue source, 2-bpp tilemap
/// interpretation, palette selection, and final pixel without relying on the private ROM.
/// </summary>
[PpuLayoutBoundary("exact hardware test: checks native CGRAM slots, native VRAM words")]
internal static void VerifyHudStateAndBg3Rendering()
{
    var bus = new TestAddressSpace();

    // Seed the three-row ROM template entirely with the canonical blank HUD tile $2C0F.
    // The synthetic digit table uses conspicuous character names $100-$109.
    SeedBlankHudTemplateAndDigitTables(bus);

    var hud = new HudState();
    hud.BindPresentation(GameplayHudPresentation.Load(new MemoryStream(
        SuperMetroid.AssetExtraction.Hud.GameplayHudPresentationExtractor.Extract(bus))));
    hud.Initialize(HudSnapshot.CeresDebug);
    AssertEqual(0x2d09, hud.Tiles[0x8c / 2], "HUD health tens digit from ROM table");
    AssertEqual(0x2d09, hud.Tiles[0x8e / 2], "HUD health ones digit from ROM table");

    var damagedSamus = new SamusState { Health = 94, MaxHealth = 99 };
    hud.UpdateGameplayCounters(damagedSamus);
    AssertEqual(0x2d09, hud.Tiles[0x8c / 2], "live HUD damage tens digit");
    AssertEqual(0x2d04, hud.Tiles[0x8e / 2], "live HUD damage ones digit");

    // HandleHudTilemap owns selection presentation independently of the input handler.
    // Installing missiles after initialization mirrors a live pickup, then selecting them
    // proves both palette directions and the one-frame QueueSfx publication.
    damagedSamus.MaxMissiles = 5;
    damagedSamus.Missiles = 5;
    damagedSamus.SelectedHudItem = HudItem.Missile;
    hud.UpdateGameplayCounters(damagedSamus);
    AssertEqual(0x1000, hud.Tiles[0x14 / 2] & 0x1c00,
        "live HUD selects missile icon with palette four");
    AssertTrue(hud.SelectionSoundRequestedThisFrame,
        "live HUD publishes cartridge selection sound on change");
    hud.UpdateGameplayCounters(damagedSamus);
    AssertTrue(!hud.SelectionSoundRequestedThisFrame,
        "stable HUD selection does not repeat the selection sound");
    damagedSamus.SelectedHudItem = HudItem.Beam;
    hud.UpdateGameplayCounters(damagedSamus);
    AssertEqual(0x1400, hud.Tiles[0x14 / 2] & 0x1c00,
        "live HUD restores deselected missile icon to palette five");

    // Area zero points to a synthetic two-screen-wide Crateria map. Give every map tile a
    // character equal to its SNES-layout index and mark every coordinate as existing; the
    // expected HUD words then prove room origin + Samus screen coordinates, 5x3 centering,
    // exploration palette, and the map-station/unexplored palette independently.
    SeedSyntheticCrateriaAreaMap(bus);

    var mapSystem = new Bank80SystemState();
    mapSystem.SetAreaMapAcquired(0);
    hud.UpdateMinimap(mapSystem, new MinimapFrame { AreaIndex = AreaId.Crateria, RoomMap = new(0x17, 0), RoomSize = new(9 * 16, 5 * 16), Samus = new(0x0440, 0x04bb), NmiFrameCounter = 8, PresentationMap = SuperMetroid.AssetExtraction.Frontend.Map.AreaMapImporter.Load(bus, AreaId.Crateria) });
    AssertEqual(new Position16<MapTiles>(27, 5), hud.MinimapCenter, "minimap Landing Site absolute tile");
    AssertEqual(0x2c99, hud.Tiles[26], "minimap top-left unvisited map-station tile");
    AssertEqual(0x28bb, hud.Tiles[60], "minimap explored center tile");

    var noMapSystem = new Bank80SystemState();
    hud.UpdateMinimap(noMapSystem, new MinimapFrame { AreaIndex = AreaId.Crateria, RoomMap = new(0x17, 0), RoomSize = new(9 * 16, 5 * 16), Samus = new(0x0440, 0x04bb), NmiFrameCounter = 0, PresentationMap = SuperMetroid.AssetExtraction.Frontend.Map.AreaMapImporter.Load(bus, AreaId.Crateria) });
    AssertEqual(0x3cbb, hud.Tiles[60], "minimap blinking center palette");
    AssertEqual(0x2c1f, hud.Tiles[26], "minimap hides unvisited tile without map station");

    // The sloping hall character explores the corner above the current cell. Native
    // renders the upper row before setting this bit, so its palette changes next update.
    foreach (int x in new[] { 31, 32 })
    foreach (ushort slope in new ushort[] { 0x0028, 0x4228, 0x8028, 0xc228 })
    {
        var slopeSystem = new Bank80SystemState();
        Position16<MapTiles> currentTile = new(checked((ushort)x), 5);
        Position16<MapTiles> aboveTile = new(checked((ushort)x), 4);
        ushort savedCurrent = ReadFixtureMapWord(bus, currentTile);
        ushort savedAbove = ReadFixtureMapWord(bus, aboveTile);
        SeedFixtureMapWord(bus, currentTile, slope);
        SeedFixtureMapWord(bus, aboveTile, 0x0029);
        hud.UpdateMinimap(slopeSystem, new MinimapFrame { AreaIndex = AreaId.Crateria, RoomMap = new((byte)x, 4), RoomSize = new(16, 16), Samus = new(128, 128), NmiFrameCounter = 8, PresentationMap = SuperMetroid.AssetExtraction.Frontend.Map.AreaMapImporter.Load(bus, AreaId.Crateria) });
        AssertTrue(slopeSystem.IsMapTileExplored(0, new(checked((ushort)x), 4)), "sloped hallway explores its upper corner on either map page");
        AssertEqual(0x2c1f, hud.Tiles[28], "upper corner remains blank on native discovery update");
        hud.UpdateMinimap(slopeSystem, new MinimapFrame { AreaIndex = AreaId.Crateria, RoomMap = new((byte)x, 4), RoomSize = new(16, 16), Samus = new(128, 128), NmiFrameCounter = 8, PresentationMap = SuperMetroid.AssetExtraction.Frontend.Map.AreaMapImporter.Load(bus, AreaId.Crateria) });
        AssertEqual(0x2829, hud.Tiles[28], "upper corner displays its explored palette on following update");
        SeedFixtureMapWord(bus, currentTile, savedCurrent);
        SeedFixtureMapWord(bus, aboveTile, savedAbove);
    }
    var ordinaryCornerSystem = new Bank80SystemState();
    // Character $029 is adjacent in the graphics catalog but must not get the $028
    // exploration side effect; this also rejects indiscriminate upper-cell revealing.
    Position16<MapTiles> ordinaryTile = new(30, 5);
    ushort ordinarySaved = ReadFixtureMapWord(bus, ordinaryTile);
    SeedFixtureMapWord(bus, ordinaryTile, 0x0029);
    hud.UpdateMinimap(ordinaryCornerSystem, new MinimapFrame { AreaIndex = AreaId.Crateria, RoomMap = new(30, 4), RoomSize = new(16, 16), Samus = new(128, 128), NmiFrameCounter = 8, PresentationMap = SuperMetroid.AssetExtraction.Frontend.Map.AreaMapImporter.Load(bus, AreaId.Crateria) });
    AssertTrue(!ordinaryCornerSystem.IsMapTileExplored(0, new(30, 4)), "ordinary map character does not explore upper corner");
    SeedFixtureMapWord(bus, ordinaryTile, ordinarySaved);

    // A reveal override combines with display state only. Remove the station-mask bit from
    // the top-left HUD cell while leaving its cartridge tile nonblank, giving Public and
    // Secret modes an observable difference without manufacturing a room approximation.
    const int secretHudX = 25;
    const int secretHudY = 4;
    const int publicHudX = 26;
    ClearFixtureMapStationBit(bus, new(secretHudX, secretHudY));
    var overrideSystem = new Bank80SystemState();
    hud.UpdateMinimap(overrideSystem, new MinimapFrame { AreaIndex = AreaId.Crateria, RoomMap = new(0x17, 0), RoomSize = new(9 * 16, 5 * 16), Samus = new(0x0440, 0x04bb), NmiFrameCounter = 8, MapRevealMode = MapRevealMode.Public, PresentationMap = SuperMetroid.AssetExtraction.Frontend.Map.AreaMapImporter.Load(bus, AreaId.Crateria) });
    AssertEqual((ushort)MapTileWords.HudBlank, hud.Tiles[26],
        "Public HUD override excludes cartridge secret-only cell");
    AssertEqual(0x2c00, hud.Tiles[27] & 0x3c00,
        "Public HUD override reveals station cell as unentered");
    hud.UpdateMinimap(overrideSystem, new MinimapFrame { AreaIndex = AreaId.Crateria, RoomMap = new(0x17, 0), RoomSize = new(9 * 16, 5 * 16), Samus = new(0x0440, 0x04bb), NmiFrameCounter = 8, MapRevealMode = MapRevealMode.Secret, PresentationMap = SuperMetroid.AssetExtraction.Frontend.Map.AreaMapImporter.Load(bus, AreaId.Crateria) });
    AssertEqual(0x2c00, hud.Tiles[26] & 0x3c00,
        "Secret HUD override reveals cartridge secret-only cell as unentered");
    overrideSystem.MarkExploredMapTile(AreaId.Crateria, new(secretHudX, secretHudY));
    hud.UpdateMinimap(overrideSystem, new MinimapFrame { AreaIndex = AreaId.Crateria, RoomMap = new(0x17, 0), RoomSize = new(9 * 16, 5 * 16), Samus = new(0x0440, 0x04bb), NmiFrameCounter = 8, MapRevealMode = MapRevealMode.Secret, PresentationMap = SuperMetroid.AssetExtraction.Frontend.Map.AreaMapImporter.Load(bus, AreaId.Crateria) });
    AssertEqual(0x2800, hud.Tiles[26] & 0x3c00,
        "Secret HUD keeps entered secret cell visually distinct");
    AssertEqual(0x2c00, hud.Tiles[27] & 0x3c00,
        "Secret HUD leaves override-only public cell unentered");
    hud.UpdateMinimap(overrideSystem, new MinimapFrame { AreaIndex = AreaId.Crateria, RoomMap = new(0x17, 0), RoomSize = new(9 * 16, 5 * 16), Samus = new(0x0440, 0x04bb), NmiFrameCounter = 8, MapRevealMode = MapRevealMode.None, PresentationMap = SuperMetroid.AssetExtraction.Frontend.Map.AreaMapImporter.Load(bus, AreaId.Crateria) });
    AssertEqual((ushort)MapTileWords.HudBlank, hud.Tiles[27],
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
    var rightPageSystem = new Bank80SystemState();
    rightPageSystem.SetAreaMapAcquired(AreaId.Crateria);
    hud.UpdateMinimap(rightPageSystem, new MinimapFrame { AreaIndex = AreaId.Crateria, RoomMap = new(34, 18), RoomSize = new(2 * 16, 16), Samus = new(0x0080, 0x0080), NmiFrameCounter = 8, PresentationMap = SuperMetroid.AssetExtraction.Frontend.Map.AreaMapImporter.Load(bus, AreaId.Crateria) });
    AssertEqual(new Position16<MapTiles>(34, rightPageCenterY), hud.MinimapCenter, "right-page minimap absolute tile");
    AssertEqual(0x2e63, hud.Tiles[61],
        "right-page minimap reads the same eastern cell as pause map");
    AssertEqual(0x2c1f, hud.Tiles[59],
        "right-page sparse map keeps the western cell blank");

    var queue = new VramWriteQueue();
    var vram = new SnesVram();
    hud.QueueUpload(queue);
    AssertEqual(HudState.MutableByteCount, (int)queue.Entries[0].SizeInBytes, "HUD queue transfer size");
    AssertEqual(HudState.WorkRamAddress, queue.Entries[0].SourceAddress, "HUD queue keeps the native $7E:C608 source on its record");
    AssertEqual(HudState.VramDestination, queue.Entries[0].Destination.Encoded, "HUD queue VRAM destination");
    queue.DrainTo(vram);
    AssertEqual(0x0f, vram.ReadByte(HudState.VramDestination * 2), "HUD first tilemap low byte reaches VRAM");
    AssertEqual(0x2c, vram.ReadByte(HudState.VramDestination * 2 + 1), "HUD first tilemap high byte reaches VRAM");

    // Isolate the BG3 renderer at a harmless tilemap base. Entry tile 2 / palette 1 points
    // at a tile whose upper-left plane-0 bit is set; CGRAM 5 is maximum green.
    SeedBg3TilemapEntry(bus);
    ImportedVramOracle.ExecuteQueued(vram, bus, 0x818000, 2, encodedDestination: 0x0100);
    SeedUpperLeftPixelTile(bus);
    ImportedVramOracle.ExecuteQueued(vram, bus, 0x828000, 16, encodedDestination: 0x0010);
    var cgram = new SnesCgram();
    cgram.SetColor(5, Bgr555.FromWord(0x03e0));
    var pixels = SnesBgTilemapRenderer.Render2Bpp(vram, cgram, tilemapBaseWord: 0x0100, characterBaseWord: 0, rowCount: 1);
    AssertEqual(new SuperMetroid.Core.Hardware.Rgba32(0, 255, 0), pixels[0], "BG3 tile/palette pixel");

    Console.WriteLine("  HUD: inventory, live minimap coordinates/blink, WRAM upload, and BG3 pixels agree.");
}

/// <summary>Reads the synthetic Crateria map tile before a fixture temporarily replaces it.</summary>
[CpuBusBoundary("exact bus test: reads a seeded bank-$B5 area-map tilemap word back from the synthetic cartridge fixture to restore it")]
static ushort ReadFixtureMapWord(TestAddressSpace bus, Position16<MapTiles> tile) =>
    SuperMetroid.AssetExtraction.Cartridge.RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus),
        0xb58000 + AreaMapLayout.GetTilemapWordIndex(tile) * 2);

/// <summary>Overwrites the synthetic Crateria bank-$B5 area-map tilemap word at <paramref name="tile"/>.</summary>
[CpuBusBoundary("exact bus test: synthetic cartridge fixture for one bank-$B5 area-map tilemap word")]
static void SeedFixtureMapWord(TestAddressSpace bus, Position16<MapTiles> tile, ushort word) =>
    WriteTestWord(bus, 0xb58000 + AreaMapLayout.GetTilemapWordIndex(tile) * 2, word);

/// <summary>
/// Clears <paramref name="tile"/>'s bit in the synthetic Crateria map-station mask at $82:9000,
/// leaving its cartridge tilemap word nonblank so reveal modes have an observable difference.
/// </summary>
[CpuBusBoundary("exact bus test: synthetic cartridge fixture for one cleared area map-station mask bit")]
static void ClearFixtureMapStationBit(TestAddressSpace bus, Position16<MapTiles> tile)
{
    int address = 0x829000 + AreaMapLayout.GetBitByteIndex(tile);
    WriteTestByte(bus, address, (byte)(bus.ReadByte(address) & ~AreaMapLayout.GetBitMask(tile.X)));
}

/// <summary>Clears the whole synthetic Crateria map-station mask at $82:9000, then sets only <paramref name="tile"/>'s bit.</summary>
[CpuBusBoundary("exact bus test: synthetic cartridge fixture for a single-tile area map-station mask")]
static void ResetFixtureMapStationBitsToSingleTile(TestAddressSpace bus, Position16<MapTiles> tile)
{
    for (int index = 0; index < 0x100; index++)
        WriteTestByte(bus, 0x829000 + index, 0);
    WriteTestByte(bus, 0x829000 + AreaMapLayout.GetBitByteIndex(tile), AreaMapLayout.GetBitMask(tile.X));
}

/// <summary>
/// Seeds area zero's synthetic two-screen-wide Crateria map: the area pointer tables name
/// mask $82:9000 and tilemap $B5:8000, every mask bit is set, and every tilemap word's
/// character equals its SNES-layout index.
/// </summary>
[CpuBusBoundary("exact bus test: synthetic cartridge fixture for the synthetic Crateria area map")]
static void SeedSyntheticCrateriaAreaMap(TestAddressSpace bus)
{
    WriteTestByte(bus, 0x82964a, 0x00);
    WriteTestByte(bus, 0x82964b, 0x80);
    WriteTestByte(bus, 0x82964c, 0xb5);
    WriteTestWord(bus, 0x829717, 0x9000);
    for (int index = 0; index < 0x100; index++)
        WriteTestByte(bus, 0x829000 + index, 0xff);
    for (int index = 0; index < 0x800; index++)
        WriteTestWord(bus, 0xb58000 + index * 2, (ushort)(index & 0x03ff));
}

/// <summary>
/// Seeds the three-row HUD ROM template entirely with the canonical blank HUD tile $2C0F, and
/// both digit tables with conspicuous character names $100-$109.
/// </summary>
[CpuBusBoundary("exact bus test: synthetic cartridge fixture for the blank HUD template and digit tables")]
static void SeedBlankHudTemplateAndDigitTables(TestAddressSpace bus)
{
    for (int tile = 0; tile < HudState.MutableTileCount; tile++)
        WriteTestWord(bus, 0x8098cb + tile * 2, 0x2c0f);
    for (int digit = 0; digit < 10; digit++)
    {
        WriteTestWord(bus, 0x809dbf + digit * 2, (ushort)(0x2c00 | (0x100 + digit)));
        WriteTestWord(bus, 0x809dd3 + digit * 2, (ushort)(0x2c00 | (0x100 + digit)));
    }
}

/// <summary>A 4-bpp tile at $82:8000 whose only set bit is plane 0's upper-left pixel (palette index 1).</summary>
[CpuBusBoundary("exact bus test: synthetic cartridge fixture for a single upper-left-pixel tile at $82:8000")]
static void SeedUpperLeftPixelTile(TestAddressSpace bus) =>
    WriteTestByte(bus, 0x828000, 0x80);

/// <summary>A one-entry common spritemap at $81:8000: one small tile-zero OBJ at offset (0, 0).</summary>
[CpuBusBoundary("exact bus test: synthetic cartridge fixture for a one-entry OBJ spritemap at $81:8000")]
static void SeedSingleSmallObjSpritemap(TestAddressSpace bus) =>
    bus.WriteBytes(0x818000, [0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00]);

/// <summary>One BG3 tilemap entry at $81:8000 naming tile 2 with palette 1.</summary>
[CpuBusBoundary("exact bus test: synthetic cartridge fixture for one BG3 tilemap entry at $81:8000")]
static void SeedBg3TilemapEntry(TestAddressSpace bus) =>
    bus.WriteBytes(0x818000, [0x02, 0x04]);

/// <summary>Seeds one bank-$A9 walk instruction stream with retail timing and harmless synthetic spritemap operands.</summary>
[CpuBusBoundary("exact bus test: synthetic cartridge fixture for one Mother Brain bank-$A9 walk program")]
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
[CpuBusBoundary("exact bus test: synthetic cartridge fixture for the Mother Brain bank-$A9 fast crouch program")]
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
    var bus = new TestAddressSpace();
    byte[] source = AllBlueExplicitScrollTable();

    // The logical 3x2 grid begins fully blue. Give the five copied padding bytes distinct
    // values to prove LoadExplicit does not synthesize zeroes after the sixth logical cell.
    for (int index = 6; index < RoomScrollGrid.StorageByteCount; index++)
        source[index] = (byte)(0x80 + index);

    RoomScrollGrid grid = LoadExplicitScrollFixture(source, new Extent16<RoomScreens>(3, 2));
    AssertEqual(0x86, grid.StorageBytes[6], "scroll loader retains first nonlogical byte");
    AssertEqual(0xb1, grid.StorageBytes[49], "scroll loader retains fiftieth byte");

    // Single Chamber's neighboring room uses a four-byte explicit table for a 4x2 room.
    // The native fixed-size copy makes the following room header ($1F,$02,$1D,$04) its
    // logical second row. Those bytes are not corrupt enum values: bank $80 compares them
    // only with zero (blocked) and one (blue alignment), treating every other nonzero byte
    // like the conventional green value. Preserve that exact cartridge behavior.
    var shortTableBus = new TestAddressSpace();
    byte[] shortTableAndFollowingHeader = [2, 2, 2, 2, 0x1f, 0x02, 0x1d, 0x04];
    RoomScrollGrid shortTableGrid = LoadExplicitScrollFixture(
        shortTableAndFollowingHeader,
        new Extent16<RoomScreens>(4, 2));
    AssertEqual(0x1f, shortTableGrid.ReadStorage(4),
        "short explicit scroll table preserves following room-header byte");
    AssertEqual(0x04, shortTableGrid.ReadStorage(7),
        "short explicit scroll table preserves complete logical overread row");

    var camera = new ScrollBoundaryCamera(grid);

    // Right: the screen to the right is red, so the attempted +16 is rejected and the
    // routine's deliberate extra two-pixel retreat clamps the signed underflow to zero.
    grid.SetLogicalState(new(1, 0), RoomScrollState.RedBoundary);
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
    grid.SetLogicalState(new(0, 0), RoomScrollState.Blue);
    grid.SetLogicalState(new(0, 1), RoomScrollState.RedBoundary);
    camera.SetPosition(new Position16<RoomPixels>(0, 224));
    camera.MoveDown(16);
    AssertEqual(222, camera.PixelPosition.Y, "$80:A893 red boundary moving down");

    // Up: starting inside that red lower cell gives the corresponding two-pixel retreat.
    camera.SetPosition(new Position16<RoomPixels>(0, 272));
    camera.MoveUp(16);
    AssertEqual(274, camera.PixelPosition.Y, "$80:A936 red boundary moving up");

    // Physical right edge is (width-1)*$100 regardless of the scroll padding bytes.
    grid.SetLogicalState(new(1, 0), RoomScrollState.Blue);
    camera.SetPosition(new Position16<RoomPixels>(0x01f8, 0));
    camera.MoveRight(16);
    AssertEqual(0x0200, camera.PixelPosition.X, "scroll camera physical room maximum");

    // Issue #12's slot-one capture is Construction Zone immediately after returning from
    // First Missile. Its room header starts `[blue, red]`; the incoming door points at
    // `$8F:BE25`, which must replace that pair with `[green, blue]` before camera tracking.
    grid.SetLogicalState(new(0, 0), RoomScrollState.Blue);
    grid.SetLogicalState(new(0, 1), RoomScrollState.RedBoundary);
    DoorSetupCodeInterpreter.ApplyScrollWrites(
        DoorSetupCode.DoorASM_Scroll_0_Green_1_Blue,
        DoorDefinitions.Get(DoorPointers.ConstructionZoneFromFirstMissile).Name,
        grid);
    AssertEqual((byte)RoomScrollState.Green, grid.ReadStorage(0),
        "$8F:BE25 writes Construction Zone screen zero green");
    AssertEqual((byte)RoomScrollState.Blue, grid.ReadStorage(1),
        "$8F:BE25 writes Construction Zone screen one blue");
    grid.SetStorage(6, RoomScrollState.RedBoundary);
    DoorSetupCodeInterpreter.ApplyScrollWrites(
        DoorSetupCode.DoorCode_Scroll6_Green,
        DoorDefinitions.Get(DoorPointers.ParlorFromClimb).Name,
        grid);
    AssertEqual((byte)RoomScrollState.Green, grid.ReadStorage(6),
        "$8F:B981 writes early-route screen six green");

    // Door $83:AB4C leaves the Ceres Mode-7 shaft through `$8F:E513`. That routine
    // restores ordinary PPU state but performs no writes to the destination scroll array.
    // The scroll interpreter must accept it without corrupting an otherwise valid grid;
    // runtime-owned Mode-7 state is cleared by the paired door dispatcher.
    grid.SetStorage(7, RoomScrollState.Blue);
    DoorSetupCodeInterpreter.ApplyScrollWrites(
        DoorSetupCode.DoorASM_FromCeresElevatorShaft,
        DoorDefinitions.Get(DoorPointers.FromCeresElevatorShaft).Name,
        grid);
    AssertEqual((byte)RoomScrollState.Blue, grid.ReadStorage(7),
        "$8F:E513 preserves destination scroll storage");

    Console.WriteLine("  Scrolls: 50-byte load and four directional boundary handlers agree.");
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
    var previous = FixedPosition<RoomPixels>.FromWords(200, 0, 100, 0);
    var current = FixedPosition<RoomPixels>.FromWords(204, 0, 100, 0);
    camera.TrackMovedSamusHorizontally(
        previous,
        current,
        new HorizontalCameraContext(KnockedBack: false, SamusMovementType.Standing, TurningAgainstMomentum: false, FacesLeft: false, CameraDistanceMode.NormalTracking));
    AssertEqual(108, camera.IdealPosition.X, "camera facing-right ideal X");
    AssertEqual(5, camera.Speed.X.WholeWord, "camera X distance includes one pixel");
    AssertEqual(105, camera.PixelPosition.X, "camera X follows by calculated speed");

    // The complete fixed-point difference matters: 200.8000 -> 201.4000 is +0.C000;
    // adding 1.0 yields a camera delta of 1.C000.
    camera.SetPosition(new Position16<RoomPixels>(100, 0));
    previous = previous.WithX(new SnesFixedPosition(200, 0x8000));
    current = current.WithX(new SnesFixedPosition(201, 0x4000));
    camera.TrackMovedSamusHorizontally(
        previous,
        current,
        new HorizontalCameraContext(KnockedBack: false, SamusMovementType.Standing, TurningAgainstMomentum: false, FacesLeft: false, CameraDistanceMode.NormalTracking));
    AssertEqual(1, camera.Speed.X.WholeWord, "camera fixed X speed integer");
    AssertEqual(0xc000, camera.Speed.X.Fraction, "camera fixed X speed fraction");
    AssertEqual(101, camera.PixelPosition.X, "camera fixed X position integer");
    AssertEqual(0xc000, camera.Position.X.Fraction, "camera fixed X position fraction");

    // Downward Samus movement uses up_scroller. 210-$64 gives ideal 110; a two-pixel move
    // becomes camera speed three and advances layer Y from 100 to 103 without overshoot.
    camera.SetPosition(new Position16<RoomPixels>(0, 100));
    previous = FixedPosition<RoomPixels>.FromWords(0, 0, 208, 0);
    current = FixedPosition<RoomPixels>.FromWords(0, 0, 210, 0);
    camera.TrackMovedSamusVertically(
        previous,
        current,
        new VerticalCameraContext(YDirection: SamusVerticalDirection.Down, UpScroller: 100, DownScroller: 112));
    AssertEqual(110, camera.IdealPosition.Y, "camera downward ideal Y");
    AssertEqual(3, camera.Speed.Y.WholeWord, "camera Y distance includes one pixel");
    AssertEqual(103, camera.PixelPosition.Y, "camera Y follows by calculated speed");

    // Equal integer coordinates take $80:A528/$80:A731. With identical fixed-point
    // samples, the bank-$90 distance routine still produces speed 1; autoscroll then adds
    // its own two pixels. A red current cell with a blue neighbor therefore drifts +3.
    grid.SetLogicalState(new(0, 0), RoomScrollState.RedBoundary);
    grid.SetLogicalState(new(1, 0), RoomScrollState.Blue);
    camera.SetPosition(new Position16<RoomPixels>(0x0020, 0));
    var stationary = FixedPosition<RoomPixels>.FromWords(200, 0, 200, 0);
    camera.TrackMovedSamusHorizontally(
        stationary,
        stationary,
        new HorizontalCameraContext(KnockedBack: false, SamusMovementType.Standing, TurningAgainstMomentum: false, FacesLeft: false, CameraDistanceMode.NormalTracking));
    AssertEqual(1, camera.Speed.X.WholeWord, "stationary camera X speed still includes one");
    AssertEqual(0x0023, camera.PixelPosition.X, "$80:A528 red-cell rightward drift");

    // Red on both sides cancels that drift by rounding back to the current screen edge.
    grid.SetLogicalState(new(1, 0), RoomScrollState.RedBoundary);
    camera.SetPosition(new Position16<RoomPixels>(0x0020, 0));
    camera.TrackMovedSamusHorizontally(
        stationary,
        stationary,
        new HorizontalCameraContext(KnockedBack: false, SamusMovementType.Standing, TurningAgainstMomentum: false, FacesLeft: false, CameraDistanceMode.NormalTracking));
    AssertEqual(0, camera.PixelPosition.X, "$80:A528 adjacent-red horizontal rounding");

    // The time-frozen flag short-circuits autoscrolling after bank $90 has calculated the
    // speed. It is intentionally not a blanket prohibition on moved-axis handling.
    grid.SetLogicalState(new(1, 0), RoomScrollState.Blue);
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
    verticalGrid.SetLogicalState(new(0, 0), RoomScrollState.RedBoundary);
    verticalGrid.SetLogicalState(new(0, 1), RoomScrollState.Blue);
    var verticalCamera = new ScrollBoundaryCamera(verticalGrid);
    verticalCamera.SetPosition(new Position16<RoomPixels>(0, 0x0020));
    verticalCamera.TrackMovedSamusVertically(
        stationary,
        stationary,
        new VerticalCameraContext(YDirection: SamusVerticalDirection.None, UpScroller: 0, DownScroller: 0));
    AssertEqual(1, verticalCamera.Speed.Y.WholeWord, "stationary camera Y speed still includes one");
    AssertEqual(0x0023, verticalCamera.PixelPosition.Y, "$80:A731 red-cell downward drift");

    // Whether Samus is knocked back, and her native acceleration-mode and pose-facing values, decode to the context the
    // camera reads: a knockback reverses the target, only the turn mode keeps the old momentum, and
    // the pose byte is the facing.
    AssertTrue(new HorizontalCameraContext(true, SamusMovementType.Standing, 0, 8, CameraDistanceMode.NormalTracking).KnockedBack,
        "a knockback reverses the camera target");
    AssertTrue(new HorizontalCameraContext(false, SamusMovementType.Standing, 1, 8, CameraDistanceMode.NormalTracking).TurningAgainstMomentum,
        "acceleration mode one is the momentum-keeping turn");
    AssertTrue(!new HorizontalCameraContext(false, SamusMovementType.Standing, SamusHorizontalAccelerationModes.Decelerating, 8,
        CameraDistanceMode.NormalTracking).TurningAgainstMomentum, "plain deceleration does not reverse the camera target");
    AssertTrue(new HorizontalCameraContext(false, SamusMovementType.Standing, 0, 4, CameraDistanceMode.NormalTracking).FacesLeft,
        "pose facing four is left");
    AssertTrue(!new HorizontalCameraContext(false, SamusMovementType.Standing, 0, 0, CameraDistanceMode.NormalTracking).FacesLeft,
        "a front-view pose takes the right-facing target");

    // The scrollers are room-header bytes: the whole byte range is kept, anything outside it is rejected.
    AssertEqual(byte.MaxValue, new VerticalCameraContext(SamusVerticalDirection.None, byte.MaxValue, byte.MaxValue).DownScroller,
        "the largest down-scroller is kept");
    AssertThrows<ArgumentOutOfRangeException>(() => _ = new VerticalCameraContext(SamusVerticalDirection.None, byte.MaxValue + 1, 0),
        "an up-scroller above a byte is rejected");
    AssertThrows<ArgumentOutOfRangeException>(() => _ = new VerticalCameraContext(SamusVerticalDirection.None, 0, -1),
        "a negative down-scroller is rejected");

    Console.WriteLine("  Camera: bank $90 tracking and bank $80 stationary autoscroll agree.");
}

/// <summary>
/// Checks the exact parallax multiply, scroll-register wrapping, frozen frames and door-setup offsets of
/// $80:A2F9-$80:A527 and $80:AE29.
/// </summary>
internal static void VerifyBackgroundScrollState()
{
    var state = new BackgroundScrollState
    {
        Layer1Position = new(0x1234, 0x0200),
        Layer2Scroll = Layer2ScrollModes.FromHeader(0x80, 0), // Horizontal 128/256 = one-half parallax.
        Bg1Offset = new(0x0010, unchecked((short)0xfff0)),
        Bg2Offset = new(3, 4),
    };

    state.StepScrolling();
    AssertEqual(0x1244, state.Bg1Scroll.X, "$80:A3B7 BG1 X plus offset");
    AssertEqual(0x01f0, state.Bg1Scroll.Y, "$80:A3C0 BG1 Y wrapping offset");
    AssertEqual(0x091a, state.Layer2Position.X, "$80:A2F9 half-speed X parallax");
    AssertEqual(0x0200, state.Layer2Position.Y, "$80:A33A zero mode copies layer 1");
    AssertEqual(0x091d, state.Bg2Scroll.X, "$80:A3CF BG2 X plus offset");

    // Mode one preserves both the layer-2 position and its PPU scroll mirror.
    ushort oldLayer2X = state.Layer2Position.X;
    ushort oldBg2X = state.Bg2Scroll.X;
    state.Layer2Scroll = Layer2ScrollModes.FromHeader(1, 1);
    state.Layer1Position = new(0x1300, 0x0300);
    state.StepScrolling();
    AssertEqual(oldLayer2X, state.Layer2Position.X, "fixed BG2 X position remains unchanged");
    AssertEqual(oldBg2X, state.Bg2Scroll.X, "fixed BG2 X register remains unchanged");

    ushort frozenBg1X = state.Bg1Scroll.X;
    state.Layer1Position = state.Layer1Position.WithX(0x4444);
    state.StepScrolling(timeIsFrozen: true);
    AssertEqual(frozenBg1X, state.Bg1Scroll.X, "$80:A3AB frozen scroll registers");

    var destinationParallax = new BackgroundScrollState
    {
        Layer2Scroll = Layer2ScrollModes.FromHeader(0x80, 0x40),
    };
    destinationParallax.PrepareDoorOpeningDestination(new(0x0400, 0x0200), new(0x0400, 0x0200));
    AssertEqual((ushort)0x0200, destinationParallax.Layer2Position.X,
        "door setup calculates destination half-speed BG2 X");
    AssertEqual((ushort)0x0080, destinationParallax.Layer2Position.Y,
        "door setup calculates destination quarter-speed BG2 Y");
    AssertEqual(new Position16<RoomPixels>(0x0400, 0x0200), destinationParallax.Layer1Position,
        "door setup publishes the whole destination layer-one position");

    // Upward doors derive BG2 from destination+$1F while layer one keeps the destination.
    var upwardParallax = new BackgroundScrollState { Layer2Scroll = Layer2ScrollModes.FromHeader(0x80, 0x40) };
    upwardParallax.PrepareDoorOpeningDestination(new(0x0400, 0x0220), new(0x0400, 0x021f));
    AssertEqual(new Position16<Bg2Pixels>(0x0200, 0x0087), upwardParallax.Layer2Position,
        "upward door setup calculates BG2 from its separate parallax basis");
    AssertEqual(new Position16<RoomPixels>(0x0400, 0x0220), upwardParallax.Layer1Position,
        "upward door setup keeps the destination layer-one position");

    // `$80:AE29` first derives four offsets from the retained source BG1 registers, the
    // cleared BG2 registers, and the destination's staged +/-$100 coordinate. The first
    // built-in four-pixel IRQ step must therefore move BG1 by exactly four pixels from the
    // source image while BG2 starts four pixels from its cleared origin.
    var rightDoor = new BackgroundScrollState
    {
        Layer1Position = new(0xff04, 0), // destination $0000 - $00FC
        Layer2Position = new(0xff04, 0),
    };
    rightDoor.ConfigureDoorOpeningOffsets(
        retainedBg1Scroll: new(0x0120, 0x0340),
        stagedLayer1: new(0xff00, 0x0000));
    AssertEqual(new Offset16<RoomPixels>(0x0220, 0x0340), rightDoor.Bg1Offset,
        "$80:AE29 BG1 offsets are the retained registers minus the staged layer-one origin");
    AssertEqual(new Offset16<Bg2Pixels>(0x0100, 0), rightDoor.Bg2Offset,
        "$80:AE3B BG2 offsets negate the staged layer-one origin on both axes");
    rightDoor.CalculateScrollRegisters();
    AssertEqual((ushort)0x0124, rightDoor.Bg1Scroll.X,
        "$80:AE29 right-door BG1 retains source plus first step");
    AssertEqual((ushort)0x0004, rightDoor.Bg2Scroll.X,
        "$80:AE29 right-door BG2 advances from cleared register");

    var leftDoor = new BackgroundScrollState
    {
        Layer1Position = new(0x00fc, 0), // destination $0000 + $00FC
        Layer2Position = new(0x00fc, 0),
    };
    leftDoor.ConfigureDoorOpeningOffsets(
        retainedBg1Scroll: new(0x0120, 0x0340),
        stagedLayer1: new(0x0100, 0x0000));
    leftDoor.CalculateScrollRegisters();
    AssertEqual((ushort)0x011c, leftDoor.Bg1Scroll.X,
        "$80:AE29 left-door BG1 retains source minus first step");
    AssertEqual((ushort)0xfffc, leftDoor.Bg2Scroll.X,
        "$80:AE29 left-door BG2 retreats from cleared register");

    // Upward setup presents destination+$FB after the built-in first moving call.
    var upwardDestination = new BackgroundScrollState
    {
        Layer1Position = new(0, 0x02fb),
        Layer2Position = new(0, 0x01dc),
    };
    upwardDestination.ConfigureDoorOpeningOffsets(
        retainedBg1Scroll: new(0x0120, 0x0340),
        stagedLayer1: new(0, 0x0300));
    upwardDestination.CalculateScrollRegisters();
    AssertEqual((ushort)0x033b, upwardDestination.Bg1Scroll.Y,
        "$80:AF89 upward setup preserves source BG1 minus five pixels");

    Console.WriteLine("  BG scroll: parallax, frozen frames and door setup offsets agree.");
}

/// <summary>Checks all four branches shared by $80:AA95 and $80:AC57.</summary>
internal static void VerifyLevelBlockTilemapExpansion()
{
    // Put the definition at index one so the test also exercises the level entry's ten-bit
    // lookup. Existing child flip/palette bits are retained and XORed, never reconstructed.
    var definitions = new byte[16];
    BinaryPrimitives.WriteUInt16LittleEndian(definitions.AsSpan(8), 0x0123);
    BinaryPrimitives.WriteUInt16LittleEndian(definitions.AsSpan(10), 0x4567);
    BinaryPrimitives.WriteUInt16LittleEndian(definitions.AsSpan(12), 0x89ab);
    BinaryPrimitives.WriteUInt16LittleEndian(definitions.AsSpan(14), 0xcdef);

    RoomLevelWord unflipped = RoomLevelWord.Create(
        1, LevelBlockFlipFlags.None, RoomCollisionType.Air);
    RoomLevelWord horizontalSlope = RoomLevelWord.Create(
        1, LevelBlockFlipFlags.Horizontal, RoomCollisionType.Slope);
    RoomLevelWord verticalExtension = RoomLevelWord.Create(
        1, LevelBlockFlipFlags.Vertical, RoomCollisionType.VerticalExtension);
    RoomLevelWord doubleFlipHorizontalExtension = RoomLevelWord.Create(
        1,
        LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical,
        RoomCollisionType.HorizontalExtension);

    AssertEqual(
        new ExpandedBlockTiles((ushort)0x0123, (ushort)0x4567, (ushort)0x89ab, (ushort)0xcdef),
        LevelBlockTilemapExpander.Expand(unflipped, definitions),
        "unflipped block expansion");
    AssertEqual(
        new ExpandedBlockTiles((ushort)0x0567, (ushort)0x4123, (ushort)0x8def, (ushort)0xc9ab),
        LevelBlockTilemapExpander.Expand(horizontalSlope, definitions),
        "horizontally flipped slope expansion ignores collision nibble");
    AssertEqual(
        new ExpandedBlockTiles((ushort)0x09ab, (ushort)0x4def, (ushort)0x8123, (ushort)0xc567),
        LevelBlockTilemapExpander.Expand(verticalExtension, definitions),
        "vertically flipped extension expansion ignores collision nibble");
    AssertEqual(
        new ExpandedBlockTiles((ushort)0x0def, (ushort)0x49ab, (ushort)0x8567, (ushort)0xc123),
        LevelBlockTilemapExpander.Expand(doubleFlipHorizontalExtension, definitions),
        "doubly flipped horizontal extension expansion ignores collision nibble");

    AssertThrows<InvalidDataException>(
        () => LevelBlockTilemapExpander.Expand(0x0002, definitions),
        "block definition bounds check");

    Console.WriteLine("  BG stream: all 16x16 block flip expansions agree.");
}

/// <summary>
/// Verifies the shared row-major BG1/BTS model that will feed bank-$94 collision while
/// continuing to construct the already-verified bank-$80 visual streamer.
/// </summary>
internal static void VerifyRoomLevelData()
{
    const int width = 3;
    const int height = 2;
    ushort[] foreground = [
        0x0000, 0x1123, 0x8567,
        0xc001, 0xe002, 0xf003,
    ];
    byte[] behavior = [0x00, 0x45, 0x80, 0x11, 0x22, 0x33];
    ushort[] background = [0, 1, 2, 3, 4, 5];
    var definitions = new byte[8 * 4];
    var level = new RoomLevelData(new Extent16<RoomBlocks>(width, height), new RoomLevelPlanes(foreground, behavior, background), definitions);

    RoomCollisionBlock block = level.GetCollisionBlock(new(1, 1));
    AssertEqual(4, block.Index, "room collision row-major index");
    AssertEqual(0xe002, block.PackedWord.Raw, "room collision level word");
    AssertEqual(0x22, block.Bts.Value, "room collision parallel BTS byte");
    AssertEqual(RoomCollisionType.GrappleBlock, block.CollisionType,
        "room collision high-nibble dispatcher type");
    AssertEqual(2, block.VisualBlockIndex, "room collision visual block index");

    // Pixel (31,17) is block (1,1); shifts must occur before multiplication/indexing.
    AssertEqual(block, level.GetCollisionBlockAtPixel(new(31, 17)), "room pixel-to-block conversion");
    AssertThrows<ArgumentOutOfRangeException>(
        () => level.GetCollisionBlock(new(width, 0)),
        "room collision rejects X beyond header width");
    AssertThrows<ArgumentOutOfRangeException>(
        () => level.GetCollisionBlock(new(0, height)),
        "room collision rejects Y beyond header height");

    Console.WriteLine("  Room level: shared BG1/BTS indexing, collision type, and pixel conversion agree.");
}

/// <summary>
/// Seeds a synthetic bank-$8F room at <paramref name="roomPointer"/>: the fixed eleven-byte
/// header (only area and door-list values are semantically interesting; zero dimensions are
/// legal because the test stops before asset construction), then the ordered selector program
/// event, boss bit, Morph Ball+missiles, power bombs, finish. Finish's following byte is the
/// inline default state header, so its expected pointer is derived from this exact command
/// layout instead of fabricated by the production code.
/// </summary>
[CpuBusBoundary("exact bus test: synthetic cartridge fixture for a room header and its ordered state-selector program")]
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
[CpuBusBoundary("exact bus test: synthetic cartridge fixture overwriting a room's leading state-selector words")]
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

    var eventBytes = new byte[Bank80SystemState.EventByteCount];
    eventBytes[0] = 1;
    AssertEqual((ushort)0x9100,
        SuperMetroid.TestSupport.RoomHeaderImport.CartridgeRoomHeaderImporter.SelectStatePointer(bus, roomPointer,
            new RoomStateSelectionContext(eventBytes, BossBits.None, false, false)),
        "room selector event branch");
    AssertEqual((ushort)0x9120,
        SuperMetroid.TestSupport.RoomHeaderImport.CartridgeRoomHeaderImporter.SelectStatePointer(bus, roomPointer,
            new RoomStateSelectionContext(Array.Empty<byte>(), BossBits.AreaTorizo, false, false)),
        "room selector boss branch");
    AssertEqual((ushort)0x9140,
        SuperMetroid.TestSupport.RoomHeaderImport.CartridgeRoomHeaderImporter.SelectStatePointer(bus, roomPointer,
            new RoomStateSelectionContext(Array.Empty<byte>(), BossBits.None, true, false)),
        "room selector Morph Ball and missiles branch");
    AssertEqual((ushort)0x9160,
        SuperMetroid.TestSupport.RoomHeaderImport.CartridgeRoomHeaderImporter.SelectStatePointer(bus, roomPointer,
            new RoomStateSelectionContext(Array.Empty<byte>(), BossBits.None, false, true)),
        "room selector power-bomb branch");

    // The interpreter must return at the first successful command; later facts do not
    // override the event-selected pointer merely because they are also true.
    AssertEqual((ushort)0x9100,
        SuperMetroid.TestSupport.RoomHeaderImport.CartridgeRoomHeaderImporter.SelectStatePointer(bus, roomPointer,
            new RoomStateSelectionContext(eventBytes, BossBits.AreaTorizo, true, true)),
        "room selector preserves cartridge priority");

    // The Tourian-specific routine owns its boss-bit operand in executable code rather
    // than the room stream. Exercise it separately so the catalogued implicit mask cannot
    // accidentally be replaced with the operand-reading behavior of the generic routine.
    SeedRoomStateSelectorWords(bus, roomPointer,
        (ushort)RoomStateSelectorCode.MainAreaBossIsDead, 0x9180, (ushort)RoomStateSelectorCode.Finish);
    AssertEqual((ushort)0x9180,
        SuperMetroid.TestSupport.RoomHeaderImport.CartridgeRoomHeaderImporter.SelectStatePointer(bus, roomPointer,
            new RoomStateSelectionContext(
                Array.Empty<byte>(),
                RoomStateSelectorOperands.MainAreaBoss,
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

/// <summary>Checks Mode-1 4-bpp BG pixels, palette selection, transparency, and H flip.</summary>
[PpuLayoutBoundary("exact hardware test: checks native CGRAM slots, native VRAM words")]
internal static void VerifyFourBitBackgroundRendering()
{
    var vram = new SnesVram();
    var cgram = new SnesCgram();

    // Character zero has only its top-left plane-zero bit set, producing color index one.
    var character = new byte[32];
    character[0] = 0x80;
    vram.LoadBytes(0, character);
    cgram.SetColor(2 * 16 + 1, Bgr555.FromWord(0x001f)); // Full SNES red in BG palette two.

    ushort[] mapEntry = [0x0800]; // Character zero, palette two, no flips.
    vram.ExecuteWordTransfer(mapEntry, destinationWord: 0x5000, wordIncrement: 1);
    Rgba32[] pixels = SnesBgTilemapRenderer.Render4BppViewport(vram, cgram, new BgVramAddresses { TilemapBaseWord = 0x5000, CharacterBaseWord = 0 }, new BgViewport { Scroll = new Position16<BgPlanePixels>(0, 0), Size = new Extent16<ScreenPixels>(8, 8) });
    AssertEqual(new Rgba32(255, 0, 0), pixels[0], "4-bpp BG palette pixel");
    AssertEqual(0, pixels[1].A, "4-bpp BG color zero transparency");

    mapEntry[0] |= 0x4000;
    vram.ExecuteWordTransfer(mapEntry, destinationWord: 0x5000, wordIncrement: 1);
    pixels = SnesBgTilemapRenderer.Render4BppViewport(vram, cgram, new BgVramAddresses { TilemapBaseWord = 0x5000, CharacterBaseWord = 0 }, new BgViewport { Scroll = new Position16<BgPlanePixels>(0, 0), Size = new Extent16<ScreenPixels>(8, 1) });
    AssertEqual(0, pixels[0].A, "4-bpp H-flip old pixel becomes transparent");
    AssertEqual(new Rgba32(255, 0, 0), pixels[7], "4-bpp H-flip mirrored pixel");

    // BG2SC=$4A means a 32x64-tile map rooted at word $4800. Once VOFS bit eight
    // is set, the PPU selects its second $400-word screen at $4C00 rather than wrapping
    // into the first screen. Landing Site relies on this exact vertical arrangement.
    vram.ExecuteWordTransfer([0x0800], destinationWord: 0x4c00, wordIncrement: 1);
    pixels = SnesBgTilemapRenderer.Render4BppViewport(vram, cgram, new BgVramAddresses { TilemapBaseWord = 0x4800, CharacterBaseWord = 0 }, new BgViewport { Scroll = new Position16<BgPlanePixels>(0, 0x0100), Size = new Extent16<ScreenPixels>(8, 1), TilemapSize = new Extent16<TilemapCells>(32, 64) });
    AssertEqual(new Rgba32(255, 0, 0), pixels[0], "4-bpp BGSC vertical second screen");

    // Exercise that same BGSC geometry through the production priority compositor. Its
    // first gameplay output row is 32, physical scanline 33, so VOFS=$E0 selects
    // source Y=$101. A per-line HOFS of eight selects tile one in the lower screen.
    // Put its only pixel on character row one: old zero-based BG sampling fails.
    var gameplayVram = new SnesVram();
    var gameplayCgram = new SnesCgram();
    var gameplaySprites = new SpriteFrameBuffer();
    var redCharacter = new byte[32];
    var blueCharacter = new byte[32];
    redCharacter[2] = 0x80;
    blueCharacter[2] = 0x80;
    // Leave character zero transparent because empty BG1 map words select it above BG2.
    gameplayVram.LoadBytes(0x0020, redCharacter);
    gameplayVram.LoadBytes(0x0040, blueCharacter);
    gameplayCgram.SetColor(2 * 16 + 1, Bgr555.FromWord(0x001f));
    gameplayCgram.SetColor(3 * 16 + 1, Bgr555.FromWord(0x7c00));
    gameplayVram.ExecuteWordTransfer([0x0801], destinationWord: 0x4800, wordIncrement: 1);
    gameplayVram.ExecuteWordTransfer([0x0801, 0x0c02], destinationWord: 0x4c00, wordIncrement: 1);
    var horizontalByLine = new ushort[SnesGameplayFrameRenderer.Height - SnesGameplayFrameRenderer.HudHeight];
    horizontalByLine[0] = 8;
    Rgba32[] gameplayFrame = SnesGameplayFrameRenderer.RenderHudOrdinaryBackgroundsAndObjs(
        gameplayVram,
        gameplayCgram,
        gameplaySprites,
        new OrdinaryGameplayRenderLayer(
            new OrdinaryGameplayRegisters(new Position16<RoomPixels>(0, 0), new Position16<Bg2Pixels>(0, 0x00e0),
                new Extent16<TilemapCells>(32, 64), GameplayVramLayout.Bg2TilemapWord, 0, 0,
                GameplayVramLayout.HudCharacterWord, SnesMainScreenLayers.Bg1 | SnesMainScreenLayers.Bg2 | SnesMainScreenLayers.Obj),
            RoomBackgroundsFixtures.Blank(),
            new Bg2HdmaScrollTables(horizontalByLine, default)));
    AssertEqual(new Rgba32(0, 0, 255),
        gameplayFrame[SnesGameplayFrameRenderer.HudHeight * SnesGameplayFrameRenderer.Width],
        "ordinary compositor consumes sky HOFS and vertical BGSC screen");

    // Kraid selects BG2SC=$43: a 64x64 map rooted at word $4000 instead of the normal
    // room BG2 map at $4800. Verify the general compositor consumes that register-derived
    // layout; accepting a 64x64 argument without using its base still leaves only Kraid's
    // ordinary OAM arm visible.
    const ushort kraidBg2BaseWord = 0x4000;
    int firstGameplaySourceRow = SnesGameplayFrameRenderer.HudHeight / 8;
    gameplayVram.ExecuteWordTransfer(
        [0x0801],
        destinationWord: unchecked((ushort)(kraidBg2BaseWord + firstGameplaySourceRow * 32)),
        wordIncrement: 1);
    gameplayFrame = SnesGameplayFrameRenderer.RenderHudOrdinaryBackgroundsAndObjs(
        gameplayVram,
        gameplayCgram,
        gameplaySprites,
        new OrdinaryGameplayRenderLayer(
            new OrdinaryGameplayRegisters(new Position16<RoomPixels>(0, 0), new Position16<Bg2Pixels>(0, 0),
                new Extent16<TilemapCells>(64, 64), kraidBg2BaseWord, 0, 0,
                GameplayVramLayout.HudCharacterWord, SnesMainScreenLayers.Bg1 | SnesMainScreenLayers.Bg2 | SnesMainScreenLayers.Obj),
            RoomBackgroundsFixtures.Blank()));
    AssertEqual(new Rgba32(255, 0, 0),
        gameplayFrame[SnesGameplayFrameRenderer.HudHeight * SnesGameplayFrameRenderer.Width],
        "ordinary compositor consumes Kraid's 64x64 BG2 map at $4000");

    Console.WriteLine("  BG render: 4-bpp pixels and BGSC geometry agree.");
}

/// <summary>
/// Guards the physical-scanline relationship between the host-composited terrain and live
/// Samus/OAM. The gameplay IRQ hides BG1 behind the HUD; it does not rewind BG1VOFS when
/// BG1 becomes visible again on line 32.
/// </summary>
internal static void VerifyHostRoomViewportAlignment()
{
    const int roomWidth = 300;
    const int roomHeight = 260;
    const int cameraX = 7;
    const int cameraY = 11;
    var room = new Rgba32[roomWidth * roomHeight];

    // Give every source row a unique red component. The first gameplay output pixel must
    // come from world row cameraY+32; cameraY would expose the old 32-pixel alignment bug.
    for (int y = 0; y < roomHeight; y++)
    {
        for (int x = 0; x < roomWidth; x++)
            room[y * roomWidth + x] = new Rgba32((byte)y, (byte)x, 0);
    }

    var vram = new SnesVram();
    var cgram = new SnesCgram();
    var sprites = new SpriteFrameBuffer();
    Rgba32[] frame = SnesGameplayFrameRenderer.RenderHudRoomAndObjs(
        vram,
        cgram,
        sprites,
        room,
        new Extent16<RoomPixels>(roomWidth, roomHeight),
        new Position16<RoomPixels>(cameraX, cameraY));

    AssertEqual(
        room[(cameraY + SnesGameplayFrameRenderer.HudHeight) * roomWidth + cameraX],
        frame[SnesGameplayFrameRenderer.HudHeight * SnesGameplayFrameRenderer.Width],
        "host terrain begins at physical scanline 32");
    AssertEqual(
        room[(cameraY + SnesGameplayFrameRenderer.Height - 1) * roomWidth + cameraX + 255],
        frame[^1],
        "host terrain bottom-right physical coordinate");

    Console.WriteLine("  Host terrain: room crop remains aligned with physical BG1 scanlines and live OAM.");
}

/// <summary>The native first pre-scaled power-bomb profile's half width for <paramref name="row"/> at $88:9F06.</summary>
[CpuBusBoundary("exact bus test: retail ROM oracle for the first pre-scaled power-bomb window profile")]
static int ReadRetailPowerBombPreScaledHalfWidth(SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace rom, int row) =>
    rom.ReadCartridgeByte(0x889f06 + row);

/// <summary>
/// Synthetic cartridge with the literal `$88:A266-$88:A2A5` horizontal samples and vertical
/// band boundaries used by `$88:8CC6/$8D04/$8D46` (kept independent of the production loop so
/// a transposed table or rounded product cannot self-validate), and all sixteen pre-explosion
/// fixed-color entries set to one unmistakable color.
/// </summary>
[CpuBusBoundary("exact bus test: synthetic cartridge fixture for power-bomb window tables and fixed colors")]
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

/// <summary>Proves that desktop power-bomb composition uses the native radius frame and center-outward half-profile orientation.</summary>
internal static void VerifyPowerBombColorMathWindow()
{
    TestAddressSpace bus = CreatePowerBombColorMathWindowCartridge();

    const ushort centerX = 100;
    const ushort centerY = 100;
    var explosion = new SamusPowerBombExplosionState { PresentationColors = SuperMetroid.Core.Samus.Artwork.PowerBombFixedColorCatalog.Load(new MemoryStream(SuperMetroid.AssetExtraction.Samus.PowerBombFixedColorExtractor.Extract(bus))) };
    explosion.Arm();
    explosion.Spawn(new(centerX, centerY));

    // Spawn happens after the native HDMA pass. Even though status is already `$8000`,
    // the host must not invent an explosion table on this frame.
    Rgba32[] spawnFrame = CreateOpaqueBlackGameplayFrame();
    SnesGameplayFrameRendererConstants.ApplyPowerBombColorMath(spawnFrame, PowerBombRenderSnapshot.Capture(explosion), new Position16<RoomPixels>(0, 0));
    AssertEqual(new Rgba32(0, 0, 0, 255),
        spawnFrame[centerY * SnesGameplayFrameRenderer.Width + centerX],
        "power-bomb spawn frame has no premature HDMA window");

    explosion.StepFrame();
    AssertEqual(PowerBombExplosionPhase.Inactive, explosion.RenderedPhase,
        "first HDMA pass initializes the list without generating a window");
    // The first pre-instruction draws radius `$04.00`, then advances the live state to
    // `$34.00`. `$04 * $BF >> 8` gives a two-line vertical extent and `$04 * $FF >> 8`
    // gives a three-pixel center half-width. Testing beyond both extents catches use of
    // the already-updated `$34.00` radius as well as floating-point ellipse substitution.
    explosion.StepFrame();
    AssertEqual(0x0400, explosion.RenderedPreExplosionRadius.RawValue,
        "power-bomb renderer retains pre-update radius");
    AssertEqual(0x3400, explosion.PreExplosionRadius.RawValue,
        "power-bomb logic advances next-frame radius");
    Rgba32[] scaledFrame = CreateOpaqueBlackGameplayFrame();
    SnesGameplayFrameRendererConstants.ApplyPowerBombColorMath(scaledFrame, PowerBombRenderSnapshot.Capture(explosion), new Position16<RoomPixels>(0, 0));
    Rgba32 fixedColor = new(8, 16, 24, 255);
    AssertEqual(fixedColor,
        scaledFrame[centerY * SnesGameplayFrameRenderer.Width + centerX + 3],
        "scaled power-bomb center uses truncated final horizontal sample");
    AssertEqual(new Rgba32(0, 0, 0, 255),
        scaledFrame[centerY * SnesGameplayFrameRenderer.Width + centerX + 4],
        "scaled power-bomb center excludes first outside pixel");
    AssertEqual(new Rgba32(0, 0, 0, 255),
        scaledFrame[(centerY + 3) * SnesGameplayFrameRenderer.Width + centerX],
        "scaled power-bomb vertical extent uses ROM boundary table");

    // A pre-scaled record is a center-outward half-profile. Native HDMA mirrors it around
    // the explosion Y coordinate; it is not indexed by adding a screen-space midpoint.
    while (explosion.Phase == PowerBombExplosionPhase.PreExplosionWhite)
        explosion.StepFrame();
    var shapeReference = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
    explosion.StepFrame();
    AssertEqual(PowerBombExplosionPhase.PreExplosionYellow, explosion.RenderedPhase,
        "first pre-scaled yellow frame is retained for composition");
    Rgba32[] shapeFrame = CreateOpaqueBlackGameplayFrame();
    SnesGameplayFrameRendererConstants.ApplyPowerBombColorMath(shapeFrame, PowerBombRenderSnapshot.Capture(explosion), new Position16<RoomPixels>(0, 0));
    AssertEqual<PowerBombShapeCursor?>(new PowerBombShapeCursor(PowerBombShapeStream.Yellow, 0), explosion.RenderedShape,
        "first yellow frame selects the native profile");
    for (int y = 0; y < SnesGameplayFrameRenderer.Height; y++)
    {
        int row = Math.Abs(y - centerY);
        int halfWidth = row < 192 ? ReadRetailPowerBombPreScaledHalfWidth(shapeReference, row) : 0;
        for (int x = 0; x < SnesGameplayFrameRenderer.Width; x++)
        {
            bool inside = y >= SnesGameplayFrameRenderer.HudHeight && halfWidth != 0 && Math.Abs(x - centerX) <= halfWidth;
            AssertEqual(inside ? fixedColor : new Rgba32(0, 0, 0, 255),
                shapeFrame[y * SnesGameplayFrameRenderer.Width + x],
                $"native pre-scaled profile pixel ({x},{y})");
        }
    }
    Console.WriteLine("  Power bomb: ROM curve bands, rendered-frame timing, and center-outward shapes agree.");
}

/// <summary>Provides a black gameplay raster that makes power-bomb color-window endpoints visible.</summary>
static Rgba32[] CreateOpaqueBlackGameplayFrame()
{
    var frame = new Rgba32[
        SnesGameplayFrameRenderer.Width * SnesGameplayFrameRenderer.Height];
    Array.Fill(frame, new Rgba32(0, 0, 0, 255));
    return frame;
}

/// <summary>Checks bank-$88 sky fixed-point bands and four queued circular-map rows.</summary>
[PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
internal static void VerifyScrollingSkyState()
{
    AssertTrue(
        ScrollingSkyState.IsLandRoomMain(RoomMainCallback.ScrollingSkyLand),
        "$8F:C116 selects land scrolling sky");
    AssertTrue(
        ScrollingSkyState.IsLandRoomMain(RoomMainCallback.ScrollingSkyLandZebesTimebombSet),
        "$8F:C120 selects land scrolling sky before quake work");
    AssertTrue(
        !ScrollingSkyState.IsLandRoomMain(RoomMainCallback.ScrollingSkyOcean),
        "$8F:C11B ocean wrapper is not silently treated as land sky");

    AssertEqual(ScrollingSkyChunkTable.Land, ScrollingSkyState.VariantOf(RoomMainCallback.ScrollingSkyLandZebesTimebombSet),
        "land sky mains read the land chunk table");
    AssertEqual(ScrollingSkyChunkTable.Ocean, ScrollingSkyState.VariantOf(RoomMainCallback.ScrollingSkyOcean),
        "the ocean main reads the ocean chunk table");
    AssertThrows<ArgumentOutOfRangeException>(() => ScrollingSkyState.VariantOf(RoomMainCallback.None),
        "a room main without a sky has no chunk table");

    var sky = new ScrollingSkyState(ScrollingSkyChunkTable.Land);
    sky.ProcessFrame(layer1YPosition: 0x041f, timeIsFrozen: false);
    AssertEqual(new Position16<Bg2Pixels>(0, 0x041f), sky.Bg2Scroll, "$88:AFB2 BG2 scroll copies layer-one Y and leaves X zero");

    // Chunk index five is the intentional table-adjacency wrap to tilemap zero.
    AssertTrue(ScrollingSkyChunkDefinitions.TryGetPage(ScrollingSkyChunkTable.Land, 5, out ushort wrapped) && wrapped == 0xb180,
        "land chunk five wraps to the first sky page");

    // The $02E0 section aliases data slot eight, which already received the $0238 row's
    // +0.8000. Its additional +0.C000 produces integer 1 after only one frame.
    AssertEqual(1, sky.GetDataSlotPosition(8), "sky aliased fast HDMA slot");
    SkyTilemap rows = RoomSkyTilemapFixtures.Numbered().Tilemap(ScrollingSkyChunkTable.Land);
    ScrollingSkyLayer layer = sky.Publish(rows, 0x02c0) ?? throw new InvalidOperationException("The configured sky publishes its layer.");
    AssertEqual(1, layer.GameplayLineScrolls(1)[0], "sky scanline resolves aliased HDMA slot");

    sky.ProcessFrame(layer1YPosition: 0x041f, timeIsFrozen: true);
    AssertEqual(false, sky.HdmaEnabled, "frozen sky terminates HDMA table");
    ScrollingSkyLayer frozen = sky.Publish(rows, 0x02c0) ?? throw new InvalidOperationException("The configured sky publishes its layer.");
    AssertTrue(!frozen.BandsShown && frozen.GameplayLineScrolls().All(scroll => scroll == 0), "a frozen frame shows every band unshifted");
    AssertEqual(1, frozen.BandOffsets[8], "a frozen frame keeps the band offsets");
    AssertTrue(new ScrollingSkyState(ScrollingSkyChunkTable.Land, horizontalHdmaConfigured: false).Publish(rows, 0) is null,
        "a sky main without the band HDMA object publishes no sky layer");
    AssertThrows<ArgumentException>(() => sky.Publish(RoomSkyTilemapFixtures.Numbered().Tilemap(ScrollingSkyChunkTable.Ocean), 0),
        "the land sky rejects the ocean rows");

    Console.WriteLine("  Sky: HDMA bands, chunk wrap and the published layer agree.");
}

// Exercises bank-$90 firing and movement, bank-$93 animation, and bank-$94 solid collision
// for an uncharged power beam without sharing implementation code with the production path.

    /// <summary>Runs the HUD template, map reveal and BG3 rendering checks.</summary>
    [VerificationCommand("--hud-state")]
    internal static int HudStateCommand()
    {
        Suite(nameof(VerifyHudStateAndBg3Rendering), () => VerifyHudStateAndBg3Rendering());
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
