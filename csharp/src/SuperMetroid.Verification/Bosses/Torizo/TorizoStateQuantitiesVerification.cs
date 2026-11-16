using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Verification.Bosses.Torizo;

/// <summary>
/// Checks Torizo's widened state: a stepping cue counts a decision step, the space-jump count wraps to zero past 65535
/// (#1945), the animation lock follows its cues, the jump acceleration changes only the vertical velocity, and the
/// posture and behaviour cues replace or add only the latches they name.
/// </summary>
internal static class TorizoStateQuantitiesVerification
{
    /// <summary>Runs the count, lock, acceleration and flag-word checks.</summary>
    internal static void VerifyTorizoStateQuantities()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom());
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        var state = new TorizoEnemyState(isGolden: true);
        slot.SetSpeciesState(state);
        var inputs = new TorizoCueInputs { Samus = SuperMetroid.Core.Enemies.RoomEnemyFrameFixtures.StandaloneSamus(), Level = SuperMetroid.Core.Enemies.RoomEnemyFrameFixtures.OpenRoom(new(64, 64)), ControllerInput = 0, FrameCounter = 0 };
        void Apply(TorizoCueKind kind) =>
            AssertTrue(enemies.TorizoBehavior.ApplyTorizoCue(slot, state, inputs, new TorizoCue.Signal(kind),
                    TorizoAnimations.Start(TorizoAnimation.BombTorizoAwakening)) is null,
                $"{kind} runs on");

        state.DecisionCounter = 7;
        Apply(TorizoCueKind.SteppedLeftWithLeftFoot);
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

        Console.WriteLine("Torizo state quantities: decision count, space-jump count wrap, animation lock, jump acceleration and posture and behaviour latches pass.");
    }

    /// <summary>Runs Torizo's state quantity checks.</summary>
    [VerificationCommand("--torizo-state-quantities")]
    internal static int TorizoStateQuantitiesCommand()
    {
        Suite(nameof(VerifyTorizoStateQuantities), () => VerifyTorizoStateQuantities());
        return 0;
    }
}
