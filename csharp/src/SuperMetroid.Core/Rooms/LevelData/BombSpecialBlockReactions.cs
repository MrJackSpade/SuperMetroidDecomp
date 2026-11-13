using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>What a normal bomb sets off at a special block whose reaction is not one of its area's.</summary>
public static class BombSpecialBlockReactions
{
    /// <summary>
    /// The crumble reveal, the Speed Booster block's reveal, or the no-op reaction that only holds its place for an
    /// update. The blocks a bomb shows as Speed Booster blocks without being one are read past the end of the reactions
    /// every area shares, into Brinstar's.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The reaction is past the table, or one of the area's.</exception>
    internal static BlockReactionChange ChangeFor(SpecialBlockReaction reaction) => reaction switch
    {
        SpecialBlockReaction.RespawningCrumble1x1 or SpecialBlockReaction.RespawningCrumble2x1 or
            SpecialBlockReaction.RespawningCrumble1x2 or SpecialBlockReaction.RespawningCrumble2x2 or
            SpecialBlockReaction.PermanentCrumble1x1 or SpecialBlockReaction.PermanentCrumble2x1 or
            SpecialBlockReaction.PermanentCrumble1x2 or SpecialBlockReaction.PermanentCrumble2x2 =>
            BlockReactionChanges.CrumbleReveal(reaction),
        SpecialBlockReaction.RespawningSpeedBlock or SpecialBlockReaction.PermanentSpeedBlock or
            SpecialBlockReaction.BombRevealsSpeedBlock => SpeedBoosterBlockObjectDefinitions.BombReveal,
        SpecialBlockReaction.XrayRevealedSolid or SpecialBlockReaction.CollectibleTrigger or
            SpecialBlockReaction.MapStationRight or SpecialBlockReaction.MapStationLeft or
            SpecialBlockReaction.EnergyStationRight or SpecialBlockReaction.EnergyStationLeft or
            SpecialBlockReaction.MissileStationRight or SpecialBlockReaction.MissileStationLeft or
            SpecialBlockReaction.SaveStationFloor or SpecialBlockReaction.Solid => BlockReactionChange.MadeAtSpawn,
        SpecialBlockReaction.PastTable or SpecialBlockReaction.WreckedShipChozoHand or SpecialBlockReaction.AreaSolid or
            SpecialBlockReaction.BrinstarSlowRespawningSpeedBlock or
            SpecialBlockReaction.BrinstarSlowPermanentSpeedBlockOrNorfairChozoHand or
            SpecialBlockReaction.BrinstarRespawningSpeedBlock or SpecialBlockReaction.BrinstarPermanentSpeedBlock or
            SpecialBlockReaction.PastAreaTable =>
            throw new ArgumentOutOfRangeException(nameof(reaction), reaction, "The special block's reaction has no bombed reaction every area shares."),
        _ => throw new InvalidOperationException($"Undefined {nameof(SpecialBlockReaction)} {reaction}."),
    };

    /// <summary>Whether <paramref name="reaction"/> is one the room's area gives special blocks.</summary>
    public static bool IsArea(SpecialBlockReaction reaction) => reaction is SpecialBlockReaction.WreckedShipChozoHand or
        SpecialBlockReaction.AreaSolid or SpecialBlockReaction.BrinstarSlowRespawningSpeedBlock or
        SpecialBlockReaction.BrinstarSlowPermanentSpeedBlockOrNorfairChozoHand or SpecialBlockReaction.BrinstarRespawningSpeedBlock or
        SpecialBlockReaction.BrinstarPermanentSpeedBlock or SpecialBlockReaction.PastAreaTable;
}
