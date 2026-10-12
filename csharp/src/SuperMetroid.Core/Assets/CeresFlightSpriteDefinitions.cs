using System.Collections;

namespace SuperMetroid.Core.Assets;

/// <summary>Six named visual selections made by the five Ceres flight instruction
/// streams. The vortex stream alternates two drawings; the other streams each
/// select one drawing. Catalog order and published asset names remain stable.</summary>
public static class CeresFlightSpriteDefinitions
{
    /// <summary>8C:9478, SpaceSpritemaps_CeresStars, selected by 8B:CDA3.</summary>
    internal const ushort Stars = 0x9478;
    /// <summary>8C:94F7, SpaceSpritemaps_CeresExplosionLargeAsteroids, selected by 8B:CE4B.</summary>
    internal const ushort LargeAsteroids = 0x94f7;
    /// <summary>8C:9150, SpaceSpritemaps_CeresUnderAttack, selected by 8B:CC47.</summary>
    internal const ushort StationUnderAttack = 0x9150;
    /// <summary>8C:90FE, SpaceSpritemaps_CeresSmallAsteroids, selected by 8B:CC4F.</summary>
    internal const ushort SmallAsteroids = 0x90fe;
    /// <summary>8C:8FE7, SpaceSpritemaps_CeresPurpleVortexFrame1, first drawing in 8B:CC57.</summary>
    internal const ushort VortexEven = 0x8fe7;
    /// <summary>8C:93D1, SpaceSpritemaps_CeresPurpleVortexFrame2, second drawing in 8B:CC57.</summary>
    internal const ushort VortexOdd = 0x93d1;

    /// <summary>Immutable six-name bank-$8C catalog in stable stars, large-asteroid, station, small-asteroid, and two-vortex order; maps editable composition names to native visual identities and descriptive stock OAM counts, not instruction timing.</summary>
    public static IReadOnlyList<CeresFlightSpriteFrameDefinition> Frames { get; } = new FrameList();

    private enum Visual { Stars, LargeAsteroids, StationUnderAttack, SmallAsteroids, VortexEven, VortexOdd }

    private sealed class FrameList : IReadOnlyList<CeresFlightSpriteFrameDefinition>
    {
        public int Count => 6;
        public CeresFlightSpriteFrameDefinition this[int index]
        {
            get
            {
                if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
                return (Visual)index switch
                {
                    Visual.Stars => new("stars", Stars, 25),
                    Visual.LargeAsteroids => new("large-asteroid", LargeAsteroids, 19),
                    Visual.StationUnderAttack => new("station-under-attack", StationUnderAttack, 41),
                    Visual.SmallAsteroids => new("small-asteroid", SmallAsteroids, 16),
                    Visual.VortexEven => new("vortex-even", VortexEven, 36),
                    Visual.VortexOdd => new("vortex-odd", VortexOdd, 33),
                    _ => throw new ArgumentOutOfRangeException(nameof(index)),
                };
            }
        }
        public IEnumerator<CeresFlightSpriteFrameDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
