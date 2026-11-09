using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// Immutable, ordered input to the room PLM allocator. Pointers survive only
/// as identities and diagnostic context; construction never reads a CPU bus.
/// </summary>
public sealed class RoomObjectPopulation
{
    /// <summary>Owned placement records kept in the order the PLM allocator consumes them.</summary>
    private readonly RoomObjectPlacement[] placements;

    /// <summary>The population's name, or a fixture's description.</summary>
    public string Name { get; }
    /// <summary>Validated placement records in allocation order, excluding the zero header terminator; scroll programs are owned copies.</summary>
    public ReadOnlyMemory<RoomObjectPlacement> Placements => placements;

    /// <summary>Copies an ordered, bounded decoded population and validates scroll programs before any PLM slots are allocated.</summary>
    /// <param name="name">The population's name, used in diagnostics.</param>
    /// <param name="records">Placement sequence without a zero terminator; at most 255 records may be supplied.</param>
    /// <exception cref="ArgumentNullException">The sequence or one of its placements is null.</exception>
    /// <exception cref="InvalidDataException">The record limit is exceeded, a header is the zero terminator, or a setup-specific payload is invalid.</exception>
    public RoomObjectPopulation(string name, IEnumerable<RoomObjectPlacement> records)
    {
        Ensure.NotNull(records);
        Name = Ensure.NotNull(name);
        // A population is bounded by its 256-record parser. Never enumerate
        // an unbounded caller sequence before enforcing that same upper limit.
        placements = records.Take(RoomObjectPopulationFormat.MaximumParserIterations).Select(CopyPlacement).ToArray();
        if (placements.Length > RoomObjectPopulationFormat.MaximumRecordCount)
            throw new InvalidDataException($"PLM population {name} exceeds the most placements a room can hold.");
    }

    /// <summary>Builds a retail population from its compiled records, binding scroll triggers to their compiled program identities.</summary>
    /// <param name="name">The population's name.</param>
    /// <param name="records">Each record's PLM header, block coordinates and room argument, in authored order.</param>
    internal static RoomObjectPopulation FromRecords(string name, RoomObjectPlacementDefinition[] records) =>
        new(name, Ensure.NotNull(records).Select(record => new RoomObjectPlacement(
            RoomObjectHeaders.Get(record.Header), record.Block, record.Argument, Cannon: record.Cannon)
            { CompiledScrollProgram = record.ScrollProgram }));

    /// <summary>Copies one placement's scroll program and rejects a malformed one.</summary>
    private static RoomObjectPlacement CopyPlacement(RoomObjectPlacement placement)
    {
        Ensure.NotNull(placement);
        if (placement.Header.Header == RoomObjectHeaderId.None)
            throw new InvalidDataException("A decoded PLM placement cannot be an empty placement.");
        RoomScrollWrite[]? program = placement.ScrollProgram?.ToArray();
        RoomScrollProgram? compiledProgram = program is null ? placement.CompiledScrollProgram : null;
        if (compiledProgram is not null)
        {
            if (placement.Header.Header != RoomObjectHeaderId.ScrollTrigger)
                throw new InvalidDataException("Only a scroll trigger may run a retail scroll program.");
        }
        else if (placement.Header.Header == RoomObjectHeaderId.ScrollTrigger)
        {
            if (program is null || program.Length >= RoomScrollGrid.KeptCellCount)
                throw new InvalidDataException("A decoded scroll PLM needs a bounded write program.");
            foreach (RoomScrollWrite write in program)
            {
                if (write.Index is < 0 or >= RoomScrollGrid.KeptCellCount || !Enum.IsDefined(write.State))
                    throw new InvalidDataException("A decoded scroll PLM contains an invalid scroll write.");
            }
        }
        else if (program is not null)
            throw new InvalidDataException("Only a resident scroll PLM may own scroll writes.");
        return placement with { ScrollProgram = program, CompiledScrollProgram = compiledProgram };
    }
}
