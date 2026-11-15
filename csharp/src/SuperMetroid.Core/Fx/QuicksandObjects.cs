using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Fx;

/// <summary>Quicksand reaction room objects and the allocation-full check their spawns depend on.</summary>
[RoomLifetime]
internal sealed class QuicksandObjects
{
    /// <summary>The room's forty room-object places; a full room keeps a quicksand reaction from spawning.</summary>
    private readonly RoomObjectPlaceList _places;

    /// <summary>Binds quicksand reactions to the room's room-object places.</summary>
    internal QuicksandObjects(RoomObjectPlaceList places)
    {
        _places = places;
    }

    /// <summary>True when every room-object place is held.</summary>
    public bool IsAllocationFull => _places.IsFull;

    /// <summary>
    /// Allocates one transient sand-reaction room object at the sand <paramref name="block"/> and leaves its setup effects
    /// to the collision owner, as spawning a room object at the current block does.
    /// </summary>
    /// <remarks>
    /// Every sand reaction leaves the room on its first room-object update. X-Ray and G-Mode stop those updates, so
    /// repeated contact fills the room; subsequent contacts run no setup and sand falls back to air. The reactions' places
    /// are therefore observable gameplay state even though they never draw.
    /// </remarks>
    /// <param name="block">The sand block Samus is inside.</param>
    /// <param name="reaction">One of the four inside sand reactions.</param>
    /// <returns>False only when the room-object pool is full.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="reaction"/> is not a sand reaction.</exception>
    public bool TrySpawnQuicksandReaction(Position16<RoomBlocks> block, SpecialAirInsideReaction reaction) =>
        QuicksandDefinitions.ObjectPlacingInsideReactions.Contains(reaction)
            ? TrySpawn(block)
            : throw new ArgumentOutOfRangeException(nameof(reaction), reaction, "Only the four inside sand reactions place a sand reaction.");

    /// <summary>
    /// Allocates one transient sand-reaction room object at the sand <paramref name="block"/> Samus's movement reached, as
    /// <see cref="TrySpawnQuicksandReaction(Position16{RoomBlocks}, SpecialAirInsideReaction)"/> does for an inside reaction.
    /// </summary>
    /// <param name="block">The sand block Samus's movement reached.</param>
    /// <param name="reaction">One of the four collision sand reactions.</param>
    /// <returns>False only when the room-object pool is full.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="reaction"/> is not a sand reaction.</exception>
    public bool TrySpawnQuicksandReaction(Position16<RoomBlocks> block, SpecialAirCollisionReaction reaction) =>
        QuicksandDefinitions.ObjectPlacingCollisionReactions.Contains(reaction)
            ? TrySpawn(block)
            : throw new ArgumentOutOfRangeException(nameof(reaction), reaction, "Only the four collision sand reactions place a sand reaction.");

    /// <summary>Places a sand reaction object at <paramref name="block"/>; it leaves on its first update.</summary>
    private bool TrySpawn(Position16<RoomBlocks> block)
    {
        if (_places.Spawn() is not { } place)
            return false;

        place.Block = block;
        place.Become(new TerrainChangeObject(TerrainChange.MadeAtSpawn));
        return true;
    }
}
