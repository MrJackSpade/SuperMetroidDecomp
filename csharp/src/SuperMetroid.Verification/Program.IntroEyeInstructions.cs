using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyIntroEyeInstructions(ISnesAddressSpace rom)
    {
        for (int pointer = 0xd5df; pointer < 0xd629; pointer++)
        {
            AssertEqual(rom.ReadByte(0x8c0000 + pointer), IntroEyeAnimationDefinitions.ReadByte((ushort)pointer), "original eye program byte");
            if (pointer == 0xd628) continue;
            AssertTrue(IntroEyeAnimationDefinitions.TryReadWord((ushort)pointer, out ushort word), "eye word admitted");
            AssertEqual((ushort)(rom.ReadByte(0x8c0000 + pointer) | rom.ReadByte(0x8c0001 + pointer) << 8), word,
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
}
