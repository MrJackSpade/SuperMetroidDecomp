using SuperMetroid.Core.Animation;
using A = SuperMetroid.Core.Frontend.Intro.IntroBabyAnimation;
using D = SuperMetroid.Core.Frontend.Intro.CinematicBeats<SuperMetroid.Core.Frontend.Intro.CinematicSpritePose, SuperMetroid.Core.Frontend.Intro.IntroDiscoveryCue>;
using L = SuperMetroid.Core.Frontend.Intro.CinematicBeats<SuperMetroid.Core.Frontend.Intro.CinematicSpritePose, SuperMetroid.Core.Frontend.Intro.IntroLaboratoryCue>;
using S = SuperMetroid.Core.Frontend.Intro.CinematicSpritePose;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// The animations of the intro's Metroid egg and Baby Metroid (#641, #1600, #2196, #2197): the discovery scene's egg and
/// confused Baby, and the laboratory scenes' delivered and examined Baby, each scene's with the cues its owner carries out.
/// </summary>
internal static class IntroBabyAnimations
{
    /// <summary>The discovery scene's egg and confused Baby; the hatching egg rocks left and right four times before it cracks.</summary>
    internal static CinematicAnimations<IntroBabyAnimation, CinematicSpritePose, IntroDiscoveryCue> Discovery { get; } = new(
        new Dictionary<IntroBabyAnimation, AnimationBeat<CinematicSpritePose, IntroDiscoveryCue>[]>
        {
            [A.Egg] =
            [
                D.Hold(S.EggIntact),
            ],
            [A.EggResting] = [D.Show(S.EggIntact, 32)],
            [A.EggRocking] =
            [
                D.Show(S.EggIntact, 5),
                D.Show(S.EggRockingLeft, 5),
                D.Show(S.EggIntact, 5),
                D.Show(S.EggRockingRight, 5),
            ],
            [A.EggHatching] =
            [
                D.Show(S.EggIntact, 10),
                D.Show(S.EggCrackingUpperLeft, 10),
                D.Show(S.EggCrackingTop, 10),
                D.Show(S.EggCrackingLeft, 10),
                D.Show(S.EggCrackingUpperRight, 10),
                D.Show(S.EggCrackingRight, 10),
                D.Show(S.EggHatched, 80),
                D.Raise(IntroDiscoveryCue.SpawnShellPieces),
                D.Show(S.EggRemnantDomeSagging, 10),
                D.Show(S.EggRemnantDomeFalling, 10),
                D.Show(S.EggRemnantDomeCrumbling, 10),
                D.Show(S.EggRemnantRibsBared, 10),
                D.Show(S.EggRemnantRibsShifting, 10),
                D.Show(S.EggRemnantRibsSettling, 10),
                D.Show(S.EggRemnantSettled, 320),
                D.Raise(IntroDiscoveryCue.StartPageThree),
                D.Raise(IntroDiscoveryCue.DeleteEggAfterCrossFade),
                D.Hold(S.EggRemnantSettled),
            ],
            [A.ConfusedBaby] =
            [
                D.Show(S.ConfusedBabyStretched, 10),
                D.Show(S.ConfusedBabyRound, 10),
                D.Show(S.ConfusedBabySquashed, 10),
                D.Show(S.ConfusedBabyRound, 10),
            ],
        },
        plays => new Dictionary<IntroBabyAnimation, Animation<IntroBabyAnimation>>
        {
            [A.Egg] = plays.Still(A.Egg),
            [A.EggHatching] = new([plays.Whole(A.EggResting), plays.Repeated(A.EggRocking, 4), plays.Whole(A.EggHatching)], AnimationEnd.Holds),
            [A.ConfusedBaby] = plays.Looping(A.ConfusedBaby),
        });

    /// <summary>The laboratory scenes' delivered and examined Baby: each pulses ten times, starts its scene's next page and pulses on.</summary>
    internal static CinematicAnimations<IntroBabyAnimation, CinematicSpritePose, IntroLaboratoryCue> Laboratory { get; } = new(
        new Dictionary<IntroBabyAnimation, AnimationBeat<CinematicSpritePose, IntroLaboratoryCue>[]>
        {
            [A.DeliveredBaby] =
            [
                L.Show(S.DeliveredBabyPulseLow, 10),
                L.Show(S.DeliveredBabyPulseMiddle, 10),
                L.Show(S.DeliveredBabyPulseHigh, 10),
                L.Show(S.DeliveredBabyPulseMiddle, 10),
            ],
            [A.DeliveredBabyStartingPageFour] = [L.Raise(IntroLaboratoryCue.StartPageFour)],
            [A.ExaminedBaby] =
            [
                L.Show(S.ExaminedBabyWide, 10),
                L.Show(S.ExaminedBabyNarrowing, 10),
                L.Show(S.ExaminedBabyNarrow, 10),
                L.Show(S.ExaminedBabyNarrowing, 10),
            ],
            [A.ExaminedBabyStartingPageFive] = [L.Raise(IntroLaboratoryCue.StartPageFive)],
        },
        plays => new Dictionary<IntroBabyAnimation, Animation<IntroBabyAnimation>>
        {
            [A.DeliveredBaby] = new(
                [plays.Repeated(A.DeliveredBaby, 10), plays.Whole(A.DeliveredBabyStartingPageFour), plays.Whole(A.DeliveredBaby)], AnimationEnd.LoopsFrom(2)),
            [A.ExaminedBaby] = new(
                [plays.Repeated(A.ExaminedBaby, 10), plays.Whole(A.ExaminedBabyStartingPageFive), plays.Whole(A.ExaminedBaby)], AnimationEnd.LoopsFrom(2)),
        });
}
