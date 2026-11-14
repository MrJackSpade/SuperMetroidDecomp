using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// Immutable, ordered input to the room-object allocator. Construction reads only the
/// decoded placements.
/// </summary>
public sealed class RoomObjectPopulation
{
    /// <summary>Owned placement records kept in the order the room-object allocator consumes them.</summary>
    private readonly RoomObjectPlacement[] placements;

    /// <summary>The population's name, or a fixture's description.</summary>
    public string Name { get; }
    /// <summary>Validated placement records in allocation order; scroll changes are owned copies.</summary>
    public ReadOnlyMemory<RoomObjectPlacement> Placements => placements;

    /// <summary>Copies an ordered decoded population and validates scroll changes before any room-object slots are allocated.</summary>
    /// <param name="name">The population's name, used in diagnostics.</param>
    /// <param name="records">
    /// The placements in order, any number of them (#1627): the room's places, not the population, bound how many objects
    /// it holds at once (<see cref="SuperMetroid.Core.Compatibility.RoomObjectPlaces"/>).
    /// </param>
    /// <exception cref="ArgumentNullException">The sequence or one of its placements is null.</exception>
    /// <exception cref="InvalidDataException">One is an empty placement, or a setup-specific payload is invalid.</exception>
    public RoomObjectPopulation(string name, IEnumerable<RoomObjectPlacement> records)
    {
        Ensure.NotNull(records);
        Name = Ensure.NotNull(name);
        placements = records.Select(CopyPlacement).ToArray();
    }

    /// <summary>Builds a retail population from its compiled records, binding scroll triggers to their compiled scroll-change identities.</summary>
    /// <param name="name">The population's name.</param>
    /// <param name="records">Each record's room-object kind, block coordinates and room argument, in authored order.</param>
    internal static RoomObjectPopulation FromRecords(string name, RoomObjectPlacementDefinition[] records) =>
        new(name, Ensure.NotNull(records).Select(record => new RoomObjectPlacement(
            RoomObjectKinds.RequirePlaced(record.Kind), record.Block, record.Argument, Cannon: record.Cannon)
            { CompiledScrollChange = record.ScrollChange }));

    /// <summary>Copies one placement's scroll change and rejects a malformed one.</summary>
    private static RoomObjectPlacement CopyPlacement(RoomObjectPlacement placement)
    {
        Ensure.NotNull(placement);
        if (placement.Kind == RoomObjectKind.None)
            throw new InvalidDataException("A decoded room-object placement cannot be an empty placement.");
        ScreenScrollChange[]? animation = placement.ScrollChange?.ToArray();
        RoomScrollChange? compiledChange = animation is null ? placement.CompiledScrollChange : null;
        if (compiledChange is not null)
        {
            if (placement.Kind != RoomObjectKind.ScrollTrigger)
                throw new InvalidDataException("Only a scroll trigger may run a retail scroll program.");
        }
        else if (placement.Kind == RoomObjectKind.ScrollTrigger)
        {
            if (animation is null || animation.Length >= RoomScrollGrid.KeptCellCount)
                throw new InvalidDataException("A decoded scroll room object needs a bounded write program.");
            foreach (ScreenScrollChange write in animation)
            {
                // A screen in column X and row Y lies, in a room of any width past its column, at least (X + 1) * (Y + 1) - 1
                // cells in; one no room width brings within the kept cells is a write to no screen.
                if ((write.Screen.X + 1) * (write.Screen.Y + 1) > RoomScrollGrid.KeptCellCount || !Enum.IsDefined(write.State))
                    throw new InvalidDataException("A decoded scroll room object contains an invalid scroll write.");
            }
        }
        else if (animation is not null)
            throw new InvalidDataException("Only a resident scroll room object may own scroll writes.");
        return placement with { ScrollChange = animation, CompiledScrollChange = compiledChange };
    }
}
