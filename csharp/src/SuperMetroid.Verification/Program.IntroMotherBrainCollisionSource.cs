using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

/// <summary>Represents intro mother brain collision source verification.</summary>
internal static class IntroMotherBrainCollisionSourceVerification
{
    /// <summary>Verifies intro mother brain collision source.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for the intro Mother Brain collision words at $8C:BEC3")]
    internal static byte[] VerifyIntroMotherBrainCollisionSource(IImportCartridgeSource rom)
    {
        byte[] native = RomDataReader.ReadFixedBank(rom, 0x8cbec3, 448);
        byte[] actual = IntroMotherBrainCollisionDefinitions.CopySourceBytes();
        AssertTrue(actual.AsSpan().SequenceEqual(native), "all224 Mother Brain collision words match original");
        actual.AsSpan().Fill(0xff);
        byte[] fresh = IntroMotherBrainCollisionDefinitions.CopySourceBytes();
        AssertTrue(!ReferenceEquals(actual, fresh) && fresh.AsSpan().SequenceEqual(native),
            "debugger-visible collision source remains independently mutable per flashback");
        return native;
    }
}
