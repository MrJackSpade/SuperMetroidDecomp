using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Frontend.FileSelect;

/// <summary>Checks file-select helmet spritemap and timing definitions against the native bank-$81 animation.</summary>
internal static class FileSelectHelmetAnimationVerification
{
    /// <summary>Compares all nine native selectors, duration and terminal clamp instructions, and rejects invalid frame indices.</summary>
    internal static void VerifyFileSelectHelmetAnimation(ISnesAddressSpace rom)
    {
        AssertEqual(9, FileSelectHelmetAnimation.EntryCount, "complete native helmet mapping");
        for (int frame = 0; frame < 9; frame++)
            AssertEqual(ReadVerificationWord(rom, 0x819e2c + 2 * frame),
                FileSelectHelmetAnimation.SpritemapId(frame), $"original helmet sprite {frame}");
        AssertEqual((byte)0xa9, FileSelectHelmetAnimationRomByte(rom, 0x819df3), "native helmet timer LDA");
        AssertEqual(ReadVerificationWord(rom, 0x819df4), FileSelectHelmetAnimation.FrameDuration,
            "native helmet frame duration");
        AssertEqual((byte)0xc9, FileSelectHelmetAnimationRomByte(rom, 0x819dfd), "native helmet frame CMP");
        AssertEqual(ReadVerificationWord(rom, 0x819dfe), FileSelectHelmetAnimation.FrameCount,
            "native helmet frame count");
        AssertEqual((byte)0xa9, FileSelectHelmetAnimationRomByte(rom, 0x819e08), "native helmet terminal LDA");
        AssertEqual(ReadVerificationWord(rom, 0x819e09), FileSelectHelmetAnimation.FrameCount - 1,
            "native helmet terminal frame clamp");
        foreach (int invalid in new[] { int.MinValue, -1, 9, 65536, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => FileSelectHelmetAnimation.SpritemapId(invalid),
                "unsupported helmet sample");
    }

    /// <summary>Reads a native helmet-animation instruction byte solely for the cartridge-oracle comparison.</summary>
    private static byte FileSelectHelmetAnimationRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
