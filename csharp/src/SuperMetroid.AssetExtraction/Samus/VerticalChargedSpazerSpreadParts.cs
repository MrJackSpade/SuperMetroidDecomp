using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Three descending columns. Side placement measures a required distance to the near cell edge and reflects across X=0; this placement policy and right/left/center traversal remain REQUIRED choices.</summary>
internal readonly struct VerticalChargedSpazerSpreadParts(int phase) : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Three four-cell vertical columns make twelve OAM parts.</summary>
    public int Count => 3 * ProjectileSpritePartDefinitions.ChargedSpazerVerticalCells;
    /// <summary>Places one cell in the right, left or center vertical column.</summary>
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            int size = SpazerCompositionGeometryDefinitions.TileSize;
            int lane = index / ProjectileSpritePartDefinitions.ChargedSpazerVerticalCells;
            int distance = SuperMetroid.Core.Samus.Projectiles.ProjectileWaveEnvelope.AxialLobeDistance(phase);
            int x = lane == 0 ? distance : lane == 1 ? -size - distance : -size / 2;
            int y = (ProjectileSpritePartDefinitions.ChargedSpazerVerticalCells / 2 - 1 - index % ProjectileSpritePartDefinitions.ChargedSpazerVerticalCells) * size;
            return new(SpritePart.Create(Offset16<ScreenPixels>.Of(x, y), false,
                ProjectileSpritePartDefinitions.ChargedSpazerVerticalGlyph, ProjectileSpritePartDefinitions.PowerPalette, ProjectileSpritePartDefinitions.PowerPriority, 0), false);
        }
    }
    /// <summary>Yields the three selected vertical charged-Spazer columns.</summary>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Exposes the three vertical charged-Spazer columns in draw order.</summary>
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}
