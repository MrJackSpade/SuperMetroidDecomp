using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The installed extended enemy frames as the import pipeline holds them: a frame's draw components, the components
/// displayed for the frame gameplay shows, and their identity.
/// </summary>
internal static class EnemyExtendedFrames
{
    /// <summary>Hashes each frame's components, then each display binding, in frame-key order.</summary>
    internal static string ContentIdentity(EnemyExtendedFrameCatalog catalog) =>
        SelectedPresentationHash.Create("enemy-extended-oam-v2", content =>
        {
            foreach ((EnemyFrameId frame, EnemyExtendedDrawComponent[] components) in catalog.selection.Frames.OrderBy(pair => EnemyFrameSheetKeys.Of(pair.Key), StringComparer.Ordinal))
            {
                content.AppendName("frame", EnemyFrameSheetKeys.Of(frame));
                content.Append("components", components.Length);
                foreach (EnemyExtendedDrawComponent component in components)
                {
                    content.Append("offset-x", component.Offset.X);
                    content.Append("offset-y", component.Offset.Y);
                    content.AppendEnemyParts(component.Parts);
                }
            }
            foreach ((EnemyFrameId stock, EnemyFrameId selected) in catalog.selection.DisplayFrames.OrderBy(pair => EnemyFrameSheetKeys.Of(pair.Key), StringComparer.Ordinal))
            {
                content.AppendName("native-binding", EnemyFrameSheetKeys.Of(stock));
                content.AppendName("selected-binding", EnemyFrameSheetKeys.Of(selected));
            }
        });

    /// <summary>
    /// Finds the installed frame <paramref name="frame"/> draws. The empty frame draws nothing, so it resolves without a
    /// stored frame.
    /// </summary>
    internal static bool TryResolve(EnemyExtendedFrameCatalog catalog, EnemyFrameId frame, out EnemyFrameId? stored)
    {
        if (frame == EnemyFrameId.Empty)
        {
            stored = null;
            return true;
        }
        stored = catalog.selection.Frames.ContainsKey(frame) ? frame : null;
        return stored is not null;
    }

    /// <summary>
    /// Finds the installed frame displayed for the frame gameplay shows: the frame its display binding selects when that
    /// frame is installed, otherwise the frame itself.
    /// </summary>
    internal static bool TryResolveDisplay(EnemyExtendedFrameCatalog catalog, EnemyFrameId frame, out EnemyFrameId? stored)
    {
        if (catalog.selection.DisplayFrames.TryGetValue(frame, out EnemyFrameId selected) &&
            catalog.selection.Frames.ContainsKey(selected))
        {
            stored = selected;
            return true;
        }
        return TryResolve(catalog, frame, out stored);
    }

    /// <summary>Maps the frame gameplay shows to the editable draw components displayed for it.</summary>
    internal static bool TryGetDisplay(EnemyExtendedFrameCatalog catalog, EnemyFrameId frame, out ReadOnlyMemory<EnemyExtendedDrawComponent> components) =>
        Components(catalog, TryResolveDisplay(catalog, frame, out EnemyFrameId? stored), stored, out components);

    /// <summary>
    /// The components of a frame the placements resolved: the stored frame's, none for the empty frame
    /// (resolved without a stored frame), or nothing when the frame is not installed.
    /// </summary>
    internal static bool Components(EnemyExtendedFrameCatalog catalog, bool resolved, EnemyFrameId? stored,
        out ReadOnlyMemory<EnemyExtendedDrawComponent> components)
    {
        components = !resolved ? default
            : stored is { } frame ? catalog.selection.Frames[frame]
            : ReadOnlyMemory<EnemyExtendedDrawComponent>.Empty;
        return resolved;
    }
}
