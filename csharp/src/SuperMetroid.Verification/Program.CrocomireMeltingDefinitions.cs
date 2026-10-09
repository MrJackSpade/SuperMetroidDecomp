using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Cross-checks Crocomire's authored melt-column table and transfer records
    /// against cartridge data, then verifies the production melting path uses compiled definitions.</summary>
    /// <param name="rom">Retail address space supplying the reference data for these checks.</param>
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

    /// <summary>Confirms the eight-mask cycle repeats across all 49 melt columns and rejects cursors outside that sequence.</summary>
    /// <param name="rom">Cartridge address space containing the reference mask bytes.</param>
    private static void VerifyCrocomireMaskAlgorithm(SuperMetroidAddressSpace rom)
    {
        for (int cursor = 0; cursor < 49; cursor++)
            AssertEqual(rom.ReadByte(CrocomireMeltingDefinitions.MaskReferenceAddress + (cursor & 7)),
                CrocomireMeltingDefinitions.SelectMask(cursor), $"Original chronological melt mask {cursor}");
        AssertThrows<ArgumentOutOfRangeException>(() => CrocomireMeltingDefinitions.SelectMask(-1), "Mask negative cursor");
        AssertThrows<ArgumentOutOfRangeException>(() => CrocomireMeltingDefinitions.SelectMask(49), "Mask upper bound");
        AssertThrows<ArgumentOutOfRangeException>(() => CrocomireMeltingDefinitions.SelectMask(int.MaxValue), "Mask invalid maximum");
    }
    /// <summary>Reads one little-endian word from the melt tables through the address-space interface.</summary>
    /// <param name="bus">Address space containing the native table bytes.</param>
    /// <param name="address">Address of the word's low byte.</param>
    /// <returns>The word formed from the addressed byte and its successor.</returns>
    private static ushort CrocomireMeltNativeWord(ISnesAddressSpace bus, int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    /// <summary>Runs the header, copy-record, upload-record, and production-graphics checks for both melt passes.</summary>
    /// <param name="rom">Cartridge address space used as the independent source of expected records.</param>
    private static void VerifyCrocomireMeltingTransferCatalog(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyCrocomireMeltHeaders), () => VerifyCrocomireMeltHeaders(rom));
        Suite(nameof(VerifyCrocomireMeltCopies), () => VerifyCrocomireMeltCopies(rom));
        Suite(nameof(VerifyCrocomireMeltUploads), () => VerifyCrocomireMeltUploads(rom));
        Suite(nameof(VerifyCrocomireMeltingGraphicsProduction), () => VerifyCrocomireMeltingGraphicsProduction(rom));
    }

    /// <summary>Checks each pass header's native fields, offsets, and association with its transfer stream.</summary>
    /// <param name="rom">Cartridge address space containing the two pass headers.</param>
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

    /// <summary>Compares each parsed graphics-copy pair with the cartridge table and validates its terminator and bounds.</summary>
    /// <param name="rom">Cartridge address space containing the pass copy records.</param>
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

    /// <summary>Checks parsed VRAM uploads against their native records, including stream termination and lookup alignment.</summary>
    /// <param name="rom">Cartridge address space containing the pass upload streams.</param>
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
    /// <summary>Exercises production melt-graphics initialization and upload delegates while guarding migrated ROM sources.</summary>
    /// <param name="rom">Cartridge address space used for expected bytes and unblocked definition reads.</param>
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
            typeof(RoomEnemySystem).GetField("_crocomireDeath", flags)!.SetValue(enemies, death);
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

    /// <summary>Checks the production column-erasure sequence against a cartridge-derived bitplane and height map.</summary>
    /// <param name="rom">Cartridge address space providing the expected column and mask sequence.</param>
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
        typeof(RoomEnemySystem).GetField("_crocomireDeath", flags)!.SetValue(enemies, death);
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

    /// <summary>Runs one installed graphics pass with migrated artwork reads blocked and returns its initialized scratch graphics.</summary>
    /// <param name="rom">Cartridge address space providing reference bytes while forbidden installed-data reads are guarded.</param>
    /// <param name="artwork">Installed enemy artwork catalog supplied to the production room system.</param>
    /// <param name="pass">Transfer pass to initialize and upload.</param>
    /// <returns>The scratch graphics produced by the production initializer.</returns>
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
        typeof(RoomEnemySystem).GetField("_crocomireDeath", flags)!.SetValue(enemies, death);
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

    /// <summary>Runs the production tilemap initializer and checks its installed tilemap and VRAM copy.</summary>
    /// <param name="rom">Cartridge address space used for the optional byte-for-byte reference comparison.</param>
    /// <param name="artwork">Installed enemy artwork catalog supplied to the production room system.</param>
    /// <param name="sourceAddress">Cartridge address of the tilemap data being initialized.</param>
    /// <param name="bodyInstructionList">Crocomire body instruction-list pointer used by the initializer.</param>
    /// <param name="compareRom">Whether to compare the installed words with the cartridge source.</param>
    /// <returns>The initialized tilemap words copied from the working tilemap.</returns>
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
        typeof(RoomEnemySystem).GetField("_crocomireDeath", flags)!.SetValue(enemies, death);
        var initialize = typeof(RoomEnemySystem).GetMethod(
            "InitializeCrocomireMeltingTilemap", flags)!
            .CreateDelegate<Action<CrocomireEnemyState, int, ushort>>(enemies);

        initialize(state, sourceAddress, bodyInstructionList);
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

    /// <summary>Address-space proxy that rejects reads from migrated Crocomire definitions and optionally installed artwork.</summary>
    /// <param name="source">Underlying address space used for reads and writes that pass the guard.</param>
    /// <param name="blockGraphics">Whether reads from installed tilemaps and graphics-copy sources are also rejected.</param>
    private sealed class CrocomireMeltingDefinitionReadGuard(
        ISnesAddressSpace source, bool blockGraphics = false) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Forwards importer cartridge reads through the same forbidden-range checks.</summary>
        /// <param name="address">Cartridge address to read.</param>
        /// <returns>The source byte when the address is permitted.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects migrated definition ranges and, when enabled, installed graphics sources before forwarding other reads.</summary>
        /// <param name="address">Address to read from the wrapped space.</param>
        /// <returns>The underlying byte for an allowed address.</returns>
        /// <exception cref="InvalidOperationException">The requested address falls in a guarded definition or artwork range.</exception>
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
                if (address >= CrocomireMeltingArtworkAddresses.FirstTilemap &&
                    address < CrocomireMeltingArtworkAddresses.SecondTilemap +
                        (CrocomireMeltingArtworkFormat.TilemapCellCount + 1) * 2)
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

        /// <summary>Forwards writes to the underlying address space without modifying the guard's read policy.</summary>
        /// <param name="address">Address that receives the byte.</param>
        /// <param name="value">Byte written to that address.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
