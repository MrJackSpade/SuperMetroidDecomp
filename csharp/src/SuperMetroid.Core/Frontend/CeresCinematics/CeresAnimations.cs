using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Frontend.Intro;
using A = SuperMetroid.Core.Frontend.CeresCinematics.CeresAnimation;
using B = SuperMetroid.Core.Frontend.Intro.CinematicBeats<SuperMetroid.Core.Frontend.Intro.CinematicSpritePose, SuperMetroid.Core.Frontend.CeresCinematics.CeresCue>;
using S = SuperMetroid.Core.Frontend.Intro.CinematicSpritePose;
using W = SuperMetroid.Core.Frontend.Intro.CinematicBeats<SuperMetroid.Core.Frontend.Intro.CinematicSpritePose, SuperMetroid.Core.Frontend.CeresCinematics.CeresExplosionSpawnerCue>;

namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>
/// The animations of the Ceres cinematics' actors (#641, #1600, #2196, #2197): the flight to Ceres, its destruction and the
/// Zebes reveal, whose only cues are the PLANET ZEBES title's; and the invisible explosion spawner's, whose cues spawn the
/// station's blasts (#2489).
/// </summary>
internal static partial class CeresAnimations
{
    /// <summary>The flight's, the destruction's and the Zebes reveal's actors.</summary>
    internal static CinematicAnimations<CeresAnimation, CinematicSpritePose, CeresCue> Scenes { get; } = new(
        new Dictionary<CeresAnimation, IHidingAnimationBeat<CinematicSpritePose, CeresCue>[]>
        {
            [A.Stars] = [B.Hold(S.FlightStars)],
            [A.LargeAsteroid] = [B.Hold(S.LargeAsteroid)],
            [A.StationUnderAttack] = [B.Hold(S.StationUnderAttack)],
            [A.SmallAsteroid] = [B.Hold(S.SmallAsteroid)],
            [A.Vortex] =
            [
                B.Show(S.VortexBrightSparks, 1),
                B.Show(S.VortexFaintSparks, 1),
            ],
            [A.DestructionLargeAsteroid] = [B.Hold(S.StationUnderAttackLargeAsteroid)],
            [A.SmallExplosion] = SmallExplosionBurst(),
            [A.RepeatingSmallExplosion] = [.. SmallExplosionBurst(), B.Hide(16)],
            [A.LargeExplosion] =
            [
                B.Show(S.LargeBlastFireballCluster, 5),
                B.Show(S.LargeBlastBreakingCluster, 5),
                B.Show(S.LargeBlastScatteringEmbers, 5),
                B.Show(S.LargeBlastFadingEmbers, 5),
                B.Hide(8),
            ],
            [A.StationBlast] =
            [
                B.Show(S.StationBlastFlare, 5),
                B.Show(S.StationBlastFireball, 5),
                B.Show(S.StationBlastBursting, 5),
                B.Show(S.StationBlastRingedFireball, 5),
                B.Show(S.StationBlastRing, 5),
                B.Show(S.StationBlastFadingRing, 5),
            ],
            [A.ZebesPlanet] = [B.Hold(S.PlanetZebes)],
            [A.ZebesStarsUpperLeft] = [B.Hold(S.ZebesStarsUpperLeft)],
            [A.ZebesStarsUpperRight] = [B.Hold(S.ZebesStarsUpperRight)],
            [A.ZebesStarsLowerLeft] = [B.Hold(S.ZebesStarsLowerLeft)],
            [A.ZebesStarsLowerRight] = [B.Hold(S.ZebesStarsLowerRight)],
            [A.ZebesTitle] =
            [
                B.Hide(64),
                B.Raise(CeresCue.FadeInZebesTitle),
                B.Show(S.PlanetZebesTitle, 32),
                B.Raise(CeresCue.SpawnJapaneseTitleIfNeeded),
                B.Show(S.PlanetZebesTitle, 192),
                B.Raise(CeresCue.FadeOutZebesTitle),
                B.Show(S.PlanetZebesTitle, 96),
                B.Raise(CeresCue.StartFlyingToZebes),
            ],
            [A.ZebesJapaneseTitle] = [B.Show(S.ZebesJapaneseTitle, 192)],
        },
        plays => new Dictionary<CeresAnimation, Animation<CeresAnimation>>
        {
            [A.Stars] = plays.Still(A.Stars),
            [A.LargeAsteroid] = plays.Still(A.LargeAsteroid),
            [A.StationUnderAttack] = plays.Still(A.StationUnderAttack),
            [A.SmallAsteroid] = plays.Still(A.SmallAsteroid),
            [A.Vortex] = plays.Looping(A.Vortex),
            [A.DestructionLargeAsteroid] = plays.Still(A.DestructionLargeAsteroid),
            [A.SmallExplosion] = plays.Through(A.SmallExplosion, AnimationEnd.Stops),
            [A.RepeatingSmallExplosion] = new([plays.Repeated(A.RepeatingSmallExplosion, 6)], AnimationEnd.Stops),
            [A.LargeExplosion] = new([plays.Repeated(A.LargeExplosion, 7)], AnimationEnd.Stops),
            [A.StationBlast] = plays.Through(A.StationBlast, AnimationEnd.Stops),
            [A.ZebesPlanet] = plays.Still(A.ZebesPlanet),
            [A.ZebesStarsUpperLeft] = plays.Still(A.ZebesStarsUpperLeft),
            [A.ZebesStarsUpperRight] = plays.Still(A.ZebesStarsUpperRight),
            [A.ZebesStarsLowerLeft] = plays.Still(A.ZebesStarsLowerLeft),
            [A.ZebesStarsLowerRight] = plays.Still(A.ZebesStarsLowerRight),
            [A.ZebesTitle] = plays.Through(A.ZebesTitle, AnimationEnd.Stops),
            [A.ZebesJapaneseTitle] = plays.Through(A.ZebesJapaneseTitle, AnimationEnd.Stops),
        });

    /// <summary>
    /// The invisible explosion spawner's animation (#2489, #2541), which the destruction's setup starts: after its opening
    /// wait it spawns the first wave; after a longer wait it plays its repeating blast once for each repeating blast's place
    /// (<see cref="CeresExplosionDefinitions.RepeatingBlastPlaces"/>), each a short wait and then the next repeating blast,
    /// so the first spawns on the first update of the stretch before the final wave; a short beat after the last it spawns
    /// the final wave and ends.
    /// </summary>
    internal static CinematicAnimations<CeresAnimation, CinematicSpritePose, CeresExplosionSpawnerCue> ExplosionSpawner { get; } = new(
        new Dictionary<CeresAnimation, IHidingAnimationBeat<CinematicSpritePose, CeresExplosionSpawnerCue>[]>
        {
            [A.ExplosionSpawner] =
            [
                W.Hide(128),
                W.Raise(CeresExplosionSpawnerCue.SpawnInitialWave),
                W.Hide(69),
            ],
            [A.ExplosionSpawnerRepeatingBlast] =
            [
                W.Hide(12),
                W.Raise(CeresExplosionSpawnerCue.SpawnRepeatingBlast),
            ],
            [A.ExplosionSpawnerFinalWave] =
            [
                W.Hide(3),
                W.Raise(CeresExplosionSpawnerCue.SpawnFinalWave),
            ],
        },
        plays => new Dictionary<CeresAnimation, Animation<CeresAnimation>>
        {
            [A.ExplosionSpawner] = new(
            [
                plays.Whole(A.ExplosionSpawner),
                plays.Repeated(A.ExplosionSpawnerRepeatingBlast, CeresExplosionDefinitions.RepeatingBlastPlaces.Count),
                plays.Whole(A.ExplosionSpawnerFinalWave),
            ], AnimationEnd.Stops),
        });

    /// <summary>A small explosion burning through its six pictures, three updates each.</summary>
    private static AnimationBeat<CinematicSpritePose, CeresCue>[] SmallExplosionBurst() =>
    [
        B.Show(S.SmallExplosionSpark, 3),
        B.Show(S.SmallExplosionFlare, 3),
        B.Show(S.SmallExplosionFireball, 3),
        B.Show(S.SmallExplosionHollowing, 3),
        B.Show(S.SmallExplosionRing, 3),
        B.Show(S.SmallExplosionFadingRing, 3),
    ];
}
