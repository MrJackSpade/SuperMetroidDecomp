using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Checked OAM compilation shared by the opening cinematic's editable actors.</summary>
internal static class IntroCinematicSpriteCompiler
{
    /// <summary>Number of tile columns addressable by the opening-cinematic OBJ tile grid.</summary>
    internal const int TileColumns = 16;

    /// <summary>Number of tile rows addressable by the opening-cinematic OBJ tile grid.</summary>
    internal const int TileRows = 32;

    /// <summary>Maximum number of ordered sprite parts that fit in one OAM definition.</summary>
    internal const int MaximumParts = 128;

    /// <summary>Validates visual-part fields and packs them into hardware OAM words in their supplied order.</summary>
    /// <param name="visual">Ordered editable parts whose tile coordinates, size, palette, priority, and flips are compiled.</param>
    /// <param name="name">Sprite identity included in validation errors.</param>
    /// <returns>A composition containing the compiled OAM parts.</returns>
    /// <exception cref="InvalidDataException">The part count exceeds OAM capacity or a part contains unsupported visual fields.</exception>
    internal static SpriteComposition Compile(SpriteVisualPart[] visual, string name)
    {
        if (visual.Length > MaximumParts)
            throw new InvalidDataException($"Opening sprite {name} exceeds OAM capacity.");
        var compiled = new CompiledSpritePart[visual.Length];
        for (int index = 0; index < visual.Length; index++)
        {
            SpriteVisualPart? part = visual[index];
            if (part is null || part.OffsetX is < -256 or > 255 ||
                part.OffsetY is < -128 or > 127 || part.Size is not (8 or 16) ||
                part.Priority is < 0 or > 3 || part.Palette is < 0 or > 7 ||
                part.TileColumn < 0 || part.TileRow < 0 ||
                part.TileColumn > TileColumns - part.Size / 8 ||
                part.TileRow > TileRows - part.Size / 8)
                throw new InvalidDataException(
                    $"Opening sprite {name} part {index} has invalid visual fields.");
            SnesTileFlipFlags flips =
                (part.FlipX ? SnesTileFlipFlags.Horizontal : 0) |
                (part.FlipY ? SnesTileFlipFlags.Vertical : 0);
            compiled[index] = new CompiledSpritePart(
                SnesSpritemapXWord.Create(part.OffsetX, part.Size == 16),
                unchecked((byte)(sbyte)part.OffsetY),
                SnesObjAttributeWord.Create(
                    part.TileRow * TileColumns + part.TileColumn,
                    part.Palette ?? 0, part.Priority, flips),
                part.Palette is null);
        }
        return new SpriteComposition(compiled);
    }
}
