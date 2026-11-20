using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.World.RoomStates;

/// <summary>
/// Runs a room's state choice (<see cref="RoomStateSelectionDefinitions"/>) with first-match
/// precedence. Only the rooms of <see cref="SuperMetroid.Core.Rooms.RoomDefinitions.All"/> are accepted.
/// </summary>
public static class RoomStateSelection
{
    /// <summary>Selects a state with first-match precedence.</summary>
    public static RoomStateSettings Select(RoomDefinition room, RoomStateSelectionContext selection)
    {
        RoomStateChoice choice = ChoiceFor(Ensure.NotNull(room));
        foreach (RoomStateBranch branch in choice.Branches)
        {
            if (branch.Condition.IsMet(selection))
                return branch.State;
        }
        return choice.Default;
    }

    /// <summary>The state choice of <paramref name="room"/>, which must be one of the game's rooms.</summary>
    internal static RoomStateChoice ChoiceFor(RoomDefinition room) =>
        RoomStateSelectionDefinitions.Choices.TryGetValue(room, out RoomStateChoice? choice) ? choice
            : throw new ArgumentOutOfRangeException(nameof(room), room.Name, "Room is not one of the game's rooms.");
}
