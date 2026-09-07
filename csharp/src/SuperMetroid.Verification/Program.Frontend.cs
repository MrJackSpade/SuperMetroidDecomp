using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
static void VerifyLoRomCrossBankCompressedData()
{
    var rom = new byte[SuperMetroidAddressSpace.RetailRomByteCount];

    // The four-byte literal begins at $94:FFFC. Its final byte is $94:FFFF and the $FF
    // terminator is the physically adjacent byte at $95:8000. This is the same bank-cross
    // shape used by the real title graphics stream, reduced to a six-byte fixture.
    byte[] compressed = [0x03, 0x11, 0x22, 0x33, 0x44, 0xff];
    int beforeCrossing = SuperMetroidAddressSpace.ToRomOffset(0x94fffc);
    int afterCrossing = SuperMetroidAddressSpace.ToRomOffset(0x958000);
    compressed.AsSpan(0, 4).CopyTo(rom.AsSpan(beforeCrossing));
    compressed.AsSpan(4, 2).CopyTo(rom.AsSpan(afterCrossing));

    var bus = new SuperMetroidAddressSpace(rom);
    byte[] output = RomDataReader.Decompress(bus, 0x94fffc, maximumCompressedBytes: 16);
    AssertEqual(4, output.Length, "cross-bank decompressed length");
    AssertEqual(0x11, output[0], "cross-bank first literal");
    AssertEqual(0x44, output[3], "cross-bank final literal");
}

