namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Named bank-$84 header cases for the two Samus Eater block actors. B6CB selects
/// the B0DC floor setup and ACB8 program; B6CF selects B113 ceiling setup and ACF8.
/// Mounting direction follows those native setup identities, including opposite
/// alignment edges and held-Y adjustments. No indexed descriptor table is stored.
/// </summary>
internal static class SamusEaterPlmDefinitions
{
    /// <summary><c>$84:B6CB/$ACB8</c>, Brinstar floor plant and its initial instruction list.</summary>
    public static readonly SamusEaterPlmDefinition Floor =
        new(0xacb8, Ceiling: false);

    /// <summary><c>$84:B6CF/$ACF8</c>, Brinstar ceiling plant and its initial instruction list.</summary>
    public static readonly SamusEaterPlmDefinition Ceiling =
        new(0xacf8, Ceiling: true);

    /// <summary>Resolves one supported plant header without interpreting adjacent bank-$84 data.</summary>
    public static SamusEaterPlmDefinition Resolve(ushort headerPointer) => headerPointer switch
    {
        0xb6cb => Floor,
        0xb6cf => Ceiling,
        _ => throw new InvalidDataException($"Unsupported Samus Eater PLM ${headerPointer:X4}."),
    };
}

/// <summary>One Samus Eater PLM header paired with its initial list and mounting direction.</summary>
internal readonly record struct SamusEaterPlmDefinition(
    ushort InstructionListPointer,
    bool Ceiling);
