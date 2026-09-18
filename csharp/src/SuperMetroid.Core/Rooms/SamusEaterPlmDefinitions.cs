namespace SuperMetroid.Core.Rooms;

/// <summary>Fixed bank-$84 header/list identities for the two Samus Eater block actors.</summary>
internal static class SamusEaterPlmDefinitions
{
    /// <summary><c>$84:B6CB/$ACB8</c>, Brinstar floor plant and its initial instruction list.</summary>
    public static readonly SamusEaterPlmDefinition Floor =
        new(0xb6cb, 0xacb8, Ceiling: false);

    /// <summary><c>$84:B6CF/$ACF8</c>, Brinstar ceiling plant and its initial instruction list.</summary>
    public static readonly SamusEaterPlmDefinition Ceiling =
        new(0xb6cf, 0xacf8, Ceiling: true);

    private static readonly SamusEaterPlmDefinition[] Definitions = [Floor, Ceiling];

    /// <summary>Complete two-record domain accepted by the translated Samus Eater allocator.</summary>
    public static ReadOnlySpan<SamusEaterPlmDefinition> All => Definitions;

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
    ushort HeaderPointer,
    ushort InstructionListPointer,
    bool Ceiling);
