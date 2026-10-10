using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyTorizoInstructionVramArtwork(
        ISnesAddressSpace rom, string stockDirectory, EnemyTileArtworkCatalog stock)
    {
        AssertEqual(52, TorizoInstructionVramTransferDefinitions.All.Length,
            "all bank-AA Torizo $814B transfer descriptors are catalogued");
        AssertTrue(stock.TorizoInstructionVram is not null,
            "installed Torizo instruction tile pages exist");
        var installedEnemies = new RoomEnemySystem { TileArtwork = stock };
        const BindingFlags instanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;
        typeof(RoomEnemySystem).GetField("_bus", instanceFlags)!
            .SetValue(installedEnemies, new ForbiddenTorizoRomBus());
        var installedVram = new SnesVram();
        typeof(RoomEnemySystem).GetField("_vram", instanceFlags)!
            .SetValue(installedEnemies, installedVram);
        MethodInfo applyTransfer = typeof(RoomEnemySystem).GetMethod(
            "ApplyEnemyInstructionVramTransfer", instanceFlags)!;
        RoomEnemySlot installedSlot = installedEnemies.Slots[0];
        installedSlot.EnemyDefinitionPointer = EnemyDefinitionId.GoldenTorizo;
        installedSlot.Definition = default(RoomEnemyDefinition) with
        {
            Bank = TorizoInstructionVramTransferDefinitions.Bank,
        };

        ushort previousInstruction = 0;
        var authoredOpcodes = new HashSet<ushort>();
        foreach (TorizoInstructionVramTransferDefinition transfer in
                 TorizoInstructionVramTransferDefinitions.All)
        {
            AssertTrue(transfer.Instruction > previousInstruction,
                "Torizo transfer opcodes are distinct and sorted");
            previousInstruction = transfer.Instruction;
            authoredOpcodes.Add(transfer.Instruction);
            AssertEqual((ushort)CommonEnemyInstruction.CopyToVram,
                ReadWord(0xaa0000 | transfer.Instruction),
                $"Torizo transfer $AA:{transfer.Instruction:X4} opcode");
            int descriptor = 0xaa0000 | unchecked((ushort)(transfer.Instruction + 2));
            AssertEqual(transfer.ByteCount, ReadWord(descriptor),
                $"Torizo transfer $AA:{transfer.Instruction:X4} byte count");
            int nativeSource = rom.ReadByte(descriptor + 2) |
                rom.ReadByte(descriptor + 3) << 8 |
                rom.ReadByte(descriptor + 4) << 16;
            AssertEqual(transfer.SourceAddress, nativeSource,
                $"Torizo transfer $AA:{transfer.Instruction:X4} source");
            AssertEqual(transfer.DestinationWord, ReadWord(descriptor + 5),
                $"Torizo transfer $AA:{transfer.Instruction:X4} VRAM destination");
            AssertTrue(TorizoInstructionVramTransferDefinitions.TryGet(
                    transfer.Instruction, out TorizoInstructionVramTransferDefinition selected) &&
                       selected == transfer,
                $"Torizo transfer $AA:{transfer.Instruction:X4} lookup");
            AssertTrue(stock.TorizoInstructionVram!.TryResolve(
                    transfer.SourceAddress, transfer.ByteCount,
                    out ReadOnlyMemory<byte> installed),
                $"Torizo transfer $AA:{transfer.Instruction:X4} installed tile source");
            for (int offset = 0; offset < transfer.ByteCount; offset++)
                AssertEqual(rom.ReadByte(transfer.SourceAddress + offset),
                    installed.Span[offset],
                    $"Torizo transfer $AA:{transfer.Instruction:X4} byte {offset:X4}");
            applyTransfer.Invoke(installedEnemies,
                [installedSlot, transfer.Instruction]);
            for (int offset = 0; offset < transfer.ByteCount; offset++)
                AssertEqual(installed.Span[offset],
                    installedVram.ReadByte(transfer.DestinationWord * 2 + offset),
                    $"production Torizo upload $AA:{transfer.Instruction:X4} byte {offset:X4}");
        }

        // Detect a missed valid $814B descriptor in the native instruction
        // region, not merely incorrect values in the hand-catalogued records.
        for (int address = 0xb000; address < 0xd369; address++)
        {
            int opcode = 0xaa0000 | address;
            if (ReadWord(opcode) != (ushort)CommonEnemyInstruction.CopyToVram)
                continue;
            int descriptor = opcode + 2;
            ushort byteCount = ReadWord(descriptor);
            int source = rom.ReadByte(descriptor + 2) |
                rom.ReadByte(descriptor + 3) << 8 |
                rom.ReadByte(descriptor + 4) << 16;
            ushort destination = ReadWord(descriptor + 5);
            if (byteCount is 0 or > 0x2000 ||
                (source >> 16) < 0x80 || destination > 0x7fff)
                continue;
            AssertTrue(authoredOpcodes.Contains(unchecked((ushort)address)),
                $"native Torizo $814B descriptor $AA:{address:X4} is catalogued");
        }
        AssertTrue(!TorizoInstructionVramTransferDefinitions.TryGet(0xb879, out _),
            "non-transfer Torizo instruction is rejected");

        TorizoInstructionTileSheetDefinition golden =
            TorizoInstructionVramArtworkDefinitions.GoldenAwakening;
        string overrideDirectory = Path.Combine(stockDirectory,
            "torizo-instruction-tile-overrides");
        Directory.CreateDirectory(overrideDirectory);
        using (var input = new MemoryStream(File.ReadAllBytes(
                   Path.Combine(stockDirectory, golden.FileName)), writable: false))
        {
            int tiles = golden.ByteCount / RoomCharacterAtlasFormat.BytesPerTile;
            int columns = Math.Min(tiles, RoomCharacterAtlasFormat.TileColumns);
            int rows = (tiles + columns - 1) / columns;
            IndexedPngImage image = IndexedPng.Read(input, columns * 8, rows * 8);
            image.Pixels[0] ^= 1;
            using var output = File.Create(Path.Combine(overrideDirectory,
                golden.FileName));
            IndexedPng.Write(output, image.Width, image.Height,
                image.Pixels, image.Palette);
        }
        EnemyTileArtworkCatalog edited = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        AssertTrue(stock.TorizoInstructionVram!.TryResolve(
                golden.SourceAddress, golden.ByteCount,
                out ReadOnlyMemory<byte> stockBytes),
            "stock Golden Torizo upload remains addressable");
        AssertTrue(edited.TorizoInstructionVram!.TryResolve(
                golden.SourceAddress, golden.ByteCount,
                out ReadOnlyMemory<byte> editedBytes),
            "edited Golden Torizo upload remains addressable");
        AssertEqual((byte)(stockBytes.Span[0] ^ 0x80), editedBytes.Span[0],
            "Golden Torizo PNG edit changes the first native tile-plane byte");
        AssertTrue(stockBytes.Span[1..].SequenceEqual(editedBytes.Span[1..]),
            "Golden Torizo PNG edit leaves every other tile byte intact");
        var editedEnemies = new RoomEnemySystem { TileArtwork = edited };
        typeof(RoomEnemySystem).GetField("_bus", instanceFlags)!
            .SetValue(editedEnemies, new ForbiddenTorizoRomBus());
        var editedVram = new SnesVram();
        typeof(RoomEnemySystem).GetField("_vram", instanceFlags)!
            .SetValue(editedEnemies, editedVram);
        RoomEnemySlot editedSlot = editedEnemies.Slots[0];
        editedSlot.EnemyDefinitionPointer = EnemyDefinitionId.GoldenTorizo;
        editedSlot.Definition = installedSlot.Definition;
        applyTransfer.Invoke(editedEnemies,
            [editedSlot, GoldenTorizoInitialInstructionProgramDefinitions.Initial]);
        AssertEqual(editedBytes.Span[0], editedVram.ReadByte(0x6d00 * 2),
            "edited Golden Torizo PNG changes production VRAM upload");
        AssertTrue(EnemyTileArtworkFiles.Load(stockDirectory, overrideDirectory)
                .TorizoInstructionVram!.TryResolve(golden.SourceAddress,
                    golden.ByteCount, out ReadOnlyMemory<byte> reloaded) &&
                   reloaded.Span.SequenceEqual(editedBytes.Span),
            "Golden Torizo tile override survives catalog reload");
        File.WriteAllBytes(Path.Combine(overrideDirectory, golden.FileName), [1, 2, 3]);
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, overrideDirectory),
            "invalid Golden Torizo PNG fails loudly");

        Console.WriteLine(
            "Torizo instruction VRAM: 52 native descriptors, eight installed tile PNGs, " +
            "byte parity, live edit, reload, and invalid-asset rejection pass.");

        ushort ReadWord(int address) =>
            (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
    }

    private sealed class ForbiddenTorizoRomBus : ISnesAddressSpace, IImportCartridgeSource
    {
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        public static byte ReadByte(int address) => throw new InvalidOperationException(
            $"Installed Torizo transfer reread cartridge byte ${address:X6}.");

        public void WriteByte(int address, byte value) => throw new InvalidOperationException(
            $"Installed Torizo transfer wrote CPU bus byte ${address:X6}.");
    }
}
