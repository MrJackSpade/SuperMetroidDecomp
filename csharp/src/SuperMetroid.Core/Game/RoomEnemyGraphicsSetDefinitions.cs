namespace SuperMetroid.Core.Game;

/// <summary>One ordered bank-$B4 graphics-set member and its native VRAM/palette selector.</summary>
public readonly record struct RoomEnemyGraphicsSetHeader(ushort DefinitionPointer, ushort VramDestination);

/// <summary>One immutable terminated bank-$B4 enemy graphics set.</summary>
public sealed class RoomEnemyGraphicsSetDefinition
{
    private readonly RoomEnemyGraphicsSetHeader[] records;
    internal RoomEnemyGraphicsSetDefinition(ushort pointer, RoomEnemyGraphicsSetHeader[] records)
    {
        Pointer = pointer;
        this.records = records;
    }
    public ushort Pointer { get; }
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
    private static readonly RoomEnemyGraphicsSetDefinition[] Definitions =
    [
        ..BuildSegment0(),
        ..BuildSegment1(),
    ];

    public const int ListCount = 302;
    public const int RecordCount = 425;

    static RoomEnemyGraphicsSetDefinitions()
    {
        if (Definitions.Length != ListCount || Definitions.Sum(list => list.Records.Length) != RecordCount)
            throw new InvalidDataException("Compiled enemy graphics-set counts changed.");
        for (int index = 1; index < Definitions.Length; index++)
            if (Definitions[index - 1].Pointer >= Definitions[index].Pointer)
                throw new InvalidDataException("Compiled enemy graphics sets are not strictly ordered.");
    }

    /// <summary>Gets a retail graphics set by its native bank-$B4 identity.</summary>
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
