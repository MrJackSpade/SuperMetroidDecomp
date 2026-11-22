using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.World;

/// <summary>
/// A freshly loaded room's objects as the door transition's step reaches them (#2553). The door's step runs on the scroll's
/// last update, before any contact of Samus's reaches the room: no item has been touched and no station is in use, so no
/// object of the room collects an item or acts as a station. The view offers no contact, and its step yields only the
/// projectiles its objects throw.
/// </summary>
/// <param name="objects">The room's objects, as their load placed them.</param>
internal readonly partial struct UntouchedRoomObjects(RoomObjectSystem objects)
{
    /// <summary>Steps the objects once, as the door's step does.</summary>
    /// <returns>The enemy projectiles the step's objects threw, in the order they threw them.</returns>
    internal IReadOnlyList<RoomObjectProjectileRequest> Step(RoomLevelData level, RoomObjectFrameInputs frame) =>
        objects.Step(level, frame).ThrownProjectiles;
}
