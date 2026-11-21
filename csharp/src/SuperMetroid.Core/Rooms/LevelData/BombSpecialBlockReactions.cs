using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>What a normal bomb sets off at a special block (#1742).</summary>
public static class BombSpecialBlockReactions
{
    /// <summary>The plain solid special blocks a normal bomb shows as Speed Booster blocks in every area.</summary>
    private static readonly BombRevealedSpeedBlockLookalikes<SpecialBlockReaction, LastingBlockReactionChange> SpeedBlockLookalikes =
        new([SpecialBlockReaction.BombRevealsSpeedBlock], SpeedBoosterBlockObjectDefinitions.BombReveal);

    /// <summary>
    /// What a bomb sets off at each special block's reaction: the crumble reveal, the Speed Booster block's reveal, or the
    /// reaction that does nothing but hold its place for an update. Brinstar's own Speed Booster blocks are revealed as
    /// Speed Booster blocks; in the other areas those blocks stay plain solid blocks (#1805),
    /// and the run-on's lookalikes take the reveal the run-on names.
    /// </summary>
    private static readonly IdentityTable<SpecialBlockReaction, LastingBlockReactionChange> Changes = new(
        new Dictionary<SpecialBlockReaction, LastingBlockReactionChange>
        {
            [SpecialBlockReaction.RespawningCrumble1x1] = BlockReactionChanges.CrumbleReveal(SpecialBlockReaction.RespawningCrumble1x1),
            [SpecialBlockReaction.RespawningCrumble2x1] = BlockReactionChanges.CrumbleReveal(SpecialBlockReaction.RespawningCrumble2x1),
            [SpecialBlockReaction.RespawningCrumble1x2] = BlockReactionChanges.CrumbleReveal(SpecialBlockReaction.RespawningCrumble1x2),
            [SpecialBlockReaction.RespawningCrumble2x2] = BlockReactionChanges.CrumbleReveal(SpecialBlockReaction.RespawningCrumble2x2),
            [SpecialBlockReaction.PermanentCrumble1x1] = BlockReactionChanges.CrumbleReveal(SpecialBlockReaction.PermanentCrumble1x1),
            [SpecialBlockReaction.PermanentCrumble2x1] = BlockReactionChanges.CrumbleReveal(SpecialBlockReaction.PermanentCrumble2x1),
            [SpecialBlockReaction.PermanentCrumble1x2] = BlockReactionChanges.CrumbleReveal(SpecialBlockReaction.PermanentCrumble1x2),
            [SpecialBlockReaction.PermanentCrumble2x2] = BlockReactionChanges.CrumbleReveal(SpecialBlockReaction.PermanentCrumble2x2),
            [SpecialBlockReaction.RespawningSpeedBlock] = SpeedBoosterBlockObjectDefinitions.BombReveal,
            [SpecialBlockReaction.PermanentSpeedBlock] = SpeedBoosterBlockObjectDefinitions.BombReveal,
            [SpecialBlockReaction.SlowRespawningSpeedBlock] = SpeedBoosterBlockObjectDefinitions.BombReveal,
            [SpecialBlockReaction.SlowPermanentSpeedBlock] = SpeedBoosterBlockObjectDefinitions.BombReveal,
            [SpecialBlockReaction.BrinstarRespawningSpeedBlock] = SpeedBoosterBlockObjectDefinitions.BombReveal,
            [SpecialBlockReaction.BrinstarPermanentSpeedBlock] = SpeedBoosterBlockObjectDefinitions.BombReveal,
            [SpecialBlockReaction.XrayRevealedSolid] = LastingBlockReactionChange.MadeAtSpawn,
            [SpecialBlockReaction.CollectibleTrigger] = LastingBlockReactionChange.MadeAtSpawn,
            [SpecialBlockReaction.MapStationRight] = LastingBlockReactionChange.MadeAtSpawn,
            [SpecialBlockReaction.MapStationLeft] = LastingBlockReactionChange.MadeAtSpawn,
            [SpecialBlockReaction.EnergyStationRight] = LastingBlockReactionChange.MadeAtSpawn,
            [SpecialBlockReaction.EnergyStationLeft] = LastingBlockReactionChange.MadeAtSpawn,
            [SpecialBlockReaction.MissileStationRight] = LastingBlockReactionChange.MadeAtSpawn,
            [SpecialBlockReaction.MissileStationLeft] = LastingBlockReactionChange.MadeAtSpawn,
            [SpecialBlockReaction.SaveStationFloor] = LastingBlockReactionChange.MadeAtSpawn,
            [SpecialBlockReaction.Solid] = LastingBlockReactionChange.MadeAtSpawn,
            [SpecialBlockReaction.WreckedShipChozoHand] = LastingBlockReactionChange.MadeAtSpawn,
            [SpecialBlockReaction.LowerNorfairChozoHand] = LastingBlockReactionChange.MadeAtSpawn,
        }.Concat(SpeedBlockLookalikes.ShownAsSpeedBlocks.Select(reaction =>
            KeyValuePair.Create(reaction, SpeedBlockLookalikes.SpeedBlockReveal))).ToDictionary());

    /// <summary>
    /// The crumble reveal, the Speed Booster block's reveal, or the reaction that does nothing but hold its place for an update,
    /// for a special block with <paramref name="reaction"/> (<see cref="Changes"/>).
    /// </summary>
    /// <param name="reaction">The block's reaction.</param>
    internal static LastingBlockReactionChange ChangeFor(SpecialBlockReaction reaction) => Changes[reaction];
}
