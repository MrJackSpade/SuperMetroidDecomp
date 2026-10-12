using System.Collections;

namespace SuperMetroid.Core.Assets;

/// <summary>The seven unique Ceres destruction and Zebes reveal backdrop spritemaps, valued by their bank-$8C pointer.</summary>
public enum CeresDestructionBackdrop : ushort
{
    /// <summary>8C:909D, SpaceSpritemaps_CeresUnderAttackLargeAsteroids.</summary>
    LargeAsteroids = 0x909d,
    /// <summary>8C:9558, SpaceSpritemaps_Zebes.</summary>
    Planet = 0x9558,
    /// <summary>8C:9654, SpaceSpritemaps_PlanetZebes title.</summary>
    Title = 0x9654,
    /// <summary>8C:975E, SpaceSpritemaps_ZebesStars2, upper-left sheet.</summary>
    UpperLeftStars = 0x975e,
    /// <summary>8C:979C, SpaceSpritemaps_ZebesStars3, upper-right sheet.</summary>
    UpperRightStars = 0x979c,
    /// <summary>8C:97BC, SpaceSpritemaps_ZebesStars4, lower-left sheet.</summary>
    LowerLeftStars = 0x97bc,
    /// <summary>8C:97D2, SpaceSpritemaps_ZebesStars5, lower-right sheet.</summary>
    LowerRightStars = 0x97d2,
}

/// <summary>Named Ceres destruction backdrops and bounded blast-frame catalogs.
/// Native OAM records have a two-byte count and five bytes per part.</summary>
public static class CeresDestructionSpriteDefinitions
{
    /// <summary>8C:98D2, SpaceSpritemaps_CeresExplosionFrame1, four single-part records.</summary>
    private const ushort LargeStart = 0x98d2;
    /// <summary>8C:98EE, SpaceSpritemaps_CeresFinalExplosionFrame1, six consecutive records.</summary>
    private const ushort StationStart = 0x98ee;

    internal static ushort SmallFrame(int frame) => IntroMotherBrainExplosionSpriteDefinitions.FramePointer(false, frame);

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
    /// <summary>Catalog order of the seven backdrops ahead of the blast frames.</summary>
    private static readonly CeresDestructionBackdrop[] BackdropOrder =
    [
        CeresDestructionBackdrop.LargeAsteroids, CeresDestructionBackdrop.Planet, CeresDestructionBackdrop.Title,
        CeresDestructionBackdrop.UpperLeftStars, CeresDestructionBackdrop.UpperRightStars,
        CeresDestructionBackdrop.LowerLeftStars, CeresDestructionBackdrop.LowerRightStars,
    ];

    private static CeresDestructionSpriteFrameDefinition BackdropFrame(CeresDestructionBackdrop backdrop) => backdrop switch
    {
        CeresDestructionBackdrop.LargeAsteroids => new("station-under-attack-large-asteroid", (ushort)backdrop, 19, backdrop),
        CeresDestructionBackdrop.Planet => new("planet-zebes", (ushort)backdrop, 50, backdrop),
        CeresDestructionBackdrop.Title => new("planet-zebes-title", (ushort)backdrop, 11, backdrop),
        CeresDestructionBackdrop.UpperLeftStars => new("zebes-stars-upper-left", (ushort)backdrop, 12, backdrop),
        CeresDestructionBackdrop.UpperRightStars => new("zebes-stars-upper-right", (ushort)backdrop, 6, backdrop),
        CeresDestructionBackdrop.LowerLeftStars => new("zebes-stars-lower-left", (ushort)backdrop, 4, backdrop),
        CeresDestructionBackdrop.LowerRightStars => new("zebes-stars-lower-right", (ushort)backdrop, 7, backdrop),
        _ => throw new InvalidOperationException($"Undefined {nameof(CeresDestructionBackdrop)} {(int)backdrop}."),
    };

    private sealed class FrameList : IReadOnlyList<CeresDestructionSpriteFrameDefinition>
    {
        public int Count => 23;
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
                return BackdropFrame(BackdropOrder[index]);
            }
        }
        public IEnumerator<CeresDestructionSpriteFrameDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}