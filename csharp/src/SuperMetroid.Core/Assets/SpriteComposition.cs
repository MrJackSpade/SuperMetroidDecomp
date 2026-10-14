namespace SuperMetroid.Core.Assets;

/// <summary>Immutable ordered visual parts. No collision, attack or attachment mechanics are stored here.</summary>
public sealed class SpriteComposition
{
    /// <summary>Ordered immutable OAM parts, including per-part palette inheritance choices.</summary>
    internal readonly IReadOnlyList<CompiledSpritePart> parts;
    /// <summary>Copies authored parts so later document edits cannot alter a compiled composition.</summary>
    internal SpriteComposition(CompiledSpritePart[] parts) => this.parts = (CompiledSpritePart[])parts.Clone();
    /// <summary>Retains an already immutable calculated view without another array copy.</summary>
    private SpriteComposition(IReadOnlyList<CompiledSpritePart> parts) => this.parts = parts;
    /// <summary>Wraps an immutable calculated part view. The caller must supply an immutable view, never a mutable document/list.</summary>
    internal static SpriteComposition FromCalculated(IReadOnlyList<CompiledSpritePart> parts) => new(parts);
    /// <summary>Number of OAM parts emitted in composition order.</summary>
    internal int PartCount => parts.Count;
    /// <summary>Selects one compiled OAM part by its drawing ordinal.</summary>
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
    /// <param name="origin">Composition origin in screen-space pixels.</param>
    /// <param name="paletteBits">OBJ palette bits applied only to parts configured to inherit them.</param>
    public void DrawOnScreen(OamBuffer oam, Position16<OamPixels> origin, ushort paletteBits)
    {
        _ = SnesObjAttributeWord.FromPaletteBits(paletteBits);
        foreach (var part in parts)
            oam.AddOnScreenSpritePart(part.X, part.Y,
                part.InheritPalette ? part.Attributes.WithPaletteBits(paletteBits) : part.Attributes, origin);
    }

    /// <summary>Preserves the cartridge's opposite Y-wrap clipping for negative origins.</summary>
    public void DrawOffScreen(OamBuffer oam, Position16<OamPixels> origin, ushort paletteBits)
    {
        _ = SnesObjAttributeWord.FromPaletteBits(paletteBits);
        foreach (var part in parts)
            oam.AddOffScreenSpritePart(part.X, part.Y,
                part.InheritPalette ? part.Attributes.WithPaletteBits(paletteBits) : part.Attributes,
                origin);
    }
}
