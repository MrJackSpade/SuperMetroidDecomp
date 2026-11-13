using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// Immutable, ordered input to the room-object allocator. Construction reads only the
/// decoded placements.
/// </summary>
public sealed class RoomObjectPopulation
{
    /// <summary>The most placements one room's object population holds.</summary>
    public const int MaximumPlacementCount = 255;

    /// <summary>Owned placement records kept in the order the room-object allocator consumes them.</summary>
    private readonly RoomObjectPlacement[] placements;

    /// <summary>The population's name, or a fixture's description.</summary>
    public string Name { get; }
    /// <summary>Validated placement records in allocation order; scroll programs are owned copies.</summary>
    public ReadOnlyMemory<RoomObjectPlacement> Placements => placements;

    /// <summary>Copies an ordered, bounded decoded population and validates scroll programs before any room-object slots are allocated.</summary>
    /// <param name="name">The population's name, used in diagnostics.</param>
    /// <param name="records">The placements in order; at most <see cref="MaximumPlacementCount"/>.</param>
    /// <exception cref="ArgumentNullException">The sequence or one of its placements is null.</exception>
    /// <exception cref="InvalidDataException">There are too many placements, one is an empty placement, or a setup-specific payload is invalid.</exception>
    public RoomObjectPopulation(string name, IEnumerable<RoomObjectPlacement> records)
    {
        Ensure.NotNull(records);
        Name = Ensure.NotNull(name);
        // Never enumerate an unbounded caller sequence: one placement past the limit is enough to reject it.
        placements = records.Take(MaximumPlacementCount + 1).Select(CopyPlacement).ToArray();
        if (placements.Length > MaximumPlacementCount)
            throw new InvalidDataException($"Room-object population {name} exceeds the most placements a room can hold.");
    }

    /// <summary>Builds a retail population from its compiled records, binding scroll triggers to their compiled program identities.</summary>
    /// <param name="name">The population's name.</param>
    /// <param name="records">Each record's room-object kind, block coordinates and room argument, in authored order.</param>
    internal static RoomObjectPopulation FromRecords(string name, RoomObjectPlacementDefinition[] records) =>
        new(name, Ensure.NotNull(records).Select(record => new RoomObjectPlacement(
            RoomObjectKinds.RequirePlaced(record.Kind), record.Block, record.Argument, Cannon: record.Cannon)
            { CompiledScrollProgram = record.ScrollProgram }));

    /// <summary>Copies one placement's scroll program and rejects a malformed one.</summary>
    private static RoomObjectPlacement CopyPlacement(RoomObjectPlacement placement)
    {
        Ensure.NotNull(placement);
        if (placement.Kind == RoomObjectKind.None)
            throw new InvalidDataException("A decoded room-object placement cannot be an empty placement.");
        RoomScrollWrite[]? program = placement.ScrollProgram?.ToArray();
        RoomScrollProgram? compiledProgram = program is null ? placement.CompiledScrollProgram : null;
        if (compiledProgram is not null)
        {
            if (placement.Kind != RoomObjectKind.ScrollTrigger)
                throw new InvalidDataException("Only a scroll trigger may run a retail scroll program.");
        }
        else if (placement.Kind == RoomObjectKind.ScrollTrigger)
        {
            if (program is null || program.Length >= RoomScrollGrid.KeptCellCount)
                throw new InvalidDataException("A decoded scroll room object needs a bounded write program.");
            foreach (RoomScrollWrite write in program)
            {
                if (write.Index is < 0 or >= RoomScrollGrid.KeptCellCount || !Enum.IsDefined(write.State))
                    throw new InvalidDataException("A decoded scroll room object contains an invalid scroll write.");
            }
        }
        else if (program is not null)
            throw new InvalidDataException("Only a resident scroll room object may own scroll writes.");
        return placement with { ScrollProgram = program, CompiledScrollProgram = compiledProgram };
    }
}
