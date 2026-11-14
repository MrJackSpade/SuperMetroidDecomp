namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// A room point an escape room object watches Samus pass: she has passed it once she is strictly right of and strictly
/// below it. The old Tourian escape shaft's fake wall and the acid of the escape room before it wake on such a point.
/// </summary>
/// <param name="Point">The whole-pixel room point Samus must be right of and below.</param>
internal readonly record struct SamusTriggerPoint(Position16<RoomPixels> Point)
{
    /// <summary>Whether <paramref name="samus"/> is strictly right of and strictly below this point.</summary>
    internal bool IsPassedBy(IRoomSamus samus) => IsPassedAt(Ensure.NotNull(samus).Position.Whole);

    /// <summary>Whether a Samus at whole-pixel <paramref name="position"/> is strictly right of and strictly below this point.</summary>
    internal bool IsPassedAt(Position16<RoomPixels> position) => Point.X < position.X && Point.Y < position.Y;
}
