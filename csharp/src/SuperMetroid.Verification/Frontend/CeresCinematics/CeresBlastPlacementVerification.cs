using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Frontend.CeresCinematics;

namespace SuperMetroid.Verification.Frontend.CeresCinematics;

/// <summary>Compares the Ceres destruction blasts' places and delays with the retail placement tables.</summary>
internal static class CeresBlastPlacementVerification
{
    /// <summary>Checks the first wave's five horizontal offsets at $8B:C475.</summary>
    internal static void VerifyCeresInitialBlastX(ISnesAddressSpace rom) => VerifyCeresBlastField(rom, 0x8bc475, CeresExplosionDefinitions.InitialWave, blast => unchecked((ushort)blast.Offset.X));
    /// <summary>Checks the first wave's five vertical offsets at $8B:C47F.</summary>
    internal static void VerifyCeresInitialBlastY(ISnesAddressSpace rom) => VerifyCeresBlastField(rom, 0x8bc47f, CeresExplosionDefinitions.InitialWave, blast => unchecked((ushort)blast.Offset.Y));
    /// <summary>Checks the first wave's five delay words at $8B:C46B.</summary>
    internal static void VerifyCeresInitialBlastDelay(ISnesAddressSpace rom) => VerifyCeresBlastField(rom, 0x8bc46b, CeresExplosionDefinitions.InitialWave, blast => checked((ushort)blast.DelayFrames));
    /// <summary>Checks the final wave's four horizontal offsets at $8B:C572.</summary>
    internal static void VerifyCeresFinalBlastX(ISnesAddressSpace rom) => VerifyCeresBlastField(rom, 0x8bc572, CeresExplosionDefinitions.FinalWave, blast => unchecked((ushort)blast.Offset.X));
    /// <summary>Checks the final wave's four delay words at $8B:C56A.</summary>
    internal static void VerifyCeresFinalBlastDelay(ISnesAddressSpace rom) => VerifyCeresBlastField(rom, 0x8bc56a, CeresExplosionDefinitions.FinalWave, blast => checked((ushort)blast.DelayFrames));

    /// <summary>Compares one field of each blast of a wave, in the order they appear, with the cartridge's word for it.</summary>
    private static void VerifyCeresBlastField(ISnesAddressSpace rom, int address, IReadOnlyList<CeresExplosionPlacement> wave, Func<CeresExplosionPlacement, ushort> actual)
    {
        for (int index = 0; index < wave.Count; index++)
        {
            ushort expected = CartridgeImportSource.Require(rom).ReadCartridgeWord(address + index * 2);
            AssertEqual(expected, actual(wave[index]), $"original Ceres blast {index} field");
        }
    }
}
