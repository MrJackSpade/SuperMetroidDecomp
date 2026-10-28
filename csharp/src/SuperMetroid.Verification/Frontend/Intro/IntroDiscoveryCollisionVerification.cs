using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Frontend.Intro;

/// <summary>Compares the intro discovery room's compiled foreground with its retail collision words.</summary>
internal static class IntroDiscoveryCollisionVerification
{
    /// <summary>Checks the copied $300-byte strip, zero tail, and independent allocations; returns the native bytes for other checks.</summary>
    internal static byte[] VerifyIntroDiscoveryCollision(IImportCartridgeSource rom)
    {
        byte[] native = RomDataReader.ReadFixedBank(rom, 0x8cc083, 0x300);
        ushort[] actual = IntroBabyDiscoveryCollisionDefinitions.CreateForeground();
        AssertEqual(32 * 16, actual.Length, "discovery room allocation");
        for (int index = 0; index < actual.Length; index++)
        {
            ushort expected = index < native.Length / 2
                ? LittleEndianBytes.Word(native[2 * index], native[2 * index + 1]) : (ushort)0;
            AssertEqual(expected, actual[index], "original discovery collision and uncopied zero tail");
        }
        actual[0] = ushort.MaxValue;
        actual[10 * 32] = 0;
        ushort[] fresh = IntroBabyDiscoveryCollisionDefinitions.CreateForeground();
        AssertTrue(!ReferenceEquals(actual, fresh), "collision allocations are independently mutable");
        AssertEqual((ushort)0, fresh[0], "modified air block does not change next room");
        AssertEqual((ushort)0x1000, fresh[10 * 32], "modified strip does not change next room");
        return native;
    }
}
