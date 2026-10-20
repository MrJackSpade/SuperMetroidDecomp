using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>One/two-cell centered startup strips; native group glyph/reflection selection and traversal remain REQUIRED composition inputs, while adjacency and centering calculate.</summary>
internal readonly struct SpazerAxialStartupParts(int group, int length) : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>One- or two-cell startup strip length chosen by the native phase.</summary>
    public int Count => length;
    /// <summary>Places an axial startup cell with its group-specific glyph and reflection.</summary>
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            int size = SpazerCompositionGeometryDefinitions.TileSize;
            bool vertical = (group & 1) != 0;
            bool reflected = group is 2 or 3;
            int along = -length * size / 2 + index * size;
            if (vertical != reflected) along = -size - along;
            int x = vertical ? -size / 2 : along;
            int y = vertical ? along : SpazerCompositionGeometryDefinitions.HorizontalStripOriginY;
            int glyph = group >= 4
                ? (vertical ? ProjectileSpritePartDefinitions.ChargedSpazerVerticalGlyph : ProjectileSpritePartDefinitions.ChargedSpazerHorizontalGlyph)
                : (vertical ? ProjectileSpritePartDefinitions.SpazerVerticalGlyph : ProjectileSpritePartDefinitions.SpazerHorizontalGlyph);
            var flips = reflected ? (vertical ? SnesTileFlipFlags.Vertical : SnesTileFlipFlags.Horizontal) : 0;
            return new(SnesSpritemapXWord.Create(x, false), SnesSpritemapYByte.Create(y),
                SnesObjAttributeWord.Create(glyph, ProjectileSpritePartDefinitions.PowerPalette, ProjectileSpritePartDefinitions.PowerPriority, flips), false);
        }
    }
    /// <summary>Yields the selected centered axial startup strip.</summary>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Exposes the selected axial startup cells as a non-generic sequence.</summary>
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}
