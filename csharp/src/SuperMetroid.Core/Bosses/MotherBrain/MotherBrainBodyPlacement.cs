namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Where Mother Brain's body shows (#1356, #1617): the room pixel her body's parts' area lies on once her body is placed,
/// and whether her death flicker hides it this frame.
/// </summary>
/// <param name="BodyOrigin">The room pixel the top-left corner of her body's parts' area lies on, or null before her body is placed.</param>
/// <param name="Hidden">Whether her death flicker hides her body.</param>
public sealed record MotherBrainBodyPlacement(Position16<RoomPixels>? BodyOrigin, bool Hidden);
