using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

/// <summary>Represents ceres blast placement verification.</summary>
internal static class CeresBlastPlacementVerification
{
    /// <summary>Verifies ceres initial blast x.</summary>
    internal static void VerifyCeresInitialBlastX(ISnesAddressSpace rom) => VerifyCeresBlastField(rom, 0x8bc475, 5, i => unchecked((ushort)CeresExplosionDefinitions.InitialExplosion(i).Offset.X));
    /// <summary>Verifies ceres initial blast y.</summary>
    internal static void VerifyCeresInitialBlastY(ISnesAddressSpace rom) => VerifyCeresBlastField(rom, 0x8bc47f, 5, i => unchecked((ushort)CeresExplosionDefinitions.InitialExplosion(i).Offset.Y));
    /// <summary>Verifies ceres initial blast delay.</summary>
    internal static void VerifyCeresInitialBlastDelay(ISnesAddressSpace rom) => VerifyCeresBlastField(rom, 0x8bc46b, 5, i => CeresExplosionDefinitions.InitialExplosion(i).DelayFrames);
    /// <summary>Verifies ceres final blast x.</summary>
    internal static void VerifyCeresFinalBlastX(ISnesAddressSpace rom) => VerifyCeresBlastField(rom, 0x8bc572, 4, i => unchecked((ushort)CeresExplosionDefinitions.FinalExplosion(i).Offset.X));
    /// <summary>Verifies ceres final blast delay.</summary>
    internal static void VerifyCeresFinalBlastDelay(ISnesAddressSpace rom) => VerifyCeresBlastField(rom, 0x8bc56a, 4, i => CeresExplosionDefinitions.FinalExplosion(i).DelayFrames);

    /// <summary>Verifies ceres blast field.</summary>
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
