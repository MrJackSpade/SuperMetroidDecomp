using SuperMetroid.Core.Frontend.FileSelect;

namespace SuperMetroid.AssetExtraction.Frontend.FileSelect;

/// <summary>Selects the selected slot's helmet spritemap for each animation frame, which the import reads (#1502).</summary>
public static partial class FileSelectHelmetSpritemaps
{
    /// <summary>The spritemap ID of the helmet picture <paramref name="frame"/> (#2359, #2565).</summary>
    /// <remarks>Extraction reads each picture's spritemap with it.</remarks>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="frame"/> is not a picture of the turn.</exception>
    internal static int SpritemapId(FileSelectHelmetFrame frame) =>
        FileSelectNativeLayout.HelmetSpritemaps.TryGetValue(frame, out int spritemap) ? spritemap : throw new ArgumentOutOfRangeException(nameof(frame));
}
