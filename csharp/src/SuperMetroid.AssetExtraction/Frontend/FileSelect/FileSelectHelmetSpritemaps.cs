using SuperMetroid.Core.Frontend.FileSelect;

namespace SuperMetroid.AssetExtraction.Frontend.FileSelect;

/// <summary>Selects the selected slot's helmet spritemap for each animation frame, which the import reads (#1502).</summary>
public static class FileSelectHelmetSpritemaps
{
    /// <summary>The spritemap ID each helmet picture selects (#2359, #2565): the consecutive spritemaps $2C..33.</summary>
    private static readonly IReadOnlyDictionary<FileSelectHelmetFrame, int> Spritemaps = new Dictionary<FileSelectHelmetFrame, int>
    {
        [FileSelectHelmetFrame.TurnedAway] = 0x2c,
        [FileSelectHelmetFrame.Turning] = 0x2d,
        [FileSelectHelmetFrame.ShineLeft] = 0x2e,
        [FileSelectHelmetFrame.ShineLeftOfMiddle] = 0x2f,
        [FileSelectHelmetFrame.ShineRightOfMiddle] = 0x30,
        [FileSelectHelmetFrame.ShineRight] = 0x31,
        [FileSelectHelmetFrame.SparkleTall] = 0x32,
        [FileSelectHelmetFrame.SparkleWide] = 0x33,
    };

    /// <summary>The spritemap ID of the helmet picture <paramref name="frame"/>.</summary>
    /// <remarks>Extraction reads each picture's spritemap with it.</remarks>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="frame"/> is not a picture of the turn.</exception>
    internal static int SpritemapId(FileSelectHelmetFrame frame) =>
        Spritemaps.TryGetValue(frame, out int spritemap) ? spritemap : throw new ArgumentOutOfRangeException(nameof(frame));
}
