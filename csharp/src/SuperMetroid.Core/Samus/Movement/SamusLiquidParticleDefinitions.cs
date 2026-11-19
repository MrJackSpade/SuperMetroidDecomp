using SuperMetroid.Core.Samus.Drawing;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// Where Samus's liquid and landing particles start relative to her: landing dust and splashes, the splashes of a shallow
/// entry into water, a diving splash's first step, breath bubbles, the lava spray and running footsteps (#2118).
/// </summary>
internal static class SamusLiquidParticleDefinitions
{
    /// <summary>
    /// A landing's two splashes: the right one four pixels right of Samus and four above her last occupied pixel, the
    /// left one seven pixels left of it.
    /// </summary>
    internal static SamusLandingParticlePair LandingSplashes { get; } = new(new Offset16<RoomPixels>(4, -4), Separation: 7);

    /// <summary>
    /// A landing's two dust clouds: the right one eight pixels right of Samus at her last occupied pixel, the left one
    /// sixteen pixels left of it.
    /// </summary>
    internal static SamusLandingParticlePair LandingDust { get; } = new(new Offset16<RoomPixels>(8, 0), Separation: 16);

    /// <summary>
    /// A shallow entry into water's two splashes, from Samus's centre and last occupied pixel: four pixels right and three
    /// left, both four pixels above.
    /// </summary>
    internal static IReadOnlyList<Offset16<RoomPixels>> ShallowSplashes { get; } = [new(4, -4), new(-3, -4)];

    /// <summary>The diving splash's first step, shown for two updates.</summary>
    internal static SamusParticleStart DivingSplashStart { get; } = SamusParticleStart.Shown(2);

    /// <summary>The pixels above Samus's top whose height is tested against the water's surface before a breath bubble forms.</summary>
    internal const int BreathTestAboveTop = 24;

    /// <summary>The pixels below Samus's top at which a breath bubble forms.</summary>
    internal const int BubbleBelowTop = 6;

    /// <summary>The pixels either side of Samus at which the lava spray's two outer particles start.</summary>
    internal const int LavaSprayOuterOffset = 6;

    /// <summary>
    /// The pixels a lava spray particle drifts sideways on each update it shows: the first two drift right by it, the last
    /// two left.
    /// </summary>
    internal const int LavaSprayDrift = 1;

    /// <summary>The first particle of a running footstep pair and the middle particles of a lava spray show one update late.</summary>
    internal static SamusParticleStart OneUpdateLate { get; } = SamusParticleStart.Hidden(2);

    /// <summary>Splashes, bubbles, spray and dust otherwise show their first step for three updates.</summary>
    internal static SamusParticleStart AtOnce { get; } = SamusParticleStart.Shown(3);

    /// <summary>The pixels ahead of a running Samus at which her leading footstep particle starts.</summary>
    internal const int FootstepLeadingAhead = 12;

    /// <summary>The pixels behind a running Samus at which her trailing footstep particle starts.</summary>
    internal const int FootstepTrailingBehind = 8;

    /// <summary>The pixels below Samus's centre at which both footstep particles start.</summary>
    internal const int FootstepBelowCentre = 16;
}
