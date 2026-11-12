using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>Cartridge values and helpers of <see cref="EnemyExtendedFrames"/> that only verification reads.</summary>
internal static class EnemyExtendedFramesConstants
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
}
