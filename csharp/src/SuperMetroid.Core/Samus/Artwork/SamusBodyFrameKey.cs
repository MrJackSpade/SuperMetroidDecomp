namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>A pre-coloured Samus body frame: a spritemap and the definition records whose characters it shows.</summary>
/// <param name="Spritemap">The spritemap's composition.</param>
/// <param name="TopRecord">The top definition record it shows, or null when it reads no top character or none is selected.</param>
/// <param name="BottomRecord">The bottom definition record it shows, or null when it reads no bottom character or none is selected.</param>
public readonly record struct SamusBodyFrameKey(SamusSpritemapId Spritemap, int? TopRecord, int? BottomRecord)
{
    /// <summary>The frame's name on its sheet.</summary>
    public string Name => $"{Spritemap.Ordinal}.{(TopRecord is { } top ? top.ToString(System.Globalization.CultureInfo.InvariantCulture) : "-")}." +
        (BottomRecord is { } bottom ? bottom.ToString(System.Globalization.CultureInfo.InvariantCulture) : "-");
}
