using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.TestSupport.VerificationOracles;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Hardware.ImportedSpritemapsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Enemies.Corpses;
using SuperMetroid.Core;
using SuperMetroid.Core.Rendering;
using SuperMetroid.AssetExtraction.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Enemies.Artwork;

namespace SuperMetroid.Verification.Enemies.Corpses;

/// <summary>Verifies installed Dead Torizo sheet, sand, spritemap, and VRAM transfer behavior against retail data.</summary>
internal static class DeadTorizoArtworkVerification
{
    /// <summary>Checks installed corpse staging and native OAM.</summary>
    internal static void VerifyInstalledDeadTorizoArtwork(ImportEnemyArtwork stock)
    {
        Suite(nameof(VerifyDeadTorizoRowUploads), VerifyDeadTorizoRowUploads);
        Suite(nameof(VerifyDeadTorizoStationaryVisual), () => VerifyDeadTorizoStationaryVisual(stock.Enemies));
        // The character rot oracle stages the corpse as its initializer does, from discontinuous portions of the ordinary ED3F sheet,
        // and drops sand words from that same sheet, to picture each step of its rot (#1392); the runtime stages nothing.
        var nativeBus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        var nativeStaging = new NativeEnemyStaging();
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
            .TryGetDisplay(DeadTorizoArtworkDefinitions.SandHeapFrame, out EnemySpritemapParts hook), "Dead Torizo hook has an installed composition");
        var installedCorpse = new OamBuffer();
        installedCorpse.AddEnemySpritemap(hook, new Position16<SpritePixels>(296, 187), 0, 0);
        AssertTrue(nativeCorpse.ImageEquals(installedCorpse) &&
                   nativeCorpse.NextByteOffset == installedCorpse.NextByteOffset,
            "Dead Torizo private corpse hook's installed composition is the native OAM");

        var missing = new ImportEnemyArtwork(EnemyTileSourceArtwork.FromArtworkForVerification(new EnemyTileArtworkParts { Sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>(), Palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>() }), new BossSourceArtwork(), new AreaSourceArtwork());
        AssertThrows<InvalidDataException>(() => StageDeadTorizoArtwork(missing),
            "bound installation missing the dead-Torizo sheet fails instead of reading ROM");

