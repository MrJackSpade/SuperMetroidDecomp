using SuperMetroid.Core;
using static SuperMetroid.Verification.Harness.FullRunSuitesVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Verification.Harness;

/// <summary>Dispatcher for a named verifier whose inputs can be supplied directly from the retail cartridge or installation.</summary>
internal static class SuiteRunnerVerification
{
    /// <summary>
    /// Resolves one verifier for <c>--suite</c>: a suite the full run registers under <paramref name="name"/>, or else
    /// the one <c>internal static</c> method of that name among this assembly's verification types, as the compiler lists
    /// them (<see cref="GeneratedVerificationRegistry"/>, #2602). A method may take no parameters, or one required
    /// parameter that is the retail cartridge address space, the shared retail <see cref="GameInstallation"/>, or a shared
    /// installed fixture of that type from <see cref="InstalledFixtures"/> (optional parameters take their defaults).
    /// Verifiers that need other prepared inputs keep running through their parent flag.
    /// </summary>
    /// <returns>Runs the verifier as a timed suite and returns its exit code.</returns>
    private static Func<int> ResolveNamedSuite(string name, RegisteredSuite[] registered)
    {
        if (registered.FirstOrDefault(suite => suite.Name == name) is { } registeredSuite)
        {
            return () =>
            {
                Suite(registeredSuite.Name, registeredSuite.Body);
                return 0;
            };
        }
        NamedVerifier[] candidates = [.. GeneratedVerificationRegistry.Verifiers().Where(verifier => verifier.Name == name)];
        NamedVerifier verifier = candidates switch
        {
            [var single] => single,
            [] => throw new ArgumentException($"No verifier method named {name}.", nameof(name)),
            _ => throw new ArgumentException(
                $"{name} names {candidates.Length} verifier methods: " +
                string.Join(", ", candidates.Select(candidate => $"{candidate.OwnerName}.{candidate.Name}")) + ".", nameof(name)),
        };
        object? input = null;
        switch (verifier.Input)
        {
            case NamedVerifierInput.InstalledFixture:
                input = (verifier.Fixture ?? throw new InvalidOperationException($"{name} names no installed fixture."))();
                break;
            case NamedVerifierInput.Cartridge:
                var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
                if (!(verifier.Accepts ?? throw new InvalidOperationException($"{name} names no cartridge test."))(rom))
                    throw new ArgumentException($"{name} needs a {verifier.InputName}, not the retail cartridge; run its parent flag.");
                input = rom;
                break;
            case NamedVerifierInput.Prepared:
                throw new ArgumentException($"{name} needs {verifier.RequiredInputs} prepared inputs; run its parent flag.");
            case NamedVerifierInput.None:
            case NamedVerifierInput.Installation:
                break;
            default:
                throw new InvalidOperationException($"{name} needs an input no case handles: {verifier.Input}.");
        }
        return () =>
        {
            // Installation verifiers edit, corrupt and repair what they are given, so a named run gets a private copy and
            // the shared repository installation stays as extracted.
            using PrivateInstallationCopy? copy = verifier.Input == NamedVerifierInput.Installation ? InstalledFixtures.CreatePrivateCopy() : null;
            int result = 0;
            Suite(name, () => result = verifier.Invoke(copy?.Installation ?? input));
            return result;
        };
    }
    /// <summary>
    /// Runs the named verifiers passed to the suite CLI flag, comma-separated, in one process. Every name resolves
    /// before any runs, so a misspelt name fails without running the rest.
    /// </summary>
    [VerificationCommand("--suite", "verifierMethod[,verifierMethod...]", Description = "Runs verifier methods by name.")]
    internal static int SuiteCommand(string verifierMethods)
    {
        RegisteredSuite[] registered = FullRunSuites();
        List<Func<int>> selected = [.. verifierMethods.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(name => ResolveNamedSuite(name, registered))];
        int code = 0;
        foreach (Func<int> run in selected)
        {
            int result = run();
            if (code == 0)
                code = result;
        }
        return code;
    }
}
