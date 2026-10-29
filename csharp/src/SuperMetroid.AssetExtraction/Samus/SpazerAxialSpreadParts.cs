using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Repeated axial seed lanes. Native vertical, initial-left and later horizontal traversal policies remain REQUIRED composition choices.</summary>
internal readonly struct SpazerAxialSpreadParts(int pose, int phase) : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Three two-cell axial lanes form six OAM parts.</summary>
    public int Count => 6;
    /// <summary>Places one axial seed cell on the selected reflected spread lane.</summary>
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            int distance = phase == 0 ? SuperMetroid.Core.Samus.Projectiles.ProjectileWaveEnvelopeDefinitions.SpazerInitialAxialSpread : SuperMetroid.Core.Samus.Projectiles.ProjectileWaveEnvelope.AxialLobeDistance(phase - 1);
            bool vertical = pose is 4 or 6;
            int seedIndex, lane;
            if (vertical)
            {
                seedIndex = index % 2;
                lane = index / 2 == 0 ? 1 : index / 2 == 1 ? -1 : 0;
            }
            else if (phase == 0)
            {
                seedIndex = index % 2;
                lane = 1 - index / 2;
            }
            else
            {
                seedIndex = index / 3;
                lane = seedIndex == 0 ? index % 3 - 1 : 1 - index % 3;
            }
            var seed = new SpazerSeedParts(pose)[seedIndex];
            return new(seed.Part.At(Offset16<ScreenPixels>.Of(seed.Part.Offset.X + (vertical ? lane * distance : 0), seed.Part.Offset.Y + (vertical ? 0 : lane * distance))), seed.InheritPalette);
        }
    }
    /// <summary>Yields all six axial spread cells in native traversal order.</summary>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Exposes the six axial Spazer cells to non-generic OAM readers.</summary>
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}
