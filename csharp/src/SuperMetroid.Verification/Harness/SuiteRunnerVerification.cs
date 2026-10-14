using SuperMetroid.Core;
using System.Reflection;
using SuperMetroid.AssetExtraction;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.AssetExtraction.Installation;

namespace SuperMetroid.Verification.Harness;

/// <summary>Reflection dispatcher for a named verifier whose inputs can be
/// supplied directly from the retail cartridge or installation.</summary>
internal static class SuiteRunnerVerification
{
    /// <summary>
    /// Runs one verifier method by name: <c>--suite VerifyName</c>. The name must identify exactly one
    /// <c>internal static</c> method among this assembly's verification types. Supports methods with no
    /// parameters, or whose single required parameter is the retail cartridge address space or the shared retail
    /// <see cref="GameInstallation"/> (optional parameters take their defaults). Verifiers that need other
    /// prepared inputs keep running through their parent flag.
    /// </summary>
    [ReflectionAudit("verifier discovery: a suite is selected by its method name from the command line")]
    internal static int RunNamedSuite(string name)
    {
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
        object? result = null;
        Suite(name, () => result = method.Invoke(null, BindingFlags.DoNotWrapExceptions, binder: null, arguments, culture: null));
        return result is int code ? code : 0;
    }

    /// <summary>Runs the uniquely named verifier passed to the suite CLI flag.</summary>
    [VerificationCommand("--suite", "verifierMethod", Description = "Runs one verifier method by name.")]
    internal static int SuiteCommand(string verifierMethod)
    {
        return RunNamedSuite(verifierMethod);
    }
}
