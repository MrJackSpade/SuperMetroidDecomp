using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>$93:F0FA/F194/F22E/F2CE and alternate groupsF36E/F408/F4A2/F542: centered startup cores share tile geometry; selected glyph/footprint/style remain REQUIRED.</summary>
internal readonly struct PlasmaStartupCoreParts(int group) : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Axial startup uses one core cell; diagonal startup uses a pair.</summary>
    public int Count => group % 4 < 2 ? 1 : 2;
    /// <summary>Places the selected charged-Plasma core cell or diagonal half.</summary>
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            int orientation = group % 4;
            if (orientation < 2)
                return new SpazerAxialStartupParts(group < 4 ? orientation : orientation + 4, 1)[index];
            if (group >= 4) return new AlternateDiagonalStartupParts(orientation == 3, 0)[index];
            int size = SpazerCompositionGeometryDefinitions.TileSize;
            bool reflected = orientation == 3;
            int x = -index * size;
            if (reflected) x = -size - x;
            return new(SnesSpritemapXWord.Create(x, false), SnesSpritemapYByte.Create(-size / 2),
                SnesObjAttributeWord.Create(ProjectileSpritePartDefinitions.SpazerDiagonalGlyph - index, ProjectileSpritePartDefinitions.PowerPalette, ProjectileSpritePartDefinitions.PowerPriority,
                    reflected ? SnesTileFlipFlags.Horizontal : 0), false);
        }
    }
    /// <summary>Yields the selected startup core's one or two OAM entries.</summary>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Exposes the chosen one- or two-part Plasma startup core.</summary>
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}
