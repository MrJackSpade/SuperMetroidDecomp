using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Bosses.Botwoon;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses;

/// <summary>
/// The boss-room terrain spawners: Botwoon's wall, the Spore Spawn ceiling, Phantoon's door, and the Kraid, Crocomire,
/// Shitroid wall and Mother Brain room mutations. Each spawner accepts only its own family's mutations, and all of them contend for the
/// room's shared room-object places with ordinary room objects.
/// </summary>
internal sealed class BossRoomMutationObjects
{
    /// <summary>Gives every family spawner the room's room-object places.</summary>
    internal BossRoomMutationObjects(RoomObjectPlaceList places)
    {
        BotwoonWall = new BotwoonWallObjects(places);
        SporeSpawnCeiling = new SporeSpawnCeilingObjects(places);
        KraidRoom = new KraidRoomObjects(places);
        CrocomireArena = new CrocomireArenaObjects(places);
        ShitroidWall = new ShitroidWallObjects(places);
        MotherBrainRoom = new MotherBrainRoomObjects(places);
        PhantoonDoor = new PhantoonDoorObjects(places);
    }

    /// <summary>Spawns Botwoon's wall clear or crumble.</summary>
    internal BotwoonWallObjects BotwoonWall { get; }

    /// <summary>Spawns Spore Spawn's ceiling crumble or clear.</summary>
    internal SporeSpawnCeilingObjects SporeSpawnCeiling { get; }

    /// <summary>Spawns Kraid's ceiling, platform and spike mutations.</summary>
    internal KraidRoomObjects KraidRoom { get; }

    /// <summary>Spawns Crocomire's bridge and invisible-wall mutations.</summary>
    internal CrocomireArenaObjects CrocomireArena { get; }

    /// <summary>Spawns the Shitroid's invisible-wall clear or create.</summary>
    internal ShitroidWallObjects ShitroidWall { get; }

    /// <summary>Spawns Mother Brain's wall, escape-door, background-row and tube mutations.</summary>
    internal MotherBrainRoomObjects MotherBrainRoom { get; }

    /// <summary>Spawns Phantoon's fight door or its restored door.</summary>
    internal PhantoonDoorObjects PhantoonDoor { get; }
}
