using System.Text.Json;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Installed enemy OAM compositions keyed by native visual identity.</summary>
public sealed class EnemySpritemapCatalog
{
    /// <summary>Canonical selected presentation data; no derived field is added to debugger states.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("enemy-oam-v1", content =>
        {
            foreach ((int frame, EnemySpritemapParts parts) in frames.OrderBy(pair => pair.Key))
            {
                content.Append("frame", frame);
                content.AppendEnemyParts(parts);
            }
            foreach ((int native, int selected) in displayFrames.OrderBy(pair => pair.Key))
            {
                content.Append("native-binding", native);
                content.Append("selected-binding", selected);
            }
        });

    /// <summary>Upper bound on sprite pieces accepted from one editable enemy composition.</summary>
    internal const int MaximumParts = 128;
    /// <summary>Character-sheet width in eight-pixel tiles for composition tile references.</summary>
    internal const int TileColumns = 16;
    /// <summary>Character-sheet height in eight-pixel tiles for composition tile references.</summary>
    internal const int TileRows = 32;

    /// <summary>Compiled OAM parts keyed by the native bank-and-pointer identity of each authored frame.</summary>
    internal readonly Dictionary<int, EnemySpritemapParts> frames;
    /// <summary>Visual-only mapping from each native frame identity to its selected displayed frame.</summary>
    internal readonly Dictionary<int, int> displayFrames;

    /// <summary>Retains validated frame compositions and same-bank display selections.</summary>
    internal EnemySpritemapCatalog(Dictionary<int, EnemySpritemapParts> frames,
        Dictionary<int, int> displayFrames)
    {
        this.frames = frames;
        this.displayFrames = displayFrames;
    }

    /// <summary>
    /// Resolves an editable presentation binding without changing the native frame pointer
    /// retained by enemy AI, hitbox selection, or instruction timing.
    /// </summary>
    /// <param name="bank">Native enemy spritemap bank, combined with the same-bank pointer to identify the authored binding.</param>
    /// <param name="nativePointer">Unmodified native frame word retained by gameplay; the selected visual frame must belong to the same bank.</param>
    /// <param name="parts">Installed OAM composition selected by the binding, or <see cref="EnemySpritemapParts.Empty"/> when no bound frame is installed.</param>
    /// <returns>True when both a display binding and its selected composition exist; missing identities return false without reading cartridge data.</returns>
    public bool TryGetDisplay(byte bank, ushort nativePointer,
        out EnemySpritemapParts parts)
    {
        int identity = ((int)new SnesAddress(bank, nativePointer));
        if (displayFrames.TryGetValue(identity, out int selected) &&
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
