using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>What a normal bomb sets off at a special block (#1742).</summary>
public static class BombSpecialBlockReactions
{
    /// <summary>The plain solid special blocks a normal bomb shows as Speed Booster blocks in every area.</summary>
    private static readonly SpecialBlockBombRunOn<SpecialBlockReaction, BlockReactionChange> RunOn =
        new([SpecialBlockReaction.BombRevealsSpeedBlock], SpeedBoosterBlockObjectDefinitions.BombReveal);

    /// <summary>
    /// The crumble reveal, the Speed Booster block's reveal, or the no-op reaction that only holds its place for an update,
    /// for a special block with <paramref name="reaction"/> in <paramref name="area"/>: Brinstar's own Speed Booster blocks
    /// are revealed only in Brinstar, and elsewhere are plain solid blocks.
    /// </summary>
    /// <param name="reaction">The block's reaction.</param>
    /// <param name="area">The room's area, one of the retail areas.</param>
    /// <exception cref="ArgumentOutOfRangeException">The block has no special-block reaction, or the area is not a retail area.</exception>
    internal static BlockReactionChange ChangeFor(SpecialBlockReaction? reaction, AreaId area)
    {
        _ = AreaIds.Require(area);
        SpecialBlockReaction read = reaction ??
            throw new ArgumentOutOfRangeException(nameof(reaction), reaction, "The block has no special-block reaction for a bomb.");
        if (RunOn.TryShowAsSpeedBlock(read, out BlockReactionChange? shown))
            return shown;
        return read switch
        {
            SpecialBlockReaction.RespawningCrumble1x1 or SpecialBlockReaction.RespawningCrumble2x1 or
                SpecialBlockReaction.RespawningCrumble1x2 or SpecialBlockReaction.RespawningCrumble2x2 or
                SpecialBlockReaction.PermanentCrumble1x1 or SpecialBlockReaction.PermanentCrumble2x1 or
                SpecialBlockReaction.PermanentCrumble1x2 or SpecialBlockReaction.PermanentCrumble2x2 =>
                BlockReactionChanges.CrumbleReveal(read),
            SpecialBlockReaction.RespawningSpeedBlock or SpecialBlockReaction.PermanentSpeedBlock =>
                SpeedBoosterBlockObjectDefinitions.BombReveal,
            SpecialBlockReaction.BrinstarSlowRespawningSpeedBlock or
                SpecialBlockReaction.BrinstarSlowPermanentSpeedBlockOrNorfairChozoHand or
                SpecialBlockReaction.BrinstarRespawningSpeedBlock or SpecialBlockReaction.BrinstarPermanentSpeedBlock =>
                area == AreaId.Brinstar ? SpeedBoosterBlockObjectDefinitions.BombReveal : BlockReactionChange.MadeAtSpawn,
            SpecialBlockReaction.XrayRevealedSolid or SpecialBlockReaction.CollectibleTrigger or
                SpecialBlockReaction.MapStationRight or SpecialBlockReaction.MapStationLeft or
                SpecialBlockReaction.EnergyStationRight or SpecialBlockReaction.EnergyStationLeft or
                SpecialBlockReaction.MissileStationRight or SpecialBlockReaction.MissileStationLeft or
                SpecialBlockReaction.SaveStationFloor or SpecialBlockReaction.Solid or
                SpecialBlockReaction.WreckedShipChozoHand or SpecialBlockReaction.SolidInEveryArea => BlockReactionChange.MadeAtSpawn,
            SpecialBlockReaction.BombRevealsSpeedBlock => throw new InvalidOperationException(
                $"{nameof(SpecialBlockReaction.BombRevealsSpeedBlock)} is shown as a Speed Booster block by the run-on."),
            _ => throw new InvalidOperationException($"Undefined {nameof(SpecialBlockReaction)} {read}."),
        };
    }
}
