using SuperMetroid.Core.Animation;
using F = SuperMetroid.Core.Samus.Artwork.SamusAtmosphereFrame;
using K = SuperMetroid.Core.Samus.Drawing.SamusParticleKind;

namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>
/// The animation of each of Samus's particles (#2388): its pictures, each with the updates it shows for, played once
/// through the shared animation player, after which the particle is gone.
/// </summary>
/// <remarks>
/// Each beat holds the updates its picture shows for, so a particle's play and the waits its breath bubbles and lava
/// spray hold are both read from them. A particle shown at once shows its first picture for its start's own updates
/// instead.
/// </remarks>
internal static class SamusParticleAnimations
{
    /// <summary>Every particle's pictures, each with the updates it shows for.</summary>
    internal static AnimationSegments<K, F, NoAnimationCue> Segments { get; } = new(
        new Dictionary<K, AnimationBeat<F, NoAnimationCue>[]>
        {
            [K.SurfaceSplash] =
            [
                Show(F.FootstepSplashing, 3), Show(F.FootstepSpraying, 3), Show(F.FootstepScattering, 3), Show(F.FootstepSettling, 3),
            ],
            [K.LavaSpray] =
            [
                Show(F.LavaAndDustPuff, 2), Show(F.LavaAndDustBillowing, 2), Show(F.LavaAndDustThinning, 2), Show(F.LavaAndDustFading, 2),
            ],
            [K.Dust] =
            [
                Show(F.LavaAndDustPuff, 3), Show(F.LavaAndDustBillowing, 3), Show(F.LavaAndDustThinning, 4), Show(F.LavaAndDustFading, 5),
            ],
            [K.Bubbles] =
            [
                Show(F.BubblesTightCluster, 5), Show(F.BubblesCluster, 5), Show(F.BubblesLoosening, 5), Show(F.BubblesSpreading, 5),
                Show(F.BubblesScattering, 5), Show(F.BubblesScattered, 5), Show(F.BubblesDispersed, 5), Show(F.BubblesFading, 5),
            ],
            [K.DivingSplash] =
            [
                Show(F.DivingSplashRing, 2), Show(F.DivingSplashSpoutsRising, 2), Show(F.DivingSplashSpoutsHigh, 2),
                Show(F.DivingSplashSpoutsHigher, 3), Show(F.DivingSplashSpoutsHighest, 3), Show(F.DivingSplashSpoutsSinking, 3),
                Show(F.DivingSplashSplashdown, 5), Show(F.DivingSplashDroplets, 5), Show(F.DivingSplashDropletsFading, 6),
            ],
        });

    /// <summary>Each particle's animation: its pictures played once, after which it is gone.</summary>
    internal static IReadOnlyDictionary<K, Animation<K>> Animations { get; } = new Dictionary<K, Animation<K>>
    {
        [K.SurfaceSplash] = new([Segments.Whole(K.SurfaceSplash)], AnimationEnd.Stops),
        [K.LavaSpray] = new([Segments.Whole(K.LavaSpray)], AnimationEnd.Stops),
        [K.Dust] = new([Segments.Whole(K.Dust)], AnimationEnd.Stops),
        [K.Bubbles] = new([Segments.Whole(K.Bubbles)], AnimationEnd.Stops),
        [K.DivingSplash] = new([Segments.Whole(K.DivingSplash)], AnimationEnd.Stops),
    };

    /// <summary>Shows <paramref name="frame"/> for <paramref name="updates"/> updates.</summary>
    private static AnimationBeat<F, NoAnimationCue> Show(F frame, int updates) => new AnimationBeat<F, NoAnimationCue>.Frame(frame, updates);
}
