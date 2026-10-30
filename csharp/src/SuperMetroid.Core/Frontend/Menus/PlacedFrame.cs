namespace SuperMetroid.Core.Frontend.Menus;

/// <summary>A menu picture placed on a page (#1467): the frame it shows, and the page cell its frame's origin lands on.</summary>
/// <param name="Frame">The picture's frame name in the menus' sheet.</param>
/// <param name="Cell">The page cell the picture's origin lands on.</param>
internal readonly record struct PlacedFrame(string Frame, Position16<TilemapCells> Cell);
