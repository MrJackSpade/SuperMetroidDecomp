using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// The area of the rooms that use each room effect selection (#1526): a record's effect bits select from its room's area's lists,
/// so <see cref="RoomFxRecordCodec"/> decodes a selection's records, and checks encode them back, in that area. Read from the
/// retail rooms' state-selection programs, whose states name the selections they load.
/// </summary>
internal static class RoomFxSelectionAreas
{
    /// <summary>Each selection's area, by selection instance.</summary>
    private static readonly Dictionary<RoomFxSelection, AreaId> Areas = Index();

    /// <summary>The area of every room whose states load <paramref name="fx"/>.</summary>
    /// <exception cref="InvalidDataException">No retail room state loads <paramref name="fx"/>.</exception>
    internal static AreaId Of(RoomFxSelection fx) =>
        Areas.TryGetValue(Ensure.NotNull(fx), out AreaId area) ? area
            : throw new InvalidDataException($"No retail room state loads room FX {fx.Name}, so its area is unknown.");

    /// <summary>Gives each selection loaded by a retail room state the area of the rooms that load it.</summary>
    /// <exception cref="InvalidDataException">Rooms of two areas load one selection.</exception>
    private static Dictionary<RoomFxSelection, AreaId> Index()
    {
        var areas = new Dictionary<RoomFxSelection, AreaId>(ReferenceEqualityComparer.Instance);
        foreach (KeyValuePair<RoomDefinition, RoomStateChoice> entry in RoomStateSelectionDefinitions.Choices)
        {
            RoomDefinition room = entry.Key;
            RoomStateChoice program = entry.Value;
            foreach (RoomStateSettings state in program.Branches.Select(branch => branch.State).Prepend(program.Default))
            {
                if (state.Fx is not { } fx)
                    continue;
                if (!areas.TryAdd(fx, room.Area) && areas[fx] != room.Area)
                {
                    throw new InvalidDataException(
                        $"Room FX {fx.Name} is loaded by rooms of {areas[fx]} and {room.Area}; its records' effects would differ.");
                }
            }
        }
        return areas;
    }
}
