namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>A pre-coloured Samus body frame: a spritemap and the half pictures whose characters it shows.</summary>
/// <param name="Spritemap">The spritemap's composition.</param>
/// <param name="TopPicture">The top half picture it shows, or null when it reads no top character or none is selected.</param>
/// <param name="BottomPicture">The bottom half picture it shows, or null when it reads no bottom character or none is selected.</param>
public readonly record struct SamusBodyFrameKey(SamusSpritemapId Spritemap, SamusBodyPicture? TopPicture, SamusBodyPicture? BottomPicture)
{
    /// <summary>The frame's name on its sheet.</summary>
    public string Name => $"{Spritemap.Ordinal}.{(TopPicture is { } top ? top.Ordinal.ToString(System.Globalization.CultureInfo.InvariantCulture) : "-")}." +
        (BottomPicture is { } bottom ? bottom.Ordinal.ToString(System.Globalization.CultureInfo.InvariantCulture) : "-");
}
