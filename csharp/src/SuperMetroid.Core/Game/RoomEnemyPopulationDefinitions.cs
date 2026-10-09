namespace SuperMetroid.Core.Game;

/// <summary>One immutable ordered bank-$A1 room population and its terminator quota.</summary>
public sealed class RoomEnemyPopulationDefinition
{
    /// <summary>Stores placements in native record order for the read-only <see cref="Records"/> view.</summary>
    private readonly RoomEnemyPopulationRecord[] records;

    /// <summary>Creates a compiled room population while retaining its native identity, ordered placements, and quota.</summary>
    /// <param name="pointer">The bank-$A1 pointer that identifies this terminated population list.</param>
    /// <param name="records">The placements preceding the native $FFFF terminator, in initialization order.</param>
    /// <param name="deathQuota">The literal byte following the terminator, used as the nonempty room's kill-count requirement.</param>
    internal RoomEnemyPopulationDefinition(ushort pointer, RoomEnemyPopulationRecord[] records, byte deathQuota)
    {
        Pointer = pointer;
        this.records = records;
        DeathQuota = deathQuota;
    }
    /// <summary>Native 16-bit list pointer within bank $A1; identifies the population without granting cartridge-read access.</summary>
    public ushort Pointer { get; }
    /// <summary>Read-only ordered placements compiled from native 16-byte records, excluding the $FFFF terminator; order controls initialization and slot allocation.</summary>
    public ReadOnlyMemory<RoomEnemyPopulationRecord> Records => records;
    /// <summary>Literal byte following the $FFFF terminator, used as the room kill-count requirement for nonempty populations; an empty load does not publish this byte.</summary>
    public byte DeathQuota { get; }
}

/// <summary>Application-owned ordered enemy placements selected by all retail room states.</summary>
/// <remarks>
/// The 302 pointers and 1,658 records come from the pinned NTSC J/U v1.0 bank-$A1
/// populations selected by 323 compiled room states. Record order and the byte after
/// each terminator govern linked slot allocation and the room enemy-death quota.
/// This is gameplay data, not an editable visual asset.
/// </remarks>
public static partial class RoomEnemyPopulationDefinitions
{
    /// <summary>Holds all compiled population definitions in ascending native-pointer order.</summary>
    private static readonly RoomEnemyPopulationDefinition[] Definitions =
    [
        ..BuildSegment0(),
        ..BuildSegment1(),
        ..BuildSegment2(),
        ..BuildSegment3(),
    ];

    /// <summary>Number of distinct bank-$A1 population pointers selected by the compiled retail room states, including empty lists.</summary>
    public const int ListCount = 302;
    /// <summary>Total placement records across the distinct compiled lists, excluding terminators and without counting repeated references from room states.</summary>
    public const int RecordCount = 1658;

    /// <summary>Checks that the compiled catalog matches its declared totals and remains strictly pointer-ordered.</summary>
    static RoomEnemyPopulationDefinitions()
    {
        if (Definitions.Length != ListCount || Definitions.Sum(list => list.Records.Length) != RecordCount)
            throw new InvalidDataException("Compiled enemy-population counts changed.");
        for (int index = 1; index < Definitions.Length; index++)
            if (Definitions[index - 1].Pointer >= Definitions[index].Pointer)
                throw new InvalidDataException("Compiled enemy populations are not strictly ordered.");
    }

    /// <summary>Gets a retail population by its native bank-$A1 identity.</summary>
    /// <param name="pointer">The 16-bit population-list address within bank $A1, not an enemy-definition pointer or a host index.</param>
    /// <returns>The shared compiled definition, preserving native record order and its trailing quota byte.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The pointer is not one of the compiled retail population identities.</exception>
    public static RoomEnemyPopulationDefinition Get(ushort pointer)
    {
        int low = 0;
        int high = Definitions.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            RoomEnemyPopulationDefinition candidate = Definitions[middle];
            if (candidate.Pointer == pointer) return candidate;
            if (candidate.Pointer < pointer) low = middle + 1;
            else high = middle - 1;
        }
        throw new ArgumentOutOfRangeException(nameof(pointer), pointer,
            "Pointer is not a retail bank-$A1 enemy population.");
    }
}
