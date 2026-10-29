using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// Whole frames for enemy-format spritemaps (enemies, enemy projectiles, room sprite objects and the Ceres elevator)
/// over the gameplay OBJ character page, as the import pipeline composes the enemy sheets. Each part's attribute word
/// is formed exactly as the cartridge's enemy spritemap writer forms it: the packed tile base is added to the whole
/// word and the palette bits are ORed in. Frames are shared per composition, tile base and palette, so a steady
/// animation publishes the same frames.
/// </summary>
internal static class EnemySpriteFrames
{
    /// <summary>Frames per composition owner, keyed by the composition's range, tile base and palette bits.</summary>
    private static readonly ConditionalWeakTable<object, ConcurrentDictionary<EnemySpriteFrameRange, SpriteFrame>> Frames = new();

    /// <summary>The frame an installed composition shows with the given tile base and palette row.</summary>
    /// <param name="parts">The composition, frontmost part first.</param>
    /// <param name="baseTileIndex">Packed tile base added to every part's attribute word.</param>
    /// <param name="palette">Palette row ORed into every part's attribute word.</param>
    public static SpriteFrame Frame(EnemySpritemapParts parts, int baseTileIndex, int palette)
    {
        Ensure.NotNull(parts);
        return Frames.GetValue(parts, _ => new()).GetOrAdd(new EnemySpriteFrameRange(0, parts.Count, baseTileIndex, palette),
            key => Compile(parts.ToArray(), key.Base, key.Palette));
    }

    /// <summary>The frame a composition slice shows with the given tile base and palette row.</summary>
    /// <param name="parts">The composition, frontmost part first; it must be array-backed so its frames can be shared.</param>
    /// <param name="baseTileIndex">Packed tile base added to every part's attribute word.</param>
    /// <param name="palette">Palette row ORed into every part's attribute word.</param>
    /// <exception cref="ArgumentException">The composition is not array-backed.</exception>
    public static SpriteFrame Frame(ReadOnlyMemory<SpritePart> parts, int baseTileIndex, int palette)
    {
        if (!MemoryMarshal.TryGetArray(parts, out ArraySegment<SpritePart> segment) || segment.Array is null)
            throw new ArgumentException("Enemy spritemap compositions are array-backed installed data.", nameof(parts));
        return Frames.GetValue(segment.Array, _ => new()).GetOrAdd(new EnemySpriteFrameRange(segment.Offset, segment.Count, baseTileIndex, palette),
            key => Compile(parts.Span, key.Base, key.Palette));
    }

    /// <summary>Forms each part's final attribute word as the cartridge's writer does and places it as a whole-frame part.</summary>
    private static SpriteFrame Compile(ReadOnlySpan<SpritePart> parts, int baseTileIndex, int paletteRow)
    {
        SnesObjAttributeWord palette = SnesObjAttributeWord.Create(tileNumber: 0, paletteIndex: paletteRow, priority: 0);
        var framed = new SpriteFramePart[parts.Length];
        for (int index = 0; index < parts.Length; index++)
        {
            SpritemapPart part = SpritemapPart.From(parts[index]);
            SnesObjAttributeWord attributes = part.Attributes.AddPackedTileBase(checked((ushort)baseTileIndex)).Or(palette);
            framed[index] = SpritePartFrames.Part((part with { Attributes = attributes }).ToPart(), firstCharacter: 0);
        }
        return new SpriteFrame($"Enemy+{baseTileIndex:X3}/{paletteRow}", ImmutableCollectionsMarshal.AsImmutableArray(framed));
    }
}
