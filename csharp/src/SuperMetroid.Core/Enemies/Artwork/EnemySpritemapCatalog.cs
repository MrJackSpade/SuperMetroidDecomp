using System.Text.Json;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Installed enemy OAM compositions keyed by native visual identity.</summary>
public sealed class EnemySpritemapCatalog
{
    /// <summary>Canonical selected presentation data; no derived field is added to debugger states.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("enemy-oam-v2", content =>
        {
            foreach ((EnemyFrameId frame, EnemySpritemapParts parts) in frames.OrderBy(pair => pair.Key.ToString(), StringComparer.Ordinal))
            {
                content.AppendName("frame", frame.ToString());
                content.AppendEnemyParts(parts);
            }
            foreach ((EnemyFrameId native, EnemyFrameId selected) in displayFrames.OrderBy(pair => pair.Key.ToString(), StringComparer.Ordinal))
            {
                content.AppendName("native-binding", native.ToString());
                content.AppendName("selected-binding", selected.ToString());
            }
        });

    /// <summary>Upper bound on sprite pieces accepted from one editable enemy composition.</summary>
    internal const int MaximumParts = 128;
    /// <summary>Character-sheet width in eight-pixel tiles for composition tile references.</summary>
    internal const int TileColumns = 16;
    /// <summary>Character-sheet height in eight-pixel tiles for composition tile references.</summary>
    internal const int TileRows = 32;

    /// <summary>Compiled OAM parts of each authored frame.</summary>
    internal readonly Dictionary<EnemyFrameId, EnemySpritemapParts> frames;
    /// <summary>Visual-only mapping from each frame an enemy shows to the frame displayed for it.</summary>
    internal readonly Dictionary<EnemyFrameId, EnemyFrameId> displayFrames;

    /// <summary>Retains validated frame compositions and display selections.</summary>
    internal EnemySpritemapCatalog(Dictionary<EnemyFrameId, EnemySpritemapParts> frames,
        Dictionary<EnemyFrameId, EnemyFrameId> displayFrames)
    {
        this.frames = frames;
        this.displayFrames = displayFrames;
    }

    /// <summary>
    /// Resolves an editable presentation binding without changing the frame retained by enemy AI, hitbox selection, or
    /// instruction timing.
    /// </summary>
    /// <param name="frame">The frame gameplay shows.</param>
    /// <param name="parts">Installed OAM composition selected by the binding, or <see cref="EnemySpritemapParts.Empty"/> when no bound frame is installed.</param>
    /// <returns>True when both a display binding and its selected composition exist.</returns>
    public bool TryGetDisplay(EnemyFrameId frame, out EnemySpritemapParts parts)
    {
        if (displayFrames.TryGetValue(frame, out EnemyFrameId selected) &&
            frames.TryGetValue(selected, out EnemySpritemapParts? found))
        {
            parts = found;
            return true;
        }
        parts = EnemySpritemapParts.Empty;
        return false;
    }

    /// <summary>Validates OAM capacity, offsets, tile bounds, palette, size, and priority before packing ordinary or extended frame pieces.</summary>
    internal static SpritemapPart[] CompileParts(
        SpriteVisualPart[] visual, string frameName)
    {
        if (visual.Length > EnemySpritemapCatalog.MaximumParts)
            throw new InvalidDataException(
                $"Enemy composition {frameName} exceeds OAM part capacity.");
        var parts = new SpritemapPart[visual.Length];
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
            SnesTileFlipFlags flips =
                (part.FlipX ? SnesTileFlipFlags.Horizontal : 0) |
                (part.FlipY ? SnesTileFlipFlags.Vertical : 0);
            parts[index] = new SpritemapPart(
                SnesSpritemapXWord.Create(part.OffsetX, part.Size == 16),
                SnesSpritemapYByte.Create(part.OffsetY),
                SnesObjAttributeWord.Create(
                    part.TileRow * EnemySpritemapCatalog.TileColumns + part.TileColumn,
                    part.Palette.Value, part.Priority, flips));
        }
        return parts;
    }

    /// <summary>Rejects duplicate composition keys before JSON deserialization can overwrite an earlier edit.</summary>
    internal static void RejectDuplicateProperties(JsonElement element, StringComparer? propertyComparer = null) =>
        JsonAssetDocument.RejectDuplicateProperties(element, propertyComparer ?? StringComparer.Ordinal,
            name => new InvalidDataException($"Duplicate enemy composition property {name}."));
}
