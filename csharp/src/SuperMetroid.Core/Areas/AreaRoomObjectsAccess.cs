using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Areas;

/// <summary>Reaches the area PLM families from a step context's room PLM owner.</summary>
internal static class AreaRoomObjectsAccess
{
    /// <summary>The area PLM families <paramref name="roomObjects"/> runs; every room's PLM owner runs them.</summary>
    /// <exception cref="InvalidOperationException">The owner runs no area PLM families.</exception>
    internal static IAreaRoomObjects Areas(this IRoomObjects roomObjects) =>
        roomObjects as IAreaRoomObjects ?? throw new InvalidOperationException($"{roomObjects.GetType().Name} runs no area PLM families.");
}
