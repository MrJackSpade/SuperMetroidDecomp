using SuperMetroid.Core.Animation;
using A = SuperMetroid.Core.Frontend.Intro.IntroMotherBrainAnimation;
using B = SuperMetroid.Core.Frontend.Intro.CinematicBeats<SuperMetroid.Core.Frontend.Intro.CinematicSpritePose, SuperMetroid.Core.Frontend.Intro.IntroMotherBrainCue>;
using K = SuperMetroid.Core.Frontend.Intro.IntroMotherBrainCue;
using S = SuperMetroid.Core.Frontend.Intro.CinematicSpritePose;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>The intro flashback Mother Brain's animations and their explosions' (#641, #1600, #2197).</summary>
internal static class IntroMotherBrainAnimations
{
    /// <summary>Mother Brain's animations: her pulse, and the start of page two, after which she pulses on.</summary>
    internal static CinematicAnimations<IntroMotherBrainAnimation, CinematicSpritePose, IntroMotherBrainCue> MotherBrain { get; } = new(
        new Dictionary<IntroMotherBrainAnimation, AnimationBeat<CinematicSpritePose, IntroMotherBrainCue>[]>
        {
            [A.Pulsing] =
            [
                B.Show(S.MotherBrainPulseLow, 16),
                B.Show(S.MotherBrainPulseMiddle, 16),
                B.Show(S.MotherBrainPulseHigh, 16),
                B.Show(S.MotherBrainPulseMiddle, 16),
            ],
            [A.StartingNextPage] =
            [
                B.Raise(K.StartNextPage),
                B.Raise(K.StartCrossFade),
            ],
        },
        plays => new Dictionary<IntroMotherBrainAnimation, Animation<IntroMotherBrainAnimation>>
        {
            [A.Pulsing] = plays.Looping(A.Pulsing),
            [A.StartingNextPage] = plays.LeadingInto(A.StartingNextPage, A.Pulsing),
        });

    /// <summary>Mother Brain's explosions: each burns through its pictures and a blank pause, over and over.</summary>
    internal static CinematicAnimations<IntroMotherBrainExplosionAnimation, CinematicSpritePose, NoAnimationCue> Explosions { get; } = new(
        new Dictionary<IntroMotherBrainExplosionAnimation, AnimationBeat<CinematicSpritePose, NoAnimationCue>[]>
        {
            [IntroMotherBrainExplosionAnimation.Big] =
            [
                Show(S.BigExplosionSpark, 6),
                Show(S.BigExplosionFlare, 6),
                Show(S.BigExplosionFireball, 6),
                Show(S.BigExplosionBursting, 6),
                Show(S.BigExplosionSpikedRing, 6),
                Show(S.BigExplosionScatteredSmoke, 6),
                Hide(16),
            ],
            [IntroMotherBrainExplosionAnimation.Small] =
            [
                Show(S.SmallExplosionSpark, 3),
                Show(S.SmallExplosionFlare, 3),
                Show(S.SmallExplosionFireball, 3),
                Show(S.SmallExplosionHollowing, 3),
                Show(S.SmallExplosionRing, 3),
                Show(S.SmallExplosionFadingRing, 3),
                Hide(16),
            ],
        },
        plays => new Dictionary<IntroMotherBrainExplosionAnimation, Animation<IntroMotherBrainExplosionAnimation>>
        {
            [IntroMotherBrainExplosionAnimation.Big] = plays.Looping(IntroMotherBrainExplosionAnimation.Big),
            [IntroMotherBrainExplosionAnimation.Small] = plays.Looping(IntroMotherBrainExplosionAnimation.Small),
        });

    /// <summary>Shows <paramref name="pose"/> for <paramref name="duration"/> updates in an animation that publishes no cue.</summary>
    private static AnimationBeat<CinematicSpritePose, NoAnimationCue> Show(CinematicSpritePose pose, int duration) =>
        CinematicBeats<CinematicSpritePose, NoAnimationCue>.Show(pose, duration);

    /// <summary>Hides the actor for <paramref name="duration"/> updates in an animation that publishes no cue.</summary>
    private static AnimationBeat<CinematicSpritePose, NoAnimationCue> Hide(int duration) =>
        CinematicBeats<CinematicSpritePose, NoAnimationCue>.Hide(duration);
}
