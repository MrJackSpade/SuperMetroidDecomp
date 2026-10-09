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

    private enum Visual
    {
        /// <summary>Starfield spritemap selected by the Ceres stars program.</summary>
        Stars,

        /// <summary>Large asteroid spritemap used during the station explosion sequence.</summary>
        LargeAsteroids,

        /// <summary>Damaged Ceres station spritemap shown while the station is under attack.</summary>
        StationUnderAttack,

        /// <summary>Small asteroid spritemap selected by the Ceres flight stream.</summary>
        SmallAsteroids,

        /// <summary>Even-frame drawing of the alternating purple vortex.</summary>
        VortexEven,

        /// <summary>Odd-frame drawing of the alternating purple vortex.</summary>
        VortexOdd
    }

    /// <summary>Provides indexed access to the six visual selections in their published catalog order.</summary>
    private sealed class FrameList : IReadOnlyList<CeresFlightSpriteFrameDefinition>
    {
        /// <summary>Number of named Ceres flight visual selections.</summary>
        public int Count => 6;

        /// <summary>Builds the descriptive asset definition for a catalog position.</summary>
        /// <param name="index">Zero-based visual position: stars, large asteroids, station, small asteroids, then the two vortex frames.</param>
        /// <returns>The native spritemap identity and stock OAM count for that selection.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The index is outside the six catalog entries.</exception>
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
        /// <summary>Enumerates the six visual definitions in catalog order.</summary>
        /// <returns>An enumerator over the descriptive Ceres flight frame definitions.</returns>
        public IEnumerator<CeresFlightSpriteFrameDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
