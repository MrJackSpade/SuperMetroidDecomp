using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Frontend.FileSelect;

namespace SuperMetroid.AssetExtraction.Frontend.FileSelect;

/// <summary>Selects the selected slot's helmet spritemap for each animation frame, which the import reads (#1502).</summary>
public static class FileSelectHelmetSpritemaps
{
    /// <summary>The animation's frame list holds eight consecutive IDs and one repeated terminal ID.</summary>
    public const int EntryCount = FileSelectHelmetAnimation.FrameCount + 1;

    /// <summary>Calculates the complete nine-entry frame list.</summary>
    /// <remarks>
    /// Frame indices 0..7 select consecutive spritemaps $2C..33. Entry 8 repeats
    /// the final image; the game clamps the active frame to 7 before drawing.
    /// Extraction reads each frame's spritemap with it. No generated table is stored.
    /// The ninth entry remains representable for exact bounded source parity;
    /// other indices reject before arithmetic, without narrowing or wrapping.
    /// </remarks>
    public static int SpritemapId(int frame)
    {
        if ((uint)frame >= EntryCount) throw new ArgumentOutOfRangeException(nameof(frame));
        return WordWrap.Unsigned(44 + Math.Min(frame, FileSelectHelmetAnimation.FrameCount - 1));
    }
}
