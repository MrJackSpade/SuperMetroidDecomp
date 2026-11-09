using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using static SuperMetroid.Core.Rooms.RoomObjects.SpeedBoosterBlockObjectDefinitions;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Which Speed Booster block a behaviour and area select.</summary>
internal static class SpeedBoosterBlocks
{
    /// <summary>
    /// Resolves precisely the two common and four Brinstar reactions that select the Speed Booster block setup. Other
    /// areas' own reactions are unrelated.
    /// </summary>
    public static bool TryResolve(
        RoomBlockBehavior behavior,
        AreaId area,
        out SpeedBoosterBlockObjectDefinition definition)
    {
        BlockReaction reaction = behavior.Reaction;
        if (CommonBlocks.TryGetValue(reaction, out definition))
            return true;
        if (area == AreaId.Brinstar && BrinstarBlocks.TryGetValue(reaction, out definition))
            return true;
        definition = default;
        return false;
    }
}
