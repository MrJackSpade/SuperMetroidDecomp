using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.World;

/// <summary>
/// The room objects one room's enemies reach (#2554): the terrain enemies break, the Bomb Torizo's hand and Mother Brain's
/// glass, with the room-object places and sounds they share with every room object. They are built before the room's
/// enemies and its room-object system both, so the enemies are built before the room objects that reach them.
/// </summary>
[RoomLifetime]
internal sealed partial class RoomObjectsReachedByEnemies
{
    /// <summary>Builds the room's shared room-object places and sounds and the room objects its enemies reach.</summary>
    /// <param name="samus">Samus as room objects see her.</param>
    /// <param name="bosses">Which of the area's bosses are defeated.</param>
    /// <param name="events">The story events that have happened.</param>
    /// <param name="audioRequests">The update's audio requests, which the room objects' sounds join.</param>
    internal RoomObjectsReachedByEnemies(IRoomSamusAccess samus, IDefeatedBossesReader bosses,
        IStoryEvents events, GameplayAudioRequests audioRequests)
    {
        Places = new RoomObjectPlaceList();
        Sounds = new RoomObjectSoundQueue(audioRequests);
        BreakableTerrain = new EnemyBreakableTerrainObjects(Places);
        BombTorizoHand = new BombTorizoHandObjects(samus, Sounds);
        MotherBrainGlass = new MotherBrainGlassObjects(bosses, events, Sounds);
    }

    /// <summary>The room's room-object places, which every room object takes.</summary>
    internal RoomObjectPlaceList Places { get; }

    /// <summary>The room objects' sounds.</summary>
    internal RoomObjectSoundQueue Sounds { get; }

    /// <summary>The terrain the room's enemies break.</summary>
    internal EnemyBreakableTerrainObjects BreakableTerrain { get; }

    /// <summary>The Bomb Torizo's hand, whose remaining the Torizo reads.</summary>
    internal BombTorizoHandObjects BombTorizoHand { get; }

    /// <summary>Mother Brain's glass, which her head's shots crack.</summary>
    internal MotherBrainGlassObjects MotherBrainGlass { get; }
}
