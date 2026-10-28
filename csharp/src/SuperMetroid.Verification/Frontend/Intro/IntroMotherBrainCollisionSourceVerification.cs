using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Frontend.Intro;

/// <summary>Checks the compiled intro Mother Brain collision source against its retail ROM words.</summary>
internal static class IntroMotherBrainCollisionSourceVerification
{
    /// <summary>Compares all 224 words and confirms callers receive an independent mutable copy.</summary>
    internal static byte[] VerifyIntroMotherBrainCollisionSource(IImportCartridgeSource rom)
    {
        byte[] native = RomDataReader.ReadFixedBank(rom, 0x8cbec3, 448);
        byte[] actual = IntroMotherBrainCollisionDefinitions.CopySourceBytes();
        AssertTrue(actual.SequenceEqual(native), "all224 Mother Brain collision words match original");
        actual.AsSpan().Fill(0xff);
        byte[] fresh = IntroMotherBrainCollisionDefinitions.CopySourceBytes();
        AssertTrue(!ReferenceEquals(actual, fresh) && fresh.SequenceEqual(native),
            "debugger-visible collision source remains independently mutable per flashback");
        return native;
    }
}
