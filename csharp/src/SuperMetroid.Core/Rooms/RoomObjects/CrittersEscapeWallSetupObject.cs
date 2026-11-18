namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The room object the Bomb Torizo room's escape setup spawns to build the animals' rescue wall (#2495). Its setup has
/// turned the wall's blocks shootable when it spawns; it holds its place until its first update and leaves the room then,
/// doing nothing else.
/// </summary>
internal sealed class CrittersEscapeWallSetupObject : RoomObjectBehaviour
{
    /// <inheritdoc/>
    internal override void Update(RoomObjectPlace place, RoomObjectStepContext context) =>
        context.RoomObjects.DeleteObject(Ensure.NotNull(place));
}
