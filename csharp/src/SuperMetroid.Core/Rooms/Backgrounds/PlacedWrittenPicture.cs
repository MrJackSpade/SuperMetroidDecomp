using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// A picture written over a room's layer as a frame shows it (#1466, #1514): its pre-coloured frame and the layer pixel
/// its top-left corner lies on.
/// </summary>
/// <param name="Picture">The picture's frame for the values the room's appearance inputs take now.</param>
/// <param name="Corner">The layer pixel, from the layer's top-left, the picture's corner lies on.</param>
public readonly record struct PlacedWrittenPicture(SpriteSheetFrame Picture, Position16<TilePlanePixels> Corner);
