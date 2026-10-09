namespace SuperMetroid.Core.Game;

/// <summary>One ordered bank-$B4 graphics-set member and its native VRAM/palette selector.</summary>
/// <param name="DefinitionPointer">Bank-$A0 enemy-header word identifying the graphics/palette source and actor association; each native list record begins with this word.</param>
/// <param name="VramDestination">Packed second record word, not a literal VRAM address: low byte selects the OBJ palette row, and bits 12..13 select staging placement for a header with special tile-size bit 15; ordinary tile data is staged in list order.</param>
public readonly record struct RoomEnemyGraphicsSetHeader(ushort DefinitionPointer, ushort VramDestination);

/// <summary>One immutable terminated bank-$B4 enemy graphics set.</summary>
public sealed class RoomEnemyGraphicsSetDefinition
{
    /// <summary>Catalog-owned graphics-set members retained in their original staging order.</summary>
    private readonly RoomEnemyGraphicsSetHeader[] records;

    /// <summary>Creates a catalog entry from its native bank-$B4 identity and ordered member records.</summary>
    /// <param name="pointer">Native list-start word identifying this graphics set.</param>
    /// <param name="records">Member records in the order used for VRAM staging.</param>
    internal RoomEnemyGraphicsSetDefinition(ushort pointer, RoomEnemyGraphicsSetHeader[] records)
    {
        Pointer = pointer;
        this.records = records;
    }
    /// <summary>Native 16-bit list identity in bank $B4, beginning at its first four-byte member record or the $FFFF terminator for an empty set.</summary>
    public ushort Pointer { get; }
    /// <summary>Read-only view of catalog-owned members in native staging order, excluding the $FFFF terminator and trailing metadata; duplicate source associations and palette sharing are retained.</summary>
    public ReadOnlyMemory<RoomEnemyGraphicsSetHeader> Records => records;
}

/// <summary>Compiled graphics-set membership and native staging order for all retail room states.</summary>
/// <remarks>
/// The 302 bank-$B4 lists contain 425 records in the pinned NTSC J/U v1.0
/// cartridge. VRAM/palette destination words are fixed engine staging metadata;
/// editable pixel/color content is loaded separately through enemy artwork assets.
/// </remarks>
public static partial class RoomEnemyGraphicsSetDefinitions
{
    /// <summary>All compiled retail set definitions in strictly increasing native pointer order.</summary>
    private static readonly RoomEnemyGraphicsSetDefinition[] Definitions =
    [
        ..BuildSegment0(),
        ..BuildSegment1(),
    ];

    /// <summary>Number of distinct retail bank-$B4 graphics-set identities in the compiled catalog, including empty sets and separately addressed sets with matching content.</summary>
    public const int ListCount = 302;
    /// <summary>Total four-byte member records across all compiled retail lists, excluding terminators and trailing metadata; not a count of unique enemy definitions or artwork images.</summary>
    public const int RecordCount = 425;

    /// <summary>Verifies the compiled catalog's record totals and pointer ordering at initialization.</summary>
    static RoomEnemyGraphicsSetDefinitions()
    {
        if (Definitions.Length != ListCount || Definitions.Sum(list => list.Records.Length) != RecordCount)
            throw new InvalidDataException("Compiled enemy graphics-set counts changed.");
        for (int index = 1; index < Definitions.Length; index++)
            if (Definitions[index - 1].Pointer >= Definitions[index].Pointer)
                throw new InvalidDataException("Compiled enemy graphics sets are not strictly ordered.");
    }

    /// <summary>Gets a retail graphics set by its native bank-$B4 identity.</summary>
    /// <param name="pointer">Native 16-bit list-start word, without the implied $B4 bank; not an index into the catalog.</param>
    /// <returns>The shared compiled definition whose record order controls tile staging and palette installation.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The pointer is not one of the compiled retail list identities.</exception>
    public static RoomEnemyGraphicsSetDefinition Get(ushort pointer)
    {
        int low = 0;
        int high = Definitions.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            RoomEnemyGraphicsSetDefinition candidate = Definitions[middle];
            if (candidate.Pointer == pointer) return candidate;
            if (candidate.Pointer < pointer) low = middle + 1;
            else high = middle - 1;
        }
        throw new ArgumentOutOfRangeException(nameof(pointer), pointer,
            "Pointer is not a retail bank-$B4 enemy graphics set.");
    }

    /// <summary>Enumerates every compiled set identity for ROM-oracle tests.</summary>
    public static IEnumerable<ushort> Pointers => Definitions.Select(list => list.Pointer);
}
