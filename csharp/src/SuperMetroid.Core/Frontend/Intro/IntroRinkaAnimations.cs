using SuperMetroid.Core.Animation;
using A = SuperMetroid.Core.Frontend.Intro.IntroRinkaAnimation;
using R = SuperMetroid.Core.Frontend.Intro.CinematicBeats<SuperMetroid.Core.Frontend.Intro.CinematicSpritePose, SuperMetroid.Core.Frontend.Intro.IntroRinkaCue>;
using S = SuperMetroid.Core.Frontend.Intro.CinematicSpritePose;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>The animations of the intro flashback's Rinkas (#641, #1600, #2196, #2197).</summary>
internal static class IntroRinkaAnimations
{
    /// <summary>
    /// A Rinka's animation: its pictures in turn, then it starts moving and loops its second, first, second and third
    /// pictures.
    /// </summary>
    internal static CinematicAnimations<IntroRinkaAnimation, CinematicSpritePose, IntroRinkaCue> Rinka { get; } = new(
        new Dictionary<IntroRinkaAnimation, AnimationBeat<CinematicSpritePose, IntroRinkaCue>[]>
        {
            [A.RinkaAppearing] =
            [
                R.Show(S.RinkaDim, 10),
                R.Show(S.RinkaGlowing, 10),
                R.Show(S.RinkaBright, 10),
                R.Raise(IntroRinkaCue.StartMoving),
            ],
            [A.Rinka] =
            [
                R.Show(S.RinkaGlowing, 10),
                R.Show(S.RinkaDim, 10),
                R.Show(S.RinkaGlowing, 10),
                R.Show(S.RinkaBright, 10),
            ],
        },
        plays => new Dictionary<IntroRinkaAnimation, Animation<IntroRinkaAnimation>>
        {
            [A.Rinka] = plays.LeadingInto(A.RinkaAppearing, A.Rinka),
        });
}
