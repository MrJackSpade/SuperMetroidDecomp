using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>The first four diagonal spreads atD124/D270/D3BC/D508 repeat each seed on center and opposite perpendicular lanes. Distances6/9/11/12 share floor(3*required axial distance/4); spacing/projection and phase-selection inputs remain REQUIRED.</summary>
internal readonly struct SpazerDiagonalSpreadParts(int pose, int phase) : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Three copies of a four-cell diagonal seed form twelve OAM parts.</summary>
    public int Count => 12;
    /// <summary>Places one seed cell on its center or perpendicular spread lane.</summary>
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            var seed = new SpazerSeedParts(pose)[index % 4];
            int lane = index / 4;
            int direction = lane == 0 ? 0 : lane == 1 ? -1 : 1;
            int distance = ProjectileSpritePartDefinitions.WaveDiagonalNumerator * SuperMetroid.Core.Samus.Projectiles.ProjectileWaveEnvelopeDefinitions.AxialLobeDistance(phase) / ProjectileSpritePartDefinitions.WaveDiagonalDenominator;
            int dx = direction * distance * (pose >= 2 ? -1 : 1);
            int dy = direction * distance * (pose is 1 or 2 ? -1 : 1);
            return new(seed.Part.At(Offset16<ScreenPixels>.Of(seed.Part.Offset.X + dx, seed.Part.Offset.Y + dy)), seed.InheritPalette);
        }
    }
    /// <summary>Yields all three diagonal lanes in native order.</summary>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Exposes all three diagonal Spazer lanes in cartridge order.</summary>
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}
