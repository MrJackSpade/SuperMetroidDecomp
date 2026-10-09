using System.Collections;

namespace SuperMetroid.Core.Assets;

/// <summary>Named Ceres destruction backdrops and bounded blast-frame catalogs.
/// Native OAM records have a two-byte count and five bytes per part.</summary>
public static class CeresDestructionSpriteDefinitions
{
    /// <summary>8C:909D, SpaceSpritemaps_CeresUnderAttackLargeAsteroids.</summary>
    internal const ushort LargeAsteroids = 0x909d;
    /// <summary>8C:9558, SpaceSpritemaps_Zebes.</summary>
    internal const ushort Planet = 0x9558;
    /// <summary>8C:9654, SpaceSpritemaps_PlanetZebes title.</summary>
    internal const ushort Title = 0x9654;
    /// <summary>8C:975E, SpaceSpritemaps_ZebesStars2, upper-left sheet.</summary>
    internal const ushort UpperLeftStars = 0x975e;
    /// <summary>8C:979C, SpaceSpritemaps_ZebesStars3, upper-right sheet.</summary>
    internal const ushort UpperRightStars = 0x979c;
    /// <summary>8C:97BC, SpaceSpritemaps_ZebesStars4, lower-left sheet.</summary>
    internal const ushort LowerLeftStars = 0x97bc;
    /// <summary>8C:97D2, SpaceSpritemaps_ZebesStars5, lower-right sheet.</summary>
    internal const ushort LowerRightStars = 0x97d2;
    /// <summary>8C:98D2, SpaceSpritemaps_CeresExplosionFrame1, four single-part records.</summary>
    private const ushort LargeStart = 0x98d2;
    /// <summary>8C:98EE, SpaceSpritemaps_CeresFinalExplosionFrame1, six consecutive records.</summary>
    private const ushort StationStart = 0x98ee;

    /// <summary>Gets the bank-$8C pointer for one of the small Ceres destruction blast frames.</summary>
    /// <param name="frame">Zero-based frame index in the small-blast sequence.</param>
    /// <returns>The native spritemap pointer for the selected frame.</returns>
    internal static ushort SmallFrame(int frame) => IntroMotherBrainExplosionSpriteDefinitions.FramePointer(false, frame);

    /// <summary>Gets a pointer into the contiguous four-record large-asteroid blast sequence.</summary>
    /// <param name="frame">Zero-based frame index from zero through three.</param>
    /// <returns>The bank-$8C address of the selected one-part OAM record.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside the four-frame sequence.</exception>
    internal static ushort LargeFrame(int frame)
    {
        if ((uint)frame >= 4) throw new ArgumentOutOfRangeException(nameof(frame));
        return (ushort)(LargeStart + 7 * frame);
    }

    /// <summary>Frames zero through two use one part per reflected quadrant;
    /// frame four has two lobes per quadrant; frames three and five add two tips
    /// to a core per quadrant. This counts OAM topology, not visible pixel area.</summary>
    internal static int StationPartCount(int frame)
    {
        if ((uint)frame >= 6) throw new ArgumentOutOfRangeException(nameof(frame));
        return 4 * (frame < 3 ? 1 : frame == 4 ? 2 : 3);
    }

    /// <summary>Calculates the pointer to a station-destruction frame with variable OAM record size.</summary>
    /// <param name="frame">Zero-based frame index from zero through five.</param>
    /// <returns>The bank-$8C address after accounting for all preceding records' part counts.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside the six-frame sequence.</exception>
    internal static ushort StationFrame(int frame)
    {
        if ((uint)frame >= 6) throw new ArgumentOutOfRangeException(nameof(frame));
        int pointer = StationStart;
        for (int previous = 0; previous < frame; previous++)
            pointer += 2 + 5 * StationPartCount(previous);
        return (ushort)pointer;
    }

    /// <summary>Immutable 23-name bank-$8C visual catalog: seven backdrops, six small blasts, four large blasts, then six station blasts; original OAM part counts describe stock geometry without restricting edited compositions or defining animation timing.</summary>
    public static IReadOnlyList<CeresDestructionSpriteFrameDefinition> Frames { get; } = new FrameList();
    /// <summary>Indices mapping the seven backdrop entries at the start of the frame catalog to native assets.</summary>
    private enum Backdrop
    {
        /// <summary>Ceres under attack with its large asteroid field.</summary>
        LargeAsteroids,
        /// <summary>The planet Zebes backdrop without title lettering.</summary>
        Planet,
        /// <summary>The planet Zebes backdrop with the title treatment.</summary>
        Title,
        /// <summary>Upper-left quadrant of the star overlay sheet.</summary>
        UpperLeftStars,
        /// <summary>Upper-right quadrant of the star overlay sheet.</summary>
        UpperRightStars,
        /// <summary>Lower-left quadrant of the star overlay sheet.</summary>
        LowerLeftStars,
        /// <summary>Lower-right quadrant of the star overlay sheet.</summary>
        LowerRightStars
    }

    /// <summary>Read-only view assembling backdrop, small-blast, large-blast, and station-blast definitions.</summary>
    private sealed class FrameList : IReadOnlyList<CeresDestructionSpriteFrameDefinition>
    {
        /// <summary>Total number of named backdrop and explosion-frame entries.</summary>
        public int Count => 23;

        /// <summary>Gets the backdrop or blast definition at its stable catalog position.</summary>
        /// <param name="index">Zero-based position in the 23-entry catalog.</param>
        /// <exception cref="ArgumentOutOfRangeException">The index is outside the catalog.</exception>
        public CeresDestructionSpriteFrameDefinition this[int index]
        {
            get
            {
                if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
                if (index >= 17)
                {
                    int frame = index - 17;
                    return new($"station-blast-{frame}", StationFrame(frame), StationPartCount(frame));
                }
                if (index >= 13)
                {
                    int frame = index - 13;
                    return new($"large-blast-{frame}", LargeFrame(frame), 1);
                }
                if (index >= 7)
                {
                    int frame = index - 7;
                    return new($"small-blast-{frame}", SmallFrame(frame), frame < 2 ? 1 : 4);
                }
                return (Backdrop)index switch
                {
                    Backdrop.LargeAsteroids => new("station-under-attack-large-asteroid", LargeAsteroids, 19),
                    Backdrop.Planet => new("planet-zebes", Planet, 50),
                    Backdrop.Title => new("planet-zebes-title", Title, 11),
                    Backdrop.UpperLeftStars => new("zebes-stars-upper-left", UpperLeftStars, 12),
                    Backdrop.UpperRightStars => new("zebes-stars-upper-right", UpperRightStars, 6),
                    Backdrop.LowerLeftStars => new("zebes-stars-lower-left", LowerLeftStars, 4),
                    Backdrop.LowerRightStars => new("zebes-stars-lower-right", LowerRightStars, 7),
                    _ => throw new ArgumentOutOfRangeException(nameof(index)),
                };
            }
        }
        /// <summary>Enumerates the catalog entries in their documented backdrop-then-blast order.</summary>
        public IEnumerator<CeresDestructionSpriteFrameDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
