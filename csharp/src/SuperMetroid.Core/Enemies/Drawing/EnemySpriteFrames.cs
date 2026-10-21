using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Drawing;

/// <summary>
/// Whole frames for enemy-format spritemaps (enemies, enemy projectiles, room sprite objects and the Ceres elevator)
/// over the gameplay OBJ character page. Each part's attribute word is formed exactly as the cartridge's enemy
/// spritemap writer forms it: the packed tile base is added to the whole word and the palette bits are ORed in.
/// Frames are shared per composition, tile base and palette, so a steady animation publishes the same frames.
/// </summary>
internal static class EnemySpriteFrames
{
    /// <summary>Frames per composition owner, keyed by the composition's range, tile base and palette bits.</summary>
    private static readonly ConditionalWeakTable<object, ConcurrentDictionary<EnemySpriteFrameRange, SpriteFrame>> Frames = new();

    /// <summary>The frame an installed composition shows with the given tile base and palette bits.</summary>
    /// <param name="parts">The composition, frontmost part first.</param>
    /// <param name="baseTileIndex">Packed tile base added to every part's attribute word.</param>
    /// <param name="paletteBits">Palette bits ORed into every part's attribute word.</param>
    public static SpriteFrame Frame(EnemySpritemapParts parts, ushort baseTileIndex, ushort paletteBits)
    {
        Ensure.NotNull(parts);
        return Frames.GetValue(parts, _ => new()).GetOrAdd(new EnemySpriteFrameRange(0, parts.Count, baseTileIndex, paletteBits),
            key => Compile(parts.ToArray(), key.Base, key.Palette));
    }

    /// <summary>The frame a composition slice shows with the given tile base and palette bits.</summary>
    /// <param name="parts">The composition, frontmost part first; it must be array-backed so its frames can be shared.</param>
    /// <param name="baseTileIndex">Packed tile base added to every part's attribute word.</param>
    /// <param name="paletteBits">Palette bits ORed into every part's attribute word.</param>
    /// <exception cref="ArgumentException">The composition is not array-backed.</exception>
    public static SpriteFrame Frame(ReadOnlyMemory<SpritemapPart> parts, ushort baseTileIndex, ushort paletteBits)
    {
        if (!MemoryMarshal.TryGetArray(parts, out ArraySegment<SpritemapPart> segment) || segment.Array is null)
            throw new ArgumentException("Enemy spritemap compositions are array-backed installed data.", nameof(parts));
        return Frames.GetValue(segment.Array, _ => new()).GetOrAdd(new EnemySpriteFrameRange(segment.Offset, segment.Count, baseTileIndex, paletteBits),
            key => Compile(parts.Span, key.Base, key.Palette));
    }

    /// <summary>
    /// The frame an extended enemy component shows at <paramref name="originY"/>: only the parts whose own row lies in
    /// the 256-row OBJ space. The cartridge's extended component writers ($81:8B22, $81:8B96) hide a part that would
    /// cross the vertical boundary instead of wrapping it to the other edge.
    /// </summary>
    /// <param name="parts">The component's composition, frontmost part first.</param>
    /// <param name="originY">The component origin's row, a signed 16-bit value.</param>
    /// <param name="baseTileIndex">Packed tile base added to every part's attribute word.</param>
    /// <param name="paletteBits">Palette bits ORed into every part's attribute word.</param>
    public static SpriteFrame RowClippedFrame(EnemySpritemapParts parts, short originY, ushort baseTileIndex, ushort paletteBits)
    {
        Ensure.NotNull(parts);
        SpritemapPart[] shown = parts.Where(part => (uint)(originY + part.Y.SignedOffset) < ObjRowSpace).ToArray();
        return shown.Length == parts.Count ? Frame(parts, baseTileIndex, paletteBits) : Compile(shown, baseTileIndex, paletteBits);
    }

    /// <summary>The rows an OBJ part can occupy without wrapping.</summary>
    private const int ObjRowSpace = 256;

    /// <summary>Forms each part's final attribute word and places it as a whole-frame part.</summary>
    private static SpriteFrame Compile(ReadOnlySpan<SpritemapPart> parts, ushort baseTileIndex, ushort paletteBits)
    {
        SnesObjAttributeWord palette = SnesObjAttributeWord.FromPaletteBits(paletteBits);
        var framed = new SpriteFramePart[parts.Length];
        for (int index = 0; index < parts.Length; index++)
        {
            SpritemapPart part = parts[index];
            SnesObjAttributeWord attributes = part.Attributes.AddPackedTileBase(baseTileIndex).Or(palette);
            framed[index] = ObjPartFrames.Part(part.X, part.Y, attributes, firstCharacter: 0);
        }
        return new SpriteFrame($"Enemy+{baseTileIndex:X3}/{palette.PaletteIndex}", ImmutableCollectionsMarshal.AsImmutableArray(framed));
    }
}
