using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Frontend.Intro;
using A = SuperMetroid.Core.Frontend.CeresCinematics.CeresAnimation;
using S = SuperMetroid.Core.Frontend.Intro.CinematicSpritePose;
using W = SuperMetroid.Core.Frontend.Intro.CinematicBeats<SuperMetroid.Core.Frontend.Intro.CinematicSpritePose, SuperMetroid.Core.Frontend.CeresCinematics.CeresExplosionSpawnerCue>;

namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>Cartridge values and helpers of <see cref="CeresAnimations"/> that only verification reads.</summary>
internal static class CeresAnimationsConstants
{
    /// <summary>
    /// The explosion spawner's schedule: invisible for 128 updates, the initial wave, 80 updates before the repeating
    /// explosions start, then 64 updates before the final wave.
    /// </summary>
    internal static CinematicAnimations<CeresAnimation, CinematicSpritePose, CeresExplosionSpawnerCue> ExplosionSpawner { get; } = new(
        new Dictionary<CeresAnimation, AnimationBeat<CinematicSpritePose, CeresExplosionSpawnerCue>[]>
        {
            [A.ExplosionSpawner] =
            [
                W.Show(S.Blank, 128),
                W.Raise(CeresExplosionSpawnerCue.SpawnInitialWave),
                W.Show(S.Blank, 80),
                W.Raise(CeresExplosionSpawnerCue.StartRepeatingExplosions),
                W.Show(S.Blank, 64),
                W.Raise(CeresExplosionSpawnerCue.SpawnFinalWave),
            ],
        },
        new Dictionary<CeresAnimation, Animation<CeresAnimation>>
        {
            [A.ExplosionSpawner] = CinematicPlays.Through(A.ExplosionSpawner, 6, AnimationEnd.Stops),
        });
}
