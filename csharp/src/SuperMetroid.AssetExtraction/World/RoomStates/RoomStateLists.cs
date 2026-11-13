using SuperMetroid.Core;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.AssetExtraction.World.RoomStates;

/// <summary>Lists every state a retail room can select, for the import's per-state sheets and layouts.</summary>
public static class RoomStateLists
{
    /// <summary>Returns the default first, then every alternative in authored order.</summary>
    public static IReadOnlyList<RoomStateSettings> GetStates(RoomHeaderDefinition room)
    {
        RoomStateChoice program = RoomStateSelection.ChoiceFor(Ensure.NotNull(room));
        return [program.Default, .. program.Branches.Select(branch => branch.State)];
    }
}
