using SuperMetroid.Core.Enemies.Drawing;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// The installed extended enemy frames as gameplay draws them: which installed frame's components are displayed for
/// the frame gameplay shows, and those components' placements.
/// </summary>
internal static class EnemyExtendedFramePlacements
{
    /// <summary>
    /// Finds the installed frame whose components <paramref name="frame"/> draws. The shared empty extended frame's sole
    /// component draws the empty spritemap, so it resolves without a stored frame before any editable drawing is needed.
    /// </summary>
    internal static bool TryResolve(EnemyExtendedFramePlacementCatalog catalog, EnemyFrameId frame, out EnemyFrameId? stored)
    {
        if (CommonEnemyEmptyFrames.IsFrame(frame))
        {
            stored = null;
            return true;
        }
        stored = frame;
        if (catalog.selection.Frames.ContainsKey(frame))
            return true;
        stored = null;
        return false;
    }

    /// <summary>
    /// Finds the installed frame whose components are displayed for the frame gameplay shows: the frame its display
    /// binding selects when that frame is installed, otherwise the frame itself.
    /// </summary>
    internal static bool TryResolveDisplay(EnemyExtendedFramePlacementCatalog catalog, EnemyFrameId frame, out EnemyFrameId? stored)
    {
        if (catalog.selection.DisplayFrames.TryGetValue(frame, out EnemyFrameId selected) &&
            catalog.selection.Frames.ContainsKey(selected))
        {
            stored = selected;
            return true;
        }
        return TryResolve(catalog, frame, out stored);
    }

    /// <summary>Maps the frame gameplay shows to the component placements displayed for it.</summary>
    internal static bool TryGetDisplay(EnemyExtendedFramePlacementCatalog catalog, EnemyFrameId frame,
        out ReadOnlyMemory<EnemyExtendedComponentPlacement> components)
    {
        bool resolved = TryResolveDisplay(catalog, frame, out EnemyFrameId? stored);
        components = !resolved ? default
            : stored is { } displayed ? catalog.selection.Frames[displayed]
            : ReadOnlyMemory<EnemyExtendedComponentPlacement>.Empty;
        return resolved;
    }

    /// <summary>The frame displayed for <paramref name="frame"/>, whose BG2 half accompanies the displayed components.</summary>
    internal static EnemyFrameId GetDisplayFrame(EnemyExtendedFramePlacementCatalog catalog, EnemyFrameId frame) =>
        catalog.selection.DisplayFrames.TryGetValue(frame, out EnemyFrameId selected) ? selected : frame;
}
