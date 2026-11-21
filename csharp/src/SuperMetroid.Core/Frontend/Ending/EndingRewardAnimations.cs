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
internal static partial class EndingRewardAnimations
{
    /// <summary>How long the suited Samus's arm stays hidden while her body stands idle, before the arm's gesture shows.</summary>
    private const int ArmHiddenUpdates = 64;

    /// <summary>The hair release's pictures before Samus stands whole: her suitless legs stand still beneath them for as long.</summary>
    private static AnimationBeat<CinematicSpritePose, EndingRewardGestureCue>[] HairLoosening =>
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
    ];

    /// <summary>
    /// The suited arm's gesture before it hands off to the jump: the body, the helmet and the helmetless head play beside it
    /// for exactly as long (#2557).
    /// </summary>
    private static AnimationBeat<CinematicSpritePose, EndingRewardGestureCue>[] ArmGesture =>
    [
        G.Hide(ArmHiddenUpdates),
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
    ];

    /// <summary>The helmetless head's pictures before its smile holds to the end of the arm's gesture.</summary>
    private static AnimationBeat<CinematicSpritePose, EndingRewardGestureCue>[] HeadStartingToSmile =>
    [
        G.Show(S.HelmetlessHeadResting, 128),
        G.Show(S.HelmetlessHeadSmileStarting, 5),
        G.Show(S.HelmetlessHeadSmileWidening, 5),
    ];

    /// <summary>How long the arm's gesture lasts before it hands off to the jump.</summary>
    private static int ArmGestureUpdates => AnimationBeats<CinematicSpritePose, EndingRewardGestureCue>.UpdatesOf(ArmGesture);

    /// <summary>
    /// The gesture's animations: the suitless legs stand still through the hair release's opening pictures, and the suited
    /// body, helmet and helmetless head last as long as the arm's gesture, so no duration restates another animation's length.
    /// </summary>
    internal static CinematicAnimations<EndingRewardAnimation, CinematicSpritePose, EndingRewardGestureCue> Gesture { get; } = new(
        new Dictionary<EndingRewardAnimation, AnimationBeat<CinematicSpritePose, EndingRewardGestureCue>[]>
        {
            [A.HairRelease] =
            [
                .. HairLoosening,
                G.Show(S.SuitlessStanding, 48),
                G.Raise(EndingRewardGestureCue.StartJump),
            ],
            [A.HairReleaseLegs] = [G.Show(S.SuitlessLower, AnimationBeats<CinematicSpritePose, EndingRewardGestureCue>.UpdatesOf(HairLoosening))],
            [A.GestureBody] =
            [
                G.Show(S.SuitedIdleBody, ArmHiddenUpdates),
                G.Show(S.SuitedHeadlessBody, ArmGestureUpdates - ArmHiddenUpdates),
            ],
            [A.GestureArm] =
            [
                .. ArmGesture,
                G.Raise(EndingRewardGestureCue.StartJump),
            ],
            [A.GestureHelmet] = [G.Show(S.SuitedHelmetHead, ArmGestureUpdates)],
            [A.GestureHead] =
            [
                .. HeadStartingToSmile,
                G.Show(S.HelmetlessHeadSmiling,
                    ArmGestureUpdates - AnimationBeats<CinematicSpritePose, EndingRewardGestureCue>.UpdatesOf(HeadStartingToSmile)),
            ],
        },
        plays => new Dictionary<EndingRewardAnimation, Animation<EndingRewardAnimation>>
        {
            [A.HairRelease] = plays.Through(A.HairRelease, AnimationEnd.Stops),
            [A.HairReleaseLegs] = plays.Through(A.HairReleaseLegs, AnimationEnd.Stops),
            [A.GestureBody] = plays.Through(A.GestureBody, AnimationEnd.Stops),
            [A.GestureArm] = plays.Through(A.GestureArm, AnimationEnd.Stops),
            [A.GestureHelmet] = plays.Through(A.GestureHelmet, AnimationEnd.Stops),
            [A.GestureHead] = plays.Through(A.GestureHead, AnimationEnd.Stops),
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
        plays => new Dictionary<EndingRewardAnimation, Animation<EndingRewardAnimation>>
        {
            [A.SuitlessJump] = plays.Through(A.SuitlessJump, AnimationEnd.Holds),
            [A.SuitedJump] = plays.Through(A.SuitedJump, AnimationEnd.Holds),
            [A.HelmetJump] = plays.Through(A.HelmetJump, AnimationEnd.Holds),
            [A.HeadJump] = plays.Through(A.HeadJump, AnimationEnd.Holds),
            [A.Falling] = plays.Still(A.Falling),
            [A.Landing] = plays.Through(A.Landing, AnimationEnd.Stops),
        });
}
