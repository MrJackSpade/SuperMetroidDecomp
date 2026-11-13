using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Verification.Bosses.Torizo;

/// <summary>
/// Checks Torizo's widened state: the decision and space-jump counts wrap to zero past their largest value, the
/// animation lock follows its cues, the jump acceleration changes only the vertical velocity, and the posture and
/// behaviour cues replace or add only the latches they name.
/// </summary>
internal static class TorizoStateQuantitiesVerification
{
    /// <summary>Runs the count, lock, acceleration and flag-word checks.</summary>
    internal static void VerifyTorizoStateQuantities()
    {
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom());
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        var state = new TorizoEnemyState(slot, isGolden: true);
        enemies.BombTorizoBehavior.Access.TorizoState = state;
        var inputs = new TorizoCueInputs { Samus = null, Level = null, ControllerInput = 0, FrameCounter = 0 };
        void Apply(TorizoCueKind kind) =>
            AssertTrue(enemies.BombTorizoBehavior.ApplyTorizoCue(slot, state, inputs, new TorizoCue.Signal(kind),
                    TorizoAnimations.Start(TorizoBodySegment.BombTorizoInitial0Resumed)) is null,
                $"{kind} runs on");

        state.DecisionCounter = 7;
        Apply(TorizoCueKind.SteppedLeftWithLeftFoot);
        AssertEqual(8, state.DecisionCounter, "a stepping cue counts one decision step");
        state.DecisionCounter = TorizoEnemyState.LargestCount;
        Apply(TorizoCueKind.SteppedRightWithRightFoot);
        AssertEqual(0, state.DecisionCounter, "counting past the largest decision count wraps to zero");

        state.SamusSpaceJumpFrames = 360;
        state.CountSamusSpaceJumpFrame();
        AssertEqual(361, state.SamusSpaceJumpFrames, "a space-jumping frame counts one frame");
        state.SamusSpaceJumpFrames = TorizoEnemyState.LargestCount;
        state.CountSamusSpaceJumpFrame();
        AssertEqual(0, state.SamusSpaceJumpFrames, "counting past the largest space-jump count wraps to zero");

        Apply(TorizoCueKind.LockAnimation);
        AssertTrue(state.AnimationLocked, "the lock cue locks the animation");
        Apply(TorizoCueKind.UnlockAnimation);
        AssertTrue(!state.AnimationLocked, "the unlock cue unlocks the animation");

        var jump = EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(-768, -16) + EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(0, 40);
        AssertEqual(-768, jump.X.InTwoHundredFiftySixthsOfPixel, "the jump acceleration leaves the horizontal velocity unchanged");
        AssertEqual(24, jump.Y.InTwoHundredFiftySixthsOfPixel, "the jump acceleration adds to the vertical velocity across zero");

        state.Posture = TorizoPostureFlags.TurningAround;
        Apply(TorizoCueKind.SteppedRightWithLeftFoot);
        AssertEqual(TorizoPostureFlags.FacingRight | TorizoPostureFlags.OppositeLegForward, state.Posture,
            "a stepping cue replaces all three posture latches");
        Apply(TorizoCueKind.SetTurningAround);
        AssertEqual(TorizoPostureFlags.FacingRight | TorizoPostureFlags.OppositeLegForward | TorizoPostureFlags.TurningAround,
            state.Posture, "the turning cue adds its posture latch alone");

        state.Behavior = TorizoBehaviorFlags.Stunned | TorizoBehaviorFlags.FaceBlownUp;
        Apply(TorizoCueKind.UnmarkStunned);
        AssertEqual(TorizoBehaviorFlags.FaceBlownUp, state.Behavior, "clearing the stun keeps the other latches");
        Apply(TorizoCueKind.MarkFaceBlownUp);
        AssertEqual(TorizoBehaviorFlags.FaceBlownUp, state.Behavior, "setting a held latch leaves the word unchanged");

        Console.WriteLine("Torizo state quantities: count wraps, animation lock, jump acceleration and posture and behaviour latches pass.");
    }

    /// <summary>Runs Torizo's state quantity checks.</summary>
    [VerificationCommand("--torizo-state-quantities")]
    internal static int TorizoStateQuantitiesCommand()
    {
        Suite(nameof(VerifyTorizoStateQuantities), () => VerifyTorizoStateQuantities());
        return 0;
    }
}
