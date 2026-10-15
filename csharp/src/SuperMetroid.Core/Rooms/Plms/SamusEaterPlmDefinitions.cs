namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// Named bank-$84 header cases for the two Samus Eater block actors. B6CB selects
/// the B0DC floor setup and the floor bite; B6CF selects B113 ceiling setup and the ceiling bite.
/// Mounting direction follows those native setup identities, including opposite
/// alignment edges and held-Y adjustments. No indexed descriptor table is stored.
/// </summary>
internal static class SamusEaterPlmDefinitions
{
    /// <summary><c>$84:B6CB</c>, Brinstar floor plant and its first program.</summary>
    public static readonly SamusEaterPlmDefinition Floor =
        new(SamusEaterProgram.FloorBite, Ceiling: false);

    /// <summary><c>$84:B6CF</c>, Brinstar ceiling plant and its first program.</summary>
    public static readonly SamusEaterPlmDefinition Ceiling =
        new(SamusEaterProgram.CeilingBite, Ceiling: true);

    /// <summary>Resolves one supported plant header without interpreting adjacent bank-$84 data.</summary>
    public static SamusEaterPlmDefinition Resolve(PlmHeaderId headerPointer) => headerPointer switch
    {
        PlmHeaderId.InsideReactionBrinstarFloorPlant => Floor,
        PlmHeaderId.InsideReactionBrinstarCeilingPlant => Ceiling,
        _ => throw new InvalidDataException($"Unsupported Samus Eater PLM ${(int)headerPointer:X4}."),
    };
}
