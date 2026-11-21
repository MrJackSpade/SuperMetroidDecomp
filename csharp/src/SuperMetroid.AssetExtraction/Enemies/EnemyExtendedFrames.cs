using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Drawing;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The installed extended enemy frames as the import pipeline holds them: a frame's draw components, the components
/// displayed for the frame gameplay shows, and their identity.
/// </summary>
internal static class EnemyExtendedFrames
{

    /// <summary>
    /// The components of the installed frame <paramref name="frame"/> draws, or null when it is not installed (#2560). The empty
    /// frame draws nothing, so it has no components and needs no stored frame.
    /// </summary>
    private static ReadOnlyMemory<EnemyExtendedDrawComponent>? ComponentsOf(EnemyExtendedFrameCatalog catalog, EnemyFrameId frame) =>
        CommonEnemyEmptyFrames.IsEmpty(frame) ? ReadOnlyMemory<EnemyExtendedDrawComponent>.Empty
        : catalog.selection.Frames.TryGetValue(frame, out EnemyExtendedDrawComponent[]? components) ? new ReadOnlyMemory<EnemyExtendedDrawComponent>(components)
        : null;

    /// <summary>
    /// The components of the installed frame displayed for the frame gameplay shows, or null when it is not installed: the
    /// frame its display binding selects when that frame is installed, otherwise the frame itself.
    /// </summary>
    private static ReadOnlyMemory<EnemyExtendedDrawComponent>? DisplayedComponentsOf(EnemyExtendedFrameCatalog catalog, EnemyFrameId frame) =>
        catalog.selection.DisplayFrames.TryGetValue(frame, out EnemyFrameId selected) &&
        catalog.selection.Frames.TryGetValue(selected, out EnemyExtendedDrawComponent[]? components)
            ? new ReadOnlyMemory<EnemyExtendedDrawComponent>(components)
            : ComponentsOf(catalog, frame);

    /// <summary>Maps the frame gameplay shows to the editable draw components displayed for it.</summary>
    internal static bool TryGetDisplay(EnemyExtendedFrameCatalog catalog, EnemyFrameId frame, out ReadOnlyMemory<EnemyExtendedDrawComponent> components)
    {
        if (DisplayedComponentsOf(catalog, frame) is { } displayed)
        {
            components = displayed;
            return true;
        }
        components = default;
        return false;
    }
}
