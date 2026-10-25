using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.World.RoomStates;

/// <summary>Derives the complete level identity domain from compiled retail room states.</summary>
public static class RoomVisualLayoutSourceDefinitions
{
    /// <summary>
    /// Distinct room levels, including alternate event/boss states' shared ones, in the order the room-state catalogs
    /// first use them. These are installation keys, not ROM-reading capabilities or editable collision data.
    /// </summary>
    /// Enumerating projects each state's level and removes duplicates. No persistent lookup or generated cache is retained.
    public static IEnumerable<RoomLevelId> All => RoomStateDefinitions.All
        .Select(state => state.Level).Distinct();
}
