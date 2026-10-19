using SuperMetroid.Core;
using System.Reflection;
using static SuperMetroid.Verification.Harness.FullRunSuitesVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation;

namespace SuperMetroid.Verification.Harness;

/// <summary>Reflection dispatcher for a named verifier whose inputs can be
/// supplied directly from the retail cartridge or installation.</summary>
internal static class SuiteRunnerVerification
{
    /// <summary>
    /// Resolves one verifier for <c>--suite</c>: a suite the full run registers under <paramref name="name"/>, or else
    /// the one <c>internal static</c> method of that name among this assembly's verification types. A method may take
    /// no parameters, or one required parameter that is the retail cartridge address space or the shared retail
    /// <see cref="GameInstallation"/> (optional parameters take their defaults). Verifiers that need other prepared
    /// inputs keep running through their parent flag.
    /// </summary>
    /// <returns>Runs the verifier as a timed suite and returns its exit code.</returns>
    [ReflectionAudit("verifier discovery: a suite is selected by its method name from the command line")]
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
        MethodInfo[] candidates = typeof(SuiteRunnerVerification).Assembly.GetTypes()
            .SelectMany(type => type.GetMethods(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly))
            .Where(method => method.Name == name && method.IsAssembly)
            .ToArray();
        MethodInfo method = candidates switch
        {
            [var single] => single,
            [] => throw new ArgumentException($"No verifier method named {name}.", nameof(name)),
            _ => throw new ArgumentException(
                $"{name} names {candidates.Length} verifier methods: " +
                string.Join(", ", candidates.Select(candidate => $"{candidate.DeclaringType?.Name}.{candidate.Name}")) + ".", nameof(name)),
        };
        ParameterInfo[] parameters = method.GetParameters();
        var required = parameters.Where(parameter => !parameter.IsOptional).ToArray();
        object?[] arguments = parameters.Select(parameter => parameter.IsOptional ? parameter.DefaultValue : null).ToArray();
        if (required.Length == 1 && required[0].ParameterType == typeof(GameInstallation))
        {
            arguments[Array.IndexOf(parameters, required[0])] = RepositoryInstallation.Installation;
        }
        else if (required.Length == 1)
        {
            var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
            if (!required[0].ParameterType.IsInstanceOfType(rom))
                throw new ArgumentException($"{name} needs a {required[0].ParameterType.Name}, not the retail cartridge; run its parent flag.");
            arguments[Array.IndexOf(parameters, required[0])] = rom;
        }
        else if (required.Length > 1)
        {
            throw new ArgumentException($"{name} needs {required.Length} prepared inputs; run its parent flag.");
        }
        return () =>
        {
            object? result = null;
            Suite(name, () => result = method.Invoke(null, BindingFlags.DoNotWrapExceptions, binder: null, arguments, culture: null));
            return result is int code ? code : 0;
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
