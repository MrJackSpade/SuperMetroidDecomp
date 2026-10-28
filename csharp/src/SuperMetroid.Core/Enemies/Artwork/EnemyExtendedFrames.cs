using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Drawing;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// The installed extended enemy frames: a frame's draw components, the components displayed for the frame gameplay
/// shows, and their identity.
/// </summary>
internal static class EnemyExtendedFrames
{
    /// <summary>Hashes each frame's components, then each display binding, in frame-key order.</summary>
    internal static string ContentIdentity(EnemyExtendedFrameCatalog catalog) =>
        SelectedPresentationHash.Create("enemy-extended-oam-v2", content =>
        {
            foreach ((EnemyFrameId frame, EnemyExtendedDrawComponent[] components) in catalog.frames.OrderBy(pair => EnemyFrameSheetKeys.Of(pair.Key), StringComparer.Ordinal))
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
            foreach ((EnemyFrameId stock, EnemyFrameId selected) in catalog.displayFrames.OrderBy(pair => EnemyFrameSheetKeys.Of(pair.Key), StringComparer.Ordinal))
            {
                content.AppendName("native-binding", EnemyFrameSheetKeys.Of(stock));
                content.AppendName("selected-binding", EnemyFrameSheetKeys.Of(selected));
            }
        });

    /// <summary>Finds a frame's components, including the shared empty extended frame's.</summary>
    internal static bool TryGet(EnemyExtendedFrameCatalog catalog, EnemyFrameId frame, out ReadOnlyMemory<EnemyExtendedDrawComponent> components)
    {
        // The shared empty extended frame's sole component draws the empty spritemap; no editable drawing is needed
        // before initialization.
        if (CommonEnemyEmptyFrames.IsFrame(frame))
        {
            components = ReadOnlyMemory<EnemyExtendedDrawComponent>.Empty;
            return true;
        }
        if (catalog.frames.TryGetValue(frame, out EnemyExtendedDrawComponent[]? found))
        {
            components = found;
            return true;
        }
        components = default;
        return false;
    }

    /// <summary>Maps the frame gameplay shows to the editable draw components displayed for it.</summary>
    internal static bool TryGetDisplay(EnemyExtendedFrameCatalog catalog, EnemyFrameId frame, out ReadOnlyMemory<EnemyExtendedDrawComponent> components)
    {
        if (catalog.displayFrames.TryGetValue(frame, out EnemyFrameId selected) &&
            catalog.frames.TryGetValue(selected, out EnemyExtendedDrawComponent[]? found))
        {
            components = found;
            return true;
        }
        return TryGet(catalog, frame, out components);
    }

    /// <summary>The frame displayed for <paramref name="frame"/>, whose BG2 half accompanies the displayed components.</summary>
    internal static EnemyFrameId GetDisplayFrame(EnemyExtendedFrameCatalog catalog, EnemyFrameId frame) =>
        catalog.displayFrames.TryGetValue(frame, out EnemyFrameId selected) ? selected : frame;
}
