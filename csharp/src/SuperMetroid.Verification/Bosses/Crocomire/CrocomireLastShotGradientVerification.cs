using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Bosses.Crocomire;

/// <summary>
/// Compares the volley's gradients with the cartridge's gradient table, and the gradient the last shot reads past it with
/// the installed <see cref="CrocomireLastShotGradient"/> (#1500).
/// </summary>
internal static class CrocomireLastShotGradientVerification
{
    /// <summary>The cartridge's volley gradients, a word per place in the volley.</summary>
    private const int VolleyGradients = 0x869059;

    /// <summary>
    /// Decodes the gradient each shot reads from the cartridge, where the spit attack's even spawn parameter, twice the
    /// shot's place in the volley, selects the word, and compares it with the volley.
    /// </summary>
    internal static void VerifyCrocomireLastShotGradient(CartridgeImportAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        CrocomireLastShotGradient installed = InstalledFixtures.BossAndIntroOverreads.CrocomireLastShotGradient;
        for (int shotsSpat = 0; shotsSpat < CrocomireProjectileDefinitions.VolleyShotCount; shotsSpat++)
        {
            int parameter = (shotsSpat + 1) * 2;
            int expected = unchecked((short)cartridge.ReadCartridgeWord(VolleyGradients + (parameter >> 1) * 2));
            AssertEqual(expected, CrocomireVolley.GradientOf(shotsSpat, installed), $"Crocomire volley gradient after {shotsSpat} shots (spawn parameter {parameter})");
        }
        AssertEqual(installed.Gradient, CrocomireVolley.GradientOf(CrocomireProjectileDefinitions.VolleyShotCount - 1, installed),
            "the volley's last shot is fired along the installed gradient");
        AssertThrows<InvalidDataException>(() => CrocomireVolley.GradientOf(CrocomireProjectileDefinitions.VolleyShotCount, installed),
            "a shot past the volley is rejected");
        AssertThrows<InvalidDataException>(() => CrocomireVolley.GradientOf(-1, installed), "the volley has no shot before its first");
        Console.WriteLine("Crocomire last shot gradient: every shot's gradient matches the cartridge.");
    }

    /// <summary>Runs the volley gradients and the installed last-shot gradient against the cartridge.</summary>
    [VerificationCommand("--crocomire-last-shot-gradient")]
    internal static int CrocomireLastShotGradientCommand()
    {
        Suite(nameof(VerifyCrocomireLastShotGradient), () => VerifyCrocomireLastShotGradient(LoadRepositoryRom()));
        return 0;
    }
}
