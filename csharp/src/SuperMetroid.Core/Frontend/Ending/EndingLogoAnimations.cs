using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Frontend.Intro;
using A = SuperMetroid.Core.Frontend.Ending.EndingLogoAnimation;
using B = SuperMetroid.Core.Frontend.Intro.CinematicBeats<SuperMetroid.Core.Frontend.Intro.CinematicSpritePose, SuperMetroid.Core.Frontend.Ending.EndingLogoCue>;
using S = SuperMetroid.Core.Frontend.Intro.CinematicSpritePose;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The ending logo's animations (#641, #1600, #2196, #2197): the S halves hold still as they move in, and each circle draws
/// round after a wait and rests on its whole picture, the upper one greying the icon out first.
/// </summary>
internal static class EndingLogoAnimations
{
    /// <summary>The logo's halves' animations.</summary>
    internal static CinematicAnimations<EndingLogoAnimation, CinematicSpritePose, EndingLogoCue> Logo { get; } = new(
        new Dictionary<EndingLogoAnimation, AnimationBeat<CinematicSpritePose, EndingLogoCue>[]>
        {
            [A.UpperS] = [B.Hold(S.LogoSUpper)],
            [A.LowerS] = [B.Hold(S.LogoSLower)],
            [A.UpperCircle] =
            [
                B.Show(S.Blank, 96),
                B.Show(S.LogoRightCircleTip, 5),
                B.Show(S.LogoRightCircleTipAndBlade, 5),
                B.Show(S.LogoRightCircleWhole, 64),
                B.Raise(EndingLogoCue.GreyOut),
                B.Hold(S.LogoRightCircleWhole),
            ],
            [A.LowerCircle] =
            [
                B.Show(S.Blank, 96),
                B.Show(S.LogoLeftCircleTip, 5),
                B.Show(S.LogoLeftCircleTipAndBlade, 5),
                B.Hold(S.LogoLeftCircleWhole),
            ],
        },
        new Dictionary<EndingLogoAnimation, Animation<EndingLogoAnimation>>
        {
            [A.UpperS] = CinematicPlays.Still(A.UpperS),
            [A.LowerS] = CinematicPlays.Still(A.LowerS),
            [A.UpperCircle] = CinematicPlays.Through(A.UpperCircle, 6, AnimationEnd.Holds),
            [A.LowerCircle] = CinematicPlays.Through(A.LowerCircle, 4, AnimationEnd.Holds),
        });
}
