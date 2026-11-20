using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Verification.Bosses.Torizo;

/// <summary>
/// Checks Torizo's widened state: a stepping cue counts a decision step, the space-jump count wraps to zero past 65535
/// (#1945), the animation lock follows its cues, and the posture and behaviour cues replace or add only what they name:
/// a step the whole posture, the turning cue the turn alone (#2237).
/// </summary>
internal static class TorizoStateQuantitiesVerification
{
    /// <summary>Runs the count, lock, acceleration and flag-word checks.</summary>
    internal static void VerifyTorizoStateQuantities()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom());
        RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
        var state = new TorizoEnemyState(TorizoVariant.Golden);
        place.SetSpeciesState(state);
        var inputs = new TorizoCueInputs { Torizo = LivingTorizo.Instance, Samus = SuperMetroid.Core.Enemies.RoomEnemyFrameFixtures.StandaloneSamus(), Level = SuperMetroid.Core.Enemies.RoomEnemyFrameFixtures.OpenRoom(new(64, 64)), ControllerInput = 0, FrameCounter = 0 };
        void ApplyCue(TorizoCue cue) =>
            AssertTrue(enemies.TorizoBehavior.ApplyTorizoCue(place, state, inputs, cue,
                    TorizoAnimations.Start(TorizoAnimation.BombTorizoAwakening)) is null,
                $"{cue} runs on");
        void Apply(TorizoCueKind kind) => ApplyCue(new TorizoCue.Signal(kind));

        state.DecisionCounter = 7;
        ApplyCue(new TorizoCue.Stepped(HorizontalDirection.Left, Foot: HorizontalDirection.Left));
        AssertEqual(8, state.DecisionCounter, "a stepping cue counts one decision step");

        state.SamusSpaceJumpFrames = 360;
        state.CountSamusSpaceJumpFrame();
        AssertEqual(361, state.SamusSpaceJumpFrames, "a space-jumping frame counts one frame");
        state.SamusSpaceJumpFrames = SuperMetroid.Core.Compatibility.WrappingCounter.LargestCount;
        state.CountSamusSpaceJumpFrame();
        AssertEqual(0, state.SamusSpaceJumpFrames, "counting past the largest space-jump count wraps to zero");

        Apply(TorizoCueKind.LockAnimation);
        AssertTrue(state.AnimationLocked, "the lock cue locks the animation");
        Apply(TorizoCueKind.UnlockAnimation);
        AssertTrue(!state.AnimationLocked, "the unlock cue unlocks the animation");

        state.Posture = TorizoPosture.Initial.TurningAround;
        ApplyCue(new TorizoCue.Stepped(HorizontalDirection.Right, Foot: HorizontalDirection.Left));
        AssertEqual(TorizoPosture.AfterStep(HorizontalDirection.Right, HorizontalDirection.Left), state.Posture,
            "a stepping cue replaces the whole posture: its facing, its leading foot and the turn");
        Apply(TorizoCueKind.SetTurningAround);
        AssertEqual(TorizoPosture.AfterStep(HorizontalDirection.Right, HorizontalDirection.Left).TurningAround,
            state.Posture, "the turning cue sets the turn alone");

        state.Stunned = true;
        Apply(TorizoCueKind.UnmarkStunned);
        AssertTrue(!state.Stunned, "the unmark cue clears the stun");
        Apply(TorizoCueKind.MarkFaceBlownUp);
        AssertTrue(state.FaceBlownUp, "the mark cue blows up the face");

        Console.WriteLine("Torizo state quantities: decision count, space-jump count wrap, animation lock, posture and behaviour latches pass.");
    }

    /// <summary>Runs Torizo's state quantity checks.</summary>
    [VerificationCommand("--torizo-state-quantities")]
    internal static int TorizoStateQuantitiesCommand()
    {
        Suite(nameof(VerifyTorizoStateQuantities), () => VerifyTorizoStateQuantities());
        return 0;
    }
}
