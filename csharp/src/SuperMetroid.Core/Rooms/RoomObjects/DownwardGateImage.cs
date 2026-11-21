namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// One image of a downward gate's course (<see cref="DownwardGateMovementDefinitions"/>): what it draws, how long it stays,
/// and the movement that starts as it draws.
/// </summary>
/// <param name="Draw">What the image draws at the gate's block.</param>
/// <param name="Updates">The updates the image stays before the course's next image, or before the course ends.</param>
/// <param name="Starts">The movement that starts as the image draws; none for an image the gate only shows.</param>
internal readonly partial record struct DownwardGateImage(RoomObjectDraw Draw, int Updates, DownwardGateMovement Starts = DownwardGateMovement.None);
