using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.World.RoomStates;

/// <summary>
/// Runs a retail room's state-selection program (<see cref="RoomStateSelectionDefinitions"/>) with first-match
/// precedence. Only the 262 retail rooms are accepted.
/// </summary>
public static class RoomStateSelection
{
    /// <summary>Selects a state with first-match precedence.</summary>
    public static RoomStateSettings Select(RoomHeaderDefinition room, RoomStateSelectionContext selection)
    {
        RoomStateChoice program = ChoiceFor(Ensure.NotNull(room));
        foreach (RoomStateBranch branch in program.Branches)
        {
            if (branch.Condition.IsMet(selection))
                return branch.State;
        }
        return program.Default;
    }

    /// <summary>The program of <paramref name="room"/>, which must be a compiled retail room.</summary>
    internal static RoomStateChoice ChoiceFor(RoomHeaderDefinition room) =>
        RoomStateSelectionDefinitions.Choices.TryGetValue(room, out RoomStateChoice? program) ? program
            : throw new ArgumentOutOfRangeException(nameof(room), room.Name, "Room is not one of the 262 compiled retail rooms.");
}
