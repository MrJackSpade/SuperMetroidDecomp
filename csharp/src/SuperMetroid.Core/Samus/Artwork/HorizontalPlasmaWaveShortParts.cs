using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Centered horizontal strip or opposite displaced copies. Selected length/glyph, lobe order and four spread distances remain REQUIRED; repeated cell geometry calculates.</summary>
internal readonly struct HorizontalPlasmaWaveShortParts(int phase) : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Four centered cells at startup or eight cells in the two displaced lobes.</summary>
    public int Count => ProjectileSpritePartDefinitions.PlasmaWaveShortCells * (phase == 0 ? 1 : 2);
    /// <summary>Places one PlasmaWave strip cell in the center or selected lobe.</summary>
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            int size = SpazerCompositionGeometryDefinitions.TileSize;
            int x = (ProjectileSpritePartDefinitions.PlasmaWaveShortCells / 2 - 1 - index % ProjectileSpritePartDefinitions.PlasmaWaveShortCells) * size;
            int direction = phase == 0 ? 0 : index < ProjectileSpritePartDefinitions.PlasmaWaveShortCells ? 1 : -1;
            int y = -size / 2 + (phase == 0 ? 0 : direction * SuperMetroid.Core.Samus.Projectiles.ProjectileWaveEnvelopeDefinitions.AxialLobeDistance(phase - 1));
            return new(SpritePart.Create(Offset16<ScreenPixels>.Of(x, y), false,
                ProjectileSpritePartDefinitions.SpazerHorizontalGlyph, ProjectileSpritePartDefinitions.PowerPalette, ProjectileSpritePartDefinitions.PowerPriority, 0), false);
        }
    }
    /// <summary>Yields the short horizontal core or its paired spread strips.</summary>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Exposes the short Plasma core or its two spread strips in draw order.</summary>
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}
