using P = SuperMetroid.Core.Enemies.Fireflea.FirefleaSegment;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Fireflea;

/// <summary>
/// Fireflea's 52-frame glow loop as a typed animation (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and no beat raises a cue for its owner.
/// </summary>
internal static class FirefleaAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<FirefleaSegment, EnemyFrameId, NoAnimationCue> Segments { get; } = new(
        new Dictionary<FirefleaSegment, AnimationBeat<EnemyFrameId, NoAnimationCue>[]>
    {
        [P.Loop] =
        [
            Show(EnemyFrameId.FirefleaGlowBrightestWingsUp, 2),
            Show(EnemyFrameId.FirefleaGlowBrightestWingsLevel, 1),
            Show(EnemyFrameId.FirefleaGlowBrightestWingsDown, 2),
            Show(EnemyFrameId.FirefleaGlowBrightestWingsLevel, 1),
            Show(EnemyFrameId.FirefleaGlowBrightestWingsUp, 2),
            Show(EnemyFrameId.FirefleaGlowBrightestWingsLevel, 1),
            Show(EnemyFrameId.FirefleaGlowBrightestWingsDown, 2),
            Show(EnemyFrameId.FirefleaGlowBrightestWingsLevel, 1),
            Show(EnemyFrameId.FirefleaGlowBrighterWingsUp, 2),
            Show(EnemyFrameId.FirefleaGlowBrighterWingsLevel, 1),
            Show(EnemyFrameId.FirefleaGlowBrighterWingsDown, 2),
            Show(EnemyFrameId.FirefleaGlowBrighterWingsLevel, 1),
            Show(EnemyFrameId.FirefleaGlowBrightWingsUp, 2),
            Show(EnemyFrameId.FirefleaGlowBrightWingsLevel, 1),
            Show(EnemyFrameId.FirefleaGlowBrightWingsDown, 2),
            Show(EnemyFrameId.FirefleaGlowBrightWingsLevel, 1),
            Show(EnemyFrameId.FirefleaGlowMidWingsUp, 2),
            Show(EnemyFrameId.FirefleaGlowMidWingsLevel, 1),
            Show(EnemyFrameId.FirefleaGlowMidWingsDown, 2),
            Show(EnemyFrameId.FirefleaGlowMidWingsLevel, 1),
            Show(EnemyFrameId.FirefleaGlowDimWingsUp, 2),
            Show(EnemyFrameId.FirefleaGlowDimWingsLevel, 1),
            Show(EnemyFrameId.FirefleaGlowDimWingsDown, 2),
            Show(EnemyFrameId.FirefleaGlowDimWingsLevel, 1),
            Show(EnemyFrameId.FirefleaGlowDimmerWingsUp, 2),
            Show(EnemyFrameId.FirefleaGlowDimmerWingsLevel, 1),
            Show(EnemyFrameId.FirefleaGlowDimmerWingsDown, 2),
            Show(EnemyFrameId.FirefleaGlowDimmerWingsLevel, 1),
            Show(EnemyFrameId.FirefleaGlowDimmestWingsUp, 2),
            Show(EnemyFrameId.FirefleaGlowDimmestWingsLevel, 1),
            Show(EnemyFrameId.FirefleaGlowDimmestWingsDown, 2),
            Show(EnemyFrameId.FirefleaGlowDimmestWingsLevel, 1),
            Show(EnemyFrameId.FirefleaGlowDimmerWingsUp, 2),
            Show(EnemyFrameId.FirefleaGlowDimmerWingsLevel, 1),
            Show(EnemyFrameId.FirefleaGlowDimmerWingsDown, 2),
            Show(EnemyFrameId.FirefleaGlowDimmerWingsLevel, 1),
            Show(EnemyFrameId.FirefleaGlowDimWingsUp, 2),
            Show(EnemyFrameId.FirefleaGlowDimWingsLevel, 1),
            Show(EnemyFrameId.FirefleaGlowDimWingsDown, 2),
            Show(EnemyFrameId.FirefleaGlowDimWingsLevel, 1),
            Show(EnemyFrameId.FirefleaGlowMidWingsUp, 2),
            Show(EnemyFrameId.FirefleaGlowMidWingsLevel, 1),
            Show(EnemyFrameId.FirefleaGlowMidWingsDown, 2),
            Show(EnemyFrameId.FirefleaGlowMidWingsLevel, 1),
            Show(EnemyFrameId.FirefleaGlowBrightWingsUp, 2),
            Show(EnemyFrameId.FirefleaGlowBrightWingsLevel, 1),
            Show(EnemyFrameId.FirefleaGlowBrightWingsDown, 2),
            Show(EnemyFrameId.FirefleaGlowBrightWingsLevel, 1),
            Show(EnemyFrameId.FirefleaGlowBrighterWingsUp, 2),
            Show(EnemyFrameId.FirefleaGlowBrighterWingsLevel, 1),
            Show(EnemyFrameId.FirefleaGlowBrighterWingsDown, 2),
            Show(EnemyFrameId.FirefleaGlowBrighterWingsLevel, 1),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<FirefleaSegment, Animation<FirefleaSegment>> StartingAt { get; } = new Dictionary<FirefleaSegment, Animation<FirefleaSegment>>
    {
        [P.Loop] = new([new(P.Loop, 0, 52, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<FirefleaSegment> StartingWith(FirefleaSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame(frame, duration);
}
