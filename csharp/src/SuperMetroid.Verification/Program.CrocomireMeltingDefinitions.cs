using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyCrocomireMeltingDefinitions(SuperMetroidAddressSpace rom)
    {
        const int columnTable = 0xa49697;
        const int maskTable = 0xa49bbd;
        for (int cursor = 0; cursor < CrocomireMeltingDefinitions.ColumnCount; cursor++)
        {
            AssertEqual(rom.ReadByte(columnTable + cursor),
                CrocomireMeltingDefinitions.SelectColumn(cursor),
                $"Crocomire melt column {cursor}");
            AssertEqual(rom.ReadByte(maskTable + (cursor & 7)),
                CrocomireMeltingDefinitions.SelectMask(cursor),
                $"Crocomire melt mask {cursor}");
        }
        AssertThrows<ArgumentOutOfRangeException>(
            () => CrocomireMeltingDefinitions.SelectColumn(-1),
            "Crocomire negative melt cursor");
        AssertThrows<ArgumentOutOfRangeException>(
            () => CrocomireMeltingDefinitions.SelectColumn(49),
            "Crocomire melt cursor after authored table");

        VerifyCrocomireMeltingTransferCatalog(rom);
        VerifyCrocomireMeltingProductionSequence(rom);
        Console.WriteLine(
            "Crocomire melting definitions: all 49 column selectors, eight masks, " +
            "both native transfer passes, the complete production erase sequence, " +
            "and source-table read guards pass.");
    }

    private static void VerifyCrocomireMeltingTransferCatalog(SuperMetroidAddressSpace rom)
    {
        static ushort NativeWord(ISnesAddressSpace bus, int address) =>
            (ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8));

        foreach (CrocomireMeltingPass pass in CrocomireMeltingTransferDefinitions.Passes)
        {
            int header = CrocomireMeltingTransferDefinitions.NativeSourceAddress +
                pass.HeaderOffset;
            AssertEqual(pass.MaximumAdjustedDestinationY, NativeWord(rom, header),
                $"Crocomire melt pass ${pass.HeaderOffset:X4} maximum Y");
            AssertEqual(pass.DistortionEndY, NativeWord(rom, header + 2),
                $"Crocomire melt pass ${pass.HeaderOffset:X4} distortion end Y");
            AssertEqual(pass.WordsToCopy, NativeWord(rom, header + 4),
                $"Crocomire melt pass ${pass.HeaderOffset:X4} copy word count");
            AssertEqual(pass.SourceBank, rom.ReadByte(header + 6),
                $"Crocomire melt pass ${pass.HeaderOffset:X4} source bank");

            for (int index = 0; index < pass.Copies.Length; index++)
            {
                int source = header + 8 + index * 4;
                CrocomireMeltingCopy copy = pass.Copies.Span[index];
                AssertEqual(copy.SourceWord, NativeWord(rom, source),
                    $"Crocomire melt pass ${pass.HeaderOffset:X4} copy {index} source");
                AssertEqual(copy.DestinationWord, NativeWord(rom, source + 2),
                    $"Crocomire melt pass ${pass.HeaderOffset:X4} copy {index} destination");
            }
            int copyEnd = header + 8 + pass.Copies.Length * 4;
            AssertEqual((ushort)0xffff, NativeWord(rom, copyEnd),
                $"Crocomire melt pass ${pass.HeaderOffset:X4} copy terminator");
            AssertEqual(pass.TransferStartOffset,
                (ushort)(copyEnd + 2 - CrocomireMeltingTransferDefinitions.NativeSourceAddress),
                $"Crocomire melt pass ${pass.HeaderOffset:X4} transfer start");

            for (int index = 0; index < pass.Uploads.Length; index++)
            {
                int offset = pass.TransferStartOffset + index * 8;
                int source = CrocomireMeltingTransferDefinitions.NativeSourceAddress + offset;
                CrocomireMeltingUpload upload = pass.Uploads.Span[index];
                AssertEqual(upload.ByteCount, NativeWord(rom, source),
                    $"Crocomire melt pass ${pass.HeaderOffset:X4} upload {index} size");
                AssertEqual(upload.DestinationWord, NativeWord(rom, source + 2),
                    $"Crocomire melt pass ${pass.HeaderOffset:X4} upload {index} destination");
                AssertEqual(upload.SourceBank, rom.ReadByte(source + 4),
                    $"Crocomire melt pass ${pass.HeaderOffset:X4} upload {index} bank");
                AssertEqual(upload.SourceWord, NativeWord(rom, source + 6),
                    $"Crocomire melt pass ${pass.HeaderOffset:X4} upload {index} source");
                AssertTrue(CrocomireMeltingTransferDefinitions.TryUpload(offset,
                    out CrocomireMeltingUpload selected) && selected == upload,
                    $"Crocomire melt pass ${pass.HeaderOffset:X4} upload {index} lookup");
            }
            AssertEqual(pass.TransferEndOffset,
                (ushort)(pass.TransferStartOffset + pass.Uploads.Length * 8),
                $"Crocomire melt pass ${pass.HeaderOffset:X4} transfer end");
            AssertEqual((ushort)0xffff, NativeWord(rom,
                CrocomireMeltingTransferDefinitions.NativeSourceAddress + pass.TransferEndOffset),
                $"Crocomire melt pass ${pass.HeaderOffset:X4} transfer terminator");
            AssertTrue(!CrocomireMeltingTransferDefinitions.TryUpload(
                pass.TransferEndOffset, out _),
                $"Crocomire melt pass ${pass.HeaderOffset:X4} terminal lookup");
            AssertEqual(pass.NextHeaderOffset, (ushort)(pass.TransferEndOffset + 2),
                $"Crocomire melt pass ${pass.HeaderOffset:X4} next header");
        }
        VerifyCrocomireMeltingGraphicsProduction(rom);
        AssertEqual(CrocomireMeltingTransferDefinitions.NativeByteCount,
            CrocomireMeltingTransferDefinitions.Passes[^1].NextHeaderOffset,
            "Crocomire melt compiled table length");
        AssertThrows<InvalidDataException>(
            () => CrocomireMeltingTransferDefinitions.TryUpload(0x0054, out _),
            "Crocomire melt header cannot be used as an upload");
    }

    private static void VerifyCrocomireMeltingGraphicsProduction(SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        foreach (CrocomireMeltingPass pass in CrocomireMeltingTransferDefinitions.Passes)
        {
            var enemies = new RoomEnemySystem();
            var state = new CrocomireEnemyState(enemies.Slots[0]);
            var death = new CrocomireDeathState { MeltingTableOffset = pass.HeaderOffset };
            var vram = new SnesVram();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(
                enemies, new CrocomireMeltingDefinitionReadGuard(rom));
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
            foreach (CrocomireMeltingCopy copy in pass.Copies.Span)
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

            foreach (CrocomireMeltingUpload record in pass.Uploads.Span)
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
        foreach (CrocomireMeltingUpload record in pass.Uploads.Span)
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

    private sealed class CrocomireMeltingDefinitionReadGuard(
        ISnesAddressSpace source, bool blockGraphics = false) :
        ISnesAddressSpace
    {
        public byte ReadByte(int address)
        {
            if (address is
                >= 0xa49697 and < 0xa496c8 or
                >= 0xa49bbd and < 0xa49bc5 or
                >= CrocomireMeltingTransferDefinitions.NativeSourceAddress and
                    < CrocomireMeltingTransferDefinitions.NativeSourceAddress +
                        CrocomireMeltingTransferDefinitions.NativeByteCount)
                throw new InvalidOperationException(
                    $"Crocomire melting attempted migrated definition read ${address:X6}.");
            if (blockGraphics)
            {
                foreach (CrocomireMeltingPass pass in CrocomireMeltingTransferDefinitions.Passes)
                foreach (CrocomireMeltingCopy copy in pass.Copies.Span)
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
