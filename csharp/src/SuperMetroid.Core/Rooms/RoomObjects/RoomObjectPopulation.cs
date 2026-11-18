namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The room objects a room state places as it loads, in order. Construction reads only the decoded placements, each
/// carrying its family's typed setup (#2383).
/// </summary>
public sealed class RoomObjectPopulation
{
    /// <summary>Owned placements, in the order the room's objects take their places.</summary>
    private readonly RoomObjectPlacement[] placements;

    /// <summary>The population's name.</summary>
    public string Name { get; }
    /// <summary>Validated placements in allocation order.</summary>
    public ReadOnlyMemory<RoomObjectPlacement> Placements => placements;

    /// <summary>Copies an ordered decoded population, rejecting an empty placement before any room object takes a place.</summary>
    /// <param name="name">The population's name, used in diagnostics.</param>
    /// <param name="placements">
    /// The placements in order, any number of them (#1627): the room's places, not the population, bound how many objects
    /// it holds at once (<see cref="SuperMetroid.Core.Compatibility.RoomObjectPlaces"/>).
    /// </param>
    /// <exception cref="ArgumentNullException">The sequence or one of its placements is null.</exception>
    /// <exception cref="InvalidDataException">One is an empty placement.</exception>
    public RoomObjectPopulation(string name, IEnumerable<RoomObjectPlacement> placements)
    {
        Ensure.NotNull(placements);
        Name = Ensure.NotNull(name);
        this.placements = placements.Select(Validated).ToArray();
    }

    /// <summary>Builds a room's population from its compiled placements, each with its family's typed setup.</summary>
    /// <param name="name">The population's name.</param>
    /// <param name="placements">Each placement's room-object kind, block coordinates and setup, in authored order.</param>
    internal static RoomObjectPopulation FromPlacements(string name, RoomObjectPlacementDefinition[] placements) =>
        new(name, Ensure.NotNull(placements).Select(placement => new RoomObjectPlacement(
            RoomObjectKinds.RequirePlaced(placement.Kind), placement.Block, placement.Setup) { Site = placement.Site }));

    /// <summary>Rejects an empty placement.</summary>
    /// <exception cref="InvalidDataException">The placement is malformed.</exception>
    private static RoomObjectPlacement Validated(RoomObjectPlacement placement)
    {
        Ensure.NotNull(placement);
        if (placement.Kind == RoomObjectKind.None)
            throw new InvalidDataException("A decoded room-object placement cannot be an empty placement.");
        return placement;
    }
}
