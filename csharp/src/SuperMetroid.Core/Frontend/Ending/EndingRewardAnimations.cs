using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Frontend.Intro;
using A = SuperMetroid.Core.Frontend.Ending.EndingRewardAnimation;
using G = SuperMetroid.Core.Frontend.Intro.CinematicBeats<SuperMetroid.Core.Frontend.Intro.CinematicSpritePose, SuperMetroid.Core.Frontend.Ending.EndingRewardGestureCue>;
using J = SuperMetroid.Core.Frontend.Intro.CinematicBeats<SuperMetroid.Core.Frontend.Intro.CinematicSpritePose, SuperMetroid.Core.Frontend.Ending.EndingRewardJumpCue>;
using S = SuperMetroid.Core.Frontend.Intro.CinematicSpritePose;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The post-credits reward Samus's animations (#641, #1600, #2196, #2197): her gesture, which hands off to her jump, and the
/// jump itself, each with the cues its own owner carries out.
/// </summary>
internal static class EndingRewardAnimations
{
    /// <summary>The gesture's animations; the suitless legs stand still for the hair release's first nine pictures, 195 updates.</summary>
    internal static CinematicAnimations<EndingRewardAnimation, CinematicSpritePose, EndingRewardGestureCue> Gesture { get; } = new(
        new Dictionary<EndingRewardAnimation, AnimationBeat<CinematicSpritePose, EndingRewardGestureCue>[]>
        {
            [A.HairRelease] =
            [
                G.Show(S.SuitlessIdleUpper, 90),
                G.Show(S.SuitlessPonytailTied, 8),
                G.Show(S.SuitlessHandsToPonytail, 10),
                G.Show(S.SuitlessUntyingPonytail, 10),
                G.Show(S.SuitlessPonytailUntied, 32),
                G.Show(S.SuitlessHairFallingLoose, 10),
                G.Show(S.SuitlessHairSwinging, 9),
                G.Show(S.SuitlessHairSweptBack, 16),
                G.Show(S.SuitlessHairFlowing, 10),
                G.Show(S.SuitlessStanding, 48),
                G.Raise(EndingRewardGestureCue.StartJump),
            ],
            [A.HairReleaseLegs] = [G.Show(S.SuitlessLower, 195)],
            [A.GestureBody] =
            [
                G.Show(S.SuitedIdleBody, 64),
                G.Show(S.SuitedHeadlessBody, 264),
            ],
            [A.GestureArm] =
            [
                G.Show(S.Blank, 64),
                G.Show(S.SuitedArmLowered, 8),
                G.Show(S.SuitedArmSwingingUp, 8),
                G.Show(S.SuitedArmFistForward, 5),
                G.Show(S.SuitedArmFistRaised, 4),
                G.Show(S.SuitedArmFistDrawingIn, 3),
                G.Show(S.SuitedArmFistAtChest, 32),
                G.Show(S.SuitedArmFistTurning, 8),
                G.Show(S.SuitedArmFistTurned, 64),
                G.Show(S.SuitedArmFistAtChest, 5),
                G.Show(S.SuitedArmFistDrawingIn, 5),
                G.Show(S.SuitedArmFistRaised, 5),
                G.Show(S.SuitedArmFistForward, 5),
                G.Show(S.SuitedArmSwingingUp, 112),
                G.Raise(EndingRewardGestureCue.StartJump),
            ],
            [A.GestureHelmet] = [G.Show(S.SuitedHelmetHead, 328)],
            [A.GestureHead] =
            [
                G.Show(S.HelmetlessHeadResting, 128),
                G.Show(S.HelmetlessHeadSmileStarting, 5),
                G.Show(S.HelmetlessHeadSmileWidening, 5),
                G.Show(S.HelmetlessHeadSmiling, 190),
            ],
        },
        new Dictionary<EndingRewardAnimation, Animation<EndingRewardAnimation>>
        {
            [A.HairRelease] = CinematicPlays.Through(A.HairRelease, 11, AnimationEnd.Stops),
            [A.HairReleaseLegs] = CinematicPlays.Through(A.HairReleaseLegs, 1, AnimationEnd.Stops),
            [A.GestureBody] = CinematicPlays.Through(A.GestureBody, 2, AnimationEnd.Stops),
            [A.GestureArm] = CinematicPlays.Through(A.GestureArm, 15, AnimationEnd.Stops),
            [A.GestureHelmet] = CinematicPlays.Through(A.GestureHelmet, 1, AnimationEnd.Stops),
            [A.GestureHead] = CinematicPlays.Through(A.GestureHead, 4, AnimationEnd.Stops),
        });

    /// <summary>The jump's animations: each launch ends in flight, which holds its picture; the fall holds; the landing shoots.</summary>
    internal static CinematicAnimations<EndingRewardAnimation, CinematicSpritePose, EndingRewardJumpCue> Jump { get; } = new(
        new Dictionary<EndingRewardAnimation, AnimationBeat<CinematicSpritePose, EndingRewardJumpCue>[]>
        {
            [A.SuitlessJump] =
            [
                J.Show(S.SuitlessStanding, 48),
                J.Show(S.SuitlessPrepareJump, 10),
                J.Raise(EndingRewardJumpCue.Launch),
                J.Raise(EndingRewardJumpCue.StartBodyFlight),
                J.Hold(S.SuitlessJumping),
            ],
            [A.SuitedJump] =
            [
                J.Raise(EndingRewardJumpCue.PrepareHead),
                J.Show(S.SuitedPrepareJump, 10),
                J.Raise(EndingRewardJumpCue.LaunchHead),
                J.Raise(EndingRewardJumpCue.Launch),
                J.Raise(EndingRewardJumpCue.StartBodyFlight),
                J.Hold(S.SuitedJumping),
            ],
            [A.HelmetJump] =
            [
                J.Show(S.SuitedHelmetJumpLevel, 10),
                J.Raise(EndingRewardJumpCue.StartHeadFlight),
                J.Hold(S.SuitedHelmetJumpTilted),
            ],
            [A.HeadJump] =
            [
                J.Show(S.HelmetlessJumpHead, 10),
                J.Raise(EndingRewardJumpCue.StartHeadFlight),
                J.Hold(S.HelmetlessJumpHead),
            ],
            [A.Falling] = [J.Hold(S.SamusFalling)],
            [A.Landing] =
            [
                J.Show(S.SamusLanding, 10),
                J.Show(S.SamusLanded, 16),
                J.Show(S.SamusShooting, 48),
                J.Raise(EndingRewardJumpCue.Shoot),
                J.Show(S.SamusShooting, 128),
            ],
        },
        new Dictionary<EndingRewardAnimation, Animation<EndingRewardAnimation>>
        {
            [A.SuitlessJump] = CinematicPlays.Through(A.SuitlessJump, 5, AnimationEnd.Holds),
            [A.SuitedJump] = CinematicPlays.Through(A.SuitedJump, 6, AnimationEnd.Holds),
            [A.HelmetJump] = CinematicPlays.Through(A.HelmetJump, 3, AnimationEnd.Holds),
            [A.HeadJump] = CinematicPlays.Through(A.HeadJump, 3, AnimationEnd.Holds),
            [A.Falling] = CinematicPlays.Still(A.Falling),
            [A.Landing] = CinematicPlays.Through(A.Landing, 5, AnimationEnd.Stops),
        });
}
