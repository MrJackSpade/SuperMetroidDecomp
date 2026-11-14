using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Rooms.LevelData;
using static SuperMetroid.Core.Rooms.RoomObjects.SpeedBoosterBlockObjectDefinitions;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Which Speed Booster block a behaviour and area select.</summary>
internal static class SpeedBoosterBlocks
{
    /// <summary>
    /// Resolves precisely the two common and four Brinstar reactions that select the Speed Booster block setup, giving the
    /// crumble the selected block plays. The import resolves the other areas' own reactions to what they are (#1805).
    /// </summary>
    public static bool TryResolve(RoomBlockBehavior behavior, [MaybeNullWhen(false)] out BlockReactionChange crumble)
    {
        if (behavior.Special is not { } reaction)
        {
            crumble = null;
            return false;
        }
        return CommonBlocks.TryGetValue(reaction, out crumble) || BrinstarBlocks.TryGetValue(reaction, out crumble);
    }
}
