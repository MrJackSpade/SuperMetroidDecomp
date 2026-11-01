using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Bosses.Crocomire;

/// <summary>
/// Compares the volley's gradients with the cartridge's gradient table, and the gradient the tenth shot reads past it with
/// the installed <see cref="CrocomireTenthShotGradient"/> (#1500).
/// </summary>
internal static class CrocomireTenthShotGradientVerification
{
    /// <summary>The cartridge's volley gradients, a word per place in the volley.</summary>
    private const int VolleyGradients = 0x869059;

    /// <summary>Decodes every gradient a spawn parameter selects from the cartridge and compares it with the volley.</summary>
    internal static void VerifyCrocomireTenthShotGradient(SuperMetroidAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        CrocomireTenthShotGradient installed = InstalledFixtures.BossAndIntroOverreads.CrocomireTenthShotGradient;
        for (int parameter = 2; parameter <= CrocomireBehavior.LastProjectileSpawnParameter; parameter += 2)
        {
            int expected = unchecked((short)cartridge.ReadCartridgeWord(VolleyGradients + (parameter >> 1) * 2));
            int actual = CrocomireVolley.IsTenthShot(parameter) ? installed.Gradient : CrocomireVolley.AuthoredGradient(parameter);
            AssertEqual(expected, actual, $"Crocomire volley gradient for spawn parameter {parameter}");
        }
        AssertTrue(CrocomireVolley.IsTenthShot(CrocomireBehavior.LastProjectileSpawnParameter), "the volley's last shot reads the tenth gradient");
        AssertThrows<InvalidDataException>(() => CrocomireVolley.AuthoredGradient(CrocomireBehavior.LastProjectileSpawnParameter),
            "the tenth gradient is not authored");
        AssertThrows<InvalidDataException>(() => CrocomireVolley.AuthoredGradient(CrocomireBehavior.LastProjectileSpawnParameter + 2),
            "a selector past the volley is rejected");
        Console.WriteLine("Crocomire tenth shot gradient: every emitted spawn parameter's gradient matches the cartridge.");
    }

    /// <summary>Runs the volley gradients and the installed tenth-shot gradient against the cartridge.</summary>
    [VerificationCommand("--crocomire-tenth-shot-gradient")]
    internal static int CrocomireTenthShotGradientCommand()
    {
        Suite(nameof(VerifyCrocomireTenthShotGradient), () => VerifyCrocomireTenthShotGradient(LoadRepositoryRom()));
        return 0;
    }
}
