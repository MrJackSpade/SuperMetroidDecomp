using F = SuperMetroid.Core.Enemies.Artwork.EnemyFrameId;
using P = SuperMetroid.Core.Enemies.Fireflea.FirefleaSegment;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Fireflea;

/// <summary>
/// Fireflea's glow loop as a typed animation (#641, #2369): two wingbeats at its brightest, one at each glow down to its
/// dimmest and one at each back up, each glow's wingbeat written once (#2477). Each
/// frame shows a <see cref="EnemyFrameId"/> and no beat raises a cue for its owner.
/// </summary>
internal static class FirefleaAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<FirefleaSegment, EnemyFrameId, NoAnimationCue> Segments { get; } = new(
        new Dictionary<FirefleaSegment, AnimationBeat<EnemyFrameId, NoAnimationCue>[]>
    {
        [P.BrightestWingbeat] = Wingbeat(F.FirefleaGlowBrightestWingsUp, F.FirefleaGlowBrightestWingsLevel, F.FirefleaGlowBrightestWingsDown),
        [P.BrighterWingbeat] = Wingbeat(F.FirefleaGlowBrighterWingsUp, F.FirefleaGlowBrighterWingsLevel, F.FirefleaGlowBrighterWingsDown),
        [P.BrightWingbeat] = Wingbeat(F.FirefleaGlowBrightWingsUp, F.FirefleaGlowBrightWingsLevel, F.FirefleaGlowBrightWingsDown),
        [P.MidWingbeat] = Wingbeat(F.FirefleaGlowMidWingsUp, F.FirefleaGlowMidWingsLevel, F.FirefleaGlowMidWingsDown),
        [P.DimWingbeat] = Wingbeat(F.FirefleaGlowDimWingsUp, F.FirefleaGlowDimWingsLevel, F.FirefleaGlowDimWingsDown),
        [P.DimmerWingbeat] = Wingbeat(F.FirefleaGlowDimmerWingsUp, F.FirefleaGlowDimmerWingsLevel, F.FirefleaGlowDimmerWingsDown),
        [P.DimmestWingbeat] = Wingbeat(F.FirefleaGlowDimmestWingsUp, F.FirefleaGlowDimmestWingsLevel, F.FirefleaGlowDimmestWingsDown),
    });

    /// <summary>How many times its wings beat at its brightest glow before it dims.</summary>
    private const int BrightestWingbeats = 2;

    /// <summary>
    /// The animation that starts at each segment an owner can start or restart at (#2369, #2477): its wings beat twice at its
    /// brightest glow, once at each glow as it dims to its dimmest and once at each as it brightens back, repeating.
    /// </summary>
    private static IReadOnlyDictionary<FirefleaSegment, Animation<FirefleaSegment>> StartingAt { get; } = new Dictionary<FirefleaSegment, Animation<FirefleaSegment>>
    {
        [P.BrightestWingbeat] = Animation<FirefleaSegment>.Looping(
        [
            Segments.Repeated(P.BrightestWingbeat, BrightestWingbeats),
            Segments.Whole(P.BrighterWingbeat),
            Segments.Whole(P.BrightWingbeat),
            Segments.Whole(P.MidWingbeat),
            Segments.Whole(P.DimWingbeat),
            Segments.Whole(P.DimmerWingbeat),
            Segments.Whole(P.DimmestWingbeat),
            Segments.Whole(P.DimmerWingbeat),
            Segments.Whole(P.DimWingbeat),
            Segments.Whole(P.MidWingbeat),
            Segments.Whole(P.BrightWingbeat),
            Segments.Whole(P.BrighterWingbeat),
        ]),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<FirefleaSegment> StartingWith(FirefleaSegment segment) => StartingAt[segment];

    /// <summary>
    /// One beat of its wings at one glow (#2477): <paramref name="up"/> for two updates, <paramref name="level"/> for one,
    /// <paramref name="down"/> for two and <paramref name="level"/> again for one.
    /// </summary>
    private static AnimationBeat<F, NoAnimationCue>[] Wingbeat(F up, F level, F down) =>
        [Show(up, 2), Show(level, 1), Show(down, 2), Show(level, 1)];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame(frame, duration);
}
