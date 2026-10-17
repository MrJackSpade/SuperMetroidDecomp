using SuperMetroid.Tooling;
using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using static SuperMetroid.Verification.Harness.LookupStream5Verification;
using System.Reflection;
using System.Text;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Enemies.Artwork.EnemyExtendedFrameArtworkVerification;
using static SuperMetroid.Verification.Hardware.ImportedSpritemapsVerification;
using static SuperMetroid.Verification.Rooms.RoomEnemiesVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Bosses.Torizo.Artwork;

/// <summary>Checks statically identified enemy definition conversions against ROM and installed artwork.</summary>
internal static class EnemyDefinitionBoundaryVerification
{
    /// <summary>Verifies only the already statically identified enemy-reader conversions; no gameplay read discovery.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    internal static void VerifyEnemyDefinitionBoundary(string sourceRom)
    {
        var source = CartridgeImportAddressSpaceTooling.LoadRetailRom(sourceRom);
        Suite(nameof(VerifyEnemyMappedSourceRouting), () => VerifyEnemyMappedSourceRouting());
        Suite(nameof(VerifyCompleteTorizoDefinitions), () => VerifyCompleteTorizoDefinitions(source));
        Suite(nameof(VerifyCorpseMetadataDefinitions), () => VerifyCorpseMetadataDefinitions(source));
        Suite(nameof(VerifyCrocomirePowerBombReactionDefinitions), () => VerifyCrocomirePowerBombReactionDefinitions(source));
        Suite(nameof(VerifyEnemyAuxiliaryColors), () => VerifyEnemyAuxiliaryColors(source));
        using var temporary = new TestScratchDirectory("map-catalog");
        EnemyTileArtworkCatalog artwork = GameAssetInstaller.Install(sourceRom, temporary.Root).LoadEnemyTiles();
        AssertTrue(artwork.AuxiliaryColors is not null, "complete installation supplies auxiliary palettes");
        var memory = SuperMetroidAddressSpace.CreateWithoutCartridge();
        int drawCases = 0;
        foreach (ushort frame in TorizoCollisionDefinitions.FramePointers)
        foreach ((ushort x, ushort y) in new (ushort, ushort)[] { (128, 96), (0, 0), (511, 252) })
        {
            AssertTrue((artwork.ExtendedFrames ?? throw new InvalidOperationException("Verification requires artwork.ExtendedFrames.")).TryGet(TorizoFrameDefinitions.Bank, frame, out _),
                $"installed Torizo extended frame {frame:X4}");
            OamBuffer actual = DrawExtendedForBank(artwork, TorizoFrameDefinitions.Bank, frame, new(x, y));
            var expected = new OamBuffer();
            int count = CartridgeImportSource.Require(source).ReadCartridgeWordFixedBank((0xaa0000 | frame));
            for (int index = 0; index < count; index++)
            {
                int component = 0xaa0000 | (frame + 2 + index * 8);
                ushort cx = unchecked((ushort)(x + (short)CartridgeImportSource.Require(source).ReadCartridgeWordFixedBank(component)));
                ushort cy = unchecked((ushort)(y + (short)CartridgeImportSource.Require(source).ReadCartridgeWordFixedBank((component + 2))));
                if (((cx + 128) & 0xfe00) != 0 || ((cy + 128) & 0xfe00) != 0)
                    continue;
                DrawImportedEnemySpritemap(source, expected, new EnemySpritemapDraw { Bank = TorizoFrameDefinitions.Bank, Pointer = CartridgeImportSource.Require(source).ReadCartridgeWordFixedBank((component + 4)), Origin = new(cx, cy), PaletteBits = 0, BaseTileIndex = 0, ClipVerticalWrap = true, OriginYIsOnScreen = (SnesLittleEndian.HighByte(cy)) == 0 });
            }
            AssertEqual(expected.NextByteOffset, actual.NextByteOffset, "Torizo composed OAM count");
            AssertTrue(expected.ImageEquals(actual), "Torizo composed OAM bytes and origin arithmetic");
            drawCases++;
        }
        AssertThrows<InvalidDataException>(() => DrawExtendedForBank(null, TorizoFrameDefinitions.Bank, 0x87d0, new(128, 96)),
            "missing enemy artwork cannot recover an address decoder");
        Console.WriteLine($"Enemy definitions: all 106 Torizo physical frames, corpse metadata, 50 Crocomire reactions, 393 editable palette colors and {drawCases} composed OAM cases pass.");
    }

