using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>What a normal bomb sets off at a special block (#1742).</summary>
public static partial class BombSpecialBlockReactions
{
    /// <summary>The plain solid special blocks a normal bomb shows as Speed Booster blocks in every area.</summary>
    private static readonly BombRevealedSpeedBlockLookalikes<SpecialBlockReaction, BlockReactionChange> SpeedBlockLookalikes =
        new([SpecialBlockReaction.BombRevealsSpeedBlock], SpeedBoosterBlockObjectDefinitions.BombReveal);

    /// <summary>
    /// What a bomb sets off at each special block's reaction: the crumble reveal, the Speed Booster block's reveal, or the
    /// reaction that does nothing but hold its place for an update. Brinstar's own Speed Booster blocks are revealed as
    /// Speed Booster blocks; in the other areas those blocks stay plain solid blocks (#1805),
    /// and the run-on's lookalikes take the reveal the run-on names.
    /// </summary>
    private static readonly IdentityTable<SpecialBlockReaction, BlockReactionChange> Changes = new(
        new Dictionary<SpecialBlockReaction, BlockReactionChange>
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
            [SpecialBlockReaction.XrayRevealedSolid] = BlockReactionChange.MadeAtSpawn,
            [SpecialBlockReaction.CollectibleTrigger] = BlockReactionChange.MadeAtSpawn,
            [SpecialBlockReaction.MapStationRight] = BlockReactionChange.MadeAtSpawn,
            [SpecialBlockReaction.MapStationLeft] = BlockReactionChange.MadeAtSpawn,
            [SpecialBlockReaction.EnergyStationRight] = BlockReactionChange.MadeAtSpawn,
            [SpecialBlockReaction.EnergyStationLeft] = BlockReactionChange.MadeAtSpawn,
            [SpecialBlockReaction.MissileStationRight] = BlockReactionChange.MadeAtSpawn,
            [SpecialBlockReaction.MissileStationLeft] = BlockReactionChange.MadeAtSpawn,
            [SpecialBlockReaction.SaveStationFloor] = BlockReactionChange.MadeAtSpawn,
            [SpecialBlockReaction.Solid] = BlockReactionChange.MadeAtSpawn,
            [SpecialBlockReaction.WreckedShipChozoHand] = BlockReactionChange.MadeAtSpawn,
            [SpecialBlockReaction.LowerNorfairChozoHand] = BlockReactionChange.MadeAtSpawn,
        }.Concat(SpeedBlockLookalikes.ShownAsSpeedBlocks.Select(reaction =>
            KeyValuePair.Create(reaction, SpeedBlockLookalikes.SpeedBlockReveal))).ToDictionary());

    /// <summary>
    /// The crumble reveal, the Speed Booster block's reveal, or the reaction that does nothing but hold its place for an update,
    /// for a special block with <paramref name="reaction"/> (<see cref="Changes"/>).
    /// </summary>
    /// <param name="reaction">The block's reaction.</param>
    /// <exception cref="ArgumentOutOfRangeException">The block has no special-block reaction.</exception>
    internal static BlockReactionChange ChangeFor(SpecialBlockReaction? reaction) =>
        Changes[reaction ?? throw new ArgumentOutOfRangeException(nameof(reaction), reaction, "The block has no special-block reaction for a bomb.")];
}
