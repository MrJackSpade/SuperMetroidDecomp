using SuperMetroid.Core.Areas;
using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Hardware.ImportedSpritemapsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Enemies.Corpses;
using SuperMetroid.Core;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.VerificationOracles;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Enemies.Corpses;

/// <summary>Verifies installed Dead Torizo sheet, sand, spritemap, and VRAM transfer behavior against retail data.</summary>
internal static class DeadTorizoArtworkVerification
{
    /// <summary>Checks installed corpse staging, native OAM, and the visible effect of an edited sheet pixel.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    internal static void VerifyInstalledDeadTorizoArtwork(
        string directory, InstalledEnemyArtwork stock)
    {
        Suite(nameof(VerifyDeadTorizoVramTransferDefinitions), () => VerifyDeadTorizoVramTransferDefinitions(stock));
        Suite(nameof(VerifyDeadTorizoStationaryVisual), () => VerifyDeadTorizoStationaryVisual(stock.Enemies));
        // The extraction stages the corpse as its initializer does, from discontinuous portions of the ordinary ED3F sheet,
        // and drops sand words from that same sheet, to picture each step of its rot (#1392); the runtime stages nothing.
        var nativeBus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        var nativeStaging = new EnemyStagingSurfaces();
        ReferenceDeadTorizoCorpseGraphics(nativeBus, nativeStaging);
        CorpseStaging installed = StageDeadTorizoArtwork(stock);
        AssertDeadTorizoBufferParity(nativeStaging.CorpseTiles, Staged(installed, 0, 0x1000), 0x1000,
            "installed dead-Torizo initial corpse sheet matches the cartridge");

        ReferenceDeadTorizoSandLine(nativeBus, nativeStaging, 0);
        installed.CopySandLine(Atlas(stock), 0);
        AssertDeadTorizoBufferParity(nativeStaging.DeadTorizoSand, Sand(installed, 0x200), 0x200,
            "installed dead-Torizo sand line matches the cartridge");

        var nativeCorpse = new OamBuffer();
        // $A9:D39A draws the fixed corpse map at world (296, 187).
        DrawImportedEnemySpritemap(nativeBus, nativeCorpse, new NativeEnemySpritemapDraw { Bank = 0xa9, Pointer = 0xd761, Origin = new(296, 187), PaletteBits = 0, BaseTileIndex = 0 });
        AssertTrue((stock.Enemies.Spritemaps ?? throw new InvalidOperationException("Verification requires stock spritemaps."))
            .TryGetDisplay(DeadTorizoArtworkDefinitions.HookFrame, out EnemySpritemapParts hook), "Dead Torizo hook has an installed composition");
        var installedCorpse = new OamBuffer();
        installedCorpse.AddEnemySpritemap(hook, new Position16<OamPixels>(296, 187), 0, 0);
        AssertTrue(nativeCorpse.ImageEquals(installedCorpse) &&
                   nativeCorpse.NextByteOffset == installedCorpse.NextByteOffset,
            "Dead Torizo private corpse hook's installed composition is the native OAM");
        AssertEqual(0, DrawDeadTorizoCorpseFrame(stock, new(0, 188)).NextByteOffset,
            "Dead Torizo private hook retains its above-screen cull");

        var missing = new InstalledEnemyArtwork(EnemyTileArtworkCatalog.FromArtworkForVerification(new EnemyTileArtworkParts { Sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>(), Palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>() }), new BossArtworkCatalog(), new AreaArtworkCatalog());
        AssertThrows<InvalidDataException>(() => StageDeadTorizoArtwork(missing),
            "bound installation missing the dead-Torizo sheet fails instead of reading ROM");

        string fileName = EnemyTileArtworkFormat.FileName(EnemyDefinitionId.CorpseTorizo);
        using var source = new MemoryStream(File.ReadAllBytes(Path.Combine(directory, fileName)));
        int tileCount = RoomCharacterAtlasFormat.ValidateTileCount(0x1800);
        int columns = Math.Min(RoomCharacterAtlasFormat.TileColumns, tileCount);
        int rows = (tileCount + columns - 1) / columns;
        IndexedPngImage image = IndexedPng.Read(source, new(checked((ushort)(columns * 8)), checked((ushort)(rows * 8))));
        // Source offset $0120 is tile 9, first row, first pixel. It is copied
        // to $7E:2060, so flipping its low indexed bit flips that planar MSB.
        image.Pixels[9 * 8] ^= 1;
        string overrideDirectory = Path.Combine(directory, "dead-torizo-overrides");
        Directory.CreateDirectory(overrideDirectory);
        using (var output = File.Create(Path.Combine(overrideDirectory, fileName)))
            IndexedPng.Write(output, new(image.Size.X, image.Size.Y), image.Pixels, image.Palette);
        byte[] installedTiles = Staged(installed, 0, 0x1000);
        byte[] editedTiles = Staged(StageDeadTorizoArtwork(EnemyTileArtworkFiles.Load(directory, overrideDirectory)), 0, 0x1000);
        AssertEqual((byte)(installedTiles[0x60] ^ 0x80),
            editedTiles[0x60],
            "dead-Torizo PNG pixel edit reaches the staged corpse");
        for (int offset = 0; offset < 0x1000; offset++)
        {
            if (offset == 0x60)
                continue;
            AssertEqual(installedTiles[offset],
                editedTiles[offset],
                "dead-Torizo PNG edit leaves neighboring corpse staging bytes unchanged");
        }
        AssertEqual(editedTiles[0x60], Staged(StageDeadTorizoArtwork(EnemyTileArtworkFiles.Load(directory, overrideDirectory)), 0x60, 1)[0],
            "dead-Torizo PNG edit survives catalog reload");

        Console.WriteLine("  Dead Torizo artwork: guarded initial corpse and sand-line ROM parity, visible PNG edit and reload pass.");
    }

    /// <summary>Matches the installed stationary frame's 25 OAM parts against the native spritemap.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for Dead Torizo stationary visual")]
    private static void VerifyDeadTorizoStationaryVisual(
        EnemyTileArtworkCatalog stock)
    {
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        // The typed oracle ties the pose to its native list; the export must publish the pose the program shows.
        ushort nativePointer = DeadTorizoAnimationsConstants.Spritemaps(DeadTorizoProgram.Stationary).Single();
        AssertEqual(DeadTorizoArtworkDefinitionsConstants.StationarySpritemap, nativePointer,
            "Dead Torizo stationary export is the pose its program shows");
        AssertTrue((stock.Spritemaps ?? throw new InvalidOperationException("Verification requires stock.Spritemaps.")).TryGetDisplay(
                DeadTorizoArtworkDefinitionsConstants.SpritemapBank, nativePointer,
                out EnemySpritemapParts installedParts),
            "Dead Torizo stationary OAM has editable installed parts");
        var nativeOam = new OamBuffer();
        var installedOam = new OamBuffer();
        DrawImportedEnemySpritemap(rom, nativeOam, new NativeEnemySpritemapDraw { Bank = DeadTorizoArtworkDefinitionsConstants.SpritemapBank, Pointer = nativePointer, Origin = new(128, 128), PaletteBits = 0, BaseTileIndex = 0 });
        installedOam.AddEnemySpritemap(installedParts, new Position16<OamPixels>(128, 128), 0, 0);
        AssertTrue(nativeOam.ImageEquals(installedOam) &&
                   nativeOam.NextByteOffset == installedOam.NextByteOffset,
            "Dead Torizo stationary installed OAM matches all native sprite entries");
        AssertEqual(25 * 4, installedOam.NextByteOffset,
            "Dead Torizo stationary map has 25 native OAM parts");
        Console.WriteLine("  Dead Torizo stationary visual: compiled selector, editable 25-part OAM, and native composition parity pass.");
    }

    /// <summary>Compares both corpse-animation transfer tables and their live queues with retail descriptors.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Dead Torizo VRAM transfer definitions")]
    private static void VerifyDeadTorizoVramTransferDefinitions(
        InstalledEnemyArtwork stock)
    {
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        for (ushort phase = 0; phase < 2; phase++)
        {
            ushort table = phase == 0
                ? DeadTorizoVramTransferDefinitions.EvenTable
                : DeadTorizoVramTransferDefinitions.OddTable;
            DeadTorizoVramTransferDefinitions.PhaseRows records =
                DeadTorizoVramTransferDefinitions.ForHalf(odd: phase == 1);
            AssertEqual(7, records.Length,
                $"Dead Torizo phase {phase} compiled descriptor count");
            for (int index = 0; index < records.Length; index++)
            {
                int address = 0xa90000 | unchecked((ushort)(table +
                    index * DeadTorizoVramTransferDefinitions.RecordByteCount));
                DeadTorizoVramTransferDefinition record = records[index];
                AssertEqual(record.SizeInBytes, rom.ReadCartridgeWord(address),
                    $"Dead Torizo phase {phase} transfer {index} size");
                AssertEqual(record.SourceBankWord, rom.ReadCartridgeWord((address + 2)),
                    $"Dead Torizo phase {phase} transfer {index} source bank");
                AssertEqual(record.SourceOffset, rom.ReadCartridgeWord((address + 4)),
                    $"Dead Torizo phase {phase} transfer {index} source offset");
                AssertEqual(record.EncodedVramDestination, rom.ReadCartridgeWord((address + 6)),
                    $"Dead Torizo phase {phase} transfer {index} VRAM destination");
            }
            AssertEqual((ushort)0, rom.ReadCartridgeWord((0xa90000 | unchecked((ushort)(
                table + records.Length *
                DeadTorizoVramTransferDefinitions.RecordByteCount)))),
                $"Dead Torizo phase {phase} native zero terminator");
        }

        RoomEnemySystem installed = InitializeDeadTorizo(stock);
        var nativeTransfers = new List<VramWriteEntry>();

        Action<DeadTorizoEnemyState> buildInstalled =
            new Action<DeadTorizoEnemyState>(installed.DeadTorizoBehavior.BuildDeadTorizoVramTransfers);
        for (int phase = 0; phase < 2; phase++)
        {
            // $A9:D4CF increments the phase before selecting the raw descriptor table.
            int address = phase == 0 ? 0xa9d583 : 0xa9d549;
            for (int index = 0; index < 7; index++, address += 8)
                nativeTransfers.Add(new VramWriteEntry(rom.ReadCartridgeWord(address),
                    (int)new SnesAddress(SnesLittleEndian.HighByte(rom.ReadCartridgeWord(address + 2)), rom.ReadCartridgeWord(address + 4)),
                    VramDmaDestination.FromNative(rom.ReadCartridgeWord((address + 6)))));
            AssertEqual((ushort)0, rom.ReadCartridgeWord(address), "native corpse queue terminates after seven entries");
            buildInstalled((installed.DeadTorizo ?? throw new InvalidOperationException("Verification requires installed.DeadTorizo.")));
            // The corpse records the rot step each uploaded row shows, where its native descriptor uploads it (#1392).
            foreach (VramWriteEntry transfer in nativeTransfers.Skip(phase * 7))
                AssertTrue(installed.Presentation.RotUploads.TryShowing(
                        new CharacterRegion(ObjCharacterUploads.TableByte(transfer.Destination), transfer.SizeInBytes), out int step) && step == 0,
                    $"Dead Torizo phase {phase} uploads each native descriptor's row");
        }
        Console.WriteLine("  Dead Torizo VRAM uploads: fourteen native descriptors and both real frame builders match.");
    }

    /// <summary>
    /// Independently transcribes <c>Torizo_CorpseRottingInitFunc</c> at $A9:DE18 from pinned
    /// <c>sm_a9.c</c>; expected copies must remain independent of runtime definitions.
    /// </summary>
    private static void ReferenceDeadTorizoCorpseGraphics(SuperMetroidAddressSpace bus, EnemyStagingSurfaces staging)
    {
        StagingByteView tiles = staging.CorpseTiles;
        tiles.Fill(0);
        DeadTorizoArtworkCopy[] copies =
        [
            new DeadTorizoArtworkCopy(288, 0x060, 0xc0), new DeadTorizoArtworkCopy(800, 0x1a0, 0xc0),
            new DeadTorizoArtworkCopy(1280, 0x2c0, 0x100), new DeadTorizoArtworkCopy(1792, 0x400, 0x100),
            new DeadTorizoArtworkCopy(2304, 0x540, 0x100), new DeadTorizoArtworkCopy(2816, 0x680, 0x100),
            new DeadTorizoArtworkCopy(3328, 0x7c0, 0x100), new DeadTorizoArtworkCopy(3840, 0x900, 0x100),
            new DeadTorizoArtworkCopy(4352, 0xa40, 0x100), new DeadTorizoArtworkCopy(4832, 0xb60, 0x120),
            new DeadTorizoArtworkCopy(5312, 0xc80, 0x140), new DeadTorizoArtworkCopy(5824, 0xdc0, 0x140),
        ];
        foreach (var copy in copies)
            for (int offset = 0; offset < copy.Length; offset++)
                tiles[copy.Destination + offset] = ReadNativeDeadTorizoSheetByte(bus, copy.Source + offset);
    }

    /// <summary>Copies eighteen sand words from the sheet using the two native $A9:D5EA offset tables.</summary>
    private static void ReferenceDeadTorizoSandLine(SuperMetroidAddressSpace bus, EnemyStagingSurfaces staging, ushort line)
    {
        (int destination, int source) = ReadNativeDeadTorizoSandLineOffsets(bus, line);
        StagingByteView sand = staging.DeadTorizoSand;
        for (int row = 0; row < 18; row++)
            for (int part = 0; part < 2; part++)
                sand[destination + row * 16 + part] = ReadNativeDeadTorizoSheetByte(bus, source + row * 16 + part);
    }

    /// <summary>Reads one oracle byte from the corpse sheet at $B7:A800.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for the Dead Torizo corpse tile sheet at $B7:A800")]
    private static byte ReadNativeDeadTorizoSheetByte(ISnesAddressSpace rom, int sheetOffset) =>
        rom.ReadCartridgeByte(0xb7a800 + sheetOffset);

    /// <summary>Obtains a sand line's WRAM destination and sheet source from separate retail tables.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for the Dead Torizo sand-line destination and source offset tables")]
    private static ReadNativeDeadTorizoSandLineOffsetsResult ReadNativeDeadTorizoSandLineOffsets(ISnesAddressSpace rom, ushort line)
    {
        return new ReadNativeDeadTorizoSandLineOffsetsResult(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa9d67c + line * 2)), CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa9d69c + line * 2)));
    }

    /// <summary>Initializes the corpse actor with installed artwork.</summary>
    private static RoomEnemySystem InitializeDeadTorizo(InstalledEnemyArtwork artwork)
    {
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance) { TileArtwork = artwork.Enemies, BossArtwork = artwork.Bosses, AreaArtwork = artwork.Areas };
        new Action<RoomEnemySlot>(enemies.DeadTorizoBehavior.InitializeDeadTorizo)(enemies.SlotTable.Slots[0]);
        return enemies;
    }

    /// <summary>The corpse as the extraction stages it from installed art.</summary>
    private static CorpseStaging StageDeadTorizoArtwork(InstalledEnemyArtwork artwork) => CorpseStaging.Torizo(Atlas(artwork));

    /// <summary>The installed dead-Torizo sheet.</summary>
    private static ReadOnlySpan<byte> Atlas(InstalledEnemyArtwork artwork) =>
        artwork.Enemies.TryResolve(DeadTorizoArtworkDefinitions.SourceAddress, DeadTorizoArtworkDefinitions.ByteCount, out ReadOnlyMemory<byte> tiles)
            ? tiles.Span : throw new InvalidDataException("Installed dead-Torizo sheet enemy-ed3f-tiles.png is missing or has the wrong size.");

    /// <summary>The <paramref name="length"/> staged corpse bytes from <paramref name="offset"/>.</summary>
    private static byte[] Staged(CorpseStaging staging, int offset, int length) =>
        staging.Upload(new CorpseRotRow(0, EnemyStagingDefinitions.Bg2NativeAddress + offset, length, 0));

    /// <summary>The first <paramref name="length"/> bytes of the staged sand strips.</summary>
    private static byte[] Sand(CorpseStaging staging, int length) =>
        staging.Upload(new CorpseRotRow(0, EnemyStagingDefinitions.RottingHeapNativeAddress + EnemyStagingDefinitions.DeadTorizoSandOffset, length, 0));

    /// <summary>Renders the corpse hook's published frame as the OAM image it stands for.</summary>
    internal static OamBuffer DrawDeadTorizoCorpseFrame(InstalledEnemyArtwork artwork, Position16<RoomPixels> camera)
    {
        RoomEnemySystem enemies = InitializeDeadTorizo(artwork);
        var oam = new OamBuffer();
        oam.AppendDrawn(frame => enemies.DeadTorizoBehavior.DrawDeadTorizoHook(frame, EnemyFormatDrawAccess.BlankObjectPage, camera));
        return oam;
    }

    /// <summary>Compares each initialized corpse or sand staging byte with the independent retail copy.</summary>
    private static void AssertDeadTorizoBufferParity(
        StagingByteView expected, byte[] actual, int length,
        string description)
    {
        for (int offset = 0; offset < length; offset++)
            AssertEqual(expected[offset], actual[offset], description);
    }

    /// <summary>The corpse hook's installed composition as OAM, where the hook draws it with the camera at the room's origin.</summary>
    internal static OamBuffer DrawDeadTorizoHookComposition(InstalledEnemyArtwork artwork)
    {
        if (artwork.Enemies.Spritemaps?.TryGetDisplay(DeadTorizoArtworkDefinitions.HookFrame, out EnemySpritemapParts hook) != true)
            throw new InvalidDataException("The dead Torizo hook has no installed composition.");
        var oam = new OamBuffer();
        oam.AddEnemySpritemap(hook, new Position16<OamPixels>(296, 187), 0, 0);
        return oam;
    }

}
