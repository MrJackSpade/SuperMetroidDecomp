using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyCrocomireMeltingDefinitions(SuperMetroidAddressSpace rom)
    {
        const int columnTable = 0xa49697;
        Suite(nameof(VerifyCrocomireMaskAlgorithm), () => VerifyCrocomireMaskAlgorithm(rom));
        for (int cursor = 0; cursor < CrocomireMeltingDefinitions.ColumnCount; cursor++)
        {
            AssertEqual(rom.ReadByte(columnTable + cursor),
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

    private static void VerifyCrocomireMaskAlgorithm(SuperMetroidAddressSpace rom)
    {
        for (int cursor = 0; cursor < 49; cursor++)
            AssertEqual(rom.ReadByte(CrocomireMeltingDefinitions.MaskReferenceAddress + (cursor & 7)),
                CrocomireMeltingDefinitions.SelectMask(cursor), $"Original chronological melt mask {cursor}");
        AssertThrows<ArgumentOutOfRangeException>(() => CrocomireMeltingDefinitions.SelectMask(-1), "Mask negative cursor");
        AssertThrows<ArgumentOutOfRangeException>(() => CrocomireMeltingDefinitions.SelectMask(49), "Mask upper bound");
        AssertThrows<ArgumentOutOfRangeException>(() => CrocomireMeltingDefinitions.SelectMask(int.MaxValue), "Mask invalid maximum");
    }
    private static ushort CrocomireMeltNativeWord(ISnesAddressSpace bus, int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    private static void VerifyCrocomireMeltingTransferCatalog(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyCrocomireMeltHeaders), () => VerifyCrocomireMeltHeaders(rom));
        Suite(nameof(VerifyCrocomireMeltCopies), () => VerifyCrocomireMeltCopies(rom));
        Suite(nameof(VerifyCrocomireMeltUploads), () => VerifyCrocomireMeltUploads(rom));
        Suite(nameof(VerifyCrocomireMeltingGraphicsProduction), () => VerifyCrocomireMeltingGraphicsProduction(rom));
    }

    private static void VerifyCrocomireMeltHeaders(SuperMetroidAddressSpace rom)
    {
        int index = 0;
        foreach (CrocomireMeltingPass pass in CrocomireMeltingTransferDefinitions.Passes)
        {
            AssertEqual((ushort)(index == 0 ? 0 : 0x54), pass.HeaderOffset, "native melt header identity");
            AssertEqual(index == 0 ? 6 : 7, pass.ChunkCount, "native melt chunk count");
            AssertEqual(pass, CrocomireMeltingTransferDefinitions.Passes[index++], "melt pass enumeration");
            int header = CrocomireMeltingTransferDefinitions.NativeSourceAddress +
                pass.HeaderOffset;
            AssertEqual(pass.MaximumAdjustedDestinationY, CrocomireMeltNativeWord(rom, header),
                $"Crocomire melt pass ${pass.HeaderOffset:X4} maximum Y");
            AssertEqual(pass.DistortionEndY, CrocomireMeltNativeWord(rom, header + 2),
                $"Crocomire melt pass ${pass.HeaderOffset:X4} distortion end Y");
            AssertEqual(pass.WordsToCopy, CrocomireMeltNativeWord(rom, header + 4),
                $"Crocomire melt pass ${pass.HeaderOffset:X4} copy word count");
            AssertEqual(pass.SourceBank, rom.ReadByte(header + 6),
                $"Crocomire melt pass ${pass.HeaderOffset:X4} source bank");

            AssertEqual(pass, CrocomireMeltingTransferDefinitions.Transfers(pass.TransferStartOffset), "melt transfer selector");
        }
        AssertEqual((ushort)0xb4, CrocomireMeltingTransferDefinitions.Passes[1].NextHeaderOffset, "native melt record extent");
        AssertThrows<IndexOutOfRangeException>(() => _ = CrocomireMeltingTransferDefinitions.Passes[-1], "negative melt pass");
        AssertThrows<IndexOutOfRangeException>(() => _ = CrocomireMeltingTransferDefinitions.Passes[2], "melt pass past end");
        AssertThrows<InvalidDataException>(() => CrocomireMeltingTransferDefinitions.Header(1), "invalid melt header");
        AssertThrows<InvalidDataException>(() => CrocomireMeltingTransferDefinitions.Transfers(0), "header is not transfer start");
    }

    private static void VerifyCrocomireMeltCopies(SuperMetroidAddressSpace rom)
    {
        foreach (ushort headerOffset in new ushort[] { 0, 0x54 })
        {
            var pass = CrocomireMeltingTransferDefinitions.Header(headerOffset);
            int header = 0xa49bc5 + headerOffset;
            for (int index = 0; index < pass.Copies.Length; index++)
            {
                int source = header + 8 + index * 4;
                CrocomireMeltingCopy copy = pass.Copies[index];
                AssertEqual(copy.SourceWord, CrocomireMeltNativeWord(rom, source),
                    $"Crocomire melt pass ${pass.HeaderOffset:X4} copy {index} source");
                AssertEqual(copy.DestinationWord, CrocomireMeltNativeWord(rom, source + 2),
                    $"Crocomire melt pass ${pass.HeaderOffset:X4} copy {index} destination");
            }
            int copyEnd = header + 8 + pass.Copies.Length * 4;
            AssertEqual((ushort)0xffff, CrocomireMeltNativeWord(rom, copyEnd),
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

    private static void VerifyCrocomireMeltUploads(SuperMetroidAddressSpace rom)
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
                AssertEqual(upload.ByteCount, CrocomireMeltNativeWord(rom, source),
                    $"Crocomire melt pass ${pass.HeaderOffset:X4} upload {index} size");
                AssertEqual(upload.DestinationWord, CrocomireMeltNativeWord(rom, source + 2),
                    $"Crocomire melt pass ${pass.HeaderOffset:X4} upload {index} destination");
                AssertEqual(upload.SourceBank, rom.ReadByte(source + 4),
                    $"Crocomire melt pass ${pass.HeaderOffset:X4} upload {index} bank");
                AssertEqual(upload.SourceWord, CrocomireMeltNativeWord(rom, source + 6),
                    $"Crocomire melt pass ${pass.HeaderOffset:X4} upload {index} source");
                AssertTrue(CrocomireMeltingTransferDefinitions.TryUpload(offset,
                    out CrocomireMeltingUpload selected) && selected == upload,
                    $"Crocomire melt pass ${pass.HeaderOffset:X4} upload {index} lookup");
            }
            AssertEqual(pass.TransferEndOffset,
                (ushort)(pass.TransferStartOffset + pass.Uploads.Length * 8),
                $"Crocomire melt pass ${pass.HeaderOffset:X4} transfer end");
            AssertEqual((ushort)0xffff, CrocomireMeltNativeWord(rom,
                CrocomireMeltingTransferDefinitions.NativeSourceAddress + pass.TransferEndOffset),
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
    private static void VerifyCrocomireMeltingGraphicsProduction(SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var artwork = RepositoryInstallation.EnemyTiles;
        foreach (CrocomireMeltingPass pass in CrocomireMeltingTransferDefinitions.Passes)
        {
            var enemies = new RoomEnemySystem { TileArtwork = artwork };
            var state = new CrocomireEnemyState(enemies.Slots[0]);
            var death = new CrocomireDeathState { MeltingTableOffset = pass.HeaderOffset };
            var vram = new SnesVram();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(
                enemies, new CrocomireMeltingDefinitionReadGuard(rom, blockGraphics: true));
            typeof(RoomEnemySystem).GetField("_vram", flags)!.SetValue(enemies, vram);
            typeof(RoomEnemySystem).GetField("<CrocomireDeath>k__BackingField", flags)!.SetValue(enemies, death);
            var initialize = typeof(RoomEnemySystem).GetMethod(
                "InitializeCrocomireMeltingGraphics", flags)!
                .CreateDelegate<Action<CrocomireEnemyState>>(enemies);
            var upload = typeof(RoomEnemySystem).GetMethod(
                "UploadNextCrocomireMeltingGraphicsSlice", flags)!
                .CreateDelegate<Action<CrocomireEnemyState>>(enemies);
            var complete = typeof(RoomEnemySystem).GetMethod(
                "CompleteCrocomireMeltingDissolve", flags)!
                .CreateDelegate<Action<CrocomireEnemyState>>(enemies);

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
                    expectedGraphics[destination + index] = rom.ReadByte(
                        (pass.SourceBank << 16) | unchecked((ushort)(copy.SourceWord + index)));
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
            ushort phaseBeforeTerminator = state.DeathSequenceIndex;
            upload(state);
            AssertEqual(unchecked((ushort)(phaseBeforeTerminator + 2)),
                state.DeathSequenceIndex,
                $"Crocomire melt pass ${pass.HeaderOffset:X4} terminal phase");
            AssertEqual((ushort)0, death.MeltingTransferOffset,
                $"Crocomire melt pass ${pass.HeaderOffset:X4} terminal reset");
            complete(state);
            AssertEqual(pass.NextHeaderOffset, death.MeltingTableOffset,
                $"Crocomire melt pass ${pass.HeaderOffset:X4} completion cursor");
        }
    }

    private static void VerifyCrocomireMeltingProductionSequence(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem();
        var death = new CrocomireDeathState
        {
            MeltingTableOffset = CrocomireMeltingTransferDefinitions.Passes[0].TransferStartOffset,
            PixelsToErasePerColumn = 48,
            TargetHeightOrSkeletonTileIndex = 48,
        };
        death.MutableMeltingGraphics.Fill(0xff);
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(
            enemies,
            new CrocomireMeltingDefinitionReadGuard(rom));
        typeof(RoomEnemySystem).GetField("_vram", flags)!.SetValue(enemies, new SnesVram());
        typeof(RoomEnemySystem).GetField("<CrocomireDeath>k__BackingField", flags)!.SetValue(enemies, death);
        var erase = typeof(RoomEnemySystem).GetMethod(
            "EraseNextCrocomireMeltingColumn",
            flags)!.CreateDelegate<Func<bool>>(enemies);

        var expectedGraphics = Enumerable.Repeat((byte)0xff,
            CrocomireDeathState.MeltingGraphicsByteCount).ToArray();
        var expectedHeights = new byte[CrocomireDeathState.MeltingColumnCount];
        for (int cursor = 0; cursor < CrocomireMeltingDefinitions.ColumnCount; cursor++)
        {
            int xColumn = rom.ReadByte(0xa49697 + cursor);
            byte mask = rom.ReadByte(0xa49bbd + (cursor & 7));
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

    private static byte[] VerifyInstalledCrocomireMeltingPass(SuperMetroidAddressSpace rom,
        EnemyTileArtworkCatalog artwork, CrocomireMeltingPass pass)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem { TileArtwork = artwork };
        var state = new CrocomireEnemyState(enemies.Slots[0]);
        var death = new CrocomireDeathState { MeltingTableOffset = pass.HeaderOffset };
        var vram = new SnesVram();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(
            enemies, new CrocomireMeltingDefinitionReadGuard(rom, blockGraphics: true));
        typeof(RoomEnemySystem).GetField("_vram", flags)!.SetValue(enemies, vram);
        typeof(RoomEnemySystem).GetField("<CrocomireDeath>k__BackingField", flags)!.SetValue(enemies, death);
        var initialize = typeof(RoomEnemySystem).GetMethod(
            "InitializeCrocomireMeltingGraphics", flags)!
            .CreateDelegate<Action<CrocomireEnemyState>>(enemies);
        var upload = typeof(RoomEnemySystem).GetMethod(
            "UploadNextCrocomireMeltingGraphicsSlice", flags)!
            .CreateDelegate<Action<CrocomireEnemyState>>(enemies);

        initialize(state);
        AssertEqual(pass.TransferStartOffset, death.MeltingTableOffset,
            $"installed Crocomire melt pass ${pass.HeaderOffset:X4} transfer start");
        AssertEqual((ushort)2, state.DeathSequenceIndex,
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
        AssertEqual((ushort)4, state.DeathSequenceIndex,
            $"installed Crocomire melt pass ${pass.HeaderOffset:X4} terminal timing");
        return scratch;
    }

    private static ushort[] VerifyInstalledCrocomireMeltingTilemap(
        SuperMetroidAddressSpace rom, EnemyTileArtworkCatalog artwork,
        int sourceAddress, ushort bodyInstructionList, bool compareRom = true)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem { TileArtwork = artwork };
        var state = new CrocomireEnemyState(enemies.Slots[0]);
        var death = new CrocomireDeathState();
        var vram = new SnesVram();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(
            enemies, new CrocomireMeltingDefinitionReadGuard(rom, blockGraphics: true));
        typeof(RoomEnemySystem).GetField("_vram", flags)!.SetValue(enemies, vram);
        typeof(RoomEnemySystem).GetField("<CrocomireDeath>k__BackingField", flags)!.SetValue(enemies, death);
        // #1269 split the loader into the two native phases ($A4:9341 and $A4:93ED).
        string loader = sourceAddress == CrocomireMeltingArtworkAddresses.FirstTilemap
            ? "LoadFirstCrocomireMeltingTilemap"
            : sourceAddress == CrocomireMeltingArtworkAddresses.SecondTilemap
                ? "LoadSecondCrocomireMeltingTilemap"
                : throw new ArgumentOutOfRangeException(nameof(sourceAddress));
        var initialize = typeof(RoomEnemySystem).GetMethod(loader, flags)!
            .CreateDelegate<Action<CrocomireEnemyState>>(enemies);

        initialize(state);
        AssertEqual(bodyInstructionList, state.Body.CurrentInstruction,
            $"installed Crocomire melt tilemap ${sourceAddress:X6} body program");
        AssertEqual((ushort)2, state.DeathSequenceIndex,
            $"installed Crocomire melt tilemap ${sourceAddress:X6} phase timing");
        AssertEqual((ushort)48, death.PixelsToErasePerColumn,
            $"installed Crocomire melt tilemap ${sourceAddress:X6} erase count");
        AssertEqual((ushort)48, death.TargetHeightOrSkeletonTileIndex,
            $"installed Crocomire melt tilemap ${sourceAddress:X6} target height");
        var result = death.Bg2WorkingTilemap.Slice(32,
            CrocomireMeltingArtworkFormat.TilemapCellCount).ToArray();
        for (int index = 0; index < result.Length; index++)
        {
            int address = sourceAddress + index * 2;
            ushort expected = (ushort)(rom.ReadByte(address) |
                rom.ReadByte(address + 1) << 8);
            if (compareRom)
                AssertEqual(expected, result[index],
                    $"installed Crocomire tilemap ${sourceAddress:X6} cell {index}");
            int vramByte = (0x4800 + 32 + index) * 2;
            AssertEqual((byte)result[index], vram.ReadByte(vramByte),
                $"installed Crocomire tilemap ${sourceAddress:X6} VRAM low {index}");
            AssertEqual((byte)(result[index] >> 8), vram.ReadByte(vramByte + 1),
                $"installed Crocomire tilemap ${sourceAddress:X6} VRAM high {index}");
        }
        return result;
    }

    private sealed class CrocomireMeltingDefinitionReadGuard(
        ISnesAddressSpace source, bool blockGraphics = false) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        public byte ReadByte(int address)
        {
            if (address is
                >= 0xa49697 and < 0xa496c8 or
                >= 0xa49bbd and < 0xa49bc5 or
                >= CrocomireMeltingTransferDefinitionsConstants.NativeSourceAddress and
                    < CrocomireMeltingTransferDefinitionsConstants.NativeSourceAddress +
                        CrocomireMeltingTransferDefinitionsConstants.NativeByteCount)
                throw new InvalidOperationException(
                    $"Crocomire melting attempted migrated definition read ${address:X6}.");
            if (blockGraphics)
            {
                if (address is >= CrocomireMeltingArtworkAddresses.FirstTilemap and
                    < (CrocomireMeltingArtworkAddresses.SecondTilemap +
                        (CrocomireMeltingArtworkFormat.TilemapCellCount + 1) * 2))
                    throw new InvalidOperationException(
                        $"Crocomire melting attempted installed tilemap read ${address:X6}.");
                foreach (CrocomireMeltingPass pass in CrocomireMeltingTransferDefinitions.Passes)
                foreach (CrocomireMeltingCopy copy in pass.Copies)
                {
                    int start = (pass.SourceBank << 16) | copy.SourceWord;
                    if (address >= start && address < start + (pass.WordsToCopy + 1) * 2)
                        throw new InvalidOperationException(
                            $"Crocomire melting attempted installed artwork read ${address:X6}.");
                }
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
