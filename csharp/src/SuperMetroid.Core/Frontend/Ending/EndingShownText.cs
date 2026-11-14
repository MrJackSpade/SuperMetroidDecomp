namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>A piece of post-credits text the screen shows (#1555): its installed picture and the screen pixel its upper-left corner sits on.</summary>
/// <param name="Picture">The picture's name in the post-credits picture sheet, as <see cref="Artwork.EndingPictureNames"/> spells it.</param>
/// <param name="Corner">The screen pixel the picture's upper-left corner sits on before the text is moved down.</param>
internal readonly record struct EndingShownText(string Picture, Position16<ScreenPixels> Corner);
