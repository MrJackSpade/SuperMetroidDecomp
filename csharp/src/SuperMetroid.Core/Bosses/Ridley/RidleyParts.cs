using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// The enemies of Ridley's two rooms that his encounters reach by name (#1877): his body, which attaches itself as Ceres
/// or Lower Norfair Ridley initializes, and the door of Ceres Ridley's room, which attaches itself as the door of
/// Ridley's room initializes; his scene draws it and his escape reads it. Each is held by reference, which is the enemy's
/// place and outlives its occupant: his scene draws the Baby only while his place holds Ceres Ridley, and every other
/// read reaches him while he lives.
/// </summary>
[RoomLifetime]
internal sealed class RidleyParts
{
    /// <summary>His body once it has initialized in this room; null in a room he has not initialized in.</summary>
    internal RoomEnemySlot? AttachedBody { get; private set; }

    /// <summary>His body, which his encounters reach once it has initialized.</summary>
    /// <exception cref="InvalidOperationException">His body has not initialized in this room.</exception>
    internal RoomEnemySlot Body => AttachedBody
        ?? throw new InvalidOperationException("Ridley's body has not initialized in this room.");

    /// <summary>
    /// The door of Ceres Ridley's room once it has initialized, whose drawing his scene takes over while the door hands it
    /// to him; null in any other room.
    /// </summary>
    internal RoomEnemySlot? CeresRoomDoor { get; private set; }

    /// <summary>Whether his place holds Ceres Ridley: false in Lower Norfair and in a room he has not initialized in.</summary>
    internal bool HoldsCeresRidley => AttachedBody?.DefinitionId == EnemyDefinitionId.RidleyCeres;

    /// <summary>Whether <paramref name="slot"/> is his body.</summary>
    internal bool IsBody(RoomEnemySlot slot) => AttachedBody is { } body && ReferenceEquals(slot, body);

    /// <summary>Attaches <paramref name="slot"/> as his body as it initializes as Ridley.</summary>
    /// <param name="slot">The enemy initializing as Ridley.</param>
    /// <param name="encounter">The encounter's name for the failure message.</param>
    /// <exception cref="InvalidDataException">Another enemy of the room has already initialized as his body.</exception>
    internal void AttachBody(RoomEnemySlot slot, string encounter)
    {
        Ensure.NotNull(slot);
        if (AttachedBody is { } body && !ReferenceEquals(body, slot))
            throw new InvalidDataException($"{encounter} initializes as enemy {slot.SlotIndex}, but enemy {body.SlotIndex} is already his body.");
        AttachedBody = slot;
    }

    /// <summary>Attaches <paramref name="door"/> as the door of Ceres Ridley's room as it initializes.</summary>
    /// <exception cref="InvalidDataException">Another door of the room has already initialized as the door of Ridley's room.</exception>
    internal void AttachCeresRoomDoor(RoomEnemySlot door)
    {
        Ensure.NotNull(door);
        if (CeresRoomDoor is { } attached && !ReferenceEquals(attached, door))
            throw new InvalidDataException($"Ceres door {door.SlotIndex} initializes as the door of Ridley's room, but door {attached.SlotIndex} already is.");
        CeresRoomDoor = door;
    }
}
