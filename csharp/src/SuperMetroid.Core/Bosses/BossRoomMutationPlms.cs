using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Bosses.Botwoon;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses;

/// <summary>
/// The boss-room terrain spawners: Botwoon's wall, the Spore Spawn ceiling, and the Kraid, Crocomire, Shitroid wall and
/// Mother Brain room mutations. Each spawner accepts only its own family's mutations, and all of them contend for the
/// room's shared PLM slots with ordinary room PLMs.
/// </summary>
internal sealed class BossRoomMutationPlms
{
    /// <summary>Gives every family spawner the room's PLM slots.</summary>
    internal BossRoomMutationPlms(PlmSlotTable slotTable)
    {
        BotwoonWall = new BotwoonWallPlms(slotTable);
        SporeSpawnCeiling = new SporeSpawnCeilingPlms(slotTable);
        KraidRoom = new KraidRoomPlms(slotTable);
        CrocomireArena = new CrocomireArenaPlms(slotTable);
        ShitroidWall = new ShitroidWallPlms(slotTable);
        MotherBrainRoom = new MotherBrainRoomPlms(slotTable);
    }

    /// <summary>Spawns Botwoon's wall clear or crumble.</summary>
    internal BotwoonWallPlms BotwoonWall { get; }

    /// <summary>Spawns Spore Spawn's ceiling crumble or clear.</summary>
    internal SporeSpawnCeilingPlms SporeSpawnCeiling { get; }

    /// <summary>Spawns Kraid's ceiling, platform and spike mutations.</summary>
    internal KraidRoomPlms KraidRoom { get; }

    /// <summary>Spawns Crocomire's bridge and invisible-wall mutations.</summary>
    internal CrocomireArenaPlms CrocomireArena { get; }

    /// <summary>Spawns the Shitroid's invisible-wall clear or create.</summary>
    internal ShitroidWallPlms ShitroidWall { get; }

    /// <summary>Spawns Mother Brain's wall, escape-door, background-row and tube mutations.</summary>
    internal MotherBrainRoomPlms MotherBrainRoom { get; }
}
