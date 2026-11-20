using SuperMetroid.Core.Animation;
using A = SuperMetroid.Core.Frontend.Intro.IntroEggEffectAnimation;
using B = SuperMetroid.Core.Frontend.Intro.CinematicBeats<SuperMetroid.Core.Frontend.Intro.CinematicSpritePose, SuperMetroid.Core.Animation.NoAnimationCue>;
using S = SuperMetroid.Core.Frontend.Intro.CinematicSpritePose;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// The animations of the hatched egg's shell pieces and slime (#641, #1600, #2196, #2197): each piece and falling drop is a
/// still picture, listed once with its picture (#2557), and a drop's splash plays once.
/// </summary>
internal static class IntroEggEffectAnimations
{
    /// <summary>The shell pieces' and slime's animations.</summary>
    internal static CinematicAnimations<IntroEggEffectAnimation, CinematicSpritePose, NoAnimationCue> Effects { get; } = new(
        new Dictionary<IntroEggEffectAnimation, CinematicSpritePose>
        {
            [A.ShellUpperLeft] = S.EggShellUpperLeft,
            [A.ShellUpperRight] = S.EggShellUpperRight,
            [A.ShellLeft] = S.EggShellLeft,
            [A.ShellCentre] = S.EggShellCentre,
            [A.ShellRight] = S.EggShellRight,
            [A.ShellLower] = S.EggShellLower,
            [A.SlimeDrop] = S.SlimeMoving,
        },
        new Dictionary<IntroEggEffectAnimation, AnimationBeat<CinematicSpritePose, NoAnimationCue>[]>
        {
            [A.SlimeImpact] =
            [
                B.Show(S.SlimeSplashCrown, 10),
                B.Show(S.SlimeSplashPeak, 10),
                B.Show(S.SlimeSplashSinking, 10),
                B.Show(S.SlimePuddle, 10),
            ],
        },
        plays => new Dictionary<IntroEggEffectAnimation, Animation<IntroEggEffectAnimation>>
        {
            [A.SlimeImpact] = plays.Through(A.SlimeImpact, AnimationEnd.Stops),
        });
}
