using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>$93:AF4C..B0C7 and B672..B7ED: two centered glyphs followed by four paired vertical displacements, each with swapped lobe glyphs.</summary>
internal readonly struct VerticalChargedWaveParts(int phase) : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Two four-cell cores grow into eight-cell paired vertical lobes.</summary>
    public int Count => phase < 2 ? 4 : 8;
    /// <summary>Places one reflected quadrant in the center or displaced lobe pair.</summary>
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            int lobe = index / 4, quadrant = index % 4;
            int column = quadrant % 2, row = quadrant / 2;
            int centerY = phase < 2 ? 0 : (lobe == 0 ? 1 : -1) * SuperMetroid.Core.Samus.Projectiles.ProjectileWaveEnvelope.AxialLobeDistance((phase - 2) / 2);
            int tile = ProjectileSpritePartDefinitions.ChargedBeamTile + (phase < 2 ? 1 - phase : (phase + lobe) % 2);
            var flips = (column == 0 ? TileFlip.Horizontal : 0) |
                (row == 0 ? TileFlip.Vertical : 0);
            return new(SpritePart.Create(Offset16<ScreenPixels>.Of(-column * 8, centerY - row * 8), false,
                tile, ProjectileSpritePartDefinitions.PowerPalette, ProjectileSpritePartDefinitions.PowerPriority, flips), false);
        }
    }
    /// <summary>Yields the selected vertical charged-Wave quadrants in OAM order.</summary>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Exposes the vertical charged-Wave quadrant order through the collection interface.</summary>
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}
