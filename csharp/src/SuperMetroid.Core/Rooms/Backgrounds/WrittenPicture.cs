namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// One picture something wrote over a room's background (#1466): its name in the room's written picture sheet and the
/// tilemap cell its top-left tile is written at, counted row by row across the background's two screens.
/// </summary>
/// <param name="Picture">The picture's name, as <c>WrittenPictureNames</c> spells it.</param>
/// <param name="Cell">The cell its top-left tile is written at.</param>
public readonly record struct WrittenPicture(string Picture, int Cell);
