using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Immutable ordered visual parts. No collision, attack or attachment mechanics are stored here.</summary>
public sealed class SpriteComposition
{
    private readonly CompiledSpritePart[] parts;
    internal SpriteComposition(CompiledSpritePart[] parts) => this.parts = (CompiledSpritePart[])parts.Clone();
    public void DrawOnScreen(OamBuffer oam, ushort x, ushort y, ushort paletteBits)
    {
        _ = SnesObjAttributeWord.FromPaletteBits(paletteBits);
        foreach (var part in parts)
            oam.AddOnScreenSpritePart(part.X, part.Y,
                part.InheritPalette ? part.Attributes.WithPaletteBits(paletteBits) : part.Attributes, x, y);
    }
}

/// <summary>Runtime PPU representation compiled from authored offsets, regions, size and color selection.</summary>
internal readonly record struct CompiledSpritePart(SnesSpritemapXWord X, byte Y, SnesObjAttributeWord Attributes, bool InheritPalette);

/// <summary>One ordered region in an indexed tile sheet. Null palette inherits its drawing owner's current palette.</summary>
public sealed record SpriteVisualPart
{
    public required int OffsetX { get; init; }
    public required int OffsetY { get; init; }
    public required int TileColumn { get; init; }
    public required int TileRow { get; init; }
    public required int Size { get; init; }
    public required int Priority { get; init; }
    public required int? Palette { get; init; }
    public required bool FlipX { get; init; }
    public required bool FlipY { get; init; }
}
