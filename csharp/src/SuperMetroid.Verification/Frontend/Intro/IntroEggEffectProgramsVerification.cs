using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Frontend.Intro;

/// <summary>Compares compiled intro egg-effect scripts and frame identities with the retail cartridge.</summary>
internal static class IntroEggEffectProgramsVerification
{
    /// <summary>Checks byte and overlapping-word views of both lists, including terminal and outside-pointer failures.</summary>
    internal static void VerifyIntroEggEffectPrograms(ISnesAddressSpace rom)
    {
        foreach ((int start, int end) in new[] { (0xcd39, 0xcd83), (0xce53, 0xce55) })
        {
            for (int pointer = start; pointer < end; pointer++)
            {
                AssertEqual(IntroEggEffectProgramsRomByte(rom, 0x8b0000 + pointer), IntroEggEffectInstructionDefinitions.ReadByte((ushort)pointer), "original egg-effect byte");
                if (pointer < end - 1)
                    AssertEqual((ushort)(SnesLittleEndian.Word(IntroEggEffectProgramsRomByte(rom, 0x8b0000 + pointer), IntroEggEffectProgramsRomByte(rom, 0x8b0001 + pointer))),
                        IntroEggEffectInstructionDefinitions.ReadWord((ushort)pointer), "original egg-effect overlapping word");
            }
            AssertThrows<InvalidDataException>(() => IntroEggEffectInstructionDefinitions.ReadWord((ushort)(end - 1)), "egg-effect terminal word crossing");
        }
        foreach (ushort pointer in new ushort[] { 0, 0xcd38, 0xcd83, 0xce52, 0xce55, ushort.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => IntroEggEffectInstructionDefinitions.ReadByte(pointer), "egg-effect byte boundary");
            AssertThrows<InvalidDataException>(() => IntroEggEffectInstructionDefinitions.ReadWord(pointer), "egg-effect word boundary");
        }
    }

    /// <summary>Reads one original bank-$8B script byte for comparison with the compiled program.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for intro egg effect programs")]
    private static byte IntroEggEffectProgramsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
    /// <summary>Checks eleven published frame names, native operands, OAM part counts, and catalog bounds.</summary>
    internal static void VerifyIntroEggEffectFrameCatalog(ISnesAddressSpace rom)
    {
        string[] names = ["fragment-0", "fragment-1", "fragment-2", "fragment-3", "fragment-4", "fragment-5",
            "slime-moving", "slime-impact-0", "slime-impact-1", "slime-impact-2", "slime-impact-3"];
        var frames = IntroEggEffectSpriteDefinitions.Frames;
        AssertEqual(11, frames.Count, "eleven egg-effect frames");
        for (int index = 0; index < 11; index++)
        {
            int operand = index < 7 ? 0x8bcd3b + 8 * index : 0x8bcd73 + 4 * (index - 7);
            ushort pointer = (ushort)(SnesLittleEndian.Word(IntroEggEffectFrameCatalogRomByte(rom, operand), IntroEggEffectFrameCatalogRomByte(rom, operand + 1)));
            AssertEqual(pointer, frames[index].Pointer, "original egg-effect frame operand");
            AssertEqual(names[index], frames[index].Name, "published egg-effect name");
            AssertEqual(IntroEggEffectSpriteDefinitions.StockPartCount,
                SnesLittleEndian.Word(IntroEggEffectFrameCatalogRomByte(rom, 0x8c0000 + pointer), IntroEggEffectFrameCatalogRomByte(rom, 0x8c0001 + pointer)), "original one-part egg-effect record");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 11, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => { _ = frames[invalid]; }, "egg-effect catalog boundary");
            AssertThrows<ArgumentOutOfRangeException>(() => IntroEggEffectSpriteDefinitions.FramePointer(invalid), "egg-effect pointer boundary");
        }
        AssertTrue(frames.SequenceEqual(frames.ToArray()), "egg-effect enumeration order");
    }

    /// <summary>Reads one original frame-operand or spritemap byte for the catalog comparison.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for intro egg effect frame catalog")]
    private static byte IntroEggEffectFrameCatalogRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
