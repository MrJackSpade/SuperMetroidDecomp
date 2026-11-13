using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using static SuperMetroid.Core.Rooms.RoomObjects.SpeedBoosterBlockObjectDefinitions;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Which Speed Booster block a behaviour and area select.</summary>
internal static class SpeedBoosterBlocks
{
    /// <summary>
    /// Resolves precisely the two common and four Brinstar reactions that select the Speed Booster block setup, giving the
    /// crumble the selected block plays. Other areas' own reactions are unrelated.
    /// </summary>
    public static bool TryResolve(
        RoomBlockBehavior behavior,
        AreaId area,
        [MaybeNullWhen(false)] out BlockReactionChange crumble)
    {
        SpecialBlockReaction reaction = behavior.Special;
        if (CommonBlocks.TryGetValue(reaction, out crumble))
            return true;
        if (area == AreaId.Brinstar && BrinstarBlocks.TryGetValue(reaction, out crumble))
            return true;
        crumble = null;
        return false;
    }
}
