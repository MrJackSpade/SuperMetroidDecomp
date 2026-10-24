using SuperMetroid.AssetExtraction.Artwork;
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
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Enemies.Corpses;
using SuperMetroid.Core;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.VerificationOracles;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Enemies.Corpses;

/// <summary>Checks installed Tourian corpse tile staging and OAM against independent retail layouts.</summary>
internal static class DeadTourianCorpseArtworkVerification
{
    /// <summary>Compares all corpse initializer variants against native WRAM and confirms a sidehopper PNG edit reaches live staging.</summary>
    internal static void VerifyInstalledDeadTourianCorpseArtwork(
        string directory, InstalledEnemyArtwork stock)
    {
        Suite(nameof(VerifyDeadTourianCorpseVisuals), () => VerifyDeadTourianCorpseVisuals(stock.Enemies));
        // Compare the installed initialization callbacks with independent native
        // copy layouts. The installed path forbids reads from the visual source bank.
        // The extraction stages each corpse as the cartridge's initializers do, to picture its rot (#1392); the runtime
        // stages nothing. Compare its staged characters with independent native copies.
        foreach (ushort parameter in new ushort[] { 0, 2 })
        {
            var nativeBus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
            var native = new NativeEnemyStaging();
            DeadTorizoArtworkCopy[] copies = ReferenceDeadTourianCorpseGraphics(nativeBus, native, EnemyDefinitionId.CorpseSidehopper, parameter / 2);
            AssertStagedParity(native, InitializeDeadSidehopperArtwork(stock, parameter), copies,
                $"dead-sidehopper parameter {parameter} installed corpse staging parity");
        }

        foreach ((EnemyDefinitionId definition, int variantCount) in new VerifyInstalledDeadTourianCorpseArtworkCase[]
        {
            new VerifyInstalledDeadTourianCorpseArtworkCase(EnemyDefinitionId.CorpseZoomer, 3),
            new VerifyInstalledDeadTourianCorpseArtworkCase(EnemyDefinitionId.CorpseRipper, 2),
            new VerifyInstalledDeadTourianCorpseArtworkCase(EnemyDefinitionId.CorpseSkree, 3),
        })
        {
            for (int variant = 0; variant < variantCount; variant++)
            {
                var nativeBus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
                var native = new NativeEnemyStaging();
                DeadTorizoArtworkCopy[] copies = ReferenceDeadTourianCorpseGraphics(nativeBus, native, definition, variant);
                AssertStagedParity(native, InitializeDeadTourianCorpseArtwork(stock, definition, variant), copies,
                    $"dead ${(int)definition:X4} variant {variant} installed corpse staging parity");
            }
        }

        var missing = new InstalledEnemyArtwork(EnemyTileArtworkCatalog.FromArtworkForVerification(new EnemyTileArtworkParts { Sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>(), Palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>() }), new BossArtworkCatalog(), new AreaArtworkCatalog());
        AssertThrows<InvalidDataException>(() => InitializeDeadSidehopperArtwork(missing, 0),
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
            IndexedPng.Write(output, new(image.Size.X, image.Size.Y), image.Pixels, image.Colours);
        var nativeCopies = ReferenceDeadTourianCorpseGraphics(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc"), new NativeEnemyStaging(),
            EnemyDefinitionId.CorpseSidehopper, 0);
        CorpseStaging stockStaging = InitializeDeadSidehopperArtwork(stock, 0);
        CorpseStaging editedStaging = InitializeDeadSidehopperArtwork(EnemyTileArtworkFiles.Load(directory, overrideDirectory), 0);
        AssertEqual((byte)(Staged(stockStaging, 0x40, 1)[0] ^ 0x80), Staged(editedStaging, 0x40, 1)[0],
            "dead-sidehopper PNG pixel edit reaches the staged corpse");
        foreach (DeadTorizoArtworkCopy copy in nativeCopies)
        {
            byte[] stockBytes = Staged(stockStaging, copy.Destination, copy.Length), editedBytes = Staged(editedStaging, copy.Destination, copy.Length);
            for (int offset = 0; offset < copy.Length; offset++)
            {
                if (copy.Destination + offset != 0x40)
                    AssertEqual(stockBytes[offset], editedBytes[offset], "dead-sidehopper PNG edit leaves other corpse bytes unchanged");
            }
        }
        CorpseStaging reloadedStaging = InitializeDeadSidehopperArtwork(EnemyTileArtworkFiles.Load(directory, overrideDirectory), 0);
        AssertEqual(Staged(editedStaging, 0x40, 1)[0], Staged(reloadedStaging, 0x40, 1)[0],
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
    private static DeadTorizoArtworkCopy[] ReferenceDeadTourianCorpseGraphics(
        SuperMetroidAddressSpace bus, NativeEnemyStaging staging, EnemyDefinitionId definition, int variant)
    {
        DeadTorizoArtworkCopy[] copies = new DeadTourianCorpseArtworkCopy(definition, variant) switch
        {
            (EnemyDefinitionId.CorpseSidehopper, 0) =>
                [new DeadTorizoArtworkCopy(64, 0x040, 0x60), new DeadTorizoArtworkCopy(512, 0x0a0, 0xa0), new DeadTorizoArtworkCopy(1024, 0x140, 0xa0),
                 new DeadTorizoArtworkCopy(1536, 0x1e0, 0xa0), new DeadTorizoArtworkCopy(2048, 0x280, 0xa0)],
            (EnemyDefinitionId.CorpseSidehopper, 1) =>
                [new DeadTorizoArtworkCopy(288, 0x320, 0x40), new DeadTorizoArtworkCopy(800, 0x3c0, 0xa0), new DeadTorizoArtworkCopy(1312, 0x460, 0xa0),
                 new DeadTorizoArtworkCopy(1824, 0x500, 0xa0), new DeadTorizoArtworkCopy(2336, 0x5a0, 0xa0)],
            (EnemyDefinitionId.CorpseZoomer, 0) => [new DeadTorizoArtworkCopy(2656, 0x940, 0x60), new DeadTorizoArtworkCopy(3168, 0x9a0, 0x60)],
            (EnemyDefinitionId.CorpseZoomer, 1) => [new DeadTorizoArtworkCopy(2752, 0xa00, 0x60), new DeadTorizoArtworkCopy(3264, 0xa60, 0x60)],
            (EnemyDefinitionId.CorpseZoomer, 2) => [new DeadTorizoArtworkCopy(2848, 0xac0, 0x60), new DeadTorizoArtworkCopy(3360, 0xb20, 0x60)],
            (EnemyDefinitionId.CorpseRipper, 0) => [new DeadTorizoArtworkCopy(2560, 0xb80, 0x60), new DeadTorizoArtworkCopy(3072, 0xbe0, 0x60)],
            (EnemyDefinitionId.CorpseRipper, 1) => [new DeadTorizoArtworkCopy(2944, 0xc40, 0x60), new DeadTorizoArtworkCopy(3456, 0xca0, 0x60)],
            (EnemyDefinitionId.CorpseSkree, 0) =>
                [new DeadTorizoArtworkCopy(672, 0x640, 0x40), new DeadTorizoArtworkCopy(1184, 0x680, 0x40), new DeadTorizoArtworkCopy(1696, 0x6c0, 0x40), new DeadTorizoArtworkCopy(2208, 0x700, 0x40)],
            (EnemyDefinitionId.CorpseSkree, 1) =>
                [new DeadTorizoArtworkCopy(224, 0x740, 0x40), new DeadTorizoArtworkCopy(736, 0x780, 0x40), new DeadTorizoArtworkCopy(1248, 0x7c0, 0x40), new DeadTorizoArtworkCopy(1760, 0x800, 0x40)],
            (EnemyDefinitionId.CorpseSkree, 2) =>
                [new DeadTorizoArtworkCopy(448, 0x840, 0x40), new DeadTorizoArtworkCopy(960, 0x880, 0x40), new DeadTorizoArtworkCopy(1472, 0x8c0, 0x40), new DeadTorizoArtworkCopy(1984, 0x900, 0x40)],
            _ => throw new InvalidDataException($"Unknown native corpse fixture {(ushort)definition:X4}/{variant}."),
        };
        StagingByteView tiles = staging.CorpseTiles;
        foreach (var copy in copies)
            for (int offset = 0; offset < copy.Length; offset++)
                tiles[copy.Destination + offset] = ReadNativeDeadTourianCorpseSheetByte(bus, copy.Source + offset);
        return copies;
    }

    /// <summary>Reads a retail corpse-sheet byte for the independent staging oracle.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for the dead Tourian corpse tile sheet at $B7:C000")]
    private static byte ReadNativeDeadTourianCorpseSheetByte(ISnesAddressSpace rom, int sheetOffset) =>
        rom.ReadCartridgeByte(0xb7c000 + sheetOffset);

    /// <summary>The Sidehopper corpse of population parameter <paramref name="parameter"/> as the extraction stages it from installed art.</summary>
    private static CorpseStaging InitializeDeadSidehopperArtwork(InstalledEnemyArtwork artwork, ushort parameter) =>
        CorpseStaging.Sidehopper(Atlas(artwork), parameter);

    /// <summary>A Zoomer, Ripper or Skree corpse variant as the extraction stages it from installed art.</summary>
    private static CorpseStaging InitializeDeadTourianCorpseArtwork(InstalledEnemyArtwork artwork, EnemyDefinitionId definition, int variant) =>
        CorpseStaging.Tourian(Atlas(artwork), DeadTourianCorpseStagingDefinitions.Variants(DeadTourianBehavior.ProfileForDeadTourianCorpse(definition).Species)[variant]);

    /// <summary>The installed corpse sheet, which the extraction stages every Tourian corpse from.</summary>
    private static ReadOnlySpan<byte> Atlas(InstalledEnemyArtwork artwork) =>
        artwork.Enemies.TryResolve(CorpseSheetDefinitions.DeadMonsterSheetAddress, CorpseSheetDefinitions.DeadMonsterSheetByteCount,
            out ReadOnlyMemory<byte> tiles) ? tiles.Span
            : throw new InvalidDataException("Installed Tourian corpse sheet enemy-ed7f-tiles.png is missing or has the wrong size.");

    /// <summary>Compares each natively copied run of staged corpse characters with the extraction's.</summary>
    private static void AssertStagedParity(NativeEnemyStaging native, CorpseStaging installed, DeadTorizoArtworkCopy[] copies, string context)
    {
        foreach (DeadTorizoArtworkCopy copy in copies)
        {
            byte[] staged = Staged(installed, copy.Destination, copy.Length);
            for (int offset = 0; offset < copy.Length; offset++)
                AssertEqual(native.CorpseTiles[copy.Destination + offset], staged[offset], context);
        }
    }

    /// <summary>The <paramref name="length"/> staged bytes from <paramref name="offset"/>.</summary>
    private static byte[] Staged(CorpseStaging staging, int offset, int length) =>
        staging.Upload(new CorpseRotRow(0, EnemyStagingDefinitions.Bg2NativeAddress + offset, length, 0));
}
