using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Hardware.ImportedSpritemapsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.VerificationOracles;

namespace SuperMetroid.Verification.Enemies.Corpses;

/// <summary>Checks installed Tourian corpse tile staging and OAM against independent retail layouts.</summary>
internal static class DeadTourianCorpseArtworkVerification
{
    /// <summary>Compares all corpse initializer variants against native WRAM and confirms a sidehopper PNG edit reaches live staging.</summary>
    internal static void VerifyInstalledDeadTourianCorpseArtwork(
        string directory, EnemyTileArtworkCatalog stock)
    {
        Suite(nameof(VerifyDeadTourianCorpseVisuals), () => VerifyDeadTourianCorpseVisuals(stock));
        // Compare the installed initialization callbacks with independent native
        // copy layouts. The installed path forbids reads from the visual source bank.
        foreach (ushort parameter in new ushort[] { 0, 2 })
        {
            var nativeBus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
            var installedBus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
            var native = new EnemyStagingSurfaces();
            ReferenceDeadTourianCorpseGraphics(nativeBus, native, EnemyDefinitionId.CorpseSidehopper, parameter / 2);
            EnemyStagingSurfaces installed = InitializeDeadSidehopperArtwork(installedBus, stock, parameter);
            for (int offset = 0; offset < 0x1000; offset++)
                AssertEqual(native.CorpseTiles[offset],
                    installed.CorpseTiles[offset],
                    $"dead-sidehopper parameter {parameter} installed corpse staging parity");
        }

        foreach ((EnemyDefinitionId definition, int variantCount) in new (EnemyDefinitionId, int)[]
        {
            (EnemyDefinitionId.CorpseZoomer, 3),
            (EnemyDefinitionId.CorpseRipper, 2),
            (EnemyDefinitionId.CorpseSkree, 3),
        })
        {
            for (int variant = 0; variant < variantCount; variant++)
            {
                var nativeBus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
                var installedBus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
                var native = new EnemyStagingSurfaces();
                ReferenceDeadTourianCorpseGraphics(nativeBus, native, definition, variant);
                EnemyStagingSurfaces installed = InitializeDeadTourianCorpseArtwork(installedBus, stock, definition, variant);
                for (int offset = 0; offset < 0x1000; offset++)
                    AssertEqual(native.CorpseTiles[offset],
                        installed.CorpseTiles[offset],
                        $"dead ${(int)definition:X4} variant {variant} installed corpse staging parity");
            }
        }

        var missing = EnemyTileArtworkCatalog.FromArtworkForVerification(new EnemyTileArtworkParts { Sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>(), Palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>() });
        AssertThrows<InvalidDataException>(() => InitializeDeadSidehopperArtwork(
                SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc"), missing, 0),
            "bound dead-sidehopper art cannot silently fall back to ROM");

        string fileName = EnemyTileArtworkFormat.FileName(EnemyDefinitionId.CorpseSidehopper);
        using var source = new MemoryStream(File.ReadAllBytes(Path.Combine(directory, fileName)));
        int tileCount = RoomCharacterAtlasFormat.ValidateTileCount(0x0e00);
        int columns = Math.Min(RoomCharacterAtlasFormat.TileColumns, tileCount);
        int rows = (tileCount + columns - 1) / columns;
        IndexedPngImage image = IndexedPng.Read(source, new(checked((ushort)(columns * 8)), checked((ushort)(rows * 8))));
        // The first copy starts at source offset $0040, tile 2. Its first
        // pixel becomes the high bit at WRAM $7E:2040.
        image.Pixels[2 * 8] ^= 1;
        string overrideDirectory = Path.Combine(directory, "dead-sidehopper-overrides");
        Directory.CreateDirectory(overrideDirectory);
        using (var output = File.Create(Path.Combine(overrideDirectory, fileName)))
            IndexedPng.Write(output, new(image.Size.X, image.Size.Y), image.Pixels, image.Palette);
        var stockBus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        var editedBus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        EnemyStagingSurfaces stockStaging = InitializeDeadSidehopperArtwork(stockBus, stock, 0);
        EnemyStagingSurfaces editedStaging = InitializeDeadSidehopperArtwork(editedBus,
            EnemyTileArtworkFiles.Load(directory, overrideDirectory), 0);
        AssertEqual((byte)(stockStaging.CorpseTiles[0x40] ^ 0x80),
            editedStaging.CorpseTiles[0x40],
            "dead-sidehopper PNG pixel edit reaches live corpse staging");
        for (int offset = 0; offset < 0x1000; offset++)
        {
            if (offset == 0x40)
                continue;
            AssertEqual(stockStaging.CorpseTiles[offset],
                editedStaging.CorpseTiles[offset],
                "dead-sidehopper PNG edit leaves other corpse bytes unchanged");
        }
        var reloadedBus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        EnemyStagingSurfaces reloadedStaging = InitializeDeadSidehopperArtwork(reloadedBus,
            EnemyTileArtworkFiles.Load(directory, overrideDirectory), 0);
        AssertEqual(editedStaging.CorpseTiles[0x40], reloadedStaging.CorpseTiles[0x40],
            "dead-sidehopper PNG edit survives catalog reload");

        Console.WriteLine("  Tourian corpse artwork: ten guarded sidehopper/Zoomer/Ripper/Skree variants match cartridge WRAM; a PNG edit and reload reach the live buffer.");
    }

    /// <summary>Matches the installed OAM compositions of every corpse pose the typed programs show to retail spritemaps.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for Dead Tourian corpse visuals")]
    private static void VerifyDeadTourianCorpseVisuals(
        EnemyTileArtworkCatalog stock)
    {
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        DeadTourianProgram[] corpsePrograms =
        [
            DeadTourianProgram.ZoomerCorpse0, DeadTourianProgram.ZoomerCorpse1, DeadTourianProgram.ZoomerCorpse2,
            DeadTourianProgram.RipperCorpse0, DeadTourianProgram.RipperCorpse1,
            DeadTourianProgram.SkreeCorpse0, DeadTourianProgram.SkreeCorpse1, DeadTourianProgram.SkreeCorpse2,
        ];
        AssertEqual(DeadTourianCorpseVisualDefinitions.CorpseFrameCount, corpsePrograms.Length,
            "every dead Tourian corpse program has an editable composition");
        foreach (DeadTourianProgram program in corpsePrograms)
            VerifyInstalledPose(DeadTourianAnimationsConstants.Spritemaps(program).Single(), $"dead Tourian corpse {program}");
        ushort[] sidehopperPointers = DeadTourianAnimationsConstants.Spritemaps(
            DeadTourianProgram.SidehopperHopping, DeadTourianProgram.SidehopperIdle,
            DeadTourianProgram.SidehopperCorpse, DeadTourianProgram.SidehopperInitiallyDead).Distinct().ToArray();
        foreach (ushort pointer in sidehopperPointers)
            VerifyInstalledPose(pointer, $"dead Sidehopper ${pointer:X4}");
        AssertEqual(DeadTourianCorpseVisualDefinitions.SidehopperFrameCount, sidehopperPointers.Length,
            "the dead Sidehopper's programs show five distinct OAM compositions");

        // The typed oracle ties each pose to its native list; here the installed composition must draw as the cartridge's.
        void VerifyInstalledPose(ushort pointer, string context)
        {
            AssertTrue((stock.Spritemaps ?? throw new InvalidOperationException("Verification requires stock.Spritemaps.")).TryGetDisplay(
                    DeadTourianCorpseVisualDefinitions.Bank, pointer, out EnemySpritemapParts installedParts),
                $"{context} has installed OAM");
            var nativeOam = new OamBuffer();
            var installedOam = new OamBuffer();
            DrawImportedEnemySpritemap(rom, nativeOam, new NativeEnemySpritemapDraw { Bank = DeadTourianCorpseVisualDefinitions.Bank, Pointer = pointer, Origin = new(128, 128), PaletteBits = 0, BaseTileIndex = 0 });
            installedOam.AddEnemySpritemap(installedParts, new Position16<OamPixels>(128, 128), 0, 0);
            AssertTrue(nativeOam.ImageEquals(installedOam) && nativeOam.NextByteOffset == installedOam.NextByteOffset,
                $"{context} installed OAM matches cartridge");
        }
        Console.WriteLine("  Dead Tourian corpses: eight Zoomer/Ripper/Skree poses and five Sidehopper compositions are editable native-parity OAM frames.");
    }

    /// <summary>Stages corpse tiles with independent pinned A9 copy spans for the retail WRAM oracle.</summary>
    /// <remarks>Independent copy spans from the pinned sm_a9.c corpse initializers $A9:DEC1-$E052. Keep these separate
    /// from the runtime's layout definitions.</remarks>
    private static void ReferenceDeadTourianCorpseGraphics(
        SuperMetroidAddressSpace bus, EnemyStagingSurfaces staging, EnemyDefinitionId definition, int variant)
    {
        (int Source, int Destination, int Length)[] copies = (definition, variant) switch
        {
            (EnemyDefinitionId.CorpseSidehopper, 0) =>
                [(64, 0x040, 0x60), (512, 0x0a0, 0xa0), (1024, 0x140, 0xa0),
                 (1536, 0x1e0, 0xa0), (2048, 0x280, 0xa0)],
            (EnemyDefinitionId.CorpseSidehopper, 1) =>
                [(288, 0x320, 0x40), (800, 0x3c0, 0xa0), (1312, 0x460, 0xa0),
                 (1824, 0x500, 0xa0), (2336, 0x5a0, 0xa0)],
            (EnemyDefinitionId.CorpseZoomer, 0) => [(2656, 0x940, 0x60), (3168, 0x9a0, 0x60)],
            (EnemyDefinitionId.CorpseZoomer, 1) => [(2752, 0xa00, 0x60), (3264, 0xa60, 0x60)],
            (EnemyDefinitionId.CorpseZoomer, 2) => [(2848, 0xac0, 0x60), (3360, 0xb20, 0x60)],
            (EnemyDefinitionId.CorpseRipper, 0) => [(2560, 0xb80, 0x60), (3072, 0xbe0, 0x60)],
            (EnemyDefinitionId.CorpseRipper, 1) => [(2944, 0xc40, 0x60), (3456, 0xca0, 0x60)],
            (EnemyDefinitionId.CorpseSkree, 0) =>
                [(672, 0x640, 0x40), (1184, 0x680, 0x40), (1696, 0x6c0, 0x40), (2208, 0x700, 0x40)],
            (EnemyDefinitionId.CorpseSkree, 1) =>
                [(224, 0x740, 0x40), (736, 0x780, 0x40), (1248, 0x7c0, 0x40), (1760, 0x800, 0x40)],
            (EnemyDefinitionId.CorpseSkree, 2) =>
                [(448, 0x840, 0x40), (960, 0x880, 0x40), (1472, 0x8c0, 0x40), (1984, 0x900, 0x40)],
            _ => throw new InvalidDataException($"Unknown native corpse fixture {(ushort)definition:X4}/{variant}."),
        };
        StagingByteView tiles = staging.CorpseTiles;
        foreach (var copy in copies)
            for (int offset = 0; offset < copy.Length; offset++)
                tiles[copy.Destination + offset] = ReadNativeDeadTourianCorpseSheetByte(bus, copy.Source + offset);
    }

    /// <summary>Reads a retail corpse-sheet byte for the independent staging oracle.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for the dead Tourian corpse tile sheet at $B7:C000")]
    private static byte ReadNativeDeadTourianCorpseSheetByte(ISnesAddressSpace rom, int sheetOffset) =>
        rom.ReadCartridgeByte(0xb7c000 + sheetOffset);

    /// <summary>Runs the real sidehopper initializer with corpse-sheet ROM reads forbidden; returns the staging it wrote.</summary>
    private static EnemyStagingSurfaces InitializeDeadSidehopperArtwork(
        SuperMetroidAddressSpace bus, EnemyTileArtworkCatalog artwork, ushort parameter)
    {
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance) { TileArtwork = artwork };
        ISnesAddressSpace source = NewDeadTourianCorpseArtworkReadGuard(bus);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.SetSpawnParameter1(parameter);
        new Action<RoomEnemySlot>(enemies.DeadSidehopperBehavior.InitializeDeadSidehopper)(slot);
        return enemies.Room.Staging;
    }

    /// <summary>Runs a Zoomer, Ripper, or Skree corpse initializer using installed artwork under a source read guard; returns the staging it wrote.</summary>
    private static EnemyStagingSurfaces InitializeDeadTourianCorpseArtwork(
        SuperMetroidAddressSpace bus, EnemyTileArtworkCatalog artwork,
        EnemyDefinitionId definition, int variant)
    {
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance) { TileArtwork = artwork };
        ISnesAddressSpace source = NewDeadTourianCorpseArtworkReadGuard(bus);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(definition, slot.Definition);
        slot.SetSpawnParameter1(checked((ushort)(variant * 2)));
        new Action<RoomEnemySlot>(enemies.DeadTourianBehavior.InitializeDeadTourianCorpse)(slot);
        return enemies.Room.Staging;
    }

    /// <summary>Rejects fallback reads of the bank-$B7 corpse character sheet.</summary>
    private static CartridgeReadGuard NewDeadTourianCorpseArtworkReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => address is >= 0xb7c000 and < 0xb7ce00,
                address => $"Tourian corpse read installed artwork from ROM at ${address:X6}.");
}
