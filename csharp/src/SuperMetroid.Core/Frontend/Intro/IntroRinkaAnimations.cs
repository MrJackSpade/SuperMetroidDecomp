using SuperMetroid.Core.Animation;
using A = SuperMetroid.Core.Frontend.Intro.IntroRinkaAnimation;
using R = SuperMetroid.Core.Frontend.Intro.CinematicBeats<SuperMetroid.Core.Frontend.Intro.CinematicSpritePose, SuperMetroid.Core.Frontend.Intro.IntroRinkaCue>;
using S = SuperMetroid.Core.Frontend.Intro.CinematicSpritePose;
using W = SuperMetroid.Core.Frontend.Intro.CinematicBeats<SuperMetroid.Core.Frontend.Intro.CinematicSpritePose, SuperMetroid.Core.Frontend.Intro.IntroRinkaSpawnerCue>;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>The animations of the intro flashback's Rinkas and of their spawner (#641, #1600, #2196, #2197).</summary>
internal static class IntroRinkaAnimations
{
    /// <summary>
    /// A Rinka's animation: its three pictures for ten updates each, then it starts moving and loops its second, first,
    /// second and third pictures for ten updates each.
    /// </summary>
    internal static CinematicAnimations<IntroRinkaAnimation, CinematicSpritePose, IntroRinkaCue> Rinka { get; } = new(
        new Dictionary<IntroRinkaAnimation, AnimationBeat<CinematicSpritePose, IntroRinkaCue>[]>
        {
            [A.Rinka] =
            [
                R.Show(S.RinkaDim, 10),
                R.Show(S.RinkaGlowing, 10),
                R.Show(S.RinkaBright, 10),
                R.Raise(IntroRinkaCue.StartMoving),
                R.Show(S.RinkaGlowing, 10),
                R.Show(S.RinkaDim, 10),
                R.Show(S.RinkaGlowing, 10),
                R.Show(S.RinkaBright, 10),
            ],
        },
        new Dictionary<IntroRinkaAnimation, Animation<IntroRinkaAnimation>>
        {
            [A.Rinka] = new([new(A.Rinka, 0, 4, 1), new(A.Rinka, 4, 4, 1)], AnimationEnd.LoopsFrom(1)),
        });

    /// <summary>The invisible spawner's animation: it waits 74 updates, launches a pair, waits 128 updates, launches another, and ends.</summary>
    internal static CinematicAnimations<IntroRinkaAnimation, CinematicSpritePose, IntroRinkaSpawnerCue> Spawner { get; } = new(
        new Dictionary<IntroRinkaAnimation, AnimationBeat<CinematicSpritePose, IntroRinkaSpawnerCue>[]>
        {
            [A.Spawner] =
            [
                W.Show(S.Blank, 74),
                W.Raise(IntroRinkaSpawnerCue.SpawnStrikingPair),
                W.Show(S.Blank, 128),
                W.Raise(IntroRinkaSpawnerCue.SpawnMissingPair),
            ],
        },
        new Dictionary<IntroRinkaAnimation, Animation<IntroRinkaAnimation>>
        {
            [A.Spawner] = new([new(A.Spawner, 0, 4, 1)], AnimationEnd.Stops),
        });
}
