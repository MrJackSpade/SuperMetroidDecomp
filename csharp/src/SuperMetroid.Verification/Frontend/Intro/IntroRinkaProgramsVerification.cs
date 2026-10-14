using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Frontend.Intro;

/// <summary>Checks compiled intro Rinka instruction bytes and frame catalog against native records.</summary>
internal static class IntroRinkaProgramsVerification
{
    /// <summary>Compares every Rinka program byte and overlapping word, including terminal boundaries.</summary>
    internal static void VerifyIntroRinkaPrograms(ISnesAddressSpace rom)
    {
        for (int pointer = 0xcdeb; pointer < 0xce1b; pointer++)
        {
            AssertEqual(IntroRinkaProgramsRomByte(rom, 0x8b0000 + pointer), IntroRinkaInstructionDefinitions.ReadByte((ushort)pointer), "original Rinka program byte");
            if (pointer < 0xce1a)
                AssertEqual((ushort)(SnesLittleEndian.Word(IntroRinkaProgramsRomByte(rom, 0x8b0000 + pointer), IntroRinkaProgramsRomByte(rom, 0x8b0001 + pointer))),
                    IntroRinkaInstructionDefinitions.ReadWord((ushort)pointer), "original Rinka overlapping word");
        }
        foreach (ushort pointer in new ushort[] { 0, 0xcdea, 0xce1b, ushort.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => IntroRinkaInstructionDefinitions.ReadByte(pointer), "Rinka byte boundary");
            AssertThrows<InvalidDataException>(() => IntroRinkaInstructionDefinitions.ReadWord(pointer), "Rinka word boundary");
        }
        AssertThrows<InvalidDataException>(() => IntroRinkaInstructionDefinitions.ReadWord(0xce1a), "Rinka terminal word crossing");
    }

    /// <summary>Reads one native instruction byte solely as a verification oracle.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for intro Rinka programs")]
    private static byte IntroRinkaProgramsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Checks three native frame operands, published names, stock part counts, and catalog bounds.</summary>
    internal static void VerifyIntroRinkaFrameCatalog(ISnesAddressSpace rom)
    {
        string[] names = ["rinka-0", "rinka-1", "rinka-2"];
        var frames = IntroRinkaSpriteDefinitions.Frames;
        AssertEqual(3, frames.Count, "three original Rinka frames");
        for (int index = 0; index < 3; index++)
        {
            int operand = 0x8bcded + 4 * index;
            ushort pointer = (ushort)(SnesLittleEndian.Word(IntroRinkaFrameCatalogRomByte(rom, operand), IntroRinkaFrameCatalogRomByte(rom, operand + 1)));
            AssertEqual(pointer, frames[index].Pointer, "native Rinka frame operand");
            AssertEqual(names[index], frames[index].Name, "published Rinka asset key");
            AssertEqual(IntroRinkaSpriteDefinitions.StockPartCount,
                SnesLittleEndian.Word(IntroRinkaFrameCatalogRomByte(rom, 0x8c0000 + pointer), IntroRinkaFrameCatalogRomByte(rom, 0x8c0001 + pointer)), "native four-part Rinka record");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 3, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => { _ = frames[invalid]; }, "Rinka catalog bounds");
            AssertThrows<ArgumentOutOfRangeException>(() => IntroRinkaSpriteDefinitions.FramePointer(invalid), "Rinka pointer bounds");
        }
        AssertTrue(frames.SequenceEqual(frames.ToArray()), "Rinka enumeration order");
    }

    /// <summary>Reads one native frame-record byte solely as a verification oracle.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for intro Rinka frame catalog")]
    private static byte IntroRinkaFrameCatalogRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
