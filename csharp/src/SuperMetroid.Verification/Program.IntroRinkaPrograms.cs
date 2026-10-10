using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// Checks that compiled Rinka instruction bytes match the cartridge, including terminal and out-of-range reads.
    /// </summary>
    /// <param name="rom">The address space containing the original Rinka instruction stream.</param>
    private static void VerifyIntroRinkaPrograms(ISnesAddressSpace rom)
    {
        for (int pointer = 0xcdeb; pointer < 0xce1b; pointer++)
        {
            AssertEqual(rom.ReadByte(0x8b0000 + pointer), IntroRinkaInstructionDefinitions.ReadByte((ushort)pointer), "original Rinka program byte");
            if (pointer < 0xce1a)
                AssertEqual((ushort)(rom.ReadByte(0x8b0000 + pointer) | rom.ReadByte(0x8b0001 + pointer) << 8),
                    IntroRinkaInstructionDefinitions.ReadWord((ushort)pointer), "original Rinka overlapping word");
        }
        foreach (ushort pointer in new ushort[] { 0, 0xcdea, 0xce1b, ushort.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => IntroRinkaInstructionDefinitions.ReadByte(pointer), "Rinka byte boundary");
            AssertThrows<InvalidDataException>(() => IntroRinkaInstructionDefinitions.ReadWord(pointer), "Rinka word boundary");
        }
        AssertThrows<InvalidDataException>(() => IntroRinkaInstructionDefinitions.ReadWord(0xce1a), "Rinka terminal word crossing");
    }

    /// <summary>
    /// Checks that published Rinka frame keys and pointers preserve the cartridge's three native records and their order.
    /// </summary>
    /// <param name="rom">The address space used to resolve native frame-table operands and record sizes.</param>
    private static void VerifyIntroRinkaFrameCatalog(ISnesAddressSpace rom)
    {
        string[] names = ["rinka-0", "rinka-1", "rinka-2"];
        var frames = IntroRinkaSpriteDefinitions.Frames;
        AssertEqual(3, frames.Count, "three original Rinka frames");
        for (int index = 0; index < 3; index++)
        {
            int operand = 0x8bcded + 4 * index;
            ushort pointer = (ushort)(rom.ReadByte(operand) | rom.ReadByte(operand + 1) << 8);
            AssertEqual(pointer, frames[index].Pointer, "native Rinka frame operand");
            AssertEqual(names[index], frames[index].Name, "published Rinka asset key");
            AssertEqual(IntroRinkaSpriteDefinitions.StockPartCount,
                rom.ReadByte(0x8c0000 + pointer) | rom.ReadByte(0x8c0001 + pointer) << 8, "native four-part Rinka record");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 3, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => { _ = frames[invalid]; }, "Rinka catalog bounds");
            AssertThrows<ArgumentOutOfRangeException>(() => IntroRinkaSpriteDefinitions.FramePointer(invalid), "Rinka pointer bounds");
        }
        AssertTrue(frames.SequenceEqual(frames.ToArray()), "Rinka enumeration order");
    }
}
