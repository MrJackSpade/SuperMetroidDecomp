using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Frontend.FrontendRomFreeVerification;
using static SuperMetroid.Verification.Hardware.ImportedSpritemapsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.CpuBusTestDoubles;

namespace SuperMetroid.Verification.Enemies.Corpses;

/// <summary>Verifies installed Dead Torizo sheet, sand, spritemap, and VRAM transfer behavior against retail data.</summary>
internal static class DeadTorizoArtworkVerification
{
    /// <summary>Checks installed corpse staging, native OAM, and the visible effect of an edited sheet pixel.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    internal static void VerifyInstalledDeadTorizoArtwork(
        string directory, EnemyTileArtworkCatalog stock)
    {
        Suite(nameof(VerifyDeadTorizoVramTransferDefinitions), () => VerifyDeadTorizoVramTransferDefinitions(stock));
        Suite(nameof(VerifyDeadTorizoStationaryVisual), () => VerifyDeadTorizoStationaryVisual(stock));
        // The corpse initializer copies discontinuous portions of the ordinary ED3F
        // enemy sheet to WRAM. Later rotting frames draw sand words from that same
        // sheet. Guard the entire source interval so neither path can accidentally
        // work by reading the cartridge despite a bound installed catalog.
        var nativeBus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        var installedBus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        var nativeStaging = new EnemyStagingSurfaces();
        ReferenceDeadTorizoCorpseGraphics(nativeBus, nativeStaging);
        RoomEnemySystem installed = InitializeDeadTorizoArtwork(installedBus, stock);
        AssertDeadTorizoBufferParity(nativeStaging.CorpseTiles,
            installed.Room.Staging.CorpseTiles, 0x1000,
            "installed dead-Torizo initial corpse sheet matches the cartridge");

        ReferenceDeadTorizoSandLine(nativeBus, nativeStaging, 0);
        CopyDeadTorizoSandLine(installed, 0);
        AssertDeadTorizoBufferParity(nativeStaging.DeadTorizoSand,
            installed.Room.Staging.DeadTorizoSand, 0x200,
            "installed dead-Torizo sand line matches the cartridge");

        var nativeCorpse = new OamBuffer();
        // $A9:D39A draws the fixed corpse map at world (296, 187).
        DrawImportedEnemySpritemap(nativeBus, nativeCorpse, new NativeEnemySpritemapDraw { Bank = 0xa9, Pointer = 0xd761, Origin = new(296, 187), PaletteBits = 0, BaseTileIndex = 0 });
        OamBuffer installedCorpse = DrawDeadTorizoCorpseFrame(stock, installedBus, Position16<RoomPixels>.Zero);
        AssertTrue(nativeCorpse.ImageEquals(installedCorpse) &&
                   nativeCorpse.NextByteOffset == installedCorpse.NextByteOffset,
            "Dead Torizo private corpse hook draws installed native-parity OAM without ROM reads");
        AssertEqual(0, DrawDeadTorizoCorpseFrame(stock, installedBus, new(0, 188)).NextByteOffset,
            "Dead Torizo private hook retains its above-screen cull");

        var missing = EnemyTileArtworkCatalog.FromArtworkForVerification(new EnemyTileArtworkParts { Sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>(), Palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>() });
        AssertThrows<InvalidDataException>(() => InitializeDeadTorizoArtwork(
                SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc"), missing),
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
        EnemyTileArtworkCatalog edited = EnemyTileArtworkFiles.Load(directory, overrideDirectory);
        var editedBus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        RoomEnemySystem editedEnemies = InitializeDeadTorizoArtwork(editedBus, edited);
        StagingByteView installedTiles = installed.Room.Staging.CorpseTiles;
        StagingByteView editedTiles = editedEnemies.Room.Staging.CorpseTiles;
        AssertEqual((byte)(installedTiles[0x60] ^ 0x80),
            editedTiles[0x60],
            "dead-Torizo PNG pixel edit reaches the live corpse staging buffer");
        for (int offset = 0; offset < 0x1000; offset++)
        {
            if (offset == 0x60)
                continue;
            AssertEqual(installedTiles[offset],
                editedTiles[offset],
                "dead-Torizo PNG edit leaves neighboring corpse staging bytes unchanged");
        }
        var reloadedBus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        RoomEnemySystem reloadedEnemies = InitializeDeadTorizoArtwork(reloadedBus,
            EnemyTileArtworkFiles.Load(directory, overrideDirectory));
        AssertEqual(editedTiles[0x60], reloadedEnemies.Room.Staging.CorpseTiles[0x60],
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
        AssertEqual(DeadTorizoArtworkDefinitions.StationarySpritemap, nativePointer,
            "Dead Torizo stationary export is the pose its program shows");
        AssertTrue((stock.Spritemaps ?? throw new InvalidOperationException("Verification requires stock.Spritemaps.")).TryGetDisplay(
                DeadTorizoArtworkDefinitions.SpritemapBank, nativePointer,
                out EnemySpritemapParts installedParts),
            "Dead Torizo stationary OAM has editable installed parts");
        var nativeOam = new OamBuffer();
        var installedOam = new OamBuffer();
        DrawImportedEnemySpritemap(rom, nativeOam, new NativeEnemySpritemapDraw { Bank = DeadTorizoArtworkDefinitions.SpritemapBank, Pointer = nativePointer, Origin = new(128, 128), PaletteBits = 0, BaseTileIndex = 0 });
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
        EnemyTileArtworkCatalog stock)
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

        var installedBus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        RoomEnemySystem installed = InitializeDeadTorizoArtwork(installedBus, stock);
        var nativeTransfers = new List<VramWriteEntry>();
        installed.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);

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
            AssertTrue(nativeTransfers.SequenceEqual(
                    installed.LastDeadTorizoVramTransfers),
                $"Dead Torizo phase {phase} installed queue matches live native descriptors");
            AssertEqual((phase + 1) * 7, installed.LastDeadTorizoVramTransfers.Count,
                $"Dead Torizo phase {phase} emits every authored transfer in order");
        }
        Console.WriteLine("  Dead Torizo VRAM queues: fourteen native descriptors and both real frame builders match with installed ROM reads forbidden.");
    }

    /// <summary>
    /// Independently transcribes <c>Torizo_CorpseRottingInitFunc</c> at $A9:DE18 from pinned
    /// <c>sm_a9.c</c>; expected copies must remain independent of runtime definitions.
    /// </summary>
    private static void ReferenceDeadTorizoCorpseGraphics(SuperMetroidAddressSpace bus, EnemyStagingSurfaces staging)
    {
        StagingByteView tiles = staging.CorpseTiles;
        tiles.Fill(0);
        (int Source, int Destination, int Length)[] copies =
        [
            (288, 0x060, 0xc0), (800, 0x1a0, 0xc0),
            (1280, 0x2c0, 0x100), (1792, 0x400, 0x100),
            (2304, 0x540, 0x100), (2816, 0x680, 0x100),
            (3328, 0x7c0, 0x100), (3840, 0x900, 0x100),
            (4352, 0xa40, 0x100), (4832, 0xb60, 0x120),
            (5312, 0xc80, 0x140), (5824, 0xdc0, 0x140),
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
    private static (int Destination, int Source) ReadNativeDeadTorizoSandLineOffsets(ISnesAddressSpace rom, ushort line)
    {
        return (CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa9d67c + line * 2)), CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa9d69c + line * 2)));
    }

    /// <summary>Initializes the corpse actor with installed artwork while forbidding reads from its ROM sheet.</summary>
    private static RoomEnemySystem InitializeDeadTorizoArtwork(
        SuperMetroidAddressSpace bus, EnemyTileArtworkCatalog artwork)
    {
        var enemies = new RoomEnemySystem(new EnemySession()) { TileArtwork = artwork };
        ISnesAddressSpace source = NewDeadTorizoArtworkReadGuard(bus);
        enemies.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);
        new Action<RoomEnemySlot>(enemies.DeadTorizoBehavior.InitializeDeadTorizo)(enemies.SlotTable.Slots[0]);
        return enemies;
    }

    /// <summary>Invokes the live corpse behavior's sand-line copy for comparison with the independent oracle.</summary>
    private static void CopyDeadTorizoSandLine(RoomEnemySystem enemies, ushort line)
    {
        new Action<int>(enemies.DeadTorizoBehavior.CopyDeadTorizoSandLine)(line);
    }

    /// <summary>Renders the corpse hook into a fresh OAM buffer with its native map reads guarded.</summary>
    internal static OamBuffer DrawDeadTorizoCorpseFrame(
        EnemyTileArtworkCatalog artwork, SuperMetroidAddressSpace bus,
        Position16<RoomPixels> camera)
    {
        RoomEnemySystem enemies = InitializeDeadTorizoArtwork(bus, artwork);
        enemies.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);
        var oam = new OamBuffer();
        new Action<OamBuffer, Position16<RoomPixels>>(enemies.DeadTorizoBehavior.DrawDeadTorizoHook)(
                oam, camera);
        return oam;
    }

    /// <summary>Compares each initialized corpse or sand staging byte with the independent retail copy.</summary>
    private static void AssertDeadTorizoBufferParity(
        StagingByteView expected, StagingByteView actual, int length,
        string description)
    {
        for (int offset = 0; offset < length; offset++)
            AssertEqual(expected[offset], actual[offset], description);
    }

    /// <summary>Blocks sheet reads from $B7:A800 through $B7:BFFF after artwork installation.</summary>
    private static MutableCartridgeReadGuard NewDeadTorizoArtworkReadGuard(ISnesAddressSpace source)
    {
        var guard = new MutableCartridgeReadGuard(source);
        guard.Forbid(address => address is >= 0xb7a800 and < 0xb7c000,
            address => $"Dead Torizo read installed artwork from ROM at ${address:X6}.");
        return guard;
    }

}