        Console.WriteLine("  Dead Torizo artwork: guarded initial corpse and sand-line ROM parity pass.");
    }

    /// <summary>Matches the installed stationary frame's 25 OAM parts against the native spritemap.</summary>
    private static void VerifyDeadTorizoStationaryVisual(
        EnemyTileSourceArtwork stock)
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
        installedOam.AddEnemySpritemap(installedParts, new Position16<SpritePixels>(128, 128), 0, 0);
        AssertTrue(nativeOam.ImageEquals(installedOam) &&
                   nativeOam.NextByteOffset == installedOam.NextByteOffset,
            "Dead Torizo stationary installed OAM matches all native sprite entries");
        AssertEqual(25 * 4, installedOam.NextByteOffset,
            "Dead Torizo stationary map has 25 native OAM parts");
        Console.WriteLine("  Dead Torizo stationary visual: compiled selector, editable 25-part OAM, and native composition parity pass.");
    }

    /// <summary>
    /// Compares the extraction's two row upload tables, which it finds the corpse's characters by, with the retail
    /// descriptors (#1406, #1539).
    /// </summary>
    private static void VerifyDeadTorizoRowUploads()
    {
        // DeadTorizo_VRAMWriteTable_Even at $A9:D549 and DeadTorizo_VRAMWriteTable_Odd at $A9:D583, four words per record.
        const int recordByteCount = 8;
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        for (ushort phase = 0; phase < 2; phase++)
        {
            ushort table = phase == 0 ? (ushort)0xd549 : (ushort)0xd583;
            DeadTorizoRowUploadDefinitions.PhaseRows records =
                DeadTorizoRowUploadDefinitions.ForHalf(odd: phase == 1);
            AssertEqual(7, records.Count,
                $"Dead Torizo phase {phase} compiled descriptor count");
            for (int index = 0; index < records.Count; index++)
            {
                int address = 0xa90000 | unchecked((ushort)(table + index * recordByteCount));
                CorpseRowUpload record = records[index];
                AssertEqual(record.SizeInBytes, rom.ReadCartridgeWord(address),
                    $"Dead Torizo phase {phase} transfer {index} size");
                AssertEqual(CorpseRowSources.BankWord, rom.ReadCartridgeWord((address + 2)),
                    $"Dead Torizo phase {phase} transfer {index} source bank");
                AssertEqual(CorpseRowSources.OfTorizoHalf(phase == 1, index), rom.ReadCartridgeWord((address + 4)),
                    $"Dead Torizo phase {phase} transfer {index} source offset");
                AssertEqual(record.DestinationWord, rom.ReadCartridgeWord((address + 6)),
                    $"Dead Torizo phase {phase} transfer {index} VRAM destination");
            }
            AssertEqual((ushort)0, rom.ReadCartridgeWord((0xa90000 | unchecked((ushort)(table + records.Count * recordByteCount)))),
                $"Dead Torizo phase {phase} native zero terminator");
        }
        Console.WriteLine("  Dead Torizo row uploads: fourteen native descriptors match.");
    }

    /// <summary>
    /// Independently transcribes <c>Torizo_CorpseRottingInitFunc</c> at $A9:DE18 from pinned
    /// <c>sm_a9.c</c>; expected copies must remain independent of runtime definitions.
    /// </summary>
    private static void ReferenceDeadTorizoCorpseGraphics(CartridgeImportAddressSpace bus, NativeEnemyStaging staging)
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
    private static void ReferenceDeadTorizoSandLine(CartridgeImportAddressSpace bus, NativeEnemyStaging staging, ushort line)
    {
        (int destination, int source) = ReadNativeDeadTorizoSandLineOffsets(bus, line);
        StagingByteView sand = staging.DeadTorizoSand;
        for (int row = 0; row < 18; row++)
            for (int part = 0; part < 2; part++)
                sand[destination + row * 16 + part] = ReadNativeDeadTorizoSheetByte(bus, source + row * 16 + part);
    }

    /// <summary>Reads one oracle byte from the corpse sheet at $B7:A800.</summary>
    private static byte ReadNativeDeadTorizoSheetByte(ISnesAddressSpace rom, int sheetOffset) =>
        rom.ReadCartridgeByte(0xb7a800 + sheetOffset);

    /// <summary>Obtains a sand line's WRAM destination and sheet source from separate retail tables.</summary>
    private static ReadNativeDeadTorizoSandLineOffsetsResult ReadNativeDeadTorizoSandLineOffsets(ISnesAddressSpace rom, ushort line)
    {
        return new ReadNativeDeadTorizoSandLineOffsetsResult(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa9d67c + line * 2)), CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa9d69c + line * 2)));
    }

    /// <summary>The corpse as the character rot oracle stages it from installed art.</summary>
    private static CorpseStaging StageDeadTorizoArtwork(ImportEnemyArtwork artwork) => CorpseStaging.Torizo(Atlas(artwork));

    /// <summary>The installed dead-Torizo sheet.</summary>
    private static ReadOnlySpan<byte> Atlas(ImportEnemyArtwork artwork) =>
        artwork.Enemies.TilesOf(CorpseSheets.DeadTorizoSheet) is { Length: CorpseSheets.DeadTorizoSheetByteCount } tiles
            ? tiles.Span : throw new InvalidDataException("Installed dead-Torizo sheet enemy-ed3f-tiles.png has the wrong size.");

    /// <summary>The <paramref name="length"/> staged corpse bytes from <paramref name="offset"/>.</summary>
    private static byte[] Staged(CorpseStaging staging, int offset, int length) =>
        staging.Upload(new CorpseRotRow(0, CorpseWorkMemory.EnemyBg2Staging + offset, length, 0));

    /// <summary>The first <paramref name="length"/> bytes of the staged sand strips.</summary>
    private static byte[] Sand(CorpseStaging staging, int length) =>
        staging.Upload(new CorpseRotRow(0, CorpseWorkMemory.CorpseRottingHeap + CorpseWorkMemory.DeadTorizoSandOffset, length, 0));

    /// <summary>Compares each initialized corpse or sand staging byte with the independent retail copy.</summary>
    private static void AssertDeadTorizoBufferParity(
        StagingByteView expected, byte[] actual, int length,
        string description)
    {
        for (int offset = 0; offset < length; offset++)
            AssertEqual(expected[offset], actual[offset], description);
    }

    /// <summary>The corpse hook's installed composition as OAM, where the hook draws it with the camera at the room's origin.</summary>
    internal static OamBuffer DrawDeadTorizoHookComposition(ImportEnemyArtwork artwork)
    {
        if (artwork.Enemies.Spritemaps?.TryGetDisplay(DeadTorizoArtworkDefinitions.SandHeapFrame, out EnemySpritemapParts hook) != true)
            throw new InvalidDataException("The dead Torizo hook has no installed composition.");
        var oam = new OamBuffer();
        oam.AddEnemySpritemap(hook, new Position16<SpritePixels>(296, 187), 0, 0);
        return oam;
    }
}
