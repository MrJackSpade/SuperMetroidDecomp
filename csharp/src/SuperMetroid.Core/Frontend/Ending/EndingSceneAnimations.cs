using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Frontend.Intro;
using A = SuperMetroid.Core.Frontend.Ending.EndingSceneAnimation;
using B = SuperMetroid.Core.Frontend.Intro.CinematicBeats<SuperMetroid.Core.Frontend.Intro.CinematicSpritePose, SuperMetroid.Core.Frontend.Ending.EndingSceneCue>;
using K = SuperMetroid.Core.Frontend.Ending.EndingSceneCue;
using S = SuperMetroid.Core.Frontend.Intro.CinematicSpritePose;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The animations of the ending scenes' picture actors (#641, #1600, #2196, #2197): a still picture holds, listed once with
/// its picture (#2557), the planet's layers and the explosion's stars play their scene and publish its cues.
/// </summary>
internal static class EndingSceneAnimations
{
    /// <summary>The ending scenes' picture actors' animations.</summary>
    internal static CinematicAnimations<EndingSceneAnimation, CinematicSpritePose, EndingSceneCue> Pictures { get; } = new(
        new Dictionary<EndingSceneAnimation, CinematicSpritePose>
        {
            [A.TopCloudPattern] = S.TopCloudPattern,
            [A.TopCloudEdge] = S.TopCloudEdge,
            [A.BottomCloudEdge] = S.BottomCloudEdge,
            [A.BottomCloudPattern] = S.BottomCloudPattern,
            [A.RightSideCloud] = S.RightSideCloud,
            [A.LeftSideCloud] = S.LeftSideCloud,
            [A.ExplosionStars] = S.EndingStarfield,
            [A.ExplosionLeftStars] = S.EndingStarfield,
            [A.ExplosionAfterglow] = S.PlanetAfterglow,
            [A.ClearTimeDigit0] = S.ClearTimeDigit0,
            [A.ClearTimeDigit1] = S.ClearTimeDigit1,
            [A.ClearTimeDigit2] = S.ClearTimeDigit2,
            [A.ClearTimeDigit3] = S.ClearTimeDigit3,
            [A.ClearTimeDigit4] = S.ClearTimeDigit4,
            [A.ClearTimeDigit5] = S.ClearTimeDigit5,
            [A.ClearTimeDigit6] = S.ClearTimeDigit6,
            [A.ClearTimeDigit7] = S.ClearTimeDigit7,
            [A.ClearTimeDigit8] = S.ClearTimeDigit8,
            [A.ClearTimeDigit9] = S.ClearTimeDigit9,
            [A.ClearTimeColon] = S.ClearTimeColon,
            [A.SuitlessIdle] = S.SuitlessIdleUpper,
            [A.SuitlessLegsIdle] = S.SuitlessLower,
            [A.SuitedIdle] = S.SuitedIdleBody,
            [A.HelmetIdle] = S.SuitedHelmetHead,
            [A.HeadIdle] = S.HelmetlessHeadResting,
        },
        new Dictionary<EndingSceneAnimation, IHidingAnimationBeat<CinematicSpritePose, EndingSceneCue>[]>
        {
            [A.PlanetRumbling] =
            [
                B.Show(S.PlanetRumbleDeepShadow, 13),
                B.Show(S.PlanetRumbleShadow, 13),
                B.Show(S.PlanetRumbleLit, 13),
                B.Show(S.PlanetRumbleBright, 13),
            ],
            [A.ExplodingPlanet] =
            [
                B.Raise(K.FadeExplosionColours),
                B.Show(S.PlanetCracking, 32),
                B.Show(S.PlanetFracturing, 32),
                B.Show(S.PlanetBreakingApart, 32),
                B.Show(S.PlanetCrumbling, 32),
                B.Raise(K.SpawnExplosionSilhouette),
            ],
            [A.ExplosionGlow] =
            [
                B.Show(S.PlanetGlowTightHalo, 16),
                B.Show(S.PlanetGlowSpreadingHalo, 16),
                B.Show(S.PlanetGlowWideHalo, 16),
                B.Show(S.PlanetGlowSpreadingHalo, 16),
            ],
            [A.ExplosionLavaHidden] = [B.Hide(156)],
            [A.ExplosionLava] =
            [
                B.Show(S.PlanetLavaDark, 10),
                B.Show(S.PlanetLavaBright, 10),
            ],
            [A.ExplosionSilhouette] =
            [
                B.Show(S.PlanetSilhouette, 8),
                B.Raise(K.StartZebesExplosion),
            ],
            [A.ExplosionRightStars] =
            [
                B.Show(S.EndingStarfield, 144),
                B.Raise(K.ExplosionFinale),
                B.Show(S.EndingStarfield, 332),
                B.Raise(K.EndZebesExplosion),
                B.Raise(K.WaitForFlyaway),
                B.Hold(S.EndingStarfield),
            ],
            [A.AnimalEscape] =
            [
                B.Show(S.EscapePodShortTrail, 1),
                B.Show(S.EscapePodLongTrail, 3),
            ],
        },
        plays => new Dictionary<EndingSceneAnimation, Animation<EndingSceneAnimation>>
        {
            [A.ExplodingPlanet] = new([plays.Repeated(A.PlanetRumbling, 5), plays.Whole(A.ExplodingPlanet)], AnimationEnd.Stops),
            [A.ExplosionGlow] = plays.Looping(A.ExplosionGlow),
            [A.ExplosionLava] = plays.LeadingInto(A.ExplosionLavaHidden, A.ExplosionLava),
            [A.ExplosionSilhouette] = plays.Through(A.ExplosionSilhouette, AnimationEnd.Stops),
            [A.ExplosionRightStars] = plays.Through(A.ExplosionRightStars, AnimationEnd.Holds),
            [A.AnimalEscape] = plays.Looping(A.AnimalEscape),
        });
}
