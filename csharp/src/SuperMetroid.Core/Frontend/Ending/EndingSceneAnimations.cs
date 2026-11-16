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
        new Dictionary<EndingSceneAnimation, AnimationBeat<CinematicSpritePose, EndingSceneCue>[]>
        {
            [A.TopCloudPattern] = [B.Hold(S.TopCloudPattern)],
            [A.TopCloudEdge] = [B.Hold(S.TopCloudEdge)],
            [A.BottomCloudEdge] = [B.Hold(S.BottomCloudEdge)],
            [A.BottomCloudPattern] = [B.Hold(S.BottomCloudPattern)],
            [A.RightSideCloud] = [B.Hold(S.RightSideCloud)],
            [A.LeftSideCloud] = [B.Hold(S.LeftSideCloud)],
            [A.ExplodingPlanet] =
            [
                B.Show(S.PlanetRumbleDeepShadow, 13),
                B.Show(S.PlanetRumbleShadow, 13),
                B.Show(S.PlanetRumbleLit, 13),
                B.Show(S.PlanetRumbleBright, 13),
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
            [A.ExplosionLava] =
            [
                B.Show(S.Blank, 156),
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
                B.Show(S.EscapePodLongTrail, 1),
                B.Show(S.EscapePodLongTrail, 1),
                B.Show(S.EscapePodLongTrail, 1),
            ],
            [A.SuitlessIdle] = [B.Hold(S.SuitlessIdleUpper)],
            [A.SuitlessLegsIdle] = [B.Hold(S.SuitlessLower)],
            [A.SuitedIdle] = [B.Hold(S.SuitedIdleBody)],
            [A.HelmetIdle] = [B.Hold(S.SuitedHelmetHead)],
            [A.HeadIdle] = [B.Hold(S.HelmetlessHeadResting)],
        },
        new Dictionary<EndingSceneAnimation, Animation<EndingSceneAnimation>>
        {
            [A.TopCloudPattern] = CinematicPlays.Still(A.TopCloudPattern),
            [A.TopCloudEdge] = CinematicPlays.Still(A.TopCloudEdge),
            [A.BottomCloudEdge] = CinematicPlays.Still(A.BottomCloudEdge),
            [A.BottomCloudPattern] = CinematicPlays.Still(A.BottomCloudPattern),
            [A.RightSideCloud] = CinematicPlays.Still(A.RightSideCloud),
            [A.LeftSideCloud] = CinematicPlays.Still(A.LeftSideCloud),
            [A.ExplodingPlanet] = new([new(A.ExplodingPlanet, 0, 4, 5), new(A.ExplodingPlanet, 4, 6, 1)], AnimationEnd.Stops),
            [A.ExplosionGlow] = CinematicPlays.Through(A.ExplosionGlow, 4, AnimationEnd.LoopsFrom(0)),
            [A.ExplosionStars] = CinematicPlays.Still(A.ExplosionStars),
            [A.ExplosionLava] = new([new(A.ExplosionLava, 0, 1, 1), new(A.ExplosionLava, 1, 2, 1)], AnimationEnd.LoopsFrom(1)),
            [A.ExplosionSilhouette] = CinematicPlays.Through(A.ExplosionSilhouette, 2, AnimationEnd.Stops),
            [A.ExplosionRightStars] = CinematicPlays.Through(A.ExplosionRightStars, 6, AnimationEnd.Holds),
            [A.ExplosionLeftStars] = CinematicPlays.Still(A.ExplosionLeftStars),
            [A.ExplosionAfterglow] = CinematicPlays.Still(A.ExplosionAfterglow),
            [A.ClearTimeDigit0] = CinematicPlays.Still(A.ClearTimeDigit0),
            [A.ClearTimeDigit1] = CinematicPlays.Still(A.ClearTimeDigit1),
            [A.ClearTimeDigit2] = CinematicPlays.Still(A.ClearTimeDigit2),
            [A.ClearTimeDigit3] = CinematicPlays.Still(A.ClearTimeDigit3),
            [A.ClearTimeDigit4] = CinematicPlays.Still(A.ClearTimeDigit4),
            [A.ClearTimeDigit5] = CinematicPlays.Still(A.ClearTimeDigit5),
            [A.ClearTimeDigit6] = CinematicPlays.Still(A.ClearTimeDigit6),
            [A.ClearTimeDigit7] = CinematicPlays.Still(A.ClearTimeDigit7),
            [A.ClearTimeDigit8] = CinematicPlays.Still(A.ClearTimeDigit8),
            [A.ClearTimeDigit9] = CinematicPlays.Still(A.ClearTimeDigit9),
            [A.ClearTimeColon] = CinematicPlays.Still(A.ClearTimeColon),
            [A.AnimalEscape] = CinematicPlays.Through(A.AnimalEscape, 4, AnimationEnd.LoopsFrom(0)),
            [A.SuitlessIdle] = CinematicPlays.Still(A.SuitlessIdle),
            [A.SuitlessLegsIdle] = CinematicPlays.Still(A.SuitlessLegsIdle),
            [A.SuitedIdle] = CinematicPlays.Still(A.SuitedIdle),
            [A.HelmetIdle] = CinematicPlays.Still(A.HelmetIdle),
            [A.HeadIdle] = CinematicPlays.Still(A.HeadIdle),
        });
}
