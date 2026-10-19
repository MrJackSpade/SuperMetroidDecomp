using SuperMetroid.Core.Frontend;
using System.Reflection;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Bosses.Crocomire;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Crocomire's typed melt selectors, transfer records, and production output against the cartridge.</summary>
internal static class CrocomireMeltingDefinitionsVerification
{
    /// <summary>Compares all melt columns, masks, transfer passes, and production erase behavior with the ROM.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Crocomire melting definitions")]
    internal static void VerifyCrocomireMeltingDefinitions(SuperMetroidAddressSpace rom)
    {
        const int columnTable = 0xa49697;
        Suite(nameof(VerifyCrocomireMaskAlgorithm), () => VerifyCrocomireMaskAlgorithm(rom));
        for (int cursor = 0; cursor < CrocomireMeltingDefinitions.ColumnCount; cursor++)
        {
            AssertEqual(rom.ReadCartridgeByte(columnTable + cursor),
                CrocomireMeltingDefinitions.SelectColumn(cursor),
                $"Crocomire melt column {cursor}");

        }
        AssertThrows<ArgumentOutOfRangeException>(
            () => CrocomireMeltingDefinitions.SelectColumn(-1),
            "Crocomire negative melt cursor");
        AssertThrows<ArgumentOutOfRangeException>(
            () => CrocomireMeltingDefinitions.SelectColumn(49),
            "Crocomire melt cursor after authored table");

        Suite(nameof(VerifyCrocomireMeltingTransferCatalog), () => VerifyCrocomireMeltingTransferCatalog(rom));
        Suite(nameof(VerifyCrocomireMeltingProductionSequence), () => VerifyCrocomireMeltingProductionSequence(rom));
        Console.WriteLine(
            "Crocomire melting definitions: all 49 column selectors, eight masks, " +
            "both native transfer passes, the complete production erase sequence, " +
            "and source-table read guards pass.");
    }

    /// <summary>Checks the eight repeating erase masks at every authored melt cursor.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Crocomire mask algorithm")]
    internal static void VerifyCrocomireMaskAlgorithm(SuperMetroidAddressSpace rom)
    {
        for (int cursor = 0; cursor < 49; cursor++)
            AssertEqual(rom.ReadCartridgeByte(CrocomireMeltingDefinitions.MaskReferenceAddress + (cursor & 7)),
                CrocomireMeltingDefinitions.SelectMask(cursor), $"Original chronological melt mask {cursor}");
        AssertThrows<ArgumentOutOfRangeException>(() => CrocomireMeltingDefinitions.SelectMask(-1), "Mask negative cursor");
        AssertThrows<ArgumentOutOfRangeException>(() => CrocomireMeltingDefinitions.SelectMask(49), "Mask upper bound");
        AssertThrows<ArgumentOutOfRangeException>(() => CrocomireMeltingDefinitions.SelectMask(int.MaxValue), "Mask invalid maximum");
    }

