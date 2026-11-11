namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// One picture something wrote over a room's layer (#1466, #1467, #1514, #1601): its name in the sheet that holds it and
/// the pixel of the layer its top-left corner shows at.
/// </summary>
/// <param name="Picture">The picture's name in its sheet, as the written-picture or ending-picture names spell it.</param>
/// <param name="Corner">The pixel, from the layer's top-left, the picture's top-left corner shows at.</param>
public readonly record struct WrittenPicture(string Picture, Position16<PlanePixels> Corner);
