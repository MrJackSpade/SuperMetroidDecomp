using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Maridia's glass tube: its setup and what its break does to Samus, the events, the water, the earthquake and the debris.</summary>
[RoomLifetime]
internal sealed class NoobTubeObjects
{
    /// <summary>Samus as the room reads and changes her.</summary>
    private readonly IRoomSamusAccess? _samus;
    /// <summary>The game's event flags.</summary>
    private readonly IGlobalEventFlags? _events;
    /// <summary>The room's effect overlay FX.</summary>
    private readonly IRoomWaterPhysics? _waterPhysics;
    /// <summary>The room's earthquake.</summary>
    private readonly IRoomEarthquakeWriter? _earthquake;
    /// <summary>The enemy effects room objects trigger.</summary>
    private readonly IRoomObjectEnemyEffects? _enemies;

    /// <summary>Shares the room services the tube's break reaches.</summary>
    internal NoobTubeObjects(IRoomSamusAccess? samus, IGlobalEventFlags? events, IRoomWaterPhysics? waterPhysics, IRoomEarthquakeWriter? earthquake, IRoomObjectEnemyEffects? enemies)
    {
        _samus = samus;
        _events = events;
        _waterPhysics = waterPhysics;
        _earthquake = earthquake;
        _enemies = enemies;
    }

    /// <summary>
    /// Runs the tube's setup: its origin block becomes a solid block whose reaction is the tube's, keeping what it shows
    /// (#1580).
    /// </summary>
    internal void SetupNoobTubeSlot(RoomLevelData level, RoomObjectSlot slot)
    {
        Ensure.NotNull(level);
        Ensure.NotNull(slot);
        slot.Become(new NoobTubeObject(this));
        RoomTerrainBlock origin = level.GetCollisionBlock(slot.Block).Terrain;
        level.SetTerrain(
            slot.Block,
            origin.WithCollision(NoobTubeObjectDefinitions.OriginCollision));
        level.SetBehavior(
            slot.Block,
            RoomBlockBehavior.Reacting(ShootableReaction.ResidentObjectTrigger));
    }

    /// <summary>Whether the tube was broken on an earlier visit (its event is set).</summary>
    internal bool IsBroken() => RequireNoobTubeEvents().Has(NoobTubeObjectDefinitions.BrokenEvent);

    /// <summary>Locks Samus in place for the break sequence.</summary>
    internal void LockSamus() => RequireNoobTubeSamus().Control.LockStationary();

    /// <summary>Releases Samus once the break sequence ends.</summary>
    internal void UnlockSamus() => RequireNoobTubeSamus().Control.Unlock();

    /// <summary>Spawns the crack projectile at the tube.</summary>
    internal void SpawnCrack(RoomObjectSlot slot) => SpawnNoobTubeProjectile(slot, NoobTubeProjectileDefinition.Crack, piece: null);

    /// <summary>Spawns the ten glass shards and then the six released-air bubbles, each naming its place in the burst.</summary>
    internal void SpawnShardsAndBubbles(RoomObjectSlot slot)
    {
        for (int shard = 0; shard < NoobTubeObjectDefinitions.ShardCount; shard++)
            SpawnNoobTubeProjectile(slot, NoobTubeProjectileDefinition.Shard, shard);
        for (int bubble = 0; bubble < NoobTubeObjectDefinitions.BubbleCount; bubble++)
            SpawnNoobTubeProjectile(slot, NoobTubeProjectileDefinition.ReleasedAirBubble, bubble);
    }

    /// <summary>Starts the break earthquake.</summary>
    internal void TriggerEarthquake()
    {
        (_earthquake ?? throw new InvalidOperationException(
            "N00b tube has no earthquake writer.")).Start(NoobTubeObjectDefinitions.Earthquake, NoobTubeObjectDefinitions.EarthquakeFrames);
    }

    /// <summary>Sets the tube's broken event.</summary>
    internal void SetBrokenEvent() =>
        (_events ?? throw new InvalidOperationException(
            "N00b tube has no event writer.")).Set(NoobTubeObjectDefinitions.BrokenEvent);

    /// <summary>Enables the room's water physics, Samus's included.</summary>
    internal void EnableWaterPhysics() =>
        (_waterPhysics ?? throw new InvalidOperationException(
            "N00b tube has no room water physics for its water-physics write.")).EnableWaterPhysics();

    /// <summary>Requires the room event owner before testing the broken-tube event.</summary>
    private IGlobalEventFlags RequireNoobTubeEvents() =>
        _events ?? throw new InvalidOperationException("N00b tube has no event reader.");

    /// <summary>Requires a live Samus actor for tube script locks or water-physics publication.</summary>
    private IRoomSamus RequireNoobTubeSamus() =>
        _samus?.Current ?? throw new InvalidOperationException(
            "N00b tube has no active Samus actor.");

    /// <summary>Routes a crack, shard, or bubble spawn to the enemy system at the tube's block.</summary>
    private void SpawnNoobTubeProjectile(RoomObjectSlot slot, NoobTubeProjectileDefinition definition, int? piece) =>
        (_enemies ?? throw new InvalidOperationException(
            "N00b tube has no enemy-projectile spawner.")).SpawnNoobTubeProjectile(
            new NoobTubeProjectileRequest(definition, piece, slot.Block));
}
