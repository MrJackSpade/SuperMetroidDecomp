using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Checks the nine helmet spritemap IDs and verifies native animation timing, terminal clamping, and selector bounds.</summary>
    /// <param name="rom">The cartridge address space supplying the original animation operands.</param>
    private static void VerifyFileSelectHelmetAnimation(ISnesAddressSpace rom)
    {
        AssertEqual(9, FileSelectHelmetAnimation.NativeEntryCount, "complete native helmet mapping");
        for (int frame = 0; frame < 9; frame++)
            AssertEqual(ReadVerificationWord(rom, 0x819e2c + 2 * frame),
                FileSelectHelmetAnimation.SpritemapId(frame), $"original helmet sprite {frame}");
        AssertEqual((byte)0xa9, rom.ReadByte(0x819df3), "native helmet timer LDA");
        AssertEqual((int)ReadVerificationWord(rom, 0x819df4), FileSelectHelmetAnimation.FrameDuration,
            "native helmet frame duration");
        AssertEqual((byte)0xc9, rom.ReadByte(0x819dfd), "native helmet frame CMP");
        AssertEqual((int)ReadVerificationWord(rom, 0x819dfe), FileSelectHelmetAnimation.FrameCount,
            "native helmet frame count");
        AssertEqual((byte)0xa9, rom.ReadByte(0x819e08), "native helmet terminal LDA");
        AssertEqual((int)ReadVerificationWord(rom, 0x819e09), FileSelectHelmetAnimation.FrameCount - 1,
            "native helmet terminal frame clamp");
        foreach (int invalid in new[] { int.MinValue, -1, 9, 65536, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => FileSelectHelmetAnimation.SpritemapId(invalid),
                "unsupported helmet sample");
    }
}
