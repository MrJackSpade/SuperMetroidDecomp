using SuperMetroid.Core.Enemies.Drawing;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// The installed extended enemy frames as gameplay draws them: which installed frame is displayed for the frame gameplay
/// shows, and whether it has a picture.
/// </summary>
internal static class EnemyExtendedFrameDisplays
{
    /// <summary>
    /// Finds the installed frame <paramref name="frame"/> draws. The shared empty extended frame draws nothing, so it
    /// resolves without a stored frame before any editable drawing is needed.
    /// </summary>
    internal static bool TryResolve(EnemyExtendedFrameDisplayCatalog catalog, EnemyFrameId frame, out EnemyFrameId? stored)
    {
        if (CommonEnemyEmptyFrames.IsFrame(frame))
        {
            stored = null;
            return true;
        }
        stored = frame;
        if (catalog.selection.Frames.Contains(frame))
            return true;
        stored = null;
        return false;
    }

    /// <summary>
    /// Finds the installed frame displayed for the frame gameplay shows: the frame its display binding selects when that
    /// frame is installed, otherwise the frame itself.
    /// </summary>
    internal static bool TryResolveDisplay(EnemyExtendedFrameDisplayCatalog catalog, EnemyFrameId frame, out EnemyFrameId? stored)
    {
        if (catalog.selection.DisplayFrames.TryGetValue(frame, out EnemyFrameId selected) &&
            catalog.selection.Frames.Contains(selected))
        {
            stored = selected;
            return true;
        }
        return TryResolve(catalog, frame, out stored);
    }

    /// <summary>Whether the frame gameplay shows is displayed through an installed frame, and whether that frame has a picture.</summary>
    internal static bool TryGetDisplay(EnemyExtendedFrameDisplayCatalog catalog, EnemyFrameId frame, out bool pictured)
    {
        bool resolved = TryResolveDisplay(catalog, frame, out EnemyFrameId? stored);
        pictured = resolved && stored is { } displayed && catalog.selection.Pictured.Contains(displayed);
        return resolved;
    }

    /// <summary>The frame displayed for <paramref name="frame"/>, whose boss body half accompanies the displayed picture.</summary>
    internal static EnemyFrameId GetDisplayFrame(EnemyExtendedFrameDisplayCatalog catalog, EnemyFrameId frame) =>
        catalog.selection.DisplayFrames.TryGetValue(frame, out EnemyFrameId selected) ? selected : frame;
}
