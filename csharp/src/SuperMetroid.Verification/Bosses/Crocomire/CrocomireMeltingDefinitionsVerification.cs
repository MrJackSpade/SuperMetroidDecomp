using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Bosses.Crocomire;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>
/// Checks Crocomire's melt definitions against the cartridge's melt table: the dissolve's columns and masks and the image
/// assembly and band layout the extraction pictures the melt from, and the distortion and band counts the runtime keeps.
/// </summary>
internal static class CrocomireMeltingDefinitionsVerification
{
    /// <summary><c>CrocomireMeltingXOffsetTable</c>: the column each dissolve step erases.</summary>
    private const int ColumnTableAddress = 0xa49697;
    /// <summary><c>TilePixelColumnBitmasks</c>: the eight masks the dissolve chooses by step.</summary>
    private const int MaskTableAddress = 0xa49bbd;
    /// <summary>The cartridge's melt table: both passes' headers, copy lists and upload lists.</summary>
    private const int MeltTableAddress = 0xa49bc5;
    /// <summary>Offset of the second pass's header in the melt table.</summary>
    private const int SecondHeaderOffset = 0x54;
    /// <summary>Work-memory address of the first byte of the image the melt assembles.</summary>
    private const int ImageAddress = 0x4000;
    /// <summary>Bank of the work memory the assembled image lives in.</summary>
    private const int ImageBank = 0x7e;
    /// <summary>The cartridge's list terminator.</summary>
    private const ushort Terminator = 0xffff;

