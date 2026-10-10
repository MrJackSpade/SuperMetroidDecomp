using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    /// <summary>Checks the discovery-room collision allocation against native bytes and verifies its zero tail and per-room mutability.</summary>
    /// <param name="rom">Cartridge source containing the original fixed-bank collision words.</param>
    /// <returns>The native collision bytes used by related intro actor verification.</returns>
    private static byte[] VerifyIntroDiscoveryCollision(IImportCartridgeSource rom)
    {
        byte[] native = RomDataReader.ReadFixedBank(rom, 0x8cc083, 0x300);
        ushort[] actual = IntroBabyDiscoveryCollisionDefinitions.CreateForeground();
        AssertEqual(32 * 16, actual.Length, "discovery room allocation");
        for (int index = 0; index < actual.Length; index++)
        {
            ushort expected = index < native.Length / 2
                ? (ushort)(native[2 * index] | native[2 * index + 1] << 8) : (ushort)0;
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
