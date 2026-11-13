namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// Where Draygon's body shows (#1354, #1617): the simulation body position and the room pixel his body's parts' area lies
/// on, as the body draw derives it after enemies draw.
/// </summary>
/// <param name="BodyPosition">Draygon's body position.</param>
/// <param name="BodyOrigin">The room pixel the top-left corner of his body's parts' area lies on.</param>
/// <param name="Deleted">Whether the body has been deleted, which hides it.</param>
public sealed record DraygonBodyPlacement(Position16<RoomPixels> BodyPosition, Position16<RoomPixels> BodyOrigin, bool Deleted);
