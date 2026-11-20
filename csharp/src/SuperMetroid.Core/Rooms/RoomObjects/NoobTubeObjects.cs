using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Maridia's glass tube: its setup and what its break does to Samus, the events, the water, the earthquake and the debris.</summary>
[RoomLifetime]
internal sealed class NoobTubeObjects
{
    /// <summary>Samus as the room reads and changes her.</summary>
    private readonly IRoomSamusAccess _samus;
    /// <summary>The game's event flags.</summary>
    private readonly IStoryEvents _events;
    /// <summary>The room's water physics, which the tube's break enables.</summary>
    private readonly IRoomWaterPhysics _waterPhysics;
    /// <summary>The room's earthquake.</summary>
    private readonly IRoomEarthquakeWriter _earthquake;
    /// <summary>The enemy effects room objects trigger.</summary>
    private readonly IRoomObjectEnemyEffects _enemies;

    /// <summary>Shares the room services the tube's break reaches.</summary>
    internal NoobTubeObjects(IRoomSamusAccess samus, IStoryEvents events, IRoomWaterPhysics waterPhysics, IRoomEarthquakeWriter earthquake, IRoomObjectEnemyEffects enemies)
    {
        _samus = samus;
        _events = events;
        _waterPhysics = waterPhysics;
        _earthquake = earthquake;
        _enemies = enemies;
    }

    /// <summary>
    /// Runs the tube's setup: its origin block becomes a solid block whose reaction is the tube's, keeping its picture
    /// and what it shows (#1580, #2502).
    /// </summary>
    internal void SetupNoobTube(RoomLevelData level, RoomObjectPlace place)
    {
        Ensure.NotNull(level);
        Ensure.NotNull(place);
        place.Become(new NoobTubeObject(this));
        level.SetCollision(place.Block, NoobTubeObjectDefinitions.OriginCollision);
        level.SetBehavior(
            place.Block,
            RoomBlockBehavior.Reacting(ShootableReaction.ResidentObjectTrigger));
    }

    /// <summary>Whether the tube was broken on an earlier visit (its event is set).</summary>
    internal bool IsBroken() => _events.Has(NoobTubeObjectDefinitions.BrokenEvent);

    /// <summary>Locks Samus in place for the break sequence.</summary>
    internal void LockSamus() => _samus.Current.Control.LockStationary();

    /// <summary>Releases Samus once the break sequence ends.</summary>
    internal void UnlockSamus() => _samus.Current.Control.Unlock();

    /// <summary>Spawns the crack projectile at the tube.</summary>
    internal void SpawnCrack(RoomObjectPlace place) => SpawnNoobTubeProjectile(place, NoobTubeDebrisDefinitions.Crack);

    /// <summary>Spawns the ten glass shards and then the six released-air bubbles, in the order they burst.</summary>
    internal void SpawnShardsAndBubbles(RoomObjectPlace place)
    {
        foreach (NoobTubePiece shard in NoobTubeDebrisDefinitions.Shards)
            SpawnNoobTubeProjectile(place, shard);
        foreach (NoobTubePiece bubble in NoobTubeDebrisDefinitions.Bubbles)
            SpawnNoobTubeProjectile(place, bubble);
    }

    /// <summary>Starts the break earthquake.</summary>
    internal void TriggerEarthquake() =>
        _earthquake.Start(NoobTubeObjectDefinitions.Earthquake, NoobTubeObjectDefinitions.EarthquakeFrames);

    /// <summary>Sets the tube's broken event.</summary>
    internal void SetBrokenEvent() => _events.Set(NoobTubeObjectDefinitions.BrokenEvent);

    /// <summary>Enables the room's water physics, Samus's included.</summary>
    internal void EnableWaterPhysics() => _waterPhysics.EnableWaterPhysics();

    /// <summary>Routes a crack, shard, or bubble spawn to the enemy system at the tube's block.</summary>
    private void SpawnNoobTubeProjectile(RoomObjectPlace place, NoobTubePiece piece) =>
        _enemies.SpawnNoobTubeProjectile(new NoobTubeProjectileRequest(piece, place.Block));
}
