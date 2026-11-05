namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// One picture something wrote over a room's layer (#1466, #1514): its name in the room's written picture sheet and where
/// it shows: the eight-pixel cell of the layer its top-left corner lies on.
/// </summary>
/// <param name="Picture">The picture's name, as <c>WrittenPictureNames</c> spells it.</param>
/// <param name="Cell">The cell of the layer, its column and row from the layer's top-left, the picture's corner lies on.</param>
public readonly record struct WrittenPicture(string Picture, Position16<TilemapCells> Cell);