    /// <summary>Checks Torizo's physical components and hitbox records against bank AA.</summary>
    private static void VerifyCompleteTorizoDefinitions(ISnesAddressSpace source)
    {
        // The lists themselves are typed programs (#1289); VerifyTorizoAnimations matches them to the native lists.
        AssertEqual(106, TorizoCollisionDefinitions.FrameCount, "complete Torizo physical frame count");
        foreach (ushort frame in TorizoCollisionDefinitions.FramePointers)
        {
            TorizoCollisionComponents components = TorizoCollisionDefinitions.ComponentsAt(frame);
            AssertEqual(CartridgeImportSource.Require(source).ReadCartridgeWordFixedBank((0xaa0000 | frame)), components.Length, "Torizo component count");
            for (int i = 0; i < components.Length; i++)
            {
                int address = 0xaa0000 | (frame + 2 + i * 8);
                AssertEqual(unchecked((short)CartridgeImportSource.Require(source).ReadCartridgeWordFixedBank(address)), components[i].Offset.X, "Torizo physical X");
                AssertEqual(unchecked((short)CartridgeImportSource.Require(source).ReadCartridgeWordFixedBank((address + 2))), components[i].Offset.Y, "Torizo physical Y");
                AssertEqual(CartridgeImportSource.Require(source).ReadCartridgeWordFixedBank((address + 6)), components[i].HitboxList, "Torizo hitbox identity");
            }
        }
        foreach (ushort pointer in TorizoCollisionDefinitions.HitboxPointers)
        {
            ReadOnlySpan<GoldenTorizoCollisionHitbox> boxes = TorizoCollisionDefinitions.HitboxesAt(pointer);
            AssertEqual(CartridgeImportSource.Require(source).ReadCartridgeWordFixedBank((0xaa0000 | pointer)), boxes.Length, "Torizo rectangle count");
            for (int i = 0; i < boxes.Length; i++)
            {
                int address = 0xaa0000 | (pointer + 2 + i * 12);
                var box = boxes[i];
                ushort[] words = [unchecked((ushort)box.Left), unchecked((ushort)box.Top),
                    unchecked((ushort)box.Right), unchecked((ushort)box.Bottom), box.TouchAi, box.ShotAi];
                for (int w = 0; w < words.Length; w++)
                    AssertEqual(CartridgeImportSource.Require(source).ReadCartridgeWordFixedBank((address + w * 2)), words[w], "Torizo rectangle/callback word");
            }
        }
    }

    /// <summary>Checks corpse rotation offsets, DMA descriptors, Torizo touch boxes, and sand tile sources.</summary>
    internal static void VerifyCorpseMetadataDefinitions(ISnesAddressSpace source)
    {
        foreach ((ushort pointer, int count) in new (ushort, int)[]
            { (0xe226, 13), (0xe240, 6), (0xe24c, 3), (0xe252, 3), (0xe258, 5) })
            for (int i = 0; i < count; i++)
                AssertEqual(CartridgeImportSource.Require(source).ReadCartridgeWordFixedBank((0xa90000 | (pointer + i * 2))),
                    DeadMonsterRottingDefinitions.RotationOffset(pointer, (ushort)(i * 8)), "corpse rotation offset");
        foreach (ushort pointer in new ushort[] { 0xe0e0, 0xe10a, 0xe134, 0xe146, 0xe158, 0xe16a, 0xe17c, 0xe18e, 0xe1b0, 0xe1d2 })
        {
            var records = DeadMonsterRottingDefinitions.ForTransferTable(pointer);
            for (int i = 0; i < records.Length; i++)
            {
                var record = records[i];
                ushort[] words = [record.SizeInBytes, record.SourceBankWord, record.SourceOffset, record.EncodedVramDestination];
                for (int w = 0; w < words.Length; w++)
                    AssertEqual(CartridgeImportSource.Require(source).ReadCartridgeWordFixedBank((0xa90000 | (pointer + i * 8 + w * 2))), words[w], "corpse transfer descriptor");
            }
            AssertEqual((ushort)0, CartridgeImportSource.Require(source).ReadCartridgeWordFixedBank((0xa90000 | (pointer + records.Length * 8))), "corpse descriptor terminator");
        }
        var boxes = DeadMonsterRottingDefinitions.TorizoTouchHitboxes;
        AssertEqual(CartridgeImportSource.Require(source).ReadCartridgeWordFixedBank(0xa9d77c), boxes.Length, "dead Torizo asymmetric rectangle count");
        for (int i = 0; i < boxes.Length; i++)
        {
            ushort[] words = [unchecked((ushort)boxes[i].Left), unchecked((ushort)boxes[i].Top),
                unchecked((ushort)boxes[i].Right), unchecked((ushort)boxes[i].Bottom)];
            for (int w = 0; w < words.Length; w++)
                AssertEqual(CartridgeImportSource.Require(source).ReadCartridgeWordFixedBank((0xa9d77e + i * 8 + w * 2)), words[w], "dead Torizo asymmetric word");
        }
        for (ushort i = 0; i < 16; i++)
        {
            AssertEqual(CartridgeImportSource.Require(source).ReadCartridgeWordFixedBank((0xa9d67c + i * 2)), DeadMonsterRottingDefinitions.SandDestination(i), "corpse sand destination");
            AssertEqual(CartridgeImportSource.Require(source).ReadCartridgeWordFixedBank((0xa9d69c + i * 2)), DeadMonsterRottingDefinitions.SandSource(i), "corpse sand source");
        }
    }

