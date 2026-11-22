namespace SuperMetroid.Verification.Harness;

/// <summary>The one input a named verifier's required parameter takes, as the compiler found it (#2602).</summary>
internal enum NamedVerifierInput
{
    /// <summary>No required parameter: optional ones take their defaults.</summary>
    None,
    /// <summary>A private copy of the shared retail game installation, made for each run.</summary>
    Installation,
    /// <summary>The shared installed fixture of the parameter's type.</summary>
    InstalledFixture,
    /// <summary>The retail cartridge address space, when the parameter accepts it.</summary>
    Cartridge,
    /// <summary>More than one required parameter: the verifier needs inputs its parent flag prepares.</summary>
    Prepared,
}

/// <summary>
/// An internal static verifier <c>--suite</c> can select by name (#2602), as the compiler lists it
/// (<see cref="GeneratedVerificationRegistry"/>): its name, the type declaring it, the input it needs and a call to it.
/// </summary>
/// <param name="Name">The method's name.</param>
/// <param name="OwnerName">The name of the type declaring it.</param>
/// <param name="Input">The input its required parameter takes.</param>
/// <param name="Invoke">Calls the verifier with its input, null when it takes none, and returns its exit code, or zero.</param>
/// <param name="Fixture">Reads the shared installed fixture it takes, for <see cref="NamedVerifierInput.InstalledFixture"/>.</param>
/// <param name="Accepts">Whether the cartridge suits its parameter, for <see cref="NamedVerifierInput.Cartridge"/>.</param>
/// <param name="InputName">Its parameter's type name, for <see cref="NamedVerifierInput.Cartridge"/>.</param>
/// <param name="RequiredInputs">How many required parameters it has, for <see cref="NamedVerifierInput.Prepared"/>.</param>
internal sealed record NamedVerifier(string Name, string OwnerName, NamedVerifierInput Input, Func<object?, int> Invoke,
    Func<object>? Fixture = null, Func<object, bool>? Accepts = null, string? InputName = null, int RequiredInputs = 0);
