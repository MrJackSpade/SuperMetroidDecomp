using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>What a normal bomb sets off at a special block (#1742).</summary>
public static class BombSpecialBlockReactions
{
    /// <summary>The plain solid special blocks a normal bomb shows as Speed Booster blocks in every area.</summary>
    private static readonly BombRevealedSpeedBlockLookalikes<SpecialBlockReaction, LastingBlockReactionChange> SpeedBlockLookalikes =
        new([SpecialBlockReaction.BombRevealsSpeedBlock], SpeedBoosterBlockObjectDefinitions.BombReveal);

    /// <summary>
    /// The crumble reveal, the Speed Booster block's reveal, or the reaction that does nothing but hold its place for an update,
    /// for a special block with <paramref name="reaction"/>. Brinstar's own Speed Booster blocks are revealed as Speed
    /// Booster blocks; in the other areas those blocks stay plain solid blocks (#1805).
    /// </summary>
    /// <param name="reaction">The block's reaction.</param>
    internal static LastingBlockReactionChange ChangeFor(SpecialBlockReaction reaction)
    {
        SpecialBlockReaction read = reaction;
        if (SpeedBlockLookalikes.TryShowAsSpeedBlock(read, out LastingBlockReactionChange? shown))
            return shown;
        return read switch
        {
            SpecialBlockReaction.RespawningCrumble1x1 or SpecialBlockReaction.RespawningCrumble2x1 or
                SpecialBlockReaction.RespawningCrumble1x2 or SpecialBlockReaction.RespawningCrumble2x2 or
                SpecialBlockReaction.PermanentCrumble1x1 or SpecialBlockReaction.PermanentCrumble2x1 or
                SpecialBlockReaction.PermanentCrumble1x2 or SpecialBlockReaction.PermanentCrumble2x2 =>
                BlockReactionChanges.CrumbleReveal(read),
            SpecialBlockReaction.RespawningSpeedBlock or SpecialBlockReaction.PermanentSpeedBlock or
                SpecialBlockReaction.SlowRespawningSpeedBlock or SpecialBlockReaction.SlowPermanentSpeedBlock or
                SpecialBlockReaction.BrinstarRespawningSpeedBlock or SpecialBlockReaction.BrinstarPermanentSpeedBlock =>
                SpeedBoosterBlockObjectDefinitions.BombReveal,
            SpecialBlockReaction.XrayRevealedSolid or SpecialBlockReaction.CollectibleTrigger or
                SpecialBlockReaction.MapStationRight or SpecialBlockReaction.MapStationLeft or
                SpecialBlockReaction.EnergyStationRight or SpecialBlockReaction.EnergyStationLeft or
                SpecialBlockReaction.MissileStationRight or SpecialBlockReaction.MissileStationLeft or
                SpecialBlockReaction.SaveStationFloor or SpecialBlockReaction.Solid or
                SpecialBlockReaction.WreckedShipChozoHand or SpecialBlockReaction.LowerNorfairChozoHand =>
                LastingBlockReactionChange.MadeAtSpawn,
            SpecialBlockReaction.BombRevealsSpeedBlock => throw new InvalidOperationException(
                $"{nameof(SpecialBlockReaction.BombRevealsSpeedBlock)} is shown as a Speed Booster block by the run-on."),
            _ => throw new InvalidOperationException($"Undefined {nameof(SpecialBlockReaction)} {read}."),
        };
    }
}
