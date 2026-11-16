using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Areas.Maridia;

/// <summary>The Maridia elevatube's room object: the spawn a ride's door setup makes to start the ride's sound.</summary>
internal sealed class MaridiaElevatubeObjects
{
    /// <summary>The room's room objects, which the elevatube joins.</summary>
    private readonly RoomObjectPlaceList _places;

    /// <summary>Attaches elevatube spawning to the room's room-object pool.</summary>
    internal MaridiaElevatubeObjects(RoomObjectPlaceList places) => _places = Ensure.NotNull(places);

    /// <summary>
    /// Spawns the effectively invisible elevatube object at (1,0): it waits sixteen updates, plays the elevatube sound
    /// and leaves the room.
    /// </summary>
    /// <returns>False when the room is full; nothing is spawned then (<see cref="Compatibility.RoomObjectPlaces"/>).</returns>
    public bool TrySpawnMaridiaElevatube(RoomLevelData level)
    {
        Ensure.NotNull(level);
        Position16<RoomBlocks> block = level.GridBlock(MaridiaElevatubeDefinitions.RoomObjectBlock);

        if (_places.Spawn() is not { } place)
            return false;

        place.Block = block;
        place.Become(new MaridiaElevatubeObject());
        return true;
    }
}
