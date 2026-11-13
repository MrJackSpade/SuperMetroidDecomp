using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// The enemies of Ridley's two rooms that his encounters reach by name. Both rooms list his body first, which is the
/// order the enemy pass runs them in; Ceres Ridley's room lists its door second, which his scene draws and his escape
/// reads. He holds both by reference, each taken once from its place in his room's listing the first time an encounter
/// reaches it, once the room's enemies are in place. A reference is the enemy's place, which outlives its occupant: his
/// scene draws the Baby only while his place holds Ceres Ridley, and every other read reaches him while he lives.
/// </summary>
[RoomLifetime]
internal sealed class RidleyParts
{
    /// <summary>His body's place in his room's enemy list.</summary>
    private const int BodyPlace = 0;
    /// <summary>The door's place in Ceres Ridley's room, after his body.</summary>
    private const int CeresRoomDoorPlace = 1;

    /// <summary>The room's enemies, whose first is Ridley's body.</summary>
    private readonly EnemySlotTable _enemies;

    /// <summary>Reaches Ridley's parts among <paramref name="enemies"/>.</summary>
    internal RidleyParts(EnemySlotTable enemies)
    {
        _enemies = Ensure.NotNull(enemies);
    }

    /// <summary>His body, taken from the room's listing the first time it is reached; null until then.</summary>
    private RoomEnemySlot? _body;

    /// <summary>The door of Ceres Ridley's room, taken from the room's listing the first time it is reached; null until then.</summary>
    private RoomEnemySlot? _ceresRoomDoor;

    /// <summary>His body, which runs first in both his rooms.</summary>
    internal RoomEnemySlot Body => _body ??= _enemies.Slots[BodyPlace];

    /// <summary>The door of Ceres Ridley's room, whose drawing his scene takes over while the door hands it to him.</summary>
    internal RoomEnemySlot CeresRoomDoor => _ceresRoomDoor ??= _enemies.Slots[CeresRoomDoorPlace];

    /// <summary>Whether <paramref name="slot"/> is his body.</summary>
    internal bool IsBody(RoomEnemySlot slot) => ReferenceEquals(slot, Body);

    /// <summary>Rejects Ridley's initialization in any place but his body's.</summary>
    /// <param name="slot">The enemy initializing as Ridley.</param>
    /// <param name="encounter">The encounter's name for the failure message.</param>
    /// <exception cref="InvalidDataException">His room lists him in another place.</exception>
    internal void RequireBody(RoomEnemySlot slot, string encounter)
    {
        if (!IsBody(Ensure.NotNull(slot)))
            throw new InvalidDataException($"{encounter} is listed in his room as enemy {slot.SlotIndex}, not as its first enemy.");
    }
}
