using SuperMetroid.Core.Animation;
using A = SuperMetroid.Core.Frontend.Intro.IntroNarrationAnimation;
using B = SuperMetroid.Core.Frontend.Intro.CinematicBeats<SuperMetroid.Core.Frontend.Intro.CinematicSpritePose, SuperMetroid.Core.Animation.NoAnimationCue>;
using S = SuperMetroid.Core.Frontend.Intro.CinematicSpritePose;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>The animations of the intro narration's caret and Japanese-text next-page arrow (#641, #1600, #2196, #2197).</summary>
internal static partial class IntroNarrationAnimations
{
    /// <summary>The caret's and the next-page arrow's animations.</summary>
    internal static CinematicAnimations<IntroNarrationAnimation, CinematicSpritePose, NoAnimationCue> Narration { get; } = new(
        new Dictionary<IntroNarrationAnimation, IHidingAnimationBeat<CinematicSpritePose, NoAnimationCue>[]>
        {
            [A.Caret] = [B.Hold(S.CaretVisible)],
            [A.CaretBlinking] =
            [
                B.Show(S.CaretVisible, 5),
                B.Hide(5),
            ],
            [A.NextPageArrow] =
            [
                B.Show(S.NextPageArrowBright, 10),
                B.Show(S.NextPageArrowFading, 10),
                B.Show(S.NextPageArrowFaint, 10),
                B.Show(S.NextPageArrowFading, 10),
            ],
            [A.NextPageArrowDelayed] = [B.Hide(60)],
        },
        plays => new Dictionary<IntroNarrationAnimation, Animation<IntroNarrationAnimation>>
        {
            [A.Caret] = plays.Still(A.Caret),
            [A.CaretBlinking] = plays.Looping(A.CaretBlinking),
            [A.NextPageArrow] = plays.Looping(A.NextPageArrow),
            [A.NextPageArrowDelayed] = plays.LeadingInto(A.NextPageArrowDelayed, A.NextPageArrow),
        });
}
