using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Frontend.Intro;
using A = SuperMetroid.Core.Frontend.Ending.EndingSceneAnimation;
using B = SuperMetroid.Core.Frontend.Intro.CinematicBeats<SuperMetroid.Core.Frontend.Intro.CinematicSpritePose, SuperMetroid.Core.Frontend.Ending.EndingSceneCue>;
using K = SuperMetroid.Core.Frontend.Ending.EndingSceneCue;
using S = SuperMetroid.Core.Frontend.Intro.CinematicSpritePose;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The animations of the ending scenes' picture actors (#641, #1600, #2196, #2197): a still picture holds, the planet's
/// layers and the explosion's stars play their scene and publish its cues.
/// </summary>
internal static class EndingSceneAnimations
{
    /// <summary>The ending scenes' picture actors' animations.</summary>
    internal static CinematicAnimations<EndingSceneAnimation, CinematicSpritePose, EndingSceneCue> Pictures { get; } = new(
        new Dictionary<EndingSceneAnimation, IHidingAnimationBeat<CinematicSpritePose, EndingSceneCue>[]>
        {
            [A.TopCloudPattern] = [B.Hold(S.TopCloudPattern)],
            [A.TopCloudEdge] = [B.Hold(S.TopCloudEdge)],
            [A.BottomCloudEdge] = [B.Hold(S.BottomCloudEdge)],
            [A.BottomCloudPattern] = [B.Hold(S.BottomCloudPattern)],
            [A.RightSideCloud] = [B.Hold(S.RightSideCloud)],
            [A.LeftSideCloud] = [B.Hold(S.LeftSideCloud)],
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
            [A.ExplosionStars] = [B.Hold(S.EndingStarfield)],
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
            [A.ExplosionLeftStars] = [B.Hold(S.EndingStarfield)],
            [A.ExplosionAfterglow] = [B.Hold(S.PlanetAfterglow)],
            [A.ClearTimeDigit0] = [B.Hold(S.ClearTimeDigit0)],
            [A.ClearTimeDigit1] = [B.Hold(S.ClearTimeDigit1)],
            [A.ClearTimeDigit2] = [B.Hold(S.ClearTimeDigit2)],
            [A.ClearTimeDigit3] = [B.Hold(S.ClearTimeDigit3)],
            [A.ClearTimeDigit4] = [B.Hold(S.ClearTimeDigit4)],
            [A.ClearTimeDigit5] = [B.Hold(S.ClearTimeDigit5)],
            [A.ClearTimeDigit6] = [B.Hold(S.ClearTimeDigit6)],
            [A.ClearTimeDigit7] = [B.Hold(S.ClearTimeDigit7)],
            [A.ClearTimeDigit8] = [B.Hold(S.ClearTimeDigit8)],
            [A.ClearTimeDigit9] = [B.Hold(S.ClearTimeDigit9)],
            [A.ClearTimeColon] = [B.Hold(S.ClearTimeColon)],
            [A.AnimalEscape] =
            [
                B.Show(S.EscapePodShortTrail, 1),
                B.Show(S.EscapePodLongTrail, 3),
            ],
            [A.SuitlessIdle] = [B.Hold(S.SuitlessIdleUpper)],
            [A.SuitlessLegsIdle] = [B.Hold(S.SuitlessLower)],
            [A.SuitedIdle] = [B.Hold(S.SuitedIdleBody)],
            [A.HelmetIdle] = [B.Hold(S.SuitedHelmetHead)],
            [A.HeadIdle] = [B.Hold(S.HelmetlessHeadResting)],
        },
        plays => new Dictionary<EndingSceneAnimation, Animation<EndingSceneAnimation>>
        {
            [A.TopCloudPattern] = plays.Still(A.TopCloudPattern),
            [A.TopCloudEdge] = plays.Still(A.TopCloudEdge),
            [A.BottomCloudEdge] = plays.Still(A.BottomCloudEdge),
            [A.BottomCloudPattern] = plays.Still(A.BottomCloudPattern),
            [A.RightSideCloud] = plays.Still(A.RightSideCloud),
            [A.LeftSideCloud] = plays.Still(A.LeftSideCloud),
            [A.ExplodingPlanet] = new([plays.Repeated(A.PlanetRumbling, 5), plays.Whole(A.ExplodingPlanet)], AnimationEnd.Stops),
            [A.ExplosionGlow] = plays.Looping(A.ExplosionGlow),
            [A.ExplosionStars] = plays.Still(A.ExplosionStars),
            [A.ExplosionLava] = plays.LeadingInto(A.ExplosionLavaHidden, A.ExplosionLava),
            [A.ExplosionSilhouette] = plays.Through(A.ExplosionSilhouette, AnimationEnd.Stops),
            [A.ExplosionRightStars] = plays.Through(A.ExplosionRightStars, AnimationEnd.Holds),
            [A.ExplosionLeftStars] = plays.Still(A.ExplosionLeftStars),
            [A.ExplosionAfterglow] = plays.Still(A.ExplosionAfterglow),
            [A.ClearTimeDigit0] = plays.Still(A.ClearTimeDigit0),
            [A.ClearTimeDigit1] = plays.Still(A.ClearTimeDigit1),
            [A.ClearTimeDigit2] = plays.Still(A.ClearTimeDigit2),
            [A.ClearTimeDigit3] = plays.Still(A.ClearTimeDigit3),
            [A.ClearTimeDigit4] = plays.Still(A.ClearTimeDigit4),
            [A.ClearTimeDigit5] = plays.Still(A.ClearTimeDigit5),
            [A.ClearTimeDigit6] = plays.Still(A.ClearTimeDigit6),
            [A.ClearTimeDigit7] = plays.Still(A.ClearTimeDigit7),
            [A.ClearTimeDigit8] = plays.Still(A.ClearTimeDigit8),
            [A.ClearTimeDigit9] = plays.Still(A.ClearTimeDigit9),
            [A.ClearTimeColon] = plays.Still(A.ClearTimeColon),
            [A.AnimalEscape] = plays.Looping(A.AnimalEscape),
            [A.SuitlessIdle] = plays.Still(A.SuitlessIdle),
            [A.SuitlessLegsIdle] = plays.Still(A.SuitlessLegsIdle),
            [A.SuitedIdle] = plays.Still(A.SuitedIdle),
            [A.HelmetIdle] = plays.Still(A.HelmetIdle),
            [A.HeadIdle] = plays.Still(A.HeadIdle),
        });
}
