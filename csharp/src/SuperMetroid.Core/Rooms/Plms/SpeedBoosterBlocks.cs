using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using static SuperMetroid.Core.Rooms.Plms.SpeedBoosterBlockPlmDefinitions;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Which Speed Booster block a behaviour and area select.</summary>
internal static class SpeedBoosterBlocks
{
    /// <summary>
    /// Resolves precisely the two area-independent and four Brinstar area-table entries
    /// that point at setup <c>$84:CDEA</c>. Other areas' negative BTS entries are unrelated.
    /// </summary>
    public static bool TryResolve(
        RoomBlockBehavior bts,
        AreaId area,
        out SpeedBoosterBlockPlmDefinition definition)
    {
        if (!bts.UsesAreaReactionTable)
        {
            if (bts.Value == RespawningBts)
            {
                definition = Respawning;
                return true;
            }
            if (bts.Value == PermanentBts)
            {
                definition = Permanent;
                return true;
            }
        }
        else if (area == AreaId.Brinstar)
        {
            definition = bts.AreaReactionIndex switch
            {
                2 => BrinstarSlowRespawning,
                3 => BrinstarSlowPermanent,
                4 => DachoraRespawning,
                5 => Permanent,
                _ => default,
            };
            return bts.AreaReactionIndex is >= 2 and <= 5;
        }

        definition = default;
        return false;
    }
}
