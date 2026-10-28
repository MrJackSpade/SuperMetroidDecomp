using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.World.RoomStates;

/// <summary>
/// Runs a retail room's state-selection program (<see cref="RoomStateSelectionDefinitions"/>) with native first-match
/// precedence, and lists the states a room can select. Only the 262 retail rooms are accepted.
/// </summary>
public static class RoomStateSelection
{
    /// <summary>Selects a state with native first-match precedence.</summary>
    public static RoomStateSettings Select(RoomHeaderDefinition room, RoomStateSelectionContext selection)
    {
        RoomStateProgram program = Program(Ensure.NotNull(room));
        foreach (RoomStateBranch branch in program.Branches)
        {
            if (branch.Condition.IsMet(selection))
                return branch.State;
        }
        return program.Default;
    }

    /// <summary>Returns the default first, then every alternative in native order.</summary>
    public static IReadOnlyList<RoomStateSettings> GetStates(RoomHeaderDefinition room)
    {
        RoomStateProgram program = Program(Ensure.NotNull(room));
        return [program.Default, .. program.Branches.Select(branch => branch.State)];
    }

    /// <summary>The program of <paramref name="room"/>, which must be a compiled retail room.</summary>
    private static RoomStateProgram Program(RoomHeaderDefinition room) =>
        RoomStateSelectionDefinitions.Programs.TryGetValue(room, out RoomStateProgram? program) ? program
            : throw new ArgumentOutOfRangeException(nameof(room), room.Name, "Room is not one of the 262 compiled retail rooms.");
}
