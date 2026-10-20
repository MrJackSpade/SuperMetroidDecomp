using SuperMetroid.Verification.Input;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Hardware.ColorWordsVerification;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Saves;
using SuperMetroid.Core.Frontend.FileSelect.Artwork;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Core.Runtime;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Frontend;
using SuperMetroid.Verification.Harness;

/// <summary>Focused cartridge and rendering checks for frontend presentation and demo input.</summary>
internal static class FrontendVerification
{
/// <summary>Confirms a LoROM compressed literal and terminator survive the physical $94/$95 bank boundary.</summary>
[CpuBusBoundary("exact bus test: LoROM cross-bank traversal of a compressed stream over a constructed cartridge")]
internal static void VerifyLoRomCrossBankCompressedData()
{
    var rom = new byte[SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace.RetailRomByteCount];

    // The four-byte literal begins at $94:FFFC. Its final byte is $94:FFFF and the $FF
    // terminator is the physically adjacent byte at $95:8000. This is the same bank-cross
    // shape used by the real title graphics stream, reduced to a six-byte fixture.
    byte[] compressed = [0x03, 0x11, 0x22, 0x33, 0x44, 0xff];
    int beforeCrossing = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace.ToRomOffset(0x94fffc);
    int afterCrossing = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace.ToRomOffset(0x958000);
    compressed.AsSpan(0, 4).CopyTo(rom.AsSpan(beforeCrossing));
    compressed.AsSpan(4, 2).CopyTo(rom.AsSpan(afterCrossing));

    var bus = new SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace(rom);
    byte[] output = RomDataReader.Decompress(bus, 0x94fffc, maximumCompressedBytes: 16);
    AssertEqual(4, output.Length, "cross-bank decompressed length");
    AssertEqual(0x11, output[0], "cross-bank first literal");
    AssertEqual(0x44, output[3], "cross-bank final literal");
}

/// <summary>Checks Mode 7 sampling, scroll transform order, transparent overflow, and Ceres's Mode 1 floor split.</summary>
[PpuLayoutBoundary("exact hardware test: checks native CGRAM slots, native VRAM words")]
internal static void VerifyMode7Rendering()
{
    var vram = new SnesVram();
    var cgram = new SnesCgram();
    cgram.SetColor(5, Bgr555.FromWord(0x001f)); // Fully red BGR555 diagnostic color.

    // Mode 7's tilemap lives in low VRAM bytes. Select character one for map cell zero,
    // then put palette index five in every high byte of that character's 64 words.
    vram.LoadBytes(0, [1]);
    for (int pixel = 0; pixel < 64; pixel++)
    {
        ushort word = (ushort)(64 + pixel);
        vram.ExecuteWordTransfer([(5 << 8)], word, 1);
    }

    Rgba32[] identity = SnesMode7Renderer.RenderViewport(vram, cgram, new Mode7RenderRegisters { MatrixA = 0x0100, MatrixB = 0, MatrixC = 0, MatrixD = 0x0100, Center = new Offset16<Mode7Pixels>(0, 0), Scroll = new Offset16<Mode7Pixels>(0, 0) }, size: new Extent16<ScreenPixels>(8, 8));
    AssertEqual(64, identity.Length, "Mode 7 identity dimensions");
    AssertTrue(identity.Take(56).All(pixel => pixel.R == 255 && pixel.G == 0 && pixel.B == 0),
        "Mode 7 identity reads character rows one through seven on physical scanlines one through seven");
    AssertTrue(identity.Skip(56).All(pixel => pixel.A == 0),
        "Mode 7 physical scanline eight samples the next tile row");

    // A center and offset of 1024 place the result outside the 10-bit map. M7SEL=$80 keeps
    // that overflow transparent; character-zero fill would require both bits ($C0).
    Rgba32[] outside = SnesMode7Renderer.RenderViewport(vram, cgram, new Mode7RenderRegisters { MatrixA = 0x0100, MatrixB = 0, MatrixC = 0, MatrixD = 0x0100, Center = new Offset16<Mode7Pixels>(1024, 0), Scroll = new Offset16<Mode7Pixels>(1024, 0) }, size: new Extent16<ScreenPixels>(1, 1));
    AssertEqual(0, outside[0].A, "Mode 7 transparent outside fill");

    // A non-identity transform is essential here: applying HOFS after the matrix gives
    // the same answer under identity and hid the Ceres diagonal-boundary defect. With a
    // 90-degree matrix around (16,16), horizontal scroll +8 maps screen (16,16) to texel
    // (16,8), not (24,16). Give those two cells different characters to make the ordering
    // directly observable instead of asserting the transform against another formula.
    cgram.SetColor(6, Bgr555.FromWord(0x03e0)); // Fully green diagnostic color.
    for (int pixel = 0; pixel < 64; pixel++)
    {
        ushort word = (ushort)(2 * 64 + pixel);
        vram.ExecuteWordTransfer([(6 << 8)], word, 1);
    }
    // Populate map low bytes after the diagnostic character upload because Mode 7 shares
    // every physical word between those two planes; a full-word test transfer would
    // otherwise clear the map byte while writing the character's high byte.
    vram.LoadBytes((1 * 128 + 2) * 2, [1]); // Correct texel cell (16,8): red character 1.
    vram.LoadBytes((2 * 128 + 3) * 2, [2]); // Old post-matrix-scroll cell: green character 2.

    Rgba32[] rotatedScroll = SnesMode7Renderer.RenderViewport(vram, cgram, new Mode7RenderRegisters { MatrixA = 0, MatrixB = 0x0100, MatrixC = -0x0100, MatrixD = 0, Center = new Offset16<Mode7Pixels>(16, 16), Scroll = new Offset16<Mode7Pixels>(8, 0) }, size: new Extent16<ScreenPixels>(17, 17));
    AssertEqual(cgram.GetRgba(5), rotatedScroll[16 * 17 + 16],
        "Mode 7 transforms scroll before adding pivot");

    // Ridley's getaway is not one uninterrupted Mode-7 field. Bank-$88's paired BGMODE/TM
    // tables return the last sixteen scanlines to Mode 1 with BG2+OBJ, preserving the arena
    // floor while the chamber above rotates. Give the two sources unmistakably different
    // colors so a compositor that accidentally extends Mode 7 to line 223 fails directly.
    var splitVram = new SnesVram();
    var splitCgram = new SnesCgram();
    var emptySprites = new SpriteFrameBuffer();
    emptySprites.BeginFrame();
    emptySprites.FinalizeFrame();
    splitCgram.SetColor(1, Bgr555.FromWord(0x001f)); // Red Mode-1 BG2 floor.
    splitCgram.SetColor(2, Bgr555.FromWord(0x03e0)); // Green Mode-7 arena.

    // Display row 207 uses physical Mode-7 scanline 208, hence map row 26. Character two is a
    // solid green tile. The following physical line is deliberately covered by the Mode-1
    // floor setup below, so its Mode-7 contents cannot influence the expected result.
    splitVram.LoadMode7MapBytes([2], destinationWord: 26 * 128);
    splitVram.LoadMode7CharacterBytes(Enumerable.Repeat((byte)2, 64).ToArray(), destinationWord: 2 * 64);

    // At screen Y=208 with zero BG2VOFS, tile row 26/pixel row zero is sampled from BG2SC
    // $48. Character one at BG12NBA=$66 supplies a solid palette-zero/color-one red row.
    splitVram.ExecuteWordTransfer([0x0001], destinationWord: 0x4800 + 26 * 32, wordIncrement: 1);
    splitVram.LoadBytes((0x6000 + 16) * 2, [0xff, 0x00]);

    Rgba32[] split = SnesGameplayFrameRendererConstants.RenderHudCeresRidleyGetawayAndObjs(splitVram, splitCgram, emptySprites, new Mode7RenderRegisters { MatrixA = 0x0100, MatrixB = 0, MatrixC = 0, MatrixD = 0x0100, Center = new Offset16<Mode7Pixels>(0, 0), Scroll = new Offset16<Mode7Pixels>(0, 0) }, new CeresGetawayFloor { Bg2Scroll = new Position16<Bg2Pixels>(0, 0), Bg2CharacterBaseWord = 0x6000 });
    AssertEqual(splitCgram.GetRgba(2), split[207 * 256],
        "Ceres Ridley scanline 207 remains Mode 7");
    AssertEqual(splitCgram.GetRgba(1), split[208 * 256],
        "Ceres Ridley scanline 208 restores Mode-1 BG2 floor");
}

/// <summary>Confirms transparent layer keys reveal opaque CGRAM color zero at final scanout.</summary>
[PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
internal static void VerifyLayerCompositorBackdrop()
{
    var cgram = new SnesCgram();
    cgram.SetColor(0, Bgr555.FromWord(0x0421));
    cgram.SetColor(1, Bgr555.FromWord(0x001f));

    // A decoded layer deliberately uses alpha zero as its palette-index-zero key. Final
    // SNES scanout never has transparency, so an untouched pixel must retain opaque CGRAM
    // color zero while a non-keyed layer pixel replaces it.
    Rgba32[] frame = SnesLayerCompositor.CreateBackdrop(cgram, pixelCount: 2);
    Rgba32[] layer =
    [
        new Rgba32(255, 0, 255, 0),
        cgram.GetRgba(1),
    ];
    SnesLayerCompositor.Composite(frame, layer);

    AssertEqual(255, frame[0].A, "keyed backdrop remains opaque");
    AssertEqual(cgram.GetRgba(0), frame[0], "keyed pixel reveals CGRAM color zero");
    AssertEqual(cgram.GetRgba(1), frame[1], "non-keyed pixel replaces backdrop");
}

/// <summary>Confirms BG map priority bit $2000 routes otherwise identical tiles to separate render planes.</summary>
[PpuLayoutBoundary("exact hardware test: checks native CGRAM slots, native VRAM words")]
internal static void VerifyBgPriorityPlaneRendering()
{
    var vram = new SnesVram();
    var cgram = new SnesCgram();
    cgram.SetColor(5, Bgr555.FromWord(0x001f));

    // Two adjacent map cells use the same visible 2-bpp character and palette. Only bit
    // $2000 differs, so priority-plane filtering must place one red pixel in each result.
    vram.ExecuteWordTransfer([0x0401, 0x2401], destinationWord: 0, wordIncrement: 1);
    vram.LoadBytes(0x0210, [0x80, 0x00]); // Character 1, first row, leftmost color-1 pixel.

    Rgba32[] low = SnesBgTilemapRenderer.Render2Bpp(
        vram, cgram, tilemapBaseWord: 0, characterBaseWord: 0x0100, rowCount: 1,
        transparentColorZero: true, priority: false);
    Rgba32[] high = SnesBgTilemapRenderer.Render2Bpp(
        vram, cgram, tilemapBaseWord: 0, characterBaseWord: 0x0100, rowCount: 1,
        transparentColorZero: true, priority: true);

    AssertEqual(255, low[0].A, "low-priority BG cell selected");
    AssertEqual(0, low[8].A, "high-priority BG cell omitted from low plane");
    AssertEqual(0, high[0].A, "low-priority BG cell omitted from high plane");
    AssertEqual(255, high[8].A, "high-priority BG cell selected");
}

/// <summary>Checks the fresh-save file-select tilemap and its surrounding labels against installed cartridge art.</summary>
internal static void VerifyFileSelectFreshSaveTilemap()
{
    var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling
        .LoadRetailRom(Path.GetFullPath("Super Metroid.smc")).Rom.ToArray();

    // FileSelectMenuState also loads the labels surrounding NO DATA. Empty streams are
    // sufficient for this focused fixture, but each one still needs the native $FFFF
    // terminator so the cartridge-backed loader cannot wander into zero-filled ROM.
    int[] unusedLabelAddresses =
    [
        0x810000 | FileSelectTilemapDefinitions.SamusData,
        0x810000 | FileSelectTilemapDefinitions.SlotLabel(0),
        0x810000 | FileSelectTilemapDefinitions.SlotLabel(1),
        0x810000 | FileSelectTilemapDefinitions.SlotLabel(2),
        0x810000 | FileSelectTilemapDefinitions.Exit,
        0x810000 | FileSelectTilemapDefinitions.DataCopyMode,
        0x810000 | FileSelectTilemapDefinitions.DataClearMode,
        0x810000 | FileSelectTilemapDefinitions.CopyWhichData,
        0x810000 | FileSelectTilemapDefinitions.CopySamusToWhere,
        0x810000 | FileSelectTilemapDefinitions.CopySamusToSamus,
        0x810000 | FileSelectTilemapDefinitions.IsThisOkay,
        0x810000 | FileSelectTilemapDefinitions.Yes,
        0x810000 | FileSelectTilemapDefinitions.No,
        0x810000 | FileSelectTilemapDefinitions.CopyCompleted,
        0x810000 | FileSelectTilemapDefinitions.ClearWhichData,
        0x810000 | FileSelectTilemapDefinitions.ClearSamus,
        0x810000 | FileSelectTilemapDefinitions.DataCleared,
    ];
    foreach (int address in unusedLabelAddresses)
        WriteRomWord(rom, address, 0xffff);
    // These two visible main-menu entries were the reported omission. Give each stream a
    // unique first tile so the fixture asserts ROM-authored rendering rather than merely
    // checking that selection index three/four became reachable.
    WriteRomWord(rom, 0x810000 | FileSelectTilemapDefinitions.DataCopy, 0x20c0);
    WriteRomWord(rom, 0x810000 | (FileSelectTilemapDefinitions.DataCopy + 2), 0xffff);
    WriteRomWord(rom, 0x810000 | FileSelectTilemapDefinitions.DataClear, 0x20c1);
    WriteRomWord(rom, 0x810000 | (FileSelectTilemapDefinitions.DataClear + 2), 0xffff);

    // The retail menu always loads the static TIME caption for each slot. Only the adjacent
    // HH:MM digits are conditional on a valid save. Reproduce the exact `$81:B4A0` stream so
    // an empty-slot fixture verifies that important distinction instead of hiding it.
    ushort[] timeWords = [0x20ad, 0x20ae, 0x20af, 0xffff];
    for (int index = 0; index < timeWords.Length; index++)
        WriteRomWord(rom, 0x810000 | (FileSelectTilemapDefinitions.Time + index * 2), timeWords[index]);
    ushort[] energyWords = [0x209d, 0x209e, 0x209f, 0x20cc, 0xffff];
    for (int index = 0; index < energyWords.Length; index++)
        WriteRomWord(rom, 0x810000 | (FileSelectTilemapDefinitions.Energy + index * 2), energyWords[index]);
    WriteRomWord(rom, 0x810000 | FileSelectTilemapDefinitions.TimeColon, 0x208c);
    WriteRomWord(rom, 0x810000 | (FileSelectTilemapDefinitions.TimeColon + 2), 0xffff);

    // This is the literal retail structure at $81:B4AC: one leading blank, "NO DATA",
    // and three trailing blanks. Keeping the exact words makes the test cover both the
    // destination coordinates and the fact that the source itself intentionally starts
    // one character before the visible N.
    ushort[] noDataWords =
    [
        0x000f,
        0x2077, 0x2078,
        0x200f,
        0x206d, 0x206a, 0x207d, 0x206a,
        0x200f, 0x200f, 0x200f,
        0xffff,
    ];
    for (int index = 0; index < noDataWords.Length; index++)
        WriteRomWord(rom, 0x810000 | (FileSelectTilemapDefinitions.NoData + index * 2), noDataWords[index]);

    // Use two sparse bytes that the real compiled Crateria codec exports. This proves SRAM
    // persists the packed representation and ReadSlot reconstructs the seven-plane domain
    // representation without allowing a synthetic ROM to redefine fixed codec metadata.
    var exploredMap = new byte[
        Bank80SystemState.ExploredMapAreaCount *
        Bank80SystemState.ExploredMapBytesPerArea];
    exploredMap[0x07] = 0x80;
    exploredMap[0x84] = 0x04;
    var usedSaveStations = new byte[Bank80SystemState.UsedSaveStationByteCount];
    usedSaveStations[0] = 0x01;
    var mapStations = new byte[Bank80SystemState.MapStationByteCount];
    mapStations[0] = 0xff;

    var addressSpace = new SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace(rom);
    using var overrideRootScratch = new TestScratchDirectory("file-select-label-fixture");
    string overrideRoot = overrideRootScratch.Root;
    Directory.CreateDirectory(overrideRoot);
    File.WriteAllBytes(Path.Combine(overrideRoot, FileSelectPresentationDefinitions.FileName),
        SuperMetroid.AssetExtraction.Frontend.FileSelectPresentationExtractor.Extract(addressSpace));
    var fixturePresentation = AreaMapPresentationCatalog.Load(
        RepositoryInstallation.Installation.MapDirectory, overrideRoot);
    var saveRam = new SuperMetroid.Core.Saves.SaveSlotStore(RetailPresentationFixture());
    var menu = new FileSelectMenuState(saveRam, mapPresentation: fixturePresentation);
    ReadOnlySpan<ushort> tilemap = menu.BackgroundTilemap;

    // Native empty-slot origins are energy-field X plus one $40-byte row: rows 6, 11,
    // and 16 at column 14. Consequently every visible N begins at column 15 and the whole
    // seven-character label remains on that row. A next-row column-zero check catches the
    // exact wraparound regression that placed N above "O DATA".
    int[] labelStarts = [0x019c / 2, 0x02dc / 2, 0x041c / 2];
    foreach (int start in labelStarts)
    {
        AssertEqual(0x000f, tilemap[start], "file-select NO DATA leading blank");
        AssertEqual(0x2077, tilemap[start + 1], "file-select NO DATA N column");
        AssertEqual(0x2078, tilemap[start + 2], "file-select NO DATA O column");
        AssertEqual(0x206d, tilemap[start + 4], "file-select NO DATA D column");
        AssertEqual(0x206a, tilemap[start + 5], "file-select NO DATA A column");
        AssertEqual(0x207d, tilemap[start + 6], "file-select NO DATA T column");
        AssertEqual(0x206a, tilemap[start + 7], "file-select NO DATA final A column");
        int followingRowStart = ((start / 32) + 1) * 32;
        AssertEqual(0x000f, tilemap[followingRowStart],
            "file-select NO DATA does not wrap to next row");
    }

    // `$81:9F3D/$9F73/$9FA9` place TIME at row 5/10/15, column 27. These calls
    // occur after Draw_FileSelection_Time returns for an empty slot and are not gated.
    int[] timeStarts = [0x0176 / 2, 0x02b6 / 2, 0x03f6 / 2];
    foreach (int start in timeStarts)
    {
        AssertEqual(0x20ad, tilemap[start], "file-select empty-slot TIME T tile");
        AssertEqual(0x20ae, tilemap[start + 1], "file-select empty-slot TIME I/M tile");
            AssertEqual(0x20af, tilemap[start + 2], "file-select empty-slot TIME M/E tile");
    }

    // `$8B:C100` saves area six/station zero through the ordinary `$81:8000` encoder.
    // Verify both redundant checksum directories, the decoded player/checkpoint data, and
    // the menu's subsequent ENERGY + HH:MM path against one shared save store.
    saveRam.SaveSlot(0, new SuperMetroidSaveSnapshot
    {
        Health = 99,
        MaxHealth = 99,
        GameTimeMinutes = 34,
        GameTimeHours = 12,
        UsedSaveStationBytes = usedSaveStations,
        MapStationBytes = mapStations,
        ExploredMapBytes = exploredMap,
        Area = 6,
        SaveStation = 0,
    });
    SuperMetroidSaveSlot saved = saveRam.ReadSlot(0)
        ?? throw new InvalidOperationException("Fresh Ceres SRAM slot failed its own checksums.");
    AssertEqual(99, saved.Health, "Ceres checkpoint saved health");
    AssertEqual(99, saved.MaxHealth, "Ceres checkpoint saved maximum health");
    AssertEqual(6, saved.Area, "Ceres checkpoint saved area");
    AssertEqual(0, saved.SaveStation, "Ceres checkpoint saved load station");
    AssertEqual(0x01, saved.UsedSaveStationBytes[0], "used Crateria save-station bit");
    AssertEqual(0xff, saved.MapStationBytes[0], "Crateria map-station byte");
    AssertEqual(0x80, saved.ExploredMapBytes[0x07], "unpacked explored-map first exported byte");
    AssertEqual(0x04, saved.ExploredMapBytes[0x84], "unpacked explored-map sparse byte");

    var savedMenu = new FileSelectMenuState(saveRam, mapPresentation: fixturePresentation);
    ReadOnlySpan<ushort> savedTilemap = savedMenu.BackgroundTilemap;
    AssertEqual(0x209d, savedTilemap[0x15c / 2], "saved slot ENERGY first tile");
    AssertEqual(0x2069, savedTilemap[(0x15c + 0x42) / 2], "saved slot energy tens");
    AssertEqual(0x2069, savedTilemap[(0x15c + 0x44) / 2], "saved slot energy ones");
    AssertEqual(0x2061, savedTilemap[0x1b4 / 2], "saved slot hour tens");
    AssertEqual(0x2062, savedTilemap[(0x1b4 + 2) / 2], "saved slot hour ones");
    AssertEqual(0x208c, savedTilemap[(0x1b4 + 4) / 2], "saved slot time colon");
    AssertEqual(0x2063, savedTilemap[(0x1b4 + 6) / 2], "saved slot minute tens");
    AssertEqual(0x2064, savedTilemap[(0x1b4 + 8) / 2], "saved slot minute ones");
    AssertEqual(0x20c0, savedTilemap[0x508 / 2], "file-select DATA COPY menu entry");
    AssertEqual(0x20c1, savedTilemap[0x5c8 / 2], "file-select DATA CLEAR menu entry");

    // Drive the real newly-pressed latch and fade states into COPY. Slot zero is the only
    // nonempty source, destination one is the first native choice, and YES is the default.
    var copyMenu = new FileSelectMenuState(saveRam, mapPresentation: fixturePresentation);
    AdvanceFileSelectToMain(copyMenu);
    PulseFileSelect(copyMenu, SnesButton.Down);
    PulseFileSelect(copyMenu, SnesButton.Down);
    PulseFileSelect(copyMenu, SnesButton.Down);
    AssertEqual(FileSelectItem.Copy, copyMenu.SelectedItem, "file-select reaches DATA COPY");
    PulseFileSelect(copyMenu, SnesButton.A);
    AdvanceFileSelectFade(copyMenu, FileSelectPhase.CopySelectSource);
    PulseFileSelect(copyMenu, SnesButton.A);
    AssertEqual(FileSelectPhase.CopySelectDestination, copyMenu.Phase,
        "copy chooses nonempty source slot");
    PulseFileSelect(copyMenu, SnesButton.A);
    AssertEqual(FileSelectPhase.CopyConfirm, copyMenu.Phase,
        "copy chooses distinct destination slot");
    copyMenu.Step(0);
    copyMenu.Step(SnesButton.A);
    AssertTrue(copyMenu.SaveRamChangedThisFrame, "copy publishes SRAM mutation frame");
    AssertEqual(FileSelectPhase.CopyCompleted, copyMenu.Phase, "copy reaches completion screen");
    SuperMetroidSaveSlot copied = saveRam.ReadSlot(1)
        ?? throw new InvalidOperationException("File-select COPY did not create slot B.");
    AssertEqual(saved.Health, copied.Health, "file-select COPY preserves health");
    AssertEqual(saved.GameTimeMinutes, copied.GameTimeMinutes, "file-select COPY preserves time");

    // Re-enter the main screen from the copied SRAM image and clear slot A. This exercises
    // the actual confirmation default and the native four-directory invalidation, not a
    // host-only hidden flag.
    var clearMenu = new FileSelectMenuState(saveRam, mapPresentation: fixturePresentation);
    AdvanceFileSelectToMain(clearMenu);
    for (int move = 0; move < 4; move++)
        PulseFileSelect(clearMenu, SnesButton.Down);
    AssertEqual(FileSelectItem.Clear, clearMenu.SelectedItem, "file-select reaches DATA CLEAR");
    PulseFileSelect(clearMenu, SnesButton.A);
    AdvanceFileSelectFade(clearMenu, FileSelectPhase.ClearSelectSlot);
    PulseFileSelect(clearMenu, SnesButton.A);
    AssertEqual(FileSelectPhase.ClearConfirm, clearMenu.Phase,
        "clear chooses first nonempty slot");
    clearMenu.Step(0);
    clearMenu.Step(SnesButton.A);
    AssertTrue(clearMenu.SaveRamChangedThisFrame, "clear publishes SRAM mutation frame");
    AssertEqual(FileSelectPhase.ClearCompleted, clearMenu.Phase,
        "clear reaches DATA CLEARED screen");
    AssertTrue(saveRam.ReadSlot(0) is null, "file-select CLEAR invalidates slot A checksums");
    AssertTrue(saveRam.ReadSlot(1) is not null, "file-select CLEAR preserves other slots");

    AdvanceFileSelectFade(savedMenu, FileSelectPhase.Main);
    savedMenu.Step(SnesButton.A);
    for (int guard = 0;
         savedMenu.Phase != FileSelectPhase.FadeOutToOptions && guard < 80;
         guard++)
    {
        savedMenu.Step(0);
    }
    AssertEqual(FileSelectPhase.FadeOutToOptions, savedMenu.Phase,
        "selected save completes helmet turn before load fade");
    int selectedHelmetFrame = savedMenu.SelectedHelmetFrame;
    AssertEqual(7, selectedHelmetFrame,
        "selected save reaches final turned-head frame");
    savedMenu.Step(0);
    AssertEqual(selectedHelmetFrame, savedMenu.SelectedHelmetFrame,
        "file-select fade retains turned Samus head instead of resetting before load");

    saveRam.SelectSlot(2);
    AssertEqual(2, saveRam.ReadSelectedSlot(), "SRAM selected-slot word and complement");
    byte[] image = SuperMetroid.Core.Saves.SaveRamCodec.Encode(saveRam, RetailPresentationFixture());
    image[SuperMetroid.Core.Saves.SaveRamLayout.SlotOffset(1) + SuperMetroid.Core.Saves.SaveRamLayout.HealthOffset] ^= 1;
    AssertTrue(SuperMetroid.Core.Saves.SaveRamCodec.Decode(image, RetailPresentationFixture()).ReadSlot(1) is null,
        "SRAM payload corruption invalidates both directories");

    Console.WriteLine("  SRAM/file select: slots, COPY/CLEAR, checksums, NO DATA, ENERGY, and TIME agree.");
}

/// <summary>Advances the native startup and fade dispatches until file select reaches its main menu.</summary>
static void AdvanceFileSelectToMain(FileSelectMenuState menu)
{
    // Native indices 0-2 take five updates (two are NMI continuations), then index
    // three's delay-one fade takes thirty dispatches (100% movie frames 399-437).
    for (int frame = 0; frame < 34; frame++)
        menu.Step(0);
    AssertTrue(menu.Phase != FileSelectPhase.Main, "file-select fade is still running on its 34th update");
    menu.Step(0);
    AssertEqual(FileSelectPhase.Main, menu.Phase, "file-select fade reaches main menu on its 35th update");
}

/// <summary>Steps a data-management fade until the expected file-select phase becomes active.</summary>
static void AdvanceFileSelectFade(FileSelectMenuState menu, FileSelectPhase expected)
{
    // Delay-one native fades: thirty updates out, thirty back in. The data-management
    // screens' own force-blank NMI waits are not yet modeled as separate updates.
    for (int guard = 0; menu.Phase != expected && guard < 64; guard++)
        menu.Step(0);
    AssertEqual(expected, menu.Phase, "file-select data-management fade completes");
}

/// <summary>Sends one button edge followed by its release to the file-select state machine.</summary>
static void PulseFileSelect(FileSelectMenuState menu, SnesButton button)
{
    menu.Step(button);
    menu.Step(0);
}

/// <summary>Checks a saved-game load's front-facing pose, 360-frame input lock, pose-history commit, and save lockout.</summary>
internal static void VerifySavedGameLoadAppearance()
{
    string romPath = Path.GetFullPath("Super Metroid.smc");
    if (!File.Exists(romPath))
    {
        Console.WriteLine("  Saved-game appearance: retail-ROM test skipped (ROM not present).");
        return;
    }

    var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
    var saveRam = new SuperMetroid.Core.Saves.SaveSlotStore(RetailPresentationFixture());
    saveRam.SaveSlot(0, new SuperMetroidSaveSnapshot
    {
        Health = 99,
        MaxHealth = 99,
        Area = 0,
        SaveStation = 1,
    });
    SuperMetroidSaveSlot slot = saveRam.ReadSlot(0)
        ?? throw new InvalidOperationException("Synthetic Crateria save did not validate.");

    var runtime = CreateRetailRuntimeFixture();
    runtime.HudSetup.InitializeHud(new HudSnapshot(
        slot.Health,
        slot.MaxHealth,
        slot.Missiles,
        slot.MaxMissiles,
        slot.SuperMissiles,
        slot.MaxSuperMissiles,
        slot.PowerBombs,
        slot.MaxPowerBombs,
        slot.EquippedItems,
        slot.HudItem,
        slot.ReserveEnergy,
        slot.ReserveMode));
    runtime.FrameBoundary.BeginAcceptedFrame(0);
    runtime.RoomLoader.InitializeSavedGame(slot);

    AssertTrue(runtime.SamusLoadAppearanceActive,
        "saved-game load enters command-nine appearance handler");
    AssertEqual(0x0168, runtime.SamusLoadAppearanceFramesRemaining,
        "saved-game appearance uses native 360-frame fanfare lifetime");
    AssertTrue(runtime.Player.Samus is { Control.AlphaLocked: true } &&
        SamusPoseMetadata.IsForwardFacingPose(runtime.Player.Samus.Pose),
        "saved-game appearance begins locked and front-facing");
    AssertTrue(runtime.Plms.StationSnapshots.Where(station => station.Kind == StationKind.Save)
        .All(station => station.SaveStationLockedOut),
        "loading onto a save room sets the native room-entry lockout");

    var appearanceHistory = runtime.Player.IntroducedSamus.PoseHistory;
    SamusPoseId appearancePose = appearanceHistory.PreviousPose;
    int appearanceMetadata = appearanceHistory.PreviousDirectionAndMovement;
    // Distinct older sample exposes the otherwise invisible same-pose shift.
    appearanceHistory.LastDifferentPose = SamusPoseId.SpinJumpRightPose;
    appearanceHistory.LastDifferentDirectionAndMovement = 0x0308;
    for (int frame = 0; frame < 0x0167; frame++)
        runtime.StepFrame(0);
    AssertTrue(runtime.SamusLoadAppearanceActive,
        "saved-game appearance remains active through frame 359");
    AssertEqual(SamusPoseId.SpinJumpRightPose, appearanceHistory.LastDifferentPose,
        "appearance does not commit history before its completion call");
    runtime.StepFrame(0);
    AssertEqual(appearancePose, appearanceHistory.LastDifferentPose, "appearance completion shifts prior pose");
    AssertEqual(appearanceMetadata, appearanceHistory.LastDifferentDirectionAndMovement, "appearance completion shifts prior metadata");
    AssertEqual(runtime.Player.IntroducedSamus.Pose, appearanceHistory.PreviousPose, "appearance completion commits current pose");
    AssertEqual(SnesLittleEndian.Word(SamusPoseMetadata.ReadPoseXDirection(runtime.Player.IntroducedSamus), (byte)SamusPoseMetadata.ReadMovementType(runtime.Player.IntroducedSamus)),
        appearanceHistory.PreviousDirectionAndMovement, "appearance completion commits current metadata");
    AssertTrue(!runtime.SamusLoadAppearanceActive &&
        runtime.Player.Samus is { Control.AlphaLocked: false },
        "saved-game appearance restores ordinary input on call 360");
    AssertTrue(!runtime.MessageBox.IsActive,
        "save pod cannot immediately reopen after a load in the same room entry");

    Console.WriteLine(
        "  Saved-game appearance: front pose, ROM palette FX, 360-frame lifetime, and save lockout agree.");
}

/// <summary>Confirms intro flashbacks inherit eight pixels of BG1 scroll and Mother Brain shake oscillates around it.</summary>
[PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
internal static void VerifyIntroGameplayFlashbackVerticalScroll()
{
    // `$8B:A66F` installs BG1VOFS eight for the illustrated page. The two following
    // gameplay-style setup functions replace BG1SC but leave that scroll word untouched.
    // Lock the inherited value independently of any host-authored actor coordinates.
    AssertEqual(8, IntroCinematicState.GameplayFlashbackBg1VerticalScroll,
        "intro gameplay flashback inherited BG1 vertical scroll");

    var motherBrain = new IntroMotherBrainSpriteState();
    AssertEqual(8, motherBrain.Background.Y,
        "intro Mother Brain starts from inherited BG1 vertical scroll");

    // Four impacts select `$8B:B80F`. Its `$8B:B877` shake adds four on an even frame and
    // removes four on an odd frame, oscillating around eight rather than around zero.
    for (int hit = 0; hit < 4; hit++)
        motherBrain.RegisterMissileHit();
    var cgram = new SnesCgram();
    var introPalette = new ushort[SnesCgram.ColorCount];
    motherBrain.RunPreInstruction(cgram, ToColors(introPalette), cinematicFrameCounter: 2, introCrossfadeTimer: 0x7f);
    AssertEqual(12, motherBrain.Background.Y,
        "intro Mother Brain even-frame shake adds four to inherited scroll");
    motherBrain.RunPreInstruction(cgram, ToColors(introPalette), cinematicFrameCounter: 3, introCrossfadeTimer: 0x7f);
    AssertEqual(8, motherBrain.Background.Y,
        "intro Mother Brain odd-frame shake returns to inherited scroll");
    AssertEqual(0, motherBrain.Background.X, "intro Mother Brain shake leaves BG1 horizontal scroll at zero");

    Console.WriteLine("  Intro: Mother Brain and SR388 BG1 retain the native eight-pixel vertical scroll.");
}

/// <summary>Checks native byte-offset clearing and fixed-point fade rounding over 32 cinematic palette steps.</summary>
[PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
internal static void VerifyCinematicPaletteFader()
{
    var target = new ushort[SnesCgram.ColorCount];
    // Component four produces step $0020, making the fixed-point rounding boundary easy
    // to see: seven calls compose to zero and the eighth composes to component one.
    target[20] = 0x1084;
    var cgram = new SnesCgram();
    var fader = new CinematicPaletteFader(ToColors(target));

    // $8B:B018 clears incoming range $28/$03. Because X is a byte offset, the first
    // affected entry is color 20; the span names that color directly.
    var color20 = new IntroPaletteSpan(20, 1);
    fader.Clear(new IntroPaletteSpan(20, 3));
    fader.ComposeInto(cgram);
    AssertEqual(0x0000, cgram.Colors[20], "cinematic clear starts at the span's first color");

    // One retail step adds component four << 3 = $0020. Compose takes the high byte,
    // so the first seven calls remain black and the eighth produces component one.
    for (int step = 0; step < 7; step++)
        fader.FadeIn(color20);
    fader.ComposeInto(cgram);
    AssertEqual(0x0000, cgram.Colors[20], "8.8 cinematic fade preserves early rounding");
    fader.FadeIn(color20);
    fader.ComposeInto(cgram);
    AssertEqual(0x0421, cgram.Colors[20], "eighth fade step reaches BGR component one");

    for (int step = 8; step < 32; step++)
        fader.FadeIn(color20);
    fader.ComposeInto(cgram);
    AssertEqual(0x1084, cgram.Colors[20], "32 fade-in calls reach exact target");

    for (int step = 0; step < 32; step++)
        fader.FadeOut(color20);
    fader.ComposeInto(cgram);
    AssertEqual(0x0000, cgram.Colors[20], "32 fade-out calls reach exact black");
}

/// <summary>Checks the Mother Brain flashback's typed demo program: held and new input edges, record timing and its end routine.</summary>
internal static void VerifyDemoInputObject()
{
    var demo = new DemoInputState();
    demo.Clear();
    demo.Enable();
    demo.Load(IntroDemoInputObjectDefinitions.IntroMotherBrainFlashback);

    // The first six records at $91:8694. The two one-frame X edges are separated by held-X
    // records; normal projectile cooldown/motion code, not this program, decides each shot.
    int[] boundaries = [90, 1, 40, 1, 29, 70];
    SnesButton[] held =
        [0, SnesButton.X, SnesButton.X, SnesButton.X,
            SnesButton.X, 0];
    SnesButton[] newlyPressed =
        [0, SnesButton.X, 0, SnesButton.X, 0, 0];
    int elapsed = 0, ends = 0;
    void EndRoutine(DemoInputCue cue)
    {
        AssertEqual(DemoInputCue.EndIntroMotherBrainFlashback, cue, "flashback demo end routine");
        ends++;
        demo.Disable();
    }
    for (int record = 0; record < boundaries.Length; record++)
    {
        for (int frame = 0; frame < boundaries[record]; frame++)
        {
            demo.Step(DemoInputState.RunReturnOnlyPreInstruction, EndRoutine);
            AssertEqual(held[record], demo.Held, $"demo record {record} held frame {frame}");
            AssertEqual(newlyPressed[record], demo.NewlyPressed,
                $"demo record {record} new frame {frame}");
            elapsed++;
        }
    }

    AssertEqual(231, elapsed, "old Mother Brain demo-input duration");
    AssertEqual(DemoInputRomDataIntroMotherBrain.NextRecord, DemoInputNativeListAddresses.Address(demo.Cursor),
        "old Mother Brain next record pointer");
    // Loading a record and publishing its first visible frame happen in the same handler
    // call. Consequently the last of its 70 visible frames leaves timer one; the following
    // call decrements to zero and fetches the next record.
    AssertEqual(1, demo.InstructionTimer.RawValue, "final visible record frame retains timer one");

    // $8739 disables the object and returns into the shared delete in the same handler call.
    for (int frame = 0; frame < 1024 && demo.IsActive; frame++)
        demo.Step(DemoInputState.RunReturnOnlyPreInstruction, EndRoutine);
    AssertEqual(1, ends, "flashback demo end routine runs once");
    AssertEqual(false, demo.Enabled, "demo end routine disables the object");
    AssertEqual(false, demo.IsActive, "demo end routine continues into delete");
    AssertEqual(SnesButton.None, demo.Held, "demo delete clears held input");
    AssertEqual(SnesButton.None, demo.NewlyPressed, "demo delete clears new input");

    Console.WriteLine("  Demo input: records, edges, end routine and deletion agree.");
}

/// <summary>Reads a fixture instruction from bank $91 through the cartridge import boundary.</summary>
[CpuBusBoundary("exact bus test: retail and constructed cartridge oracle for bank-$91 demo-input object and instruction words")]
internal static ushort ReadDemoFixtureWord(ISnesAddressSpace bus, ushort pointer) =>
    CartridgeImportSource.Require(bus).ReadCartridgeWordFixedBank(DemoInputRomData.BankBase | pointer);
/// <summary>Writes a little-endian fixture word at a mapped SNES ROM address.</summary>
internal static void WriteRomWord(byte[] rom, int snesAddress, ushort value)
{
    int offset = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace.ToRomOffset(snesAddress);
    rom[offset] = unchecked((byte)value);
    rom[offset + 1] = SnesLittleEndian.HighByte(value);
}


    /// <summary>Runs the constructed demo-input object and control-opcode fixture.</summary>
    [VerificationCommand("--demo-input-fixture")]
    internal static int DemoInputFixtureCommand()
    {
     VerifyDemoInputObject(); return 0; 
    }

    /// <summary>Runs the cartridge-backed fresh-save file-select tilemap check.</summary>
    [VerificationCommand("--file-select-fresh-save")]
    internal static int FileSelectFreshSaveCommand()
    {
        Suite(nameof(VerifyFileSelectFreshSaveTilemap), () => VerifyFileSelectFreshSaveTilemap());
        return 0;
    }

    /// <summary>Runs the demo object check through the normal verification suite wrapper.</summary>
    [VerificationCommand("--demo-input-object")]
    internal static int DemoInputObjectCommand()
    {
        Suite(nameof(VerifyDemoInputObject), () => VerifyDemoInputObject());
        return 0;
    }
}
