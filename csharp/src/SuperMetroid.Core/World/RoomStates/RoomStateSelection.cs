using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.World.RoomStates;

/// <summary>
/// Runs a retail room's state choice (<see cref="RoomStateSelectionDefinitions"/>) with first-match
/// precedence. Only the 262 retail rooms are accepted.
/// </summary>
public static class RoomStateSelection
{
    /// <summary>Selects a state with first-match precedence.</summary>
    public static RoomStateSettings Select(RoomDefinition room, RoomStateSelectionContext selection)
    {
        RoomStateChoice animation = ChoiceFor(Ensure.NotNull(room));
        foreach (RoomStateBranch branch in animation.Branches)
        {
            if (branch.Condition.IsMet(selection))
                return branch.State;
        }
        return animation.Default;
    }

    /// <summary>The state choice of <paramref name="room"/>, which must be a compiled retail room.</summary>
    internal static RoomStateChoice ChoiceFor(RoomDefinition room) =>
        RoomStateSelectionDefinitions.Choices.TryGetValue(room, out RoomStateChoice? animation) ? animation
            : throw new ArgumentOutOfRangeException(nameof(room), room.Name, "Room is not one of the 262 compiled retail rooms.");
}