    /// <summary>Compares all Crocomire frame mouth selections used for Power Bomb reactions with ROM.</summary>
    private static void VerifyCrocomirePowerBombReactionDefinitions(ISnesAddressSpace source)
    {
        int tested = 0;
        for (int index = 0; index < CrocomireBodyVisualDefinitions.BodyFrameCount; index++)
        {
            var frame = new CrocomireBodyFrame(CrocomireBodyFrameSet.Body, index);
            CrocomireBodyProgram expected = CrocomireBodyProgram.PowerBombReactionMouthNotOpen;
            int count = CartridgeImportSource.Require(source).ReadCartridgeWordFixedBank((0xa40000 | frame.Spritemap));
            for (int i = 0; i < count; i++)
            {
                ushort sprite = CartridgeImportSource.Require(source).ReadCartridgeWordFixedBank((0xa40000 | (frame.Spritemap + 6 + i * 8)));
                if (sprite == 0xd600) { expected = CrocomireBodyProgram.PowerBombReactionMouthFullyOpen; break; }
                if (sprite == 0xd51c) { expected = CrocomireBodyProgram.PowerBombReactionMouthPartiallyOpen; break; }
            }
            AssertEqual(expected, CrocomirePowerBombReactionDefinitions.ForFrame(frame), "Crocomire mechanical mouth admission");
            tested++;
        }
        AssertEqual(50, tested, "all Crocomire reaction frames");
        AssertThrows<InvalidDataException>(() => CrocomirePowerBombReactionDefinitions.ForFrame(
            new CrocomireBodyFrame(CrocomireBodyFrameSet.Skeleton, 0)), "a skeleton frame takes no power-bomb reaction");
    }

    /// <summary>Checks extracted RGB5 colors, editable override compilation, and invalid color and JSON rejection.</summary>
    private static void VerifyEnemyAuxiliaryColors(ISnesAddressSpace source)
    {
        byte[] json = EnemyAuxiliaryColorFiles.Extract(source);
        EnemyAuxiliaryColorCatalog catalog = EnemyAuxiliaryColorCatalog.Load(new MemoryStream(json, writable: false));
        foreach (var definition in EnemyAuxiliaryColorDefinitions.All)
            for (int frame = 0; frame < definition.FrameCount; frame++)
                for (int color = 0; color < definition.ColorCount; color++)
                    AssertEqual((ushort)(CartridgeImportSource.Require(source).ReadCartridgeWordFixedBank((definition.SourceAddress +
                        (frame * definition.NativeFrameStrideColors + color) * 2)) & 0x7fff),
                        catalog.Resolve(definition.Id, frame, color), "editable auxiliary RGB5 color");
        var document = RequiredJson.Deserialize<EnemyAuxiliaryColorDocument>(json,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter<EnemyAuxiliaryPalette>() } });
        document.Palettes[EnemyAuxiliaryPalette.FaceBlock][0][0] = new PaletteRgb5 { Red = 31, Green = 0, Blue = 0 };
        EnemyAuxiliaryColorCatalog edited = EnemyAuxiliaryColorCatalog.Load(
            new MemoryStream(EnemyAuxiliaryColorCatalog.Write(document), writable: false));
        AssertEqual((ushort)31, edited.Resolve(EnemyAuxiliaryPalette.FaceBlock, 0, 0), "auxiliary art override survives compilation");
        document.Palettes[EnemyAuxiliaryPalette.FaceBlock][0][0] = new PaletteRgb5 { Red = 32, Green = 0, Blue = 0 };
        AssertThrows<InvalidDataException>(() => EnemyAuxiliaryColorCatalog.Write(document), "out-of-range RGB5 rejected");
        string text = Encoding.UTF8.GetString(json);
        AssertThrows<InvalidDataException>(() => EnemyAuxiliaryColorCatalog.Load(new MemoryStream(
            Encoding.UTF8.GetBytes(text.Replace("\"version\": 1", "\"version\": 1, \"version\": 1", StringComparison.Ordinal)))),
            "duplicate palette JSON property rejected");
        AssertThrows<ArgumentOutOfRangeException>(() => catalog.Resolve(EnemyAuxiliaryPalette.FaceBlock, 8, 0), "palette frame bounded");
    }

    /// <summary>Runs corpse geometry and typed view checks through the verification command dispatcher.</summary>
    [VerificationCommand("--lookup-stream5-corpse-geometry")]
    internal static int LookupStream5CorpseGeometryCommand()
    {
        Suite(nameof(VerifyCorpseMetadataDefinitions), () => VerifyCorpseMetadataDefinitions(SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        Suite(nameof(VerifyLookupStream5CorpseViews), () => VerifyLookupStream5CorpseViews());
        Console.WriteLine("Corpse geometry: all30 rotation offsets,32 sand offsets,32 four-field DMA descriptors and10 terminators match original ROM.");
        return 0;
    }

    /// <summary>Runs the converted enemy definition checks using the repository retail ROM.</summary>
    [VerificationCommand("--enemy-definition-boundary")]
    internal static int EnemyDefinitionBoundaryCommand()
    {
        string enemyDefinitionRom = RepositoryRomPath;
        Suite(nameof(VerifyEnemyDefinitionBoundary), () => VerifyEnemyDefinitionBoundary(enemyDefinitionRom));
        return 0;
    }
}
