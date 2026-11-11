using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Drawing;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// Builds what gameplay reads to draw every installed enemy frame (#1603) from the two installed enemy frame documents:
/// the installed frames, the frames pictured on the enemy sheets, and the display bindings.
/// </summary>
internal static class InstalledEnemyFrameDisplays
{
    /// <summary>
    /// Every installed frame of <paramref name="spritemaps"/> and <paramref name="extended"/>. A frame composed of one
    /// spritemap is pictured unless it draws nothing; a frame composed of placed pieces is pictured when it has a sprite
    /// piece, so a frame shown only on a boss body has no picture.
    /// </summary>
    /// <exception cref="InvalidDataException">The two documents install the same frame or bind the same frame.</exception>
    internal static EnemyFrameDisplays Of(EnemySpritemapCatalog spritemaps, EnemyExtendedFrameCatalog extended)
    {
        var frames = new HashSet<EnemyFrameId>();
        var pictured = new HashSet<EnemyFrameId>();
        var displayFrames = new Dictionary<EnemyFrameId, EnemyFrameId>();
        foreach (EnemyFrameId frame in spritemaps.selection.Frames.Keys)
        {
            Install(frames, frame);
            if (!CommonEnemyEmptyFrames.DrawsNothing(frame))
                pictured.Add(frame);
        }
        foreach ((EnemyFrameId frame, EnemyExtendedDrawComponent[] components) in extended.selection.Frames)
        {
            Install(frames, frame);
            if (components.Length > 0)
                pictured.Add(frame);
        }
        foreach ((EnemyFrameId shown, EnemyFrameId selected) in spritemaps.selection.DisplayFrames.Concat(extended.selection.DisplayFrames))
            if (!displayFrames.TryAdd(shown, selected))
                throw new InvalidDataException($"The installed enemy frame documents both bind enemy frame {shown}.");
        return new EnemyFrameDisplays(frames, pictured, displayFrames);
    }

    /// <summary>Adds <paramref name="frame"/> to the installed frames.</summary>
    /// <exception cref="InvalidDataException">Another document already installs it.</exception>
    private static void Install(HashSet<EnemyFrameId> frames, EnemyFrameId frame)
    {
        if (!frames.Add(frame))
            throw new InvalidDataException($"The installed enemy frame documents both install enemy frame {frame}.");
    }
}
