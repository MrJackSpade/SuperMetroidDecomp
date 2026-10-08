using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Immutable ordered visual parts. No collision, attack or attachment mechanics are stored here.</summary>
public sealed class SpriteComposition
{
    private readonly IReadOnlyList<CompiledSpritePart> parts;
    internal SpriteComposition(CompiledSpritePart[] parts) => this.parts = (CompiledSpritePart[])parts.Clone();
    private SpriteComposition(IReadOnlyList<CompiledSpritePart> parts) => this.parts = parts;
    /// <summary>Wraps an immutable calculated part view. The caller must supply an immutable view, never a mutable document/list.</summary>
    internal static SpriteComposition FromCalculated(IReadOnlyList<CompiledSpritePart> parts) => new(parts);
    internal int PartCount => parts.Count;
    internal CompiledSpritePart Part(int index) => parts[index];

    /// <summary>Use an immutable calculated view only when every supplied visual field matches.
    /// Independently edited compositions retain their compiled parts.</summary>
    internal SpriteComposition CalculateIfMatching(IReadOnlyList<CompiledSpritePart> calculated) =>
        parts.SequenceEqual(calculated) ? new SpriteComposition(calculated) : this;

    /// <summary>Hashes only compiled visual fields, preserving the draw order and palette inheritance.</summary>
    internal void AppendIdentity(SelectedPresentationHash content)
    {
        content.Append("parts", parts.Count);
        foreach (CompiledSpritePart part in parts)
        {
            content.Append("x-and-size", part.X.Raw);
            content.Append("y", part.Y);
            content.Append("attributes", part.Attributes.Raw);
            content.Append("inherit-palette", part.InheritPalette ? 1 : 0);
        }
    }
    /// <summary>Appends all parts using the ordinary on-screen origin clipping path.</summary>
    /// <param name="oam">Object-attribute buffer that receives the ordered parts.</param>
    /// <param name="x">Horizontal composition origin in screen-space pixels.</param>
    /// <param name="y">Vertical composition origin in screen-space pixels.</param>
    /// <param name="paletteBits">OBJ palette bits applied only to parts configured to inherit them.</param>
    public void DrawOnScreen(OamBuffer oam, ushort x, ushort y, ushort paletteBits)
    {
        _ = SnesObjAttributeWord.FromPaletteBits(paletteBits);
        foreach (var part in parts)
            oam.AddOnScreenSpritePart(part.X, part.Y,
                part.InheritPalette ? part.Attributes.WithPaletteBits(paletteBits) : part.Attributes, x, y);
    }

    /// <summary>Preserves the cartridge's opposite Y-wrap clipping for negative origins.</summary>
    public void DrawOffScreen(OamBuffer oam, ushort x, ushort y, ushort paletteBits)
    {
        _ = SnesObjAttributeWord.FromPaletteBits(paletteBits);
        foreach (var part in parts)
            oam.AddOffScreenSpritePart(part.X, part.Y,
                part.InheritPalette ? part.Attributes.WithPaletteBits(paletteBits) : part.Attributes,
                x, y);
    }
}

/// <summary>Runtime PPU representation compiled from authored offsets, regions, size and color selection.</summary>
internal readonly record struct CompiledSpritePart(SnesSpritemapXWord X, byte Y, SnesObjAttributeWord Attributes, bool InheritPalette);

/// <summary>One ordered region in an indexed tile sheet. Null palette inherits its drawing owner's current palette.</summary>
public sealed record SpriteVisualPart
{
    /// <summary>Gets the signed horizontal pixel offset from the composition origin.</summary>
    public required int OffsetX { get; init; }
    /// <summary>Gets the signed vertical pixel offset from the composition origin.</summary>
    public required int OffsetY { get; init; }
    /// <summary>Gets the zero-based tile column in the presentation's indexed artwork atlas.</summary>
    public required int TileColumn { get; init; }
    /// <summary>Gets the zero-based tile row in the presentation's indexed artwork atlas.</summary>
    public required int TileRow { get; init; }
    /// <summary>Gets the square OBJ size in pixels, either 8 or 16.</summary>
    public required int Size { get; init; }
    /// <summary>Gets the SNES OBJ priority tier from 0 through 3.</summary>
    public required int Priority { get; init; }
    /// <summary>Gets the OBJ palette selector, or <see langword="null"/> to inherit the drawing owner's palette.</summary>
    public required int? Palette { get; init; }
    /// <summary>Gets whether the selected artwork region is reflected horizontally.</summary>
    public required bool FlipX { get; init; }
    /// <summary>Gets whether the selected artwork region is reflected vertically.</summary>
    public required bool FlipY { get; init; }
}
