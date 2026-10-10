using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    /// <summary>Checks that the copied Mother Brain collision table matches retail data and that mutating one copy does not alter a fresh copy.</summary>
    /// <param name="rom">Cartridge source used to read the native collision table bytes.</param>
    /// <returns>The 448 native bytes for use by the related collision verification.</returns>
    private static byte[] VerifyIntroMotherBrainCollisionSource(IImportCartridgeSource rom)
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
