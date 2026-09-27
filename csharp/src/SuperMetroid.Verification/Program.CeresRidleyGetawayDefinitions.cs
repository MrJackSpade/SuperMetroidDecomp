using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyCompiledCeresRidleyGetaway(SuperMetroidAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        for (ushort offset = 0; offset <= 224; offset += 2)
        {
            var frame = CeresRidleyGetawayDefinitions.FromByteIndex(offset);
            AssertEqual(Word(0xa6ae4d + offset), frame.Zoom, "Ceres getaway exact native zoom");
            if (offset == 224)
            {
                AssertEqual(CeresRidleyGetawayDefinitions.Finished, frame.Zoom, "Ceres getaway terminator");
                continue;
            }
            AssertEqual(Word(0xa6af2f + offset), frame.YVelocity, "Ceres getaway exact native Y increment");
            AssertEqual(Word(0xa6b00f + offset), frame.XVelocity, "Ceres getaway exact native X subtraction");
        }
        foreach (ushort invalid in new ushort[] { 1, 223, 225, 226, 65535 })
            AssertThrows<InvalidDataException>(() => CeresRidleyGetawayDefinitions.FromByteIndex(invalid), "Ceres invalid curve index");
        VerifyCompiledCeresRidleyMode7Transfers(rom);
        Console.WriteLine("Ceres Ridley compiled getaway: all 337 native curve words match, including irregular zoom steps and terminal frame.");
    }

    private static void VerifyCompiledCeresRidleyMode7Transfers(SuperMetroidAddressSpace rom)
    {
        ushort[] pointers =
        [
            CeresMode7TransferDefinitions.ElevatorLight,
            CeresMode7TransferDefinitions.ElevatorDark,
            CeresMode7TransferDefinitions.BabyFrame0,
            CeresMode7TransferDefinitions.BabyFrame1,
            CeresMode7TransferDefinitions.BabyFrame2,
            CeresMode7TransferDefinitions.WingFrame0,
            CeresMode7TransferDefinitions.WingFrame1,
        ];
        int checkedTransfers = 0;
        foreach (ushort pointer in pointers)
        {
            ReadOnlySpan<CeresMode7Transfer> compiled =
                CeresMode7TransferDefinitions.Get(pointer);
            int cursor = 0xa60000 | pointer;
            var expectedVram = new byte[SnesVram.ByteCount];
            Array.Fill(expectedVram, (byte)0x5a);
            var actualVram = new SnesVram();
            actualVram.LoadBytes(0, expectedVram);
            foreach (CeresMode7Transfer transfer in compiled)
            {
                AssertEqual((byte)0x80, rom.ReadByte(cursor),
                    "Ceres Mode 7 native transfer control");
                int source = rom.ReadByte(cursor + 1) |
                    rom.ReadByte(cursor + 2) << 8 |
                    rom.ReadByte(cursor + 3) << 16;
                ushort size = (ushort)(rom.ReadByte(cursor + 4) | rom.ReadByte(cursor + 5) << 8);
                ushort destination = (ushort)(rom.ReadByte(cursor + 6) | rom.ReadByte(cursor + 7) << 8);
                AssertEqual((byte)0, rom.ReadByte(cursor + 8),
                    "Ceres Mode 7 native VRAM increment mode");
                AssertEqual(source, transfer.SourceAddress,
                    "Ceres Mode 7 compiled source identity");
                AssertEqual(size, transfer.TileNumbers.Length,
                    "Ceres Mode 7 compiled byte count");
                AssertEqual(destination, transfer.DestinationWord,
                    "Ceres Mode 7 compiled VRAM destination");
                for (int index = 0; index < size; index++)
                {
                    byte value = rom.ReadByte(source + index);
                    AssertEqual(value, transfer.TileNumbers.Span[index],
                        "Ceres Mode 7 compiled tilemap source byte");
                    expectedVram[((destination + index) & 0x7fff) * 2] = value;
                }
                cursor += 9;
                checkedTransfers++;
            }
            AssertEqual((byte)0, rom.ReadByte(cursor),
                "Ceres Mode 7 compiled list reaches native terminator");
            CeresMode7TransferDefinitions.ApplyTo(actualVram, pointer);
            AssertTrue(actualVram.Bytes.SequenceEqual(expectedVram),
                "Ceres Mode 7 compiled list preserves low-byte-only VRAM effects and ordering");
        }
        AssertThrows<InvalidDataException>(() =>
            CeresMode7TransferDefinitions.Get(0xffff),
            "Ceres Mode 7 unknown transfer list fails loudly");
        Console.WriteLine($"Ceres compiled Mode 7: {checkedTransfers} native transfers across all seven lists match source bytes, destinations, and VRAM effects.");
    }
}
