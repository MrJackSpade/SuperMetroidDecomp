using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// Immutable, ordered input to the room-object allocator. Construction reads only the
/// decoded placements, and requires a scroll change of every scroll trigger and of no other room object.
/// </summary>
public sealed class RoomObjectPopulation
{
    /// <summary>Owned placements kept in the order the room-object allocator consumes them.</summary>
    private readonly RoomObjectPlacement[] placements;

    /// <summary>The population's name.</summary>
    public string Name { get; }
    /// <summary>Validated placements in allocation order.</summary>
    public ReadOnlyMemory<RoomObjectPlacement> Placements => placements;

    /// <summary>Copies an ordered decoded population and validates its scroll changes before any room object takes a place.</summary>
    /// <param name="name">The population's name, used in diagnostics.</param>
    /// <param name="placements">
    /// The placements in order, any number of them (#1627): the room's places, not the population, bound how many objects
    /// it holds at once (<see cref="SuperMetroid.Core.Compatibility.RoomObjectPlaces"/>).
    /// </param>
    /// <exception cref="ArgumentNullException">The sequence or one of its placements is null.</exception>
    /// <exception cref="InvalidDataException">
    /// One is an empty placement, a scroll trigger without a scroll change, or another room object with one.
    /// </exception>
    public RoomObjectPopulation(string name, IEnumerable<RoomObjectPlacement> placements)
    {
        Ensure.NotNull(placements);
        Name = Ensure.NotNull(name);
        this.placements = placements.Select(Validated).ToArray();
    }

    /// <summary>Builds a room's population from its compiled placements, binding scroll triggers to their compiled scroll-change identities.</summary>
    /// <param name="name">The population's name.</param>
    /// <param name="placements">Each placement's room-object kind, block coordinates and room argument, in authored order.</param>
    internal static RoomObjectPopulation FromPlacements(string name, RoomObjectPlacementDefinition[] placements) =>
        new(name, Ensure.NotNull(placements).Select(placement => new RoomObjectPlacement(
            RoomObjectKinds.RequirePlaced(placement.Kind), placement.Block, placement.Argument, Cannon: placement.Cannon)
            { ScrollChange = placement.ScrollChange, Site = placement.Site }));

    /// <summary>Rejects an empty placement, a scroll trigger without its scroll change and any other room object with one.</summary>
    /// <exception cref="InvalidDataException">The placement is malformed.</exception>
    private static RoomObjectPlacement Validated(RoomObjectPlacement placement)
    {
        Ensure.NotNull(placement);
        if (placement.Kind == RoomObjectKind.None)
            throw new InvalidDataException("A decoded room-object placement cannot be an empty placement.");
        if (placement.Kind == RoomObjectKind.ScrollTrigger && placement.ScrollChange is null)
            throw new InvalidDataException("A scroll trigger needs the scroll change it applies.");
        if (placement.Kind != RoomObjectKind.ScrollTrigger && placement.ScrollChange is not null)
            throw new InvalidDataException("Only a scroll trigger may apply a scroll change.");
        return placement;
    }
}
