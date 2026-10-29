using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Builds one selected Bomb or beam-explosion effect from its native glyph progression.</summary>
internal readonly struct SimpleEffectParts(int phase, bool quad) : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Bomb poses use one cell; beam-explosion quadrant poses use four.</summary>
    public int Count => quad ? 4 : 1;
    /// <summary>Selects the bomb glyph or reflected explosion quadrant for this phase.</summary>
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            int column = index / 2, row = index % 2;
            int x = quad ? -column * 8 : -8 / 2;
            int y = quad ? -row * 8 : -8 / 2;
            int tile = (quad ? ProjectileSpritePartDefinitions.BeamExplosionQuadGlyphStart : ProjectileSpritePartDefinitions.BombGlyphStart) + phase;
            var flips = quad ? (column == 0 ? TileFlip.Horizontal : 0) |
                (row == 0 ? TileFlip.Vertical : 0) : 0;
            return new(SpritePart.Create(Offset16<ScreenPixels>.Of(x, y), false,
                tile, quad ? ProjectileSpritePartDefinitions.BeamExplosionPalette : ProjectileSpritePartDefinitions.BombPalette, ProjectileSpritePartDefinitions.EffectPriority, flips), false);
        }
    }
    /// <summary>Yields the selected simple effect's one or four OAM parts.</summary>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Exposes the selected one- or four-part effect to non-generic callers.</summary>
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}