    /// <summary>Runs focused comparisons for pass headers, copies, uploads, and their graphics effects.</summary>
    private static void VerifyCrocomireMeltingTransferCatalog(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyCrocomireMeltHeaders), () => VerifyCrocomireMeltHeaders(rom));
        Suite(nameof(VerifyCrocomireMeltCopies), () => VerifyCrocomireMeltCopies(rom));
        Suite(nameof(VerifyCrocomireMeltUploads), () => VerifyCrocomireMeltUploads(rom));
        Suite(nameof(VerifyCrocomireMeltingGraphicsProduction), () => VerifyCrocomireMeltingGraphicsProduction(rom));
    }

    /// <summary>Checks both typed pass headers and their transfer cursors against native words.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Crocomire melt headers")]
    internal static void VerifyCrocomireMeltHeaders(SuperMetroidAddressSpace rom)
    {
        int index = 0;
        foreach (CrocomireMeltingPass pass in CrocomireMeltingTransferDefinitions.Passes)
        {
            AssertEqual((ushort)(index == 0 ? 0 : 0x54), pass.HeaderOffset, "native melt header identity");
            AssertEqual(index == 0 ? 6 : 7, pass.ChunkCount, "native melt chunk count");
            AssertEqual(pass, CrocomireMeltingTransferDefinitions.Passes[index++], "melt pass enumeration");
            int header = CrocomireMeltingTransferDefinitions.NativeSourceAddress +
                pass.HeaderOffset;
            AssertEqual(pass.MaximumAdjustedDestinationY, CartridgeImportSource.Require(rom).ReadCartridgeWord(header),
                $"Crocomire melt pass ${pass.HeaderOffset:X4} maximum Y");
            AssertEqual(pass.DistortionEndY, CartridgeImportSource.Require(rom).ReadCartridgeWord((header + 2)),
                $"Crocomire melt pass ${pass.HeaderOffset:X4} distortion end Y");
            AssertEqual(pass.WordsToCopy, CartridgeImportSource.Require(rom).ReadCartridgeWord((header + 4)),
                $"Crocomire melt pass ${pass.HeaderOffset:X4} copy word count");
            AssertEqual(pass.SourceBank, rom.ReadCartridgeByte(header + 6),
                $"Crocomire melt pass ${pass.HeaderOffset:X4} source bank");

            AssertEqual(pass, CrocomireMeltingTransferDefinitions.Transfers(pass.TransferStartOffset), "melt transfer selector");
        }
        AssertEqual((ushort)0xb4, CrocomireMeltingTransferDefinitions.Passes[1].NextHeaderOffset, "native melt record extent");
        AssertThrows<IndexOutOfRangeException>(() => _ = CrocomireMeltingTransferDefinitions.Passes[-1], "negative melt pass");
        AssertThrows<IndexOutOfRangeException>(() => _ = CrocomireMeltingTransferDefinitions.Passes[2], "melt pass past end");
        AssertThrows<InvalidDataException>(() => CrocomireMeltingTransferDefinitions.Header(1), "invalid melt header");
        AssertThrows<InvalidDataException>(() => CrocomireMeltingTransferDefinitions.Transfers(0), "header is not transfer start");
    }

    /// <summary>Checks every source and destination word in the two native scratch-copy lists.</summary>
    internal static void VerifyCrocomireMeltCopies(SuperMetroidAddressSpace rom)
    {
        foreach (ushort headerOffset in new ushort[] { 0, 0x54 })
        {
            var pass = CrocomireMeltingTransferDefinitions.Header(headerOffset);
            int header = 0xa49bc5 + headerOffset;
            for (int index = 0; index < pass.Copies.Length; index++)
            {
                int source = header + 8 + index * 4;
                CrocomireMeltingCopy copy = pass.Copies[index];
                AssertEqual(copy.SourceWord, CartridgeImportSource.Require(rom).ReadCartridgeWord(source),
                    $"Crocomire melt pass ${pass.HeaderOffset:X4} copy {index} source");
                AssertEqual(copy.DestinationWord, CartridgeImportSource.Require(rom).ReadCartridgeWord((source + 2)),
                    $"Crocomire melt pass ${pass.HeaderOffset:X4} copy {index} destination");
            }
            int copyEnd = header + 8 + pass.Copies.Length * 4;
            AssertEqual((ushort)0xffff, CartridgeImportSource.Require(rom).ReadCartridgeWord(copyEnd),
                $"Crocomire melt pass ${pass.HeaderOffset:X4} copy terminator");
            AssertEqual(pass.TransferStartOffset,
                (ushort)(copyEnd + 2 - CrocomireMeltingTransferDefinitions.NativeSourceAddress),
                $"Crocomire melt pass ${pass.HeaderOffset:X4} transfer start");

            AssertEqual(pass.ChunkCount, pass.Copies.Length, "melt copy domain");
            int enumerated = 0;
            foreach (var copy in pass.Copies) AssertEqual(pass.Copies[enumerated++], copy, "melt copy enumeration");
            AssertEqual(pass.ChunkCount, enumerated, "melt copy enumeration count");
            AssertThrows<IndexOutOfRangeException>(() => _ = pass.Copies[-1], "negative melt copy");
            AssertThrows<IndexOutOfRangeException>(() => _ = pass.Copies[pass.ChunkCount], "melt copy past end");
        }
    }

    /// <summary>Checks both upload lists, terminators, and typed lookup boundaries against the ROM.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Crocomire melt uploads")]
    internal static void VerifyCrocomireMeltUploads(SuperMetroidAddressSpace rom)
    {
        foreach (ushort headerOffset in new ushort[] { 0, 0x54 })
        {
            var pass = CrocomireMeltingTransferDefinitions.Header(headerOffset);
            int header = 0xa49bc5 + headerOffset;
            for (int index = 0; index < pass.Uploads.Length; index++)
            {
                int offset = pass.TransferStartOffset + index * 8;
                int source = CrocomireMeltingTransferDefinitions.NativeSourceAddress + offset;
                CrocomireMeltingUpload upload = pass.Uploads[index];
                AssertEqual(upload.ByteCount, CartridgeImportSource.Require(rom).ReadCartridgeWord(source),
                    $"Crocomire melt pass ${pass.HeaderOffset:X4} upload {index} size");
                AssertEqual(upload.DestinationWord, CartridgeImportSource.Require(rom).ReadCartridgeWord((source + 2)),
                    $"Crocomire melt pass ${pass.HeaderOffset:X4} upload {index} destination");
                AssertEqual(upload.SourceBank, rom.ReadCartridgeByte(source + 4),
                    $"Crocomire melt pass ${pass.HeaderOffset:X4} upload {index} bank");
                AssertEqual(upload.SourceWord, CartridgeImportSource.Require(rom).ReadCartridgeWord((source + 6)),
                    $"Crocomire melt pass ${pass.HeaderOffset:X4} upload {index} source");
                AssertTrue(CrocomireMeltingTransferDefinitions.TryUpload(offset,
                    out CrocomireMeltingUpload selected) && selected == upload,
                    $"Crocomire melt pass ${pass.HeaderOffset:X4} upload {index} lookup");
            }
            AssertEqual(pass.TransferEndOffset,
                (ushort)(pass.TransferStartOffset + pass.Uploads.Length * 8),
                $"Crocomire melt pass ${pass.HeaderOffset:X4} transfer end");
            AssertEqual((ushort)0xffff, CartridgeImportSource.Require(rom).ReadCartridgeWord((CrocomireMeltingTransferDefinitions.NativeSourceAddress + pass.TransferEndOffset)),
                $"Crocomire melt pass ${pass.HeaderOffset:X4} transfer terminator");
            AssertTrue(!CrocomireMeltingTransferDefinitions.TryUpload(
                pass.TransferEndOffset, out _),
                $"Crocomire melt pass ${pass.HeaderOffset:X4} terminal lookup");
            AssertEqual(pass.NextHeaderOffset, (ushort)(pass.TransferEndOffset + 2),
                $"Crocomire melt pass ${pass.HeaderOffset:X4} next header");
        }
        foreach (var pass in CrocomireMeltingTransferDefinitions.Passes)
        {
            int enumerated = 0;
            foreach (var upload in pass.Uploads) AssertEqual(pass.Uploads[enumerated++], upload, "melt upload enumeration");
            AssertEqual(pass.ChunkCount, enumerated, "melt upload enumeration count");
            AssertThrows<IndexOutOfRangeException>(() => _ = pass.Uploads[-1], "negative melt upload");
            AssertThrows<IndexOutOfRangeException>(() => _ = pass.Uploads[pass.ChunkCount], "melt upload past end");
            AssertThrows<InvalidDataException>(() => CrocomireMeltingTransferDefinitions.TryUpload(pass.TransferStartOffset + 1, out _), "misaligned melt upload");
        }
        AssertThrows<InvalidDataException>(() => CrocomireMeltingTransferDefinitions.TryUpload(0x54, out _), "header is not an upload");
        AssertThrows<InvalidDataException>(() => CrocomireMeltingTransferDefinitions.TryUpload(-1, out _), "negative upload cursor");
        AssertThrows<InvalidDataException>(() => CrocomireMeltingTransferDefinitions.TryUpload(0xb4, out _), "upload after records");
    }
    /// <summary>Runs production copy and upload handlers and compares scratch graphics and VRAM with native records.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for Crocomire melting graphics production")]
    private static void VerifyCrocomireMeltingGraphicsProduction(SuperMetroidAddressSpace rom)
    {
        var artwork = InstalledFixtures.EnemyTiles;
        foreach (CrocomireMeltingPass pass in CrocomireMeltingTransferDefinitions.Passes)
        {
            var enemies = new RoomEnemySystem(new EnemySession()) { TileArtwork = artwork };
            var state = new CrocomireEnemyState(enemies.SlotTable.Slots[0]);
            var death = new CrocomireDeathState { MeltingTableOffset = pass.HeaderOffset };
            var vram = new SnesVram();
            enemies.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);
            enemies.Presentation.BindVram(vram);
            enemies.CrocomireBehavior.CrocomireDeath = death;
            Action<CrocomireEnemyState> initialize = enemies.CrocomireBehavior.InitializeCrocomireMeltingGraphics;
            Action<CrocomireEnemyState> upload = enemies.CrocomireBehavior.UploadNextCrocomireMeltingGraphicsSlice;
            Action<CrocomireEnemyState> complete = enemies.CrocomireBehavior.CompleteCrocomireMeltingDissolve;

            initialize(state);
            AssertEqual(pass.MaximumAdjustedDestinationY, death.MaximumAdjustedDestinationY,
                $"Crocomire melt pass ${pass.HeaderOffset:X4} production maximum Y");
            AssertEqual(pass.DistortionEndY, death.DistortionEndY,
                $"Crocomire melt pass ${pass.HeaderOffset:X4} production end Y");
            AssertEqual(pass.TransferStartOffset, death.MeltingTableOffset,
                $"Crocomire melt pass ${pass.HeaderOffset:X4} production transfer cursor");

            var expectedGraphics = new byte[CrocomireDeathState.MeltingGraphicsByteCount];
            foreach (CrocomireMeltingCopy copy in pass.Copies)
            {
                int destination = copy.DestinationWord - 0x4000;
                for (int index = 0; index < (pass.WordsToCopy + 1) * 2; index++)
                {
                    expectedGraphics[destination + index] = rom.ReadCartridgeByte(
                        ((int)new SnesAddress(checked((byte)pass.SourceBank), checked((ushort)(copy.SourceWord + index)))));
                }
            }
            AssertTrue(death.MeltingGraphics.SequenceEqual(expectedGraphics),
                $"Crocomire melt pass ${pass.HeaderOffset:X4} production scratch graphics");

            foreach (CrocomireMeltingUpload record in pass.Uploads)
            {
                upload(state);
                int source = record.SourceWord - 0x4000;
                for (int index = 0; index < record.ByteCount; index++)
                {
                    AssertEqual(expectedGraphics[source + index],
                        vram.ReadByte(record.DestinationWord * 2 + index),
                        $"Crocomire melt pass ${pass.HeaderOffset:X4} VRAM byte {index}");
                }
            }
            AssertEqual((ushort)(pass.Uploads.Length * 8), death.MeltingTransferOffset,
                $"Crocomire melt pass ${pass.HeaderOffset:X4} final transfer offset");
            CrocomireDeathPhase phaseBeforeTerminator = state.DeathSequenceIndex;
            upload(state);
            AssertEqual(phaseBeforeTerminator.Next(), state.DeathSequenceIndex,
                $"Crocomire melt pass ${pass.HeaderOffset:X4} terminal phase");
            AssertEqual((ushort)0, death.MeltingTransferOffset,
                $"Crocomire melt pass ${pass.HeaderOffset:X4} terminal reset");
            complete(state);
            AssertEqual(pass.NextHeaderOffset, death.MeltingTableOffset,
                $"Crocomire melt pass ${pass.HeaderOffset:X4} completion cursor");
        }
    }

    /// <summary>Compares all 49 production erase calls with an independently calculated bitplane silhouette.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Crocomire melting production sequence")]
    internal static void VerifyCrocomireMeltingProductionSequence(
        SuperMetroidAddressSpace rom)
    {
        var enemies = new RoomEnemySystem(new EnemySession());
        var death = new CrocomireDeathState
        {
            MeltingTableOffset = CrocomireMeltingTransferDefinitions.Passes[0].TransferStartOffset,
            PixelsToErasePerColumn = 48,
            MeltTargetHeight = 48,
        };
        death.MutableMeltingGraphics.Fill(0xff);
        enemies.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);
        enemies.Presentation.BindVram(new SnesVram());
        enemies.CrocomireBehavior.CrocomireDeath = death;
        Func<bool> erase = enemies.CrocomireBehavior.EraseNextCrocomireMeltingColumn;

        var expectedGraphics = Enumerable.Repeat((byte)0xff,
            CrocomireDeathState.MeltingGraphicsByteCount).ToArray();
        var expectedHeights = new byte[CrocomireDeathState.MeltingColumnCount];
        for (int cursor = 0; cursor < CrocomireMeltingDefinitions.ColumnCount; cursor++)
        {
            int xColumn = rom.ReadCartridgeByte(0xa49697 + cursor);
            byte mask = rom.ReadCartridgeByte(0xa49bbd + (cursor & 7));
            for (int remaining = 48; remaining != 0; remaining--)
            {
                int y = expectedHeights[xColumn];
                int byteIndex = 2 * (y & 7) + ((y & ~7) << 6) + 4 * (xColumn & ~7);
                expectedGraphics[byteIndex] &= mask;
                expectedGraphics[byteIndex + 1] &= mask;
                expectedGraphics[byteIndex + 16] &= mask;
                expectedGraphics[byteIndex + 17] &= mask;
                expectedHeights[xColumn]++;
            }

            AssertTrue(erase(), $"Crocomire melt production cursor {cursor} succeeds");
            AssertEqual((ushort)cursor, death.MeltingColumnCursor,
                $"Crocomire melt production cursor {cursor}");
            AssertEqual((byte)48, death.MeltingColumnHeights[xColumn],
                $"Crocomire melt production column {xColumn} height");
        }

        AssertTrue(death.MeltingGraphics.SequenceEqual(expectedGraphics),
            "Crocomire melt production bitplane silhouette");
        AssertTrue(death.MeltingColumnHeights.SequenceEqual(expectedHeights),
            "Crocomire melt production column heights");
        AssertTrue(erase(), "Crocomire melt post-table guard retains distortion phase");
        AssertEqual((ushort)CrocomireMeltingDefinitions.ColumnCount,
            death.MeltingColumnCursor,
            "Crocomire melt post-table cursor");
    }

    /// <summary>Runs one installed artwork melt pass and returns its scratch graphics after verifying VRAM uploads.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    internal static byte[] VerifyInstalledCrocomireMeltingPass(EnemyTileArtworkCatalog artwork, CrocomireMeltingPass pass)
    {
        var enemies = new RoomEnemySystem(new EnemySession()) { TileArtwork = artwork };
        var state = new CrocomireEnemyState(enemies.SlotTable.Slots[0]);
        var death = new CrocomireDeathState { MeltingTableOffset = pass.HeaderOffset };
        var vram = new SnesVram();
        enemies.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);
        enemies.Presentation.BindVram(vram);
        enemies.CrocomireBehavior.CrocomireDeath = death;
        Action<CrocomireEnemyState> initialize = enemies.CrocomireBehavior.InitializeCrocomireMeltingGraphics;
        Action<CrocomireEnemyState> upload = enemies.CrocomireBehavior.UploadNextCrocomireMeltingGraphicsSlice;

        initialize(state);
        AssertEqual(pass.TransferStartOffset, death.MeltingTableOffset,
            $"installed Crocomire melt pass ${pass.HeaderOffset:X4} transfer start");
        AssertEqual(CrocomireDeathPhase.CrumbleBridgeAndSink, state.DeathSequenceIndex,
            $"installed Crocomire melt pass ${pass.HeaderOffset:X4} phase timing");
        byte[] scratch = death.MeltingGraphics.ToArray();
        foreach (CrocomireMeltingUpload record in pass.Uploads)
        {
            upload(state);
            int source = record.SourceWord - 0x4000;
            for (int index = 0; index < record.ByteCount; index++)
                AssertEqual(scratch[source + index],
                    vram.ReadByte(record.DestinationWord * 2 + index),
                    $"installed Crocomire melt pass ${pass.HeaderOffset:X4} VRAM {index}");
        }
        upload(state);
        AssertEqual(CrocomireDeathPhase.FirstSubmergedPause, state.DeathSequenceIndex,
            $"installed Crocomire melt pass ${pass.HeaderOffset:X4} terminal timing");
        return scratch;
    }

    /// <summary>Loads one installed BG2 melt tilemap and compares its working copy and VRAM with native words.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for installed Crocomire melting tilemap")]
    internal static ushort[] VerifyInstalledCrocomireMeltingTilemap(
        SuperMetroidAddressSpace rom, EnemyTileArtworkCatalog artwork,
        CrocomireMeltingTilemapAddress source, CrocomireBodyProgram bodyProgram, bool compareRom = true)
    {
        int sourceAddress = (int)source;
        var enemies = new RoomEnemySystem(new EnemySession()) { TileArtwork = artwork };
        var state = new CrocomireEnemyState(enemies.SlotTable.Slots[0]);
        var death = new CrocomireDeathState();
        var vram = new SnesVram();
        enemies.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);
        enemies.Presentation.BindVram(vram);
        enemies.CrocomireBehavior.CrocomireDeath = death;
        // #1269 split the loader into the two native phases ($A4:9341 and $A4:93ED).
        Action<CrocomireEnemyState> initialize = source switch
        {
            CrocomireMeltingTilemapAddress.FirstTilemap => enemies.CrocomireBehavior.LoadFirstCrocomireMeltingTilemap,
            CrocomireMeltingTilemapAddress.SecondTilemap => enemies.CrocomireBehavior.LoadSecondCrocomireMeltingTilemap,
            _ => throw new ArgumentOutOfRangeException(nameof(source)),
        };

        initialize(state);
        AssertEqual(bodyProgram, state.BodyCursor.Program,
            $"installed Crocomire melt tilemap ${sourceAddress:X6} body program");
        AssertEqual(CrocomireDeathPhase.CrumbleBridgeAndSink, state.DeathSequenceIndex,
            $"installed Crocomire melt tilemap ${sourceAddress:X6} phase timing");
        AssertEqual((ushort)48, death.PixelsToErasePerColumn,
            $"installed Crocomire melt tilemap ${sourceAddress:X6} erase count");
        AssertEqual(48, death.MeltTargetHeight,
            $"installed Crocomire melt tilemap ${sourceAddress:X6} target height");
        var result = death.Bg2WorkingTilemap.Slice(32,
            CrocomireMeltingArtworkFormat.TilemapCellCount).ToArray();
        for (int index = 0; index < result.Length; index++)
        {
            int address = sourceAddress + index * 2;
            ushort expected = CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
            if (compareRom)
                AssertEqual(expected, result[index],
                    $"installed Crocomire tilemap ${sourceAddress:X6} cell {index}");
            int vramByte = (0x4800 + 32 + index) * 2;
            AssertEqual((byte)result[index], vram.ReadByte(vramByte),
                $"installed Crocomire tilemap ${sourceAddress:X6} VRAM low {index}");
            AssertEqual(SnesLittleEndian.HighByte(result[index]), vram.ReadByte(vramByte + 1),
                $"installed Crocomire tilemap ${sourceAddress:X6} VRAM high {index}");
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
