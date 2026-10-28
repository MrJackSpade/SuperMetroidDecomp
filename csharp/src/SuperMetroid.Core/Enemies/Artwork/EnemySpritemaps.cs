using System.Text.Json;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// The installed enemy OAM compositions: compiling and checking authored parts, the composition displayed for the frame
/// gameplay shows, and their identity.
/// </summary>
internal static class EnemySpritemaps
{
    /// <summary>Hashes each frame's parts, then each display binding, in frame-key order.</summary>
    internal static string ContentIdentity(EnemySpritemapCatalog catalog) =>
        SelectedPresentationHash.Create("enemy-oam-v2", content =>
        {
            foreach ((EnemyFrameId frame, EnemySpritemapParts parts) in catalog.selection.Frames.OrderBy(pair => EnemyFrameSheetKeys.Of(pair.Key), StringComparer.Ordinal))
            {
                content.AppendName("frame", EnemyFrameSheetKeys.Of(frame));
                content.AppendEnemyParts(parts);
            }
            foreach ((EnemyFrameId stock, EnemyFrameId selected) in catalog.selection.DisplayFrames.OrderBy(pair => EnemyFrameSheetKeys.Of(pair.Key), StringComparer.Ordinal))
            {
                content.AppendName("native-binding", EnemyFrameSheetKeys.Of(stock));
                content.AppendName("selected-binding", EnemyFrameSheetKeys.Of(selected));
            }
        });

    /// <summary>Resolves an editable presentation binding, or <see cref="EnemySpritemapParts.Empty"/> when no bound frame is installed.</summary>
    internal static bool TryGetDisplay(EnemySpritemapCatalog catalog, EnemyFrameId frame, out EnemySpritemapParts parts)
    {
        if (catalog.selection.DisplayFrames.TryGetValue(frame, out EnemyFrameId selected) &&
            catalog.selection.Frames.TryGetValue(selected, out EnemySpritemapParts? found))
        {
            parts = found;
            return true;
        }
        parts = EnemySpritemapParts.Empty;
        return false;
    }

    /// <summary>Validates part capacity, offsets, tile bounds, palette, size, and priority of ordinary or extended frame pieces.</summary>
    internal static SpritePart[] CompileParts(
        SpriteVisualPart[] visual, string frameName)
    {
        if (visual.Length > EnemySpritemapCatalog.MaximumParts)
            throw new InvalidDataException(
                $"Enemy composition {frameName} exceeds OAM part capacity.");
        var parts = new SpritePart[visual.Length];
        for (int index = 0; index < parts.Length; index++)
        {
            SpriteVisualPart? part = visual[index];
            if (part is null || part.OffsetX is < -256 or > 255 ||
                part.OffsetY is < -128 or > 127 || part.Size is not (8 or 16) ||
                part.Priority is < 0 or > 3 || part.Palette is null or < 0 or > 7 ||
                part.TileColumn is < 0 or >= EnemySpritemapCatalog.TileColumns ||
                part.TileRow is < 0 or >= EnemySpritemapCatalog.TileRows)
                throw new InvalidDataException(
                    $"Enemy composition {frameName} part {index} has invalid visual fields.");
            TileFlip flips =
                (part.FlipX ? TileFlip.Horizontal : 0) |
                (part.FlipY ? TileFlip.Vertical : 0);
            parts[index] = SpritePart.Create(Offset16<ScreenPixels>.Of(part.OffsetX, part.OffsetY), part.Size == 16,
                part.TileRow * EnemySpritemapCatalog.TileColumns + part.TileColumn, part.Palette.Value, part.Priority, flips);
        }
        return parts;
    }

    /// <summary>Rejects duplicate composition keys before JSON deserialization can overwrite an earlier edit.</summary>
    internal static void RejectDuplicateProperties(JsonElement element, StringComparer? propertyComparer = null) =>
        JsonAssetDocument.RejectDuplicateProperties(element, propertyComparer ?? StringComparer.Ordinal,
            name => new InvalidDataException($"Duplicate enemy composition property {name}."));
}
