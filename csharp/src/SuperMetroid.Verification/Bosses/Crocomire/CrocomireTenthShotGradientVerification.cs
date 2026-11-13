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

    /// <summary>
    /// Decodes the gradient each shot reads from the cartridge, where the spit attack's even spawn parameter, twice the
    /// shot's place in the volley, selects the word, and compares it with the volley.
    /// </summary>
    internal static void VerifyCrocomireTenthShotGradient(CartridgeImportAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        CrocomireTenthShotGradient installed = InstalledFixtures.BossAndIntroOverreads.CrocomireTenthShotGradient;
        for (int shot = 1; shot <= CrocomireProjectileDefinitions.VolleyShotCount; shot++)
        {
            int parameter = shot * 2;
            int expected = unchecked((short)cartridge.ReadCartridgeWord(VolleyGradients + (parameter >> 1) * 2));
            int actual = CrocomireVolley.IsLastShot(shot) ? installed.Gradient : CrocomireVolley.AuthoredGradient(shot);
            AssertEqual(expected, actual, $"Crocomire volley gradient for shot {shot} (spawn parameter {parameter})");
        }
        AssertTrue(CrocomireVolley.IsLastShot(CrocomireProjectileDefinitions.VolleyShotCount), "the volley's last shot reads the gradient past the authored ones");
        AssertThrows<InvalidDataException>(() => CrocomireVolley.AuthoredGradient(CrocomireProjectileDefinitions.VolleyShotCount),
            "the last shot's gradient is not authored");
        AssertThrows<InvalidDataException>(() => CrocomireVolley.AuthoredGradient(CrocomireProjectileDefinitions.VolleyShotCount + 1),
            "a shot past the volley is rejected");
        AssertThrows<InvalidDataException>(() => CrocomireVolley.AuthoredGradient(0), "the volley has no shot before its first");
        Console.WriteLine("Crocomire tenth shot gradient: every shot's gradient matches the cartridge.");
    }

    /// <summary>Runs the volley gradients and the installed tenth-shot gradient against the cartridge.</summary>
    [VerificationCommand("--crocomire-tenth-shot-gradient")]
    internal static int CrocomireTenthShotGradientCommand()
    {
        Suite(nameof(VerifyCrocomireTenthShotGradient), () => VerifyCrocomireTenthShotGradient(LoadRepositoryRom()));
        return 0;
    }
}
