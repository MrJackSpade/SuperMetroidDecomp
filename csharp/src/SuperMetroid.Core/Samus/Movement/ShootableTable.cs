using System.Collections.Frozen;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// The game's one shootable reaction table, which a bomb and the grapple beam read in shootable air and shootable blocks
/// alike (#2553): each reaction sets off its own room object, the gate triggers, blue door caps,
/// resident and item triggers and the escape animals' wall theirs, and every other reaction its shot block.
/// </summary>
internal static partial class ShootableTable
{
    /// <summary>
    /// The reactions that set off a room object of their own, neither a gate trigger, a blue door cap nor a shot block: the
    /// resident and item triggers and the escape animals' wall. A block no object occupies hears nothing
    /// (<see cref="Projectiles.ProjectileTriggerObjects.NotifyResidentProjectileHit"/>). Declared before <see cref="Entries"/>, which
    /// reads it as it is built.
    /// </summary>
    private static readonly FrozenDictionary<ShootableReaction, Action<RoomTerrain, RoomCollisionBlock, BlockStrike, bool>> ObjectTriggers =
        new Dictionary<ShootableReaction, Action<RoomTerrain, RoomCollisionBlock, BlockStrike, bool>>
        {
            [ShootableReaction.ResidentObjectTrigger] = static (terrain, block, strike, _) =>
                terrain.RoomObjects.ProjectileTriggers.NotifyResidentProjectileHit(block.At, strike),
            [ShootableReaction.CollectibleTrigger] = static (terrain, block, _, _) =>
                terrain.RoomObjects.CollectibleObjects.NotifyCollectibleProjectileHit(block.At),
            [ShootableReaction.AnimalEscape] = static (terrain, block, strike, _) =>
                _ = terrain.RoomObjects.BlockReactions.TrySpawnAnimalEscape(terrain.Level, block.At, strike),
        }.ToFrozenDictionary();

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
    /// <exception cref="KeyNotFoundException">The reaction means none of the table's room objects.</exception>
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
        return ObjectTriggers[reaction];
    }
}
