using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Frontend.Intro;

/// <summary>Checks the compiled eye-animation instruction slice against bank-$8C cartridge bytes.</summary>
internal static class IntroEyeInstructionsVerification
{
    /// <summary>Compares each byte and overlapping word in $D5DF-$D628, then checks both ends and rejected pointers.</summary>
    internal static void VerifyIntroEyeInstructions(ISnesAddressSpace rom)
    {
        for (int pointer = 0xd5df; pointer < 0xd629; pointer++)
        {
            AssertEqual(IntroEyeInstructionsRomByte(rom, 0x8c0000 + pointer), IntroEyeAnimationDefinitions.ReadByte((ushort)pointer), "original eye program byte");
            if (pointer == 0xd628) continue;
            AssertTrue(IntroEyeAnimationDefinitions.TryReadWord((ushort)pointer, out ushort word), "eye word admitted");
            AssertEqual((ushort)(SnesLittleEndian.Word(IntroEyeInstructionsRomByte(rom, 0x8c0000 + pointer), IntroEyeInstructionsRomByte(rom, 0x8c0001 + pointer))), word,
                "original eye aligned or overlapping word");
        }
        foreach (ushort pointer in new ushort[] { 0, 0xd5de, 0xd629, ushort.MaxValue })
        {
            AssertTrue(!IntroEyeAnimationDefinitions.TryReadWord(pointer, out ushort word), "other BG programs rejected");
            AssertEqual((ushort)0, word, "rejected eye word is zero");
            AssertThrows<ArgumentOutOfRangeException>(() => IntroEyeAnimationDefinitions.ReadByte(pointer), "eye byte boundary");
        }
        AssertThrows<InvalidDataException>(() => IntroEyeAnimationDefinitions.TryReadWord(0xd628, out _), "eye word crosses program boundary");
    }

    /// <summary>Reads one cartridge byte solely for the verification oracle.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for intro eye instructions")]
    private static byte IntroEyeInstructionsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
