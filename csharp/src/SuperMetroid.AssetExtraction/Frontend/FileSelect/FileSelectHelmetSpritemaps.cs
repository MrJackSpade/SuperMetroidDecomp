using SuperMetroid.Core.Frontend.FileSelect;

namespace SuperMetroid.AssetExtraction.Frontend.FileSelect;

/// <summary>Selects the selected slot's helmet spritemap for each animation frame, which the import reads (#1502).</summary>
public static class FileSelectHelmetSpritemaps
{
    /// <summary>The spritemap ID of the helmet picture <paramref name="frame"/>, by its place in the turn (#2359).</summary>
    /// <remarks>The turn's pictures select consecutive spritemaps $2C..33; extraction reads each picture's spritemap with it.</remarks>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="frame"/> is not a picture of the turn.</exception>
    internal static int SpritemapId(FileSelectHelmetFrame frame)
    {
        int place = FileSelectHelmetAnimation.Turn.ToList().IndexOf(frame);
        return place >= 0 ? 44 + place : throw new ArgumentOutOfRangeException(nameof(frame));
    }
}
