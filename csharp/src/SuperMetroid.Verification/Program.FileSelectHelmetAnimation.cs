using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using static DoorDefinitionsVerification;

/// <summary>Represents file select helmet animation verification.</summary>
internal static class FileSelectHelmetAnimationVerification
{
    /// <summary>Verifies file select helmet animation.</summary>
    internal static void VerifyFileSelectHelmetAnimation(ISnesAddressSpace rom)
    {
        AssertEqual(9, FileSelectHelmetAnimation.NativeEntryCount, "complete native helmet mapping");
        for (int frame = 0; frame < 9; frame++)
            AssertEqual(ReadVerificationWord(rom, 0x819e2c + 2 * frame),
                FileSelectHelmetAnimation.SpritemapId(frame), $"original helmet sprite {frame}");
        AssertEqual((byte)0xa9, FileSelectHelmetAnimationRomByte(rom, 0x819df3), "native helmet timer LDA");
        AssertEqual((int)ReadVerificationWord(rom, 0x819df4), FileSelectHelmetAnimation.FrameDuration,
            "native helmet frame duration");
        AssertEqual((byte)0xc9, FileSelectHelmetAnimationRomByte(rom, 0x819dfd), "native helmet frame CMP");
        AssertEqual((int)ReadVerificationWord(rom, 0x819dfe), FileSelectHelmetAnimation.FrameCount,
            "native helmet frame count");
        AssertEqual((byte)0xa9, FileSelectHelmetAnimationRomByte(rom, 0x819e08), "native helmet terminal LDA");
        AssertEqual((int)ReadVerificationWord(rom, 0x819e09), FileSelectHelmetAnimation.FrameCount - 1,
            "native helmet terminal frame clamp");
        foreach (int invalid in new[] { int.MinValue, -1, 9, 65536, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => FileSelectHelmetAnimation.SpritemapId(invalid),
                "unsupported helmet sample");
    }

    /// <summary>File select helmet animation rom byte.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for file select helmet animation")]
    private static byte FileSelectHelmetAnimationRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
