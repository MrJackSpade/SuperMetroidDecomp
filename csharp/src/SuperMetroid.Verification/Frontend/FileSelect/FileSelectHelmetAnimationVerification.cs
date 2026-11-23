using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.AssetExtraction.Frontend.FileSelect;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend;

namespace SuperMetroid.Verification.Frontend.FileSelect;

/// <summary>Checks file-select helmet spritemap and timing definitions against the native bank-$81 animation.</summary>
internal static class FileSelectHelmetAnimationVerification
{
    /// <summary>
    /// Compares the turn's native selectors, the repeated terminal selector the clamp never draws, the duration and the
    /// terminal clamp instructions, and rejects a picture outside the turn.
    /// </summary>
    internal static void VerifyFileSelectHelmetAnimation(ISnesAddressSpace rom)
    {
        int place = 0;
        foreach (FileSelectHelmetFrame frame in FileSelectHelmetAnimation.Turn)
        {
            AssertEqual(ReadVerificationWord(rom, 0x819e2c + 2 * place),
                FileSelectHelmetSpritemaps.SpritemapId(frame), $"original helmet sprite {frame}");
            place++;
        }
        AssertEqual(ReadVerificationWord(rom, 0x819e2c + 2 * place),
            FileSelectHelmetSpritemaps.SpritemapId(FileSelectHelmetAnimation.Turn[^1]), "native repeated terminal helmet sprite");
        AssertEqual((byte)0xa9, FileSelectHelmetAnimationRomByte(rom, 0x819df3), "native helmet timer LDA");
        AssertEqual(ReadVerificationWord(rom, 0x819df4), MenuAnimationTimings.HelmetFrameDuration(rom),
            "the import installs the native helmet frame duration");
        AssertEqual((byte)0xc9, FileSelectHelmetAnimationRomByte(rom, 0x819dfd), "native helmet frame CMP");
        AssertEqual(ReadVerificationWord(rom, 0x819dfe), FileSelectHelmetAnimation.Turn.Count,
            "native helmet frame count");
        AssertEqual((byte)0xa9, FileSelectHelmetAnimationRomByte(rom, 0x819e08), "native helmet terminal LDA");
        AssertEqual(ReadVerificationWord(rom, 0x819e09), FileSelectHelmetAnimation.Turn.Count - 1,
            "native helmet terminal frame clamp");
    }

    /// <summary>Reads a native helmet-animation instruction byte solely for the cartridge-oracle comparison.</summary>
    private static byte FileSelectHelmetAnimationRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
