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

    /// <summary>
    /// Each room's state choice, checked once as the selection loads to name exactly the rooms of
    /// <see cref="RoomDefinitions.All"/>, the only rooms there are (<see cref="RoomDefinition"/>).
    /// </summary>
    private static readonly Dictionary<RoomDefinition, RoomStateChoice> Choices = CheckedChoices();

    /// <summary>The state choice of <paramref name="room"/>, one of the game's rooms as every room is.</summary>
    internal static RoomStateChoice ChoiceFor(RoomDefinition room) => Choices[room];

    /// <summary>The state choices, checked to name every catalog room and no other.</summary>
    /// <exception cref="InvalidOperationException">The choices name another set of rooms than the catalog's.</exception>
    private static Dictionary<RoomDefinition, RoomStateChoice> CheckedChoices()
    {
        Dictionary<RoomDefinition, RoomStateChoice> choices = RoomStateSelectionDefinitions.Choices;
        if (choices.Count != RoomDefinitions.All.Length || !RoomDefinitions.All.All(choices.ContainsKey))
            throw new InvalidOperationException("The rooms' state choices name another set of rooms than the room catalog.");
        return choices;
    }
}
