using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Frontend.Menus;

/// <summary>Shared checked compilation for compositions on the indexed menu OBJ sheet.</summary>
internal static class MenuSpriteCompiler
{
    /// <summary>
    /// Validates each part against the indexed menu OBJ sheet and SNES OAM limits, then
    /// packs tile, palette, priority, flip, size, and signed offsets into a composition.
    /// </summary>
    public static SpriteComposition Compile(SpriteVisualPart[] parts, string name)
    {
        if (parts.Length > MapSpriteFormat.MaximumParts)
            throw new InvalidDataException($"Menu sprite {name} exceeds 128 parts.");
        var compiled = new CompiledSpritePart[parts.Length];
        for (int index = 0; index < parts.Length; index++)
        {
            var part = parts[index];
            if (part is null || part.OffsetX is < -256 or > 255 || part.OffsetY is < -128 or > 127 ||
                part.Size is not (8 or 16) || (uint)part.Priority > 3 || part.Palette is < 0 or > 7 ||
                part.TileColumn < 0 || part.TileRow < 0 || part.TileColumn > MapSpriteFormat.TileColumns - part.Size / 8 ||
                part.TileRow > MapSpriteFormat.TileRows - part.Size / 8)
                throw new InvalidDataException($"Menu sprite {name} part {index} has an invalid offset, region, size, priority or palette.");
            var piece = SpritePart.Create(Offset16<ScreenPixels>.Of(part.OffsetX, part.OffsetY), part.Size == 16,
                part.TileRow * MapSpriteFormat.TileColumns + part.TileColumn,
                part.Palette ?? 0, part.Priority, (part.FlipX ? TileFlip.Horizontal : 0) | (part.FlipY ? TileFlip.Vertical : 0));
            compiled[index] = new(piece, part.Palette is null);
        }
        return new(compiled);
    }
}
