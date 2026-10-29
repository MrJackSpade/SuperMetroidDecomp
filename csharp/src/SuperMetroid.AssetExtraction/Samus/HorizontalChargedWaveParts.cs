using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>$93:B394-B4E3 and BABA-BC09: paired horizontal lobes reuse the four required axial distances and alternating glyphs. The independently chosen left clockwise/right row-order traversal remains REQUIRED artwork policy.</summary>
internal readonly struct HorizontalChargedWaveParts(int phase) : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Two horizontal lobes each contain four reflected quadrants.</summary>
    public int Count => 8;
    /// <summary>Places one quadrant using the left lobe's clockwise or right lobe's row order.</summary>
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            int lobe = index / 4, corner = index % 4;
            bool left = lobe == 0;
            // The left lobe follows upper-right,lower-right,lower-left,upper-left;
            // the right lobe follows the native row traversal. Selection of these
            // two orders is still a required source input, not a derived choice.
            int column = left ? corner / 2 : corner % 2;
            int row = left ? 1 - ((corner ^ (corner >> 1)) & 1) : corner / 2;
            int centerX = (left ? -1 : 1) * SuperMetroid.Core.Samus.Projectiles.ProjectileWaveEnvelope.AxialLobeDistance(phase / 2);
            int tile = ProjectileSpritePartDefinitions.ChargedBeamTile + 1 - (phase + lobe) % 2;
            var flips = (column == 0 ? TileFlip.Horizontal : 0) |
                (row == 0 ? TileFlip.Vertical : 0);
            return new(SpritePart.Create(Offset16<ScreenPixels>.Of(centerX - column * 8, -row * 8), false,
                tile, ProjectileSpritePartDefinitions.PowerPalette, ProjectileSpritePartDefinitions.PowerPriority, flips), false);
        }
    }
    /// <summary>Yields both horizontal lobes in their native asymmetric order.</summary>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Exposes the asymmetric horizontal Wave lobes in their draw order.</summary>
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}
