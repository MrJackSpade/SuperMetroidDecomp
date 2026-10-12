using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

internal static class CeresBlastPlacementVerification
{
    internal static void VerifyCeresInitialBlastX(ISnesAddressSpace rom) => VerifyCeresBlastField(rom, 0x8bc475, 5, i => unchecked((ushort)CeresExplosionDefinitions.InitialExplosion(i).Offset.X));
    internal static void VerifyCeresInitialBlastY(ISnesAddressSpace rom) => VerifyCeresBlastField(rom, 0x8bc47f, 5, i => unchecked((ushort)CeresExplosionDefinitions.InitialExplosion(i).Offset.Y));
    internal static void VerifyCeresInitialBlastDelay(ISnesAddressSpace rom) => VerifyCeresBlastField(rom, 0x8bc46b, 5, i => CeresExplosionDefinitions.InitialExplosion(i).DelayFrames);
    internal static void VerifyCeresFinalBlastX(ISnesAddressSpace rom) => VerifyCeresBlastField(rom, 0x8bc572, 4, i => unchecked((ushort)CeresExplosionDefinitions.FinalExplosion(i).Offset.X));
    internal static void VerifyCeresFinalBlastDelay(ISnesAddressSpace rom) => VerifyCeresBlastField(rom, 0x8bc56a, 4, i => CeresExplosionDefinitions.FinalExplosion(i).DelayFrames);

    [CpuBusBoundary("exact bus test: retail ROM oracle for Ceres blast field")]
    private static void VerifyCeresBlastField(ISnesAddressSpace rom, int address, int count, Func<int, ushort> actual)
    {
        for (int index = 0; index < count; index++)
        {
            int word = address + index * 2;
            ushort expected = CartridgeImportSource.Require(rom).ReadCartridgeWord(word);
            AssertEqual(expected, actual(index), "original Ceres explosion field");
        }
        foreach (int invalid in new[] { int.MinValue, -1, count, 256, int.MaxValue })
        {
            try { _ = actual(invalid); }
            catch (ArgumentOutOfRangeException exception)
            {
                AssertEqual("index", exception.ParamName!, "blast placement index parameter");
                continue;
            }
            throw new InvalidOperationException("Unsupported blast index was accepted.");
        }
    }
}
