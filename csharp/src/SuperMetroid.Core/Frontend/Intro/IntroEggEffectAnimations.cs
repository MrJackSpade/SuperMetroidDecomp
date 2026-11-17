using SuperMetroid.Core.Animation;
using A = SuperMetroid.Core.Frontend.Intro.IntroEggEffectAnimation;
using B = SuperMetroid.Core.Frontend.Intro.CinematicBeats<SuperMetroid.Core.Frontend.Intro.CinematicSpritePose, SuperMetroid.Core.Animation.NoAnimationCue>;
using S = SuperMetroid.Core.Frontend.Intro.CinematicSpritePose;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// The animations of the hatched egg's shell pieces and slime (#641, #1600, #2196, #2197): each piece and falling drop is a
/// still picture, and a drop's splash plays once.
/// </summary>
internal static class IntroEggEffectAnimations
{
    /// <summary>The shell pieces' and slime's animations.</summary>
    internal static CinematicAnimations<IntroEggEffectAnimation, CinematicSpritePose, NoAnimationCue> Effects { get; } = new(
        new Dictionary<IntroEggEffectAnimation, AnimationBeat<CinematicSpritePose, NoAnimationCue>[]>
        {
            [A.ShellUpperLeft] = [B.Hold(S.EggShellUpperLeft)],
            [A.ShellUpperRight] = [B.Hold(S.EggShellUpperRight)],
            [A.ShellLeft] = [B.Hold(S.EggShellLeft)],
            [A.ShellCentre] = [B.Hold(S.EggShellCentre)],
            [A.ShellRight] = [B.Hold(S.EggShellRight)],
            [A.ShellLower] = [B.Hold(S.EggShellLower)],
            [A.SlimeDrop] = [B.Hold(S.SlimeMoving)],
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
            [A.ShellUpperLeft] = plays.Still(A.ShellUpperLeft),
            [A.ShellUpperRight] = plays.Still(A.ShellUpperRight),
            [A.ShellLeft] = plays.Still(A.ShellLeft),
            [A.ShellCentre] = plays.Still(A.ShellCentre),
            [A.ShellRight] = plays.Still(A.ShellRight),
            [A.ShellLower] = plays.Still(A.ShellLower),
            [A.SlimeDrop] = plays.Still(A.SlimeDrop),
            [A.SlimeImpact] = plays.Through(A.SlimeImpact, AnimationEnd.Stops),
        });
}
