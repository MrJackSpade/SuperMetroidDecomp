namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// One picture something wrote over a room's layer or a page of text (#1466, #1467, #1514): its name in the sheet that
/// holds it and where it shows: the eight-pixel cell of the layer or page its top-left corner lies on.
/// </summary>
/// <param name="Picture">The picture's name in its sheet, as the written-picture or ending-picture names spell it.</param>
/// <param name="Cell">The cell, its column and row from the layer's or page's top-left, the picture's corner lies on.</param>
public readonly record struct WrittenPicture(string Picture, Position16<TilemapCells> Cell);