static void VerifyMode7Rendering()
{
    var vram = new SnesVram();
    var cgram = new SnesCgram();
    cgram.SetColor(5, 0x001f); // Fully red BGR555 diagnostic color.

    // Mode 7's tilemap lives in low VRAM bytes. Select character one for map cell zero,
    // then put palette index five in every high byte of that character's 64 words.
    vram.LoadBytes(0, [1]);
    for (int pixel = 0; pixel < 64; pixel++)
    {
        ushort word = (ushort)(64 + pixel);
        vram.ExecuteWordTransfer([(ushort)(5 << 8)], word, 1);
    }

    Rgba32[] identity = SnesMode7Renderer.RenderViewport(
        vram, cgram,
        matrixA: 0x0100, matrixB: 0,
        matrixC: 0, matrixD: 0x0100,
        centerX: 0, centerY: 0,
        horizontalOffset: 0, verticalOffset: 0,
        width: 8, height: 8);
    AssertEqual(64, identity.Length, "Mode 7 identity dimensions");
    AssertTrue(identity.Take(56).All(pixel => pixel.R == 255 && pixel.G == 0 && pixel.B == 0),
        "Mode 7 identity reads character rows one through seven on physical scanlines one through seven");
    AssertTrue(identity.Skip(56).All(pixel => pixel.A == 0),
        "Mode 7 physical scanline eight samples the next tile row");

    // A center and offset of 1024 place the result outside the 10-bit map. M7SEL=$80 keeps
    // that overflow transparent; character-zero fill would require both bits ($C0).
    Rgba32[] outside = SnesMode7Renderer.RenderViewport(
        vram, cgram,
        matrixA: 0x0100, matrixB: 0,
        matrixC: 0, matrixD: 0x0100,
        centerX: 1024, centerY: 0,
        horizontalOffset: 1024, verticalOffset: 0,
        width: 1, height: 1);
    AssertEqual(0, outside[0].A, "Mode 7 transparent outside fill");

    // A non-identity transform is essential here: applying HOFS after the matrix gives
    // the same answer under identity and hid the Ceres diagonal-boundary defect. With a
    // 90-degree matrix around (16,16), horizontal scroll +8 maps screen (16,16) to texel
    // (16,8), not (24,16). Give those two cells different characters to make the ordering
    // directly observable instead of asserting the transform against another formula.
    cgram.SetColor(6, 0x03e0); // Fully green diagnostic color.
    for (int pixel = 0; pixel < 64; pixel++)
    {
        ushort word = (ushort)(2 * 64 + pixel);
        vram.ExecuteWordTransfer([(ushort)(6 << 8)], word, 1);
    }
    // Populate map low bytes after the diagnostic character upload because Mode 7 shares
    // every physical word between those two planes; a full-word test transfer would
    // otherwise clear the map byte while writing the character's high byte.
    vram.LoadBytes((1 * 128 + 2) * 2, [1]); // Correct texel cell (16,8): red character 1.
    vram.LoadBytes((2 * 128 + 3) * 2, [2]); // Old post-matrix-scroll cell: green character 2.

    Rgba32[] rotatedScroll = SnesMode7Renderer.RenderViewport(
        vram, cgram,
        matrixA: 0, matrixB: 0x0100,
        matrixC: -0x0100, matrixD: 0,
        centerX: 16, centerY: 16,
        horizontalOffset: 8, verticalOffset: 0,
        width: 17, height: 17);
    AssertEqual(cgram.GetRgba(5), rotatedScroll[16 * 17 + 16],
        "Mode 7 transforms scroll before adding pivot");

    // Ridley's getaway is not one uninterrupted Mode-7 field. Bank-$88's paired BGMODE/TM
    // tables return the last sixteen scanlines to Mode 1 with BG2+OBJ, preserving the arena
    // floor while the chamber above rotates. Give the two sources unmistakably different
    // colors so a compositor that accidentally extends Mode 7 to line 223 fails directly.
    var splitVram = new SnesVram();
    var splitCgram = new SnesCgram();
    var emptyOam = new OamBuffer();
    emptyOam.BeginFrame();
    emptyOam.FinalizeFrame();
    splitCgram.SetColor(1, 0x001f); // Red Mode-1 BG2 floor.
    splitCgram.SetColor(2, 0x03e0); // Green Mode-7 arena.

    // Display row 207 uses physical Mode-7 scanline 208, hence map row 26. Character two is a
    // solid green tile. The following physical line is deliberately covered by the Mode-1
    // floor setup below, so its Mode-7 contents cannot influence the expected result.
    splitVram.LoadMode7MapBytes([2], destinationWord: 26 * 128);
    splitVram.LoadMode7CharacterBytes(Enumerable.Repeat((byte)2, 64).ToArray(), destinationWord: 2 * 64);

    // At screen Y=208 with zero BG2VOFS, tile row 26/pixel row zero is sampled from BG2SC
    // $48. Character one at BG12NBA=$66 supplies a solid palette-zero/color-one red row.
    splitVram.ExecuteWordTransfer([0x0001], destinationWord: 0x4800 + 26 * 32, wordIncrement: 1);
    splitVram.LoadBytes((0x6000 + 16) * 2, [0xff, 0x00]);

    Rgba32[] split = SnesGameplayFrameRenderer.RenderHudCeresRidleyGetawayAndObjs(
        splitVram,
        splitCgram,
        emptyOam,
        matrixA: 0x0100,
        matrixB: 0,
        matrixC: 0,
        matrixD: 0x0100,
        centerX: 0,
        centerY: 0,
        horizontalOffset: 0,
        verticalOffset: 0,
        bg2HorizontalScroll: 0,
        bg2VerticalScroll: 0,
        bg2CharacterBaseWord: 0x6000);
    AssertEqual(splitCgram.GetRgba(2), split[207 * 256],
        "Ceres Ridley scanline 207 remains Mode 7");
    AssertEqual(splitCgram.GetRgba(1), split[208 * 256],
        "Ceres Ridley scanline 208 restores Mode-1 BG2 floor");
}

