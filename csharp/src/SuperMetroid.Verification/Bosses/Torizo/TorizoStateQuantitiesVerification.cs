using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Programs;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Bosses.Torizo;

/// <summary>
/// Checks Torizo's widened state: the decision and space-jump counts wrap to zero past their largest value, the
/// animation lock follows its cues, the jump acceleration changes only the vertical velocity, and the posture and
/// behaviour cues keep the parameter words' other bits.
/// </summary>
internal static class TorizoStateQuantitiesVerification
{
    /// <summary>Runs the count, lock, acceleration and flag-word checks.</summary>
    internal static void VerifyTorizoStateQuantities()
    {
        var enemies = new RoomEnemySystem(new EnemySession());
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        var state = new TorizoEnemyState(slot, isGolden: true);
        enemies.BombTorizoBehavior.Access.TorizoState = state;
        var inputs = new TorizoInstructionInputs { Samus = null, Level = null, ControllerInput = 0, FrameCounterLow = 0 };
        void Apply(TorizoCueKind kind) =>
            AssertTrue(enemies.BombTorizoBehavior.ApplyTorizoCue(slot, state, inputs, new TorizoCue.Command(kind),
                    AnimationProgramCursor<TorizoBodyProgram>.Start(TorizoBodyProgram.BombTorizoInitial1)) is null,
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

        var jump = new EightEightVector<RoomPixels>(-768, -16) + new EightEightVector<RoomPixels>(0, 40);
        AssertEqual((short)-768, jump.X.SignedRawValue, "the jump acceleration leaves the horizontal velocity unchanged");
        AssertEqual((short)24, jump.Y.SignedRawValue, "the jump acceleration adds to the vertical velocity across zero");

        const int lowParameterBits = 4660;
        state.PostureParameterLowBits = lowParameterBits;
        Apply(TorizoCueKind.SteppedRightWithLeftFoot);
        // The cartridge's parameter-1 posture bits: facing right $8000, the opposite leg forward $2000, turning $4000.
        AssertEqual(lowParameterBits + 0xa000,
            TorizoParameterFlagCodec.ToParameterWord(state.Posture, state.PostureParameterLowBits), "a stepping cue replaces the posture bits and keeps parameter 1's low bits");
        Apply(TorizoCueKind.SetTurningAround);
        AssertEqual(lowParameterBits + 0xe000, TorizoParameterFlagCodec.ToParameterWord(state.Posture, state.PostureParameterLowBits),
            "the turning cue adds its posture bit alone");

        state.Behavior = TorizoBehaviorFlags.Stunned | TorizoBehaviorFlags.FaceBlownUp;
        Apply(TorizoCueKind.UnmarkStunned);
        AssertEqual(TorizoBehaviorFlags.FaceBlownUp, state.Behavior, "clearing the stun keeps the other latches");
        Apply(TorizoCueKind.MarkFaceBlownUp);
        AssertEqual(TorizoBehaviorFlags.FaceBlownUp, state.Behavior, "setting a held latch leaves the word unchanged");

        Console.WriteLine("Torizo state quantities: count wraps, animation lock, jump acceleration and parameter flag words pass.");
    }

    /// <summary>Runs Torizo's state quantity checks.</summary>
    [VerificationCommand("--torizo-state-quantities")]
    internal static int TorizoStateQuantitiesCommand()
    {
        Suite(nameof(VerifyTorizoStateQuantities), () => VerifyTorizoStateQuantities());
        return 0;
    }
}
