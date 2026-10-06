using System.Reflection;

internal static partial class Program
{
    /// <summary>
    /// Runs one verifier method by name: <c>--suite VerifyName</c>. Supports methods with no
    /// parameters, or whose single required parameter accepts the retail cartridge address
    /// space (optional parameters take their defaults). Verifiers that need other prepared
    /// inputs keep running through their parent flag.
    /// </summary>
    private static int RunNamedSuite(string name)
    {
        MethodInfo method = typeof(Program).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new ArgumentException($"No verifier method named {name}.", nameof(name));
        ParameterInfo[] parameters = method.GetParameters();
        var required = parameters.Where(parameter => !parameter.IsOptional).ToArray();
        object?[] arguments = parameters.Select(parameter => parameter.IsOptional ? parameter.DefaultValue : null).ToArray();
        if (required.Length == 1)
        {
            var rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
            if (!required[0].ParameterType.IsInstanceOfType(rom))
                throw new ArgumentException($"{name} needs a {required[0].ParameterType.Name}, not the retail cartridge; run its parent flag.");
            arguments[Array.IndexOf(parameters, required[0])] = rom;
        }
        else if (required.Length > 1)
        {
            throw new ArgumentException($"{name} needs {required.Length} prepared inputs; run its parent flag.");
        }
        object? result = null;
        try
        {
            Suite(name, () => result = method.Invoke(null, arguments));
        }
        catch (TargetInvocationException invocation) when (invocation.InnerException is not null)
        {
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(invocation.InnerException).Throw();
        }
        return result is int code ? code : 0;
    }
}