static void VerifyLayerCompositorBackdrop()
{
    var cgram = new SnesCgram();
    cgram.SetColor(0, 0x0421);
    cgram.SetColor(1, 0x001f);

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

static void VerifyBgPriorityPlaneRendering()
{
    var vram = new SnesVram();
    var cgram = new SnesCgram();
    cgram.SetColor(5, 0x001f);

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

static void VerifyFileSelectFreshSaveTilemap()
{
    var rom = new byte[SuperMetroidAddressSpace.RetailRomByteCount];

    // FileSelectMenuState also loads the labels surrounding NO DATA. Empty streams are
    // sufficient for this focused fixture, but each one still needs the native $FFFF
    // terminator so the cartridge-backed loader cannot wander into zero-filled ROM.
    int[] unusedLabelAddresses =
    [
        0x810000 | FileSelectTilemaps.SamusData,
        0x810000 | FileSelectTilemaps.SamusA,
        0x810000 | FileSelectTilemaps.SamusB,
        0x810000 | FileSelectTilemaps.SamusC,
        0x810000 | FileSelectTilemaps.Exit,
        0x810000 | FileSelectTilemaps.DataCopyMode,
        0x810000 | FileSelectTilemaps.DataClearMode,
        0x810000 | FileSelectTilemaps.CopyWhichData,
        0x810000 | FileSelectTilemaps.CopySamusToWhere,
        0x810000 | FileSelectTilemaps.CopySamusToSamus,
        0x810000 | FileSelectTilemaps.IsThisOkay,
        0x810000 | FileSelectTilemaps.Yes,
        0x810000 | FileSelectTilemaps.No,
        0x810000 | FileSelectTilemaps.CopyCompleted,
        0x810000 | FileSelectTilemaps.ClearWhichData,
        0x810000 | FileSelectTilemaps.ClearSamus,
        0x810000 | FileSelectTilemaps.DataCleared,
    ];
    foreach (int address in unusedLabelAddresses)
        WriteRomWord(rom, address, 0xffff);
    // These two visible main-menu entries were the reported omission. Give each stream a
    // unique first tile so the fixture asserts ROM-authored rendering rather than merely
    // checking that selection index three/four became reachable.
    WriteRomWord(rom, 0x810000 | FileSelectTilemaps.DataCopy, 0x20c0);
    WriteRomWord(rom, 0x810000 | (FileSelectTilemaps.DataCopy + 2), 0xffff);
    WriteRomWord(rom, 0x810000 | FileSelectTilemaps.DataClear, 0x20c1);
    WriteRomWord(rom, 0x810000 | (FileSelectTilemaps.DataClear + 2), 0xffff);

    // The retail menu always loads the static TIME caption for each slot. Only the adjacent
    // HH:MM digits are conditional on a valid save. Reproduce the exact `$81:B4A0` stream so
    // an empty-slot fixture verifies that important distinction instead of hiding it.
    ushort[] timeWords = [0x20ad, 0x20ae, 0x20af, 0xffff];
    for (int index = 0; index < timeWords.Length; index++)
        WriteRomWord(rom, 0x810000 | (FileSelectTilemaps.Time + index * 2), timeWords[index]);
    ushort[] energyWords = [0x209d, 0x209e, 0x209f, 0x20cc, 0xffff];
    for (int index = 0; index < energyWords.Length; index++)
        WriteRomWord(rom, 0x810000 | (FileSelectTilemaps.Energy + index * 2), energyWords[index]);
    WriteRomWord(rom, 0x810000 | FileSelectTilemaps.TimeColon, 0x208c);
    WriteRomWord(rom, 0x810000 | (FileSelectTilemaps.TimeColon + 2), 0xffff);

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
        WriteRomWord(rom, 0x810000 | (FileSelectTilemaps.NoData + index * 2), noDataWords[index]);

    // Give PackMapToSave a minimal but nontrivial native table: two Crateria bytes at
    // sparse unpacked offsets become compressed bytes $10/$11. Areas one through five
    // retain zero counts in this focused fixture. This proves SRAM persists the packed
    // representation and ReadSlot reconstructs the seven-plane domain representation.
    rom[SuperMetroidAddressSpace.ToRomOffset(0x818131)] = 2;
    WriteRomWord(rom, 0x818138, 0x0010);
    WriteRomWord(rom, 0x8182d6, 0x8400);
    rom[SuperMetroidAddressSpace.ToRomOffset(0x818400)] = 0x00;
    rom[SuperMetroidAddressSpace.ToRomOffset(0x818401)] = 0x84;

    var exploredMap = new byte[
        Bank80SystemState.ExploredMapAreaCount *
        Bank80SystemState.ExploredMapBytesPerArea];
    exploredMap[0x00] = 0x80;
    exploredMap[0x84] = 0x04;
    var usedSaveStations = new byte[Bank80SystemState.UsedSaveStationByteCount];
    usedSaveStations[0] = 0x01;
    var mapStations = new byte[Bank80SystemState.MapStationByteCount];
    mapStations[0] = 0xff;

    var addressSpace = new SuperMetroidAddressSpace(rom);
    var menu = new FileSelectMenuState(addressSpace);
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
    // the menu's subsequent ENERGY + HH:MM path against one shared physical SRAM image.
    var saveRam = new SuperMetroidSaveRam(addressSpace);
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
    AssertEqual(0x80, saved.ExploredMapBytes[0x00], "unpacked explored-map byte zero");
    AssertEqual(0x04, saved.ExploredMapBytes[0x84], "unpacked explored-map sparse byte");

    var savedMenu = new FileSelectMenuState(addressSpace);
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
    var copyMenu = new FileSelectMenuState(addressSpace);
    AdvanceFileSelectToMain(copyMenu);
    PulseFileSelect(copyMenu, SnesButton.Down);
    PulseFileSelect(copyMenu, SnesButton.Down);
    PulseFileSelect(copyMenu, SnesButton.Down);
    AssertEqual(3, copyMenu.SelectedItem, "file-select reaches DATA COPY");
    PulseFileSelect(copyMenu, SnesButton.A);
    AdvanceFileSelectFade(copyMenu, FileSelectPhase.CopySelectSource);
    PulseFileSelect(copyMenu, SnesButton.A);
    AssertEqual(FileSelectPhase.CopySelectDestination, copyMenu.Phase,
        "copy chooses nonempty source slot");
    PulseFileSelect(copyMenu, SnesButton.A);
    AssertEqual(FileSelectPhase.CopyConfirm, copyMenu.Phase,
        "copy chooses distinct destination slot");
    copyMenu.Step(0);
    copyMenu.Step((ushort)SnesButton.A);
    AssertTrue(copyMenu.SaveRamChangedThisFrame, "copy publishes SRAM mutation frame");
    AssertEqual(FileSelectPhase.CopyCompleted, copyMenu.Phase, "copy reaches completion screen");
    SuperMetroidSaveSlot copied = saveRam.ReadSlot(1)
        ?? throw new InvalidOperationException("File-select COPY did not create slot B.");
    AssertEqual(saved.Health, copied.Health, "file-select COPY preserves health");
    AssertEqual(saved.GameTimeMinutes, copied.GameTimeMinutes, "file-select COPY preserves time");

    // Re-enter the main screen from the copied SRAM image and clear slot A. This exercises
    // the actual confirmation default and the native four-directory invalidation, not a
    // host-only hidden flag.
    var clearMenu = new FileSelectMenuState(addressSpace);
    AdvanceFileSelectToMain(clearMenu);
    for (int move = 0; move < 4; move++)
        PulseFileSelect(clearMenu, SnesButton.Down);
    AssertEqual(4, clearMenu.SelectedItem, "file-select reaches DATA CLEAR");
    PulseFileSelect(clearMenu, SnesButton.A);
    AdvanceFileSelectFade(clearMenu, FileSelectPhase.ClearSelectSlot);
    PulseFileSelect(clearMenu, SnesButton.A);
    AssertEqual(FileSelectPhase.ClearConfirm, clearMenu.Phase,
        "clear chooses first nonempty slot");
    clearMenu.Step(0);
    clearMenu.Step((ushort)SnesButton.A);
    AssertTrue(clearMenu.SaveRamChangedThisFrame, "clear publishes SRAM mutation frame");
    AssertEqual(FileSelectPhase.ClearCompleted, clearMenu.Phase,
        "clear reaches DATA CLEARED screen");
    AssertTrue(saveRam.ReadSlot(0) is null, "file-select CLEAR invalidates slot A checksums");
    AssertTrue(saveRam.ReadSlot(1) is not null, "file-select CLEAR preserves other slots");

    for (int frame = 0; frame < 15; frame++)
        savedMenu.Step(0);
    savedMenu.Step((ushort)SnesButton.A);
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
    addressSpace.SaveRam[0x0010 + 0x20] ^= 1;
    AssertTrue(saveRam.ReadSlot(0) is null, "SRAM payload corruption invalidates both directories");

    Console.WriteLine("  SRAM/file select: slots, COPY/CLEAR, checksums, NO DATA, ENERGY, and TIME agree.");
}

static void AdvanceFileSelectToMain(FileSelectMenuState menu)
{
    for (int frame = 0; frame < 15; frame++)
        menu.Step(0);
    AssertEqual(FileSelectPhase.Main, menu.Phase, "file-select fade reaches main menu");
}

static void AdvanceFileSelectFade(FileSelectMenuState menu, FileSelectPhase expected)
{
    for (int guard = 0; menu.Phase != expected && guard < 40; guard++)
        menu.Step(0);
    AssertEqual(expected, menu.Phase, "file-select data-management fade completes");
}

static void PulseFileSelect(FileSelectMenuState menu, SnesButton button)
{
    menu.Step((ushort)button);
    menu.Step(0);
}

static void VerifySavedGameLoadAppearance()
{
    string romPath = Path.GetFullPath("Super Metroid.smc");
    if (!File.Exists(romPath))
    {
        Console.WriteLine("  Saved-game appearance: retail-ROM test skipped (ROM not present).");
        return;
    }

    var bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
    var saveRam = new SuperMetroidSaveRam(bus);
    saveRam.SaveSlot(0, new SuperMetroidSaveSnapshot
    {
        Health = 99,
        MaxHealth = 99,
        Area = 0,
        SaveStation = 1,
    });
    SuperMetroidSaveSlot slot = saveRam.ReadSlot(0)
        ?? throw new InvalidOperationException("Synthetic Crateria save did not validate.");

    var runtime = new SuperMetroidRuntime(bus);
    runtime.InitializeHud(new HudSnapshot(
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
    runtime.RunNmi(0, mainLoopRequestedNmi: true);
    runtime.InitializeSavedGame(slot);

    AssertTrue(runtime.SamusLoadAppearanceActive,
        "saved-game load enters command-nine appearance handler");
    AssertEqual(0x0168, runtime.SamusLoadAppearanceFramesRemaining,
        "saved-game appearance uses native 360-frame fanfare lifetime");
    AssertTrue(runtime.Samus is { InputLocked: true } &&
        SamusState.IsForwardFacingPose(runtime.Samus.Pose),
        "saved-game appearance begins locked and front-facing");
    AssertTrue(runtime.Plms.Stations.Where(station => station.Kind == StationKind.Save)
        .All(station => station.SaveStationLockedOut),
        "loading onto a save room sets the native room-entry lockout");

    for (int frame = 0; frame < 0x0167; frame++)
        runtime.StepFrame(0);
    AssertTrue(runtime.SamusLoadAppearanceActive,
        "saved-game appearance remains active through frame 359");
    runtime.StepFrame(0);
    AssertTrue(!runtime.SamusLoadAppearanceActive &&
        runtime.Samus is { InputLocked: false },
        "saved-game appearance restores ordinary input on call 360");
    AssertTrue(!runtime.MessageBox.IsActive,
        "save pod cannot immediately reopen after a load in the same room entry");

    Console.WriteLine(
        "  Saved-game appearance: front pose, ROM palette FX, 360-frame lifetime, and save lockout agree.");
}

static void VerifyIntroGameplayFlashbackVerticalScroll()
{
    // `$8B:A66F` installs BG1VOFS eight for the illustrated page. The two following
    // gameplay-style setup functions replace BG1SC but leave that scroll word untouched.
    // Lock the inherited value independently of any host-authored actor coordinates.
    AssertEqual(8, IntroCinematicState.GameplayFlashbackBg1VerticalScroll,
        "intro gameplay flashback inherited BG1 vertical scroll");

    var motherBrain = new IntroMotherBrainSpriteState();
    AssertEqual(8, motherBrain.BackgroundVerticalScroll,
        "intro Mother Brain starts from inherited BG1 vertical scroll");

    // Four impacts select `$8B:B80F`. Its `$8B:B877` shake adds four on an even frame and
    // removes four on an odd frame, oscillating around eight rather than around zero.
    for (int hit = 0; hit < 4; hit++)
        motherBrain.RegisterMissileHit();
    var cgram = new SnesCgram();
    var introPalette = new ushort[SnesCgram.ColorCount];
    motherBrain.RunPreInstruction(cgram, introPalette, cinematicFrameCounter: 2, introCrossfadeTimer: 0x7f);
    AssertEqual(12, motherBrain.BackgroundVerticalScroll,
        "intro Mother Brain even-frame shake adds four to inherited scroll");
    motherBrain.RunPreInstruction(cgram, introPalette, cinematicFrameCounter: 3, introCrossfadeTimer: 0x7f);
    AssertEqual(8, motherBrain.BackgroundVerticalScroll,
        "intro Mother Brain odd-frame shake returns to inherited scroll");

    Console.WriteLine("  Intro: Mother Brain and SR388 BG1 retain the native eight-pixel vertical scroll.");
}

static void VerifyCinematicPaletteFader()
{
    var target = new ushort[SnesCgram.ColorCount];
    // Component four produces step $0020, making the fixed-point rounding boundary easy
    // to see: seven calls compose to zero and the eighth composes to component one.
    target[20] = 0x1084;
    var cgram = new SnesCgram();
    var fader = new CinematicPaletteFader(target);

    // $8B:B018 clears incoming range $28/$03. Because X is a byte offset, the first
    // affected entry is color $14 rather than color $28.
    fader.Clear(0x0028, 3);
    fader.ComposeInto(cgram);
    AssertEqual(0x0000, cgram.Colors[20], "cinematic clear uses byte-offset color index");

    // One retail step adds component four << 3 = $0020. Compose takes the high byte,
    // so the first seven calls remain black and the eighth produces component one.
    for (int step = 0; step < 7; step++)
        fader.FadeIn(0x0028, 1);
    fader.ComposeInto(cgram);
    AssertEqual(0x0000, cgram.Colors[20], "8.8 cinematic fade preserves early rounding");
    fader.FadeIn(0x0028, 1);
    fader.ComposeInto(cgram);
    AssertEqual(0x0421, cgram.Colors[20], "eighth fade step reaches BGR component one");

    for (int step = 8; step < 32; step++)
        fader.FadeIn(0x0028, 1);
    fader.ComposeInto(cgram);
    AssertEqual(0x1084, cgram.Colors[20], "32 fade-in calls reach exact target");

    for (int step = 0; step < 32; step++)
        fader.FadeOut(0x0028, 1);
    fader.ComposeInto(cgram);
    AssertEqual(0x0000, cgram.Colors[20], "32 fade-out calls reach exact black");
}

static void VerifyDemoInputObject()
{
    var rom = new byte[SuperMetroidAddressSpace.RetailRomByteCount];
    int objectAddress = DemoInputRomData.BankBase | DemoInputRomData.IntroMotherBrain.Object;
    WriteRomWord(rom, objectAddress, DemoInputRomData.Routines.NoOp); // Initializer: RTS.
    WriteRomWord(rom, objectAddress + 2, DemoInputRomData.Routines.NoOp); // Pre-instruction: RTS.
    WriteRomWord(
        rom,
        objectAddress + 4,
        DemoInputRomData.IntroMotherBrain.InputList);

    // Exact six records at $91:8694. The two one-frame X edges are separated by held-X
    // records; normal projectile cooldown/motion code—not this fixture—decides each shot.
    ushort[] words =
    [
        0x005a, 0x0000, 0x0000,
        0x0001, (ushort)SnesButton.X, (ushort)SnesButton.X,
        0x0028, (ushort)SnesButton.X, 0x0000,
        0x0001, (ushort)SnesButton.X, (ushort)SnesButton.X,
        0x001d, (ushort)SnesButton.X, 0x0000,
        0x0046, 0x0000, 0x0000,
    ];
    for (int index = 0; index < words.Length; index++)
        WriteRomWord(
            rom,
            (DemoInputRomData.BankBase | DemoInputRomData.IntroMotherBrain.InputList) +
                index * sizeof(ushort),
            words[index]);

    var demo = new DemoInputState();
    var bus = new SuperMetroidAddressSpace(rom);
    demo.Clear();
    demo.Enable();
    demo.LoadObject(bus, DemoInputRomData.IntroMotherBrain.Object);

    int[] boundaries = [90, 1, 40, 1, 29, 70];
    ushort[] held =
        [0, (ushort)SnesButton.X, (ushort)SnesButton.X, (ushort)SnesButton.X,
            (ushort)SnesButton.X, 0];
    ushort[] newlyPressed =
        [0, (ushort)SnesButton.X, 0, (ushort)SnesButton.X, 0, 0];
    int elapsed = 0;
    for (int record = 0; record < boundaries.Length; record++)
    {
        for (int frame = 0; frame < boundaries[record]; frame++)
        {
            demo.Step(bus);
            AssertEqual(held[record], demo.Held, $"demo record {record} held frame {frame}");
            AssertEqual(newlyPressed[record], demo.NewlyPressed,
                $"demo record {record} new frame {frame}");
            elapsed++;
        }
    }

    AssertEqual(231, elapsed, "old Mother Brain demo-input duration");
    AssertEqual(DemoInputRomData.IntroMotherBrain.NextRecord, demo.InstructionPointer,
        "old Mother Brain next record pointer");
    // Loading a record and publishing its first visible frame happen in the same handler
    // call. Consequently the last of its 70 visible frames leaves timer one; the following
    // call decrements to zero and fetches the next record.
    AssertEqual(1, demo.InstructionTimer, "final visible record frame retains timer one");

    string retailPath = Path.GetFullPath("Super Metroid.smc");
    if (File.Exists(retailPath))
    {
        var retailDemo = new DemoInputState();
        var retailBus = new SuperMetroidAddressSpace(File.ReadAllBytes(retailPath));
        retailDemo.Enable();
        retailDemo.LoadObject(retailBus, DemoInputRomData.IntroMotherBrain.Object);
        for (int frame = 0; frame < elapsed; frame++)
            retailDemo.Step(retailBus);
        AssertEqual(demo.InstructionPointer, retailDemo.InstructionPointer,
            "retail old-Mother-Brain stream reaches catalogued record boundary");
        AssertEqual(demo.Held, retailDemo.Held,
            "retail old-Mother-Brain stream publishes catalogued held input");
    }

    // Exercise the generic control instructions separately: set a loop count, consume the
    // decrement/goto twice, then delete. This verifies that the reusable interpreter is not
    // accidentally hard-coded to the intro's all-record list.
    ushort fixtureHeld = (ushort)(
        SnesButton.Start | SnesButton.Right | SnesButton.L | SnesButton.R);
    WriteRomWord(rom, 0x918700, DemoInputRomData.Instructions.SetTimer);
    WriteRomWord(rom, 0x918702, 0x0002);
    WriteRomWord(rom, 0x918704, 0x0001);
    WriteRomWord(rom, 0x918706, fixtureHeld);
    WriteRomWord(rom, 0x918708, 0x0020);
    WriteRomWord(rom, 0x91870a, DemoInputRomData.Instructions.DecrementTimerAndGoto);
    WriteRomWord(rom, 0x91870c, 0x8704);
    WriteRomWord(rom, 0x91870e, DemoInputRomData.Instructions.Delete);
    WriteRomWord(rom, 0x918720, DemoInputRomData.Routines.NoOp);
    WriteRomWord(rom, 0x918722, DemoInputRomData.Routines.NoOp);
    WriteRomWord(rom, 0x918724, 0x8700);

    bus = new SuperMetroidAddressSpace(rom);
    demo.Clear();
    demo.Enable();
    demo.LoadObject(bus, 0x8720);
    demo.Step(bus);
    AssertEqual(fixtureHeld, demo.Held, "demo opcode fixture first held word");
    AssertEqual(2, demo.Timer, "demo set-timer opcode");
    demo.Step(bus);
    AssertEqual(1, demo.Timer, "demo decrement/goto loops while nonzero");
    AssertEqual(fixtureHeld, demo.Held, "demo loop replays input record");
    demo.Step(bus);
    AssertEqual(0, demo.Timer, "demo decrement/goto falls through at zero");
    AssertEqual(0, demo.InstructionPointer, "demo delete clears list pointer");
    AssertEqual(0, demo.Held, "demo delete clears held input");
    AssertEqual(0, demo.NewlyPressed, "demo delete clears new input");

    // Object-specific routine pointers must be acknowledged explicitly and return the exact
    // bytecode cursor after their operands. Model $8739's no-argument disable followed by
    // the shared delete opcode, just as the physical Mother Brain list ends in the ROM.
    WriteRomWord(rom, 0x918730, DemoInputRomData.Routines.NoOp);
    WriteRomWord(rom, 0x918732, DemoInputRomData.Routines.NoOp);
    WriteRomWord(rom, 0x918734, 0x8750);
    WriteRomWord(rom, 0x918750, 0x8739);
    WriteRomWord(rom, 0x918752, DemoInputRomData.Instructions.Delete);
    bus = new SuperMetroidAddressSpace(rom);
    demo.Clear();
    demo.Enable();
    demo.LoadObject(bus, 0x8730);
    int specialInstructionCalls = 0;
    demo.Step(bus, specialInstruction: (state, instruction, argumentPointer) =>
    {
        AssertEqual(0x8739, instruction, "demo special instruction pointer");
        specialInstructionCalls++;
        state.Disable();
        return DemoInputInstructionResult.ContinueAt(argumentPointer);
    });
    AssertEqual(1, specialInstructionCalls, "demo special instruction callback count");
    AssertEqual(false, demo.Enabled, "demo special instruction disable");
    AssertEqual(0, demo.InstructionPointer, "demo special instruction continues into delete");

    Console.WriteLine("  Demo input: records, edges, shared opcodes, special dispatch, and deletion agree.");
}

static void WriteRomWord(byte[] rom, int snesAddress, ushort value)
{
    int offset = SuperMetroidAddressSpace.ToRomOffset(snesAddress);
    rom[offset] = unchecked((byte)value);
    rom[offset + 1] = unchecked((byte)(value >> 8));
}
}
