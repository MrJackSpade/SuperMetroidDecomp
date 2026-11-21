using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// The game's one shootable reaction table, which a bomb and the grapple beam read in shootable air and shootable blocks
/// alike ($94:9E55, $94:9E73, #2553): each reaction sets off its own room object, the gate triggers, blue door caps,
/// resident and item triggers and the escape animals' wall theirs, and every other reaction its shot block.
/// </summary>
internal static class ShootableTable
{
    /// <summary>What each reaction sets off for a strike at its block, given whether the block is solid.</summary>
    private static readonly IdentityTable<ShootableReaction, Action<RoomTerrain, RoomCollisionBlock, BlockStrike, bool>> Entries =
        new(EntryOf);

    /// <summary>Sets off what the table names for <paramref name="block"/>'s reaction.</summary>
    /// <param name="terrain">The room's terrain, whose objects the reaction reaches.</param>
    /// <param name="block">The struck shootable block or shootable air.</param>
    /// <param name="strike">The bomb or grapple touch that struck it.</param>
    /// <param name="solidBlock">Whether the block is a shootable block rather than shootable air.</param>
    internal static void SetOff(RoomTerrain terrain, RoomCollisionBlock block, BlockStrike strike, bool solidBlock) =>
        Entries[block.Behavior.Shootable](terrain, block, strike, solidBlock);

    /// <summary>The entry of <paramref name="reaction"/>, read from its meanings.</summary>
    /// <exception cref="InvalidOperationException">The reaction means none of the table's room objects.</exception>
    private static Action<RoomTerrain, RoomCollisionBlock, BlockStrike, bool> EntryOf(ShootableReaction reaction)
    {
        if (BlockReactionDefinitions.DownwardGateTriggers.TryGetMeaning(reaction, out _))
            return static (terrain, block, strike, _) =>
                terrain.RoomObjects.Gates.SpawnDownwardGateTrigger(terrain.Level, block.At, block.Behavior, strike);
        if (BlockReactionDefinitions.BlueDoorCaps.TryGetMeaning(reaction, out _))
            return static (terrain, block, strike, _) =>
                terrain.RoomObjects.DoorObjects.SpawnBlueDoorOpening(terrain.Level, block.At, block.Behavior, strike);
        if (ShotBlockReactions.ShotBlocks.TryGetValue(reaction, out ShotBlock? shot))
            return (terrain, block, strike, solidBlock) =>
                _ = terrain.RoomObjects.BlockReactions.TrySpawnProjectileShotBlock(terrain.Level, block.At, shot, strike, solidBlock);
        return reaction switch
        {
            // A block no object occupies hears nothing (ProjectileTriggerObjects.TryNotifyResidentProjectileHit).
            ShootableReaction.ResidentObjectTrigger => static (terrain, block, strike, _) =>
                _ = terrain.RoomObjects.ProjectileTriggers.TryNotifyResidentProjectileHit(block.At, strike),
            ShootableReaction.CollectibleTrigger => static (terrain, block, _, _) =>
                terrain.RoomObjects.CollectibleObjects.NotifyCollectibleProjectileHit(block.At),
            ShootableReaction.AnimalEscape => static (terrain, block, strike, _) =>
                _ = terrain.RoomObjects.BlockReactions.TrySpawnAnimalEscape(terrain.Level, block.At, strike),
            _ => throw new InvalidOperationException($"The shootable reaction {reaction} names no room object of the table."),
        };
    }
}
