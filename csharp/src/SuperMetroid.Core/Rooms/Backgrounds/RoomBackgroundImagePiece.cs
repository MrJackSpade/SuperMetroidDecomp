namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// A rectangle of a room's background image placed on the background (#1555): where it lies on the image's area, the image
/// pixel its top-left corner shows, and its size.
/// </summary>
/// <param name="Corner">The background pixel, from the area's top-left, the rectangle's corner lies on.</param>
/// <param name="Source">The image pixel the rectangle's corner shows.</param>
/// <param name="Size">The rectangle's width and height.</param>
public readonly record struct RoomBackgroundImagePiece(Position16<BackgroundPixels> Corner, Position16<ImagePixels> Source, Extent16<BackgroundPixels> Size);
