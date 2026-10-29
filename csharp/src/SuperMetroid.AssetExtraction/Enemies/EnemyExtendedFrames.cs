using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The installed extended enemy frames as the import pipeline holds them: a frame's draw components, the components
/// displayed for the frame gameplay shows, the placements gameplay reads, and their identity.
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
    /// Each frame's component placements, in component order, and the display bindings: what gameplay reads to place the
    /// components' sheet pictures.
    /// </summary>
    internal static EnemyExtendedFramePlacementCatalog Placements(EnemyExtendedFrameSelection selection) =>
        new(selection.Frames.ToDictionary(pair => pair.Key,
                pair => pair.Value.Select(component => new EnemyExtendedComponentPlacement(component.Offset)).ToArray()),
            new Dictionary<EnemyFrameId, EnemyFrameId>(selection.DisplayFrames));

    /// <summary>Finds a frame's components, including the shared empty extended frame's.</summary>
    internal static bool TryGet(EnemyExtendedFrameCatalog catalog, EnemyFrameId frame, out ReadOnlyMemory<EnemyExtendedDrawComponent> components) =>
        Components(catalog, EnemyExtendedFramePlacements.TryResolve(catalog.Placements, frame, out EnemyFrameId? stored), stored, out components);

    /// <summary>Maps the frame gameplay shows to the editable draw components displayed for it.</summary>
    internal static bool TryGetDisplay(EnemyExtendedFrameCatalog catalog, EnemyFrameId frame, out ReadOnlyMemory<EnemyExtendedDrawComponent> components) =>
        Components(catalog, EnemyExtendedFramePlacements.TryResolveDisplay(catalog.Placements, frame, out EnemyFrameId? stored), stored, out components);

    /// <summary>
    /// The components of a frame the placements resolved: the stored frame's, none for the shared empty extended frame
    /// (resolved without a stored frame), or nothing when the frame is not installed.
    /// </summary>
    private static bool Components(EnemyExtendedFrameCatalog catalog, bool resolved, EnemyFrameId? stored,
        out ReadOnlyMemory<EnemyExtendedDrawComponent> components)
    {
        components = !resolved ? default
            : stored is { } frame ? catalog.selection.Frames[frame]
            : ReadOnlyMemory<EnemyExtendedDrawComponent>.Empty;
        return resolved;
    }
}
