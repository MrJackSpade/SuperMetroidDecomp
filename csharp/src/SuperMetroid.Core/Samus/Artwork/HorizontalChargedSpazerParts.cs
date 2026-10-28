using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Horizontal charged Spazer lanes share required spread distances. Center-first initial versus lower-first later ordering remains REQUIRED composition policy.</summary>
internal readonly struct HorizontalChargedSpazerParts(int phase) : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Four centered cells at startup, then three four-cell horizontal lanes.</summary>
    public int Count => ProjectileSpritePartDefinitions.ChargedSpazerHorizontalCells * (phase == 0 ? 1 : 3);
    /// <summary>Places one charged-Spazer strip cell on its selected horizontal lane.</summary>
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            int lane = index / ProjectileSpritePartDefinitions.ChargedSpazerHorizontalCells;
            int direction = phase <= 1 ? (lane == 0 ? 0 : lane == 1 ? 1 : -1) : 1 - lane;
            int distance = phase <= 1 ? SuperMetroid.Core.Samus.Projectiles.ProjectileWaveEnvelopeDefinitions.SpazerInitialAxialSpread : SuperMetroid.Core.Samus.Projectiles.ProjectileWaveEnvelopeDefinitions.AxialLobeDistance(phase - 2);
            int size = SpazerCompositionGeometryDefinitions.TileSize;
            int x = (ProjectileSpritePartDefinitions.ChargedSpazerHorizontalCells / 2 - 1 - index % ProjectileSpritePartDefinitions.ChargedSpazerHorizontalCells) * size;
            int y = SpazerCompositionGeometryDefinitions.HorizontalStripOriginY + direction * distance;
            return new(SpritePart.Create(x, y, false,
                ProjectileSpritePartDefinitions.ChargedSpazerHorizontalGlyph, ProjectileSpritePartDefinitions.PowerPalette, ProjectileSpritePartDefinitions.PowerPriority, 0), false);
        }
    }
    /// <summary>Yields the centered or three-lane charged-Spazer strip.</summary>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Exposes the chosen centered or three-lane charged Spazer strip.</summary>
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}
