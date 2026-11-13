namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// A room object that holds its own state as domain values and advances by its own named steps (#1554). The room
/// advances it once per room-object update, at its place in the room's update order (<see cref="RoomObjectSlotTable"/>);
/// it runs no sequence, frame hold, loop count or per-update step of the kind the room's remaining older objects
/// still step through (<see cref="RoomObjectSlot"/>).
/// </summary>
internal abstract class RoomObjectBehaviour
{
    /// <summary>Advances the object by one room-object update.</summary>
    /// <param name="place">The object's place in the update order, which holds its block and its identity.</param>
    /// <param name="context">The update: the room's objects, its level and what the update reads from the frame.</param>
    internal abstract void Update(RoomObjectSlot place, RoomObjectStepContext context);
}
