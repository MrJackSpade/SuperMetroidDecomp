using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>$93:EC54..ED88 and ED9E..EDE0 mirror one glyph across a centered two-by-two cell square; Power uses column order and Ice row order.</summary>
internal readonly struct ChargedBeamParts(int phase, bool ice) : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>A charged Power or Ice body uses four reflected cells.</summary>
    public int Count => 4;
    /// <summary>Maps one quadrant to its charged glyph and phase-dependent reflection.</summary>
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= 4) throw new ArgumentOutOfRangeException(nameof(index));
            int column = ice ? index % 2 : index / 2;
            int row = ice ? index / 2 : index % 2;
            int tile = ProjectileSpritePartDefinitions.ChargedBeamTile + (ice ? phase % 2 : phase / 8);
            var flips = (column == 0 ? TileFlip.Horizontal : 0) |
                (row == 0 ? TileFlip.Vertical : 0);
            return new(SpritePart.Create(Offset16<ScreenPixels>.Of(-column * 8, -row * 8), false,
                tile, ProjectileSpritePartDefinitions.PowerPalette, ProjectileSpritePartDefinitions.PowerPriority, flips), false);
        }
    }
    /// <summary>Yields all four charged-beam quadrants in native OAM order.</summary>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Exposes the charged beam quadrants without changing their native draw order.</summary>
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}