    /// <summary>Compares the melt's columns, masks, passes and dissolve with the ROM.</summary>
    internal static void VerifyCrocomireMeltingDefinitions(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyCrocomireMaskAlgorithm), () => VerifyCrocomireMaskAlgorithm(rom));
        Suite(nameof(VerifyCrocomireMeltHeaders), () => VerifyCrocomireMeltHeaders(rom));
        Suite(nameof(VerifyCrocomireMeltCopies), () => VerifyCrocomireMeltCopies(rom));
        Suite(nameof(VerifyCrocomireMeltUploads), () => VerifyCrocomireMeltUploads(rom));
        Suite(nameof(VerifyCrocomireMeltingProductionSequence), () => VerifyCrocomireMeltingProductionSequence(rom));
        Console.WriteLine(
            "Crocomire melting definitions: all 49 columns, eight masks, both passes' headers, " +
            "band copies and band uploads, and the complete extraction dissolve pass.");
    }

    /// <summary>Checks the eight repeating erase masks at every dissolve step.</summary>
    internal static void VerifyCrocomireMaskAlgorithm(SuperMetroidAddressSpace rom)
    {
        for (int step = 0; step < CrocomireMeltingDefinitions.ColumnCount; step++)
            AssertEqual(rom.ReadCartridgeByte(MaskTableAddress + (step & 7)),
                CrocomireMeltDissolve.SelectMask(step), $"Original chronological melt mask {step}");
        AssertThrows<ArgumentOutOfRangeException>(() => CrocomireMeltDissolve.SelectMask(-1), "Mask negative step");
        AssertThrows<ArgumentOutOfRangeException>(() => CrocomireMeltDissolve.SelectMask(49), "Mask upper bound");
        AssertThrows<ArgumentOutOfRangeException>(() => CrocomireMeltDissolve.SelectMask(int.MaxValue), "Mask invalid maximum");
    }

    /// <summary>The melt table offset of <paramref name="pass"/>'s header.</summary>
    private static int HeaderAddress(CrocomireMeltingIllustration pass) =>
        MeltTableAddress + (pass == CrocomireMeltingIllustration.First ? 0 : SecondHeaderOffset);

    /// <summary>The cartridge address of <paramref name="pass"/>'s upload list.</summary>
    private static int UploadListAddress(CrocomireMeltingIllustration pass) =>
        HeaderAddress(pass) + 8 + 4 * CrocomireMelting.BandCount(pass) + 2;

    /// <summary>Checks each pass's header: its distortion rows, its copy length and its graphics bank.</summary>
    internal static void VerifyCrocomireMeltHeaders(SuperMetroidAddressSpace rom)
    {
        var source = CartridgeImportSource.Require(rom);
        foreach (CrocomireMeltingIllustration pass in Enum.GetValues<CrocomireMeltingIllustration>())
        {
            int header = HeaderAddress(pass);
            AssertEqual(CrocomireMeltingDefinitions.LowestDistortionSourceRow, source.ReadCartridgeWord(header),
                $"Crocomire {pass} melt lowest distortion source row");
            AssertEqual(CrocomireMeltingDefinitions.DistortionEndY, source.ReadCartridgeWord(header + 2),
                $"Crocomire {pass} melt distortion end");
            AssertEqual(CrocomireMeltingSourceDefinitions.CopyByteCount, (source.ReadCartridgeWord(header + 4) + 1) * 2,
                $"Crocomire {pass} melt copy length, counted inclusively");
            AssertEqual(CrocomireMeltingSources.Graphics(pass).Bank, rom.ReadCartridgeByte(header + 6),
                $"Crocomire {pass} melt graphics bank");
        }
        AssertEqual(MeltTableAddress + SecondHeaderOffset,
            UploadListAddress(CrocomireMeltingIllustration.First) + 8 * CrocomireMeltingDefinitions.FirstPassBandCount + 2,
            "the second pass's header follows the first pass's upload terminator");
    }

    /// <summary>Checks every band copy each pass makes: its graphics source and where in the image it lands.</summary>
    internal static void VerifyCrocomireMeltCopies(SuperMetroidAddressSpace rom)
    {
        var source = CartridgeImportSource.Require(rom);
        foreach (CrocomireMeltingIllustration pass in Enum.GetValues<CrocomireMeltingIllustration>())
        {
            int bands = CrocomireMelting.BandCount(pass);
            for (int band = 0; band < bands; band++)
            {
                int copy = HeaderAddress(pass) + 8 + band * 4;
                AssertEqual(CrocomireMeltingSources.SourceWord(pass, band), source.ReadCartridgeWord(copy),
                    $"Crocomire {pass} melt band {band} copy source");
                AssertEqual(ImageAddress + CrocomireMeltingSources.ImageByte(band), source.ReadCartridgeWord(copy + 2),
                    $"Crocomire {pass} melt band {band} copy destination");
            }
            AssertEqual(Terminator, source.ReadCartridgeWord(HeaderAddress(pass) + 8 + bands * 4),
                $"Crocomire {pass} melt copy terminator");
        }
    }

    /// <summary>Checks every band upload each pass makes: its size, the characters it shows over and its row of the image.</summary>
    internal static void VerifyCrocomireMeltUploads(SuperMetroidAddressSpace rom)
    {
        var source = CartridgeImportSource.Require(rom);
        foreach (CrocomireMeltingIllustration pass in Enum.GetValues<CrocomireMeltingIllustration>())
        {
            int bands = CrocomireMelting.BandCount(pass);
            for (int band = 0; band < bands; band++)
            {
                int upload = UploadListAddress(pass) + band * 8;
                AssertEqual(CrocomireMeltingDefinitions.BandCharacterCount * RoomCharacterAtlasFormat.BytesPerTile,
                    source.ReadCartridgeWord(upload), $"Crocomire {pass} melt band {band} size");
                AssertEqual(CrocomireMeltDissolve.BandFirstWord(band), source.ReadCartridgeWord(upload + 2),
                    $"Crocomire {pass} melt band {band} first character word");
                AssertEqual(ImageBank, rom.ReadCartridgeByte(upload + 4), $"Crocomire {pass} melt band {band} image bank");
                AssertEqual(ImageAddress + band * CrocomireMeltingDefinitions.BandRowCharacters * RoomCharacterAtlasFormat.BytesPerTile,
                    source.ReadCartridgeWord(upload + 6), $"Crocomire {pass} melt band {band} image row");
            }
            AssertEqual(Terminator, source.ReadCartridgeWord(UploadListAddress(pass) + bands * 8),
                $"Crocomire {pass} melt upload terminator");
        }
    }

    /// <summary>Compares all 49 extraction erase steps with an independently calculated bitplane silhouette.</summary>
    internal static void VerifyCrocomireMeltingProductionSequence(SuperMetroidAddressSpace rom)
    {
        var image = Enumerable.Repeat((byte)0xff, CrocomireMeltingArtworkFormat.SecondByteCount).ToArray();
        var expected = (byte[])image.Clone();
        for (int step = 0; step < CrocomireMeltingDefinitions.ColumnCount; step++)
        {
            int column = rom.ReadCartridgeByte(ColumnTableAddress + step);
            AssertEqual(column, CrocomireMeltDissolve.SelectColumn(step), $"Crocomire melt column {step}");
            byte mask = rom.ReadCartridgeByte(MaskTableAddress + (step & 7));
            for (int y = 0; y < 48; y++)
            {
                int byteIndex = 2 * (y & 7) + ((y & ~7) << 6) + 4 * (column & ~7);
                expected[byteIndex] &= mask;
                expected[byteIndex + 1] &= mask;
                expected[byteIndex + 16] &= mask;
                expected[byteIndex + 17] &= mask;
            }
            CrocomireMeltDissolve.EraseColumn(image, step);
            AssertTrue(image.SequenceEqual(expected), $"Crocomire melt silhouette after step {step}");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => CrocomireMeltDissolve.SelectColumn(-1), "Crocomire negative melt step");
        AssertThrows<ArgumentOutOfRangeException>(() => CrocomireMeltDissolve.SelectColumn(49), "Crocomire melt step after authored table");
    }

    /// <summary>The characters of <paramref name="pass"/>'s installed melt image, which the extraction pictures its bands from.</summary>
    internal static byte[] VerifyInstalledCrocomireMeltingPass(InstalledEnemyArtwork artwork, CrocomireMeltingIllustration pass)
    {
        CrocomireMeltingArtwork melt = Ensure.NotNull(artwork).Bosses.CrocomireMelting
            ?? throw new InvalidOperationException("Installed boss artwork has no Crocomire melt images.");
        byte[] characters = melt.Characters(pass).ToArray();
        AssertTrue(characters.AsSpan(CrocomireMeltingArtwork.UsedByteCount(pass)).IndexOfAnyExcept((byte)0) < 0,
            $"installed Crocomire {pass} melt image holds nothing past the bytes the melt assembles");
        return characters;
    }

    /// <summary>Loads one installed BG2 melt tilemap and compares its working copy and shown words with native words.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    internal static ushort[] VerifyInstalledCrocomireMeltingTilemap(
        SuperMetroidAddressSpace rom, InstalledEnemyArtwork artwork,
        CrocomireMeltingIllustration source, CrocomireBodyProgram bodyProgram, bool compareRom = true)
    {
        int sourceAddress = CrocomireMeltingSources.Tilemap(source);
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance) { TileArtwork = artwork.Enemies, BossArtwork = artwork.Bosses, AreaArtwork = artwork.Areas };
        var state = new CrocomireEnemyState(enemies.SlotTable.Slots[0]);
        var death = new CrocomireDeathState();
        var vram = new SnesVram();
        enemies.Presentation.BindBackgrounds(BgArtworkFixtures.For(vram));
        enemies.CrocomireBehavior.Access.CrocomireDeath = death;
        // Crocomire's initialization attaches the enemy BG2 image his words draw into (#1355).
        enemies.Presentation.AttachBg2Image(new EnemyBg2Image(CrocomireMeltingTilemap.BlankGlyph.Raw));
        // #1269 split the loader into the two native phases ($A4:9341 and $A4:93ED).
        Action<CrocomireEnemyState> initialize = source switch
        {
            CrocomireMeltingIllustration.First => enemies.CrocomireBehavior.Melting.LoadFirstCrocomireMeltingTilemap,
            CrocomireMeltingIllustration.Second => enemies.CrocomireBehavior.Melting.LoadSecondCrocomireMeltingTilemap,
            _ => throw new ArgumentOutOfRangeException(nameof(source)),
        };

        initialize(state);
        AssertEqual(bodyProgram, state.BodyCursor.Program,
            $"installed Crocomire melt tilemap ${sourceAddress:X6} body program");
        AssertEqual(CrocomireDeathPhase.CrumbleBridgeAndSink, state.DeathSequenceIndex,
            $"installed Crocomire melt tilemap ${sourceAddress:X6} phase timing");
        var result = death.Bg2WorkingTilemap.Slice(32,
            CrocomireMeltingArtworkFormat.TilemapCellCount).ToArray();
        for (int index = 0; index < result.Length; index++)
        {
            int address = sourceAddress + index * 2;
            ushort expected = CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
            if (compareRom)
                AssertEqual(expected, result[index],
                    $"installed Crocomire tilemap ${sourceAddress:X6} cell {index}");
            AssertEqual(result[index], enemies.Presentation.RequireBg2Image().Publish().Words[32 + index],
                $"installed Crocomire tilemap ${sourceAddress:X6} shown word {index}");
        }
        return result;
    }

    /// <summary>Runs the complete Crocomire melting definition check using the repository cartridge.</summary>
    [VerificationCommand("--crocomire-melting-definitions")]
    internal static int CrocomireMeltingDefinitionsCommand()
    {
        Suite(nameof(VerifyCrocomireMeltingDefinitions), () => VerifyCrocomireMeltingDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
