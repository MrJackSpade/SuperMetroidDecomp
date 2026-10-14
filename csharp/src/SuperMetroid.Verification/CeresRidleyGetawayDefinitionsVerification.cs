using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static SuiteTimingVerification;

/// <summary>Checks Ceres Ridley's getaway curve and Mode 7 transfer lists against retail data.</summary>
internal static class CeresRidleyGetawayDefinitionsVerification
{
    /// <summary>Compares zoom and both velocity components through the terminal frame.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for compiled Ceres Ridley getaway")]
    internal static void VerifyCompiledCeresRidleyGetaway(SuperMetroidAddressSpace rom)
    {
        for (ushort offset = 0; offset <= 224; offset += 2)
        {
            var frame = CeresRidleyGetawayDefinitions.FromByteIndex(offset);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa6ae4d + offset)), frame.Zoom, "Ceres getaway exact native zoom");
            if (offset == 224)
            {
                AssertEqual(CeresRidleyGetawayDefinitions.Finished, frame.Zoom, "Ceres getaway terminator");
                continue;
            }
            AssertEqual(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa6af2f + offset))), frame.Velocity.Y, "Ceres getaway exact native Y increment");
            AssertEqual(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa6b00f + offset))), frame.Velocity.X, "Ceres getaway exact native X subtraction");
        }
        foreach (ushort invalid in new ushort[] { 1, 223, 225, 226, 65535 })
            AssertThrows<InvalidDataException>(() => CeresRidleyGetawayDefinitions.FromByteIndex(invalid), "Ceres invalid curve index");
        Suite(nameof(VerifyCompiledCeresRidleyMode7Transfers), () => VerifyCompiledCeresRidleyMode7Transfers(rom));
        Console.WriteLine("Ceres Ridley compiled getaway: all 337 native curve words match, including irregular zoom steps and terminal frame.");
    }

    /// <summary>Checks seven compiled transfer lists and their low-byte-only VRAM effects.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for compiled Ceres Ridley Mode 7 transfers")]
    internal static void VerifyCompiledCeresRidleyMode7Transfers(SuperMetroidAddressSpace rom)
    {
        int checkedTransfers = 0;
        foreach (CeresMode7TransferList pointer in Enum.GetValues<CeresMode7TransferList>())
        {
            CeresMode7TransferDefinitions.TransferSequence compiled =
                CeresMode7TransferDefinitions.Get(pointer);
            int cursor = 0xa60000 | (ushort)pointer;
            var expectedVram = new byte[SnesVram.ByteCount];
            Array.Fill(expectedVram, (byte)0x5a);
            var actualVram = new SnesVram();
            actualVram.LoadBytes(0, expectedVram);
            foreach (CeresMode7Transfer transfer in compiled)
            {
                AssertEqual((byte)0x80, rom.ReadCartridgeByte(cursor),
                    "Ceres Mode 7 native transfer control");
                int source = (CartridgeImportSource.Require(rom).ReadCartridgeLong(cursor + 1));
                ushort size = CartridgeImportSource.Require(rom).ReadCartridgeWord(cursor + 4);
                ushort destination = CartridgeImportSource.Require(rom).ReadCartridgeWord(cursor + 6);
                AssertEqual((byte)0, rom.ReadCartridgeByte(cursor + 8),
                    "Ceres Mode 7 native VRAM increment mode");
                AssertEqual(size, transfer.TileNumbers.Length,
                    "Ceres Mode 7 compiled byte count");
                AssertEqual(destination, transfer.DestinationWord,
                    "Ceres Mode 7 compiled VRAM destination");
                for (int index = 0; index < size; index++)
                {
                    byte value = rom.ReadCartridgeByte(source + index);
                    AssertEqual(value, transfer.TileNumbers[index],
                        "Ceres Mode 7 compiled tilemap source byte");
                    expectedVram[((destination + index) & 0x7fff) * 2] = value;
                }
                cursor += 9;
                checkedTransfers++;
            }
            AssertEqual((byte)0, rom.ReadCartridgeByte(cursor),
                "Ceres Mode 7 compiled list reaches native terminator");
            CeresMode7TransferDefinitions.ApplyTo(actualVram, pointer);
            AssertTrue(actualVram.Bytes.SequenceEqual(expectedVram),
                "Ceres Mode 7 compiled list preserves low-byte-only VRAM effects and ordering");
        }
        AssertThrows<InvalidDataException>(() =>
            CeresMode7TransferDefinitions.Get((CeresMode7TransferList)0xffff),
            "Ceres Mode 7 unknown transfer list fails loudly");
        Console.WriteLine($"Ceres compiled Mode 7: {checkedTransfers} native transfers across all seven lists match source bytes, destinations, and VRAM effects.");
    }
}
