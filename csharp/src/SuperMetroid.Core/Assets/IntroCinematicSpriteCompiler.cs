using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Checked OAM compilation shared by the opening cinematic's editable actors.</summary>
internal static class IntroCinematicSpriteCompiler
{
    internal const int TileColumns = 16;
    internal const int TileRows = 32;
    internal const int MaximumParts = 128;

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
