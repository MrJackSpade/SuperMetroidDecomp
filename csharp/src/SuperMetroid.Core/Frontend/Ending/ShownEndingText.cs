namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>One piece of post-credits text a text page shows (#1467): its installed picture's name and the cell its upper-left corner sits in.</summary>
/// <param name="Picture">The name of the picture in the post-credits pictures sheet (<see cref="Artwork.EndingPictureNames"/>).</param>
/// <param name="Cell">The page cell the picture's upper-left corner sits in.</param>
internal readonly record struct ShownEndingText(string Picture, Position16<TilemapCells> Cell);
