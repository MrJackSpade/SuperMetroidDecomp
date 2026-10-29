using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.World.Artwork;

/// <summary>
/// Compiles the authored parts of an ordinary or extended enemy composition document into the parts the enemy sheets are
/// composed from (#1463), checking them against the composition sheet's bounds. Gameplay draws the composed sheets and
/// never reads parts.
/// </summary>
internal static class EnemySpritemapCompiler
{
    /// <summary>Upper bound on sprite pieces accepted from one editable enemy composition.</summary>
    internal const int MaximumParts = 128;

    /// <summary>Character-sheet width in eight-pixel tiles for composition tile references.</summary>
    internal const int TileColumns = 16;

    /// <summary>Character-sheet height in eight-pixel tiles for composition tile references.</summary>
    internal const int TileRows = 32;

    /// <summary>Validates part capacity, offsets, tile bounds, palette, size, and priority of ordinary or extended frame pieces.</summary>
    /// <exception cref="InvalidDataException">The composition has too many parts or a part has invalid visual fields.</exception>
    internal static SpritePart[] CompileParts(SpriteVisualPart[] visual, string frameName)
    {
        if (visual.Length > MaximumParts)
            throw new InvalidDataException(
                $"Enemy composition {frameName} exceeds OAM part capacity.");
        var parts = new SpritePart[visual.Length];
        for (int index = 0; index < parts.Length; index++)
        {
            SpriteVisualPart? part = visual[index];
            if (part is null || part.OffsetX is < -256 or > 255 ||
                part.OffsetY is < -128 or > 127 || part.Size is not (8 or 16) ||
                part.Priority is < 0 or > 3 || part.Palette is null or < 0 or > 7 ||
                part.TileColumn is < 0 or >= TileColumns ||
                part.TileRow is < 0 or >= TileRows)
                throw new InvalidDataException(
                    $"Enemy composition {frameName} part {index} has invalid visual fields.");
            TileFlip flips =
                (part.FlipX ? TileFlip.Horizontal : 0) |
                (part.FlipY ? TileFlip.Vertical : 0);
            parts[index] = SpritePart.Create(Offset16<ScreenPixels>.Of(part.OffsetX, part.OffsetY), part.Size == 16,
                part.TileRow * TileColumns + part.TileColumn, part.Palette.Value, part.Priority, flips);
        }
        return parts;
    }
}
