using SuperMetroid.Core.Bosses;
using SuperMetroid.Core.Bosses.MotherBrain;
namespace SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Verification.Harness;

/// <summary>Checks how Mother Brain's painful backward walk admits its target and arena limit.</summary>
internal static class MotherBrainWalkBackwardsPoseVerification
{
    /// <summary>
    /// Confirms a mid-step body short of the target ignores the $30 arena limit, while a
    /// standing body at that position finishes the walk as native does.
    /// </summary>
    internal static void VerifyMotherBrainWalkBackwardsPose()
    {

        MotherBrainRainbowBeamAttackSequence Walking(ushort x, MotherBrainBodyPose pose)
        {
            var sequence = MotherBrainLateFightFixtures.NewSequence();
            // The stagger walks back in its second stage, at its fastest.
            global::SuperMetroid.Core.PrivateState.Set(sequence.DrainedByBaby, "PainfulWalkingStage", (global::SuperMetroid.Core.Bosses.ListStep<global::SuperMetroid.Core.Bosses.MotherBrain.MotherBrainStaggerStage>)(SuperMetroid.Core.Bosses.ListStep<MotherBrainStaggerStage>.At(MotherBrainPainfulWalkingDefinitions.Stages, 1)));
            global::SuperMetroid.Core.PrivateState.Set(sequence.DrainedByBaby, "PainfulWalkingSpeed", (global::SuperMetroid.Core.Bosses.MotherBrain.MotherBrainWalkSpeed)(MotherBrainWalkSpeed.ReallyFast));
            sequence.Encounter.Body.Position = sequence.Encounter.Body.Position.WithWholeX(x);
            sequence.Encounter.Pose = pose;
            return sequence;
        }

        MotherBrainRainbowBeamAttackSequence midStep = Walking(0x0029, pose: MotherBrainBodyPose.Walking);
        midStep.DrainedByBaby.StepPainfulWalking();
        AssertTrue(midStep.DrainedByBaby.PainfulWalkingPause is null, "a mid-step body short of the target has not arrived");

        MotherBrainRainbowBeamAttackSequence standing = Walking(0x0029, pose: MotherBrainBodyPose.Standing);
        standing.DrainedByBaby.StepPainfulWalking();
        AssertEqual(0x0010 - 1, standing.DrainedByBaby.PainfulWalkingPause?.CartridgeCount, "a standing body inside the $30 limit has arrived");

        MotherBrainRainbowBeamAttackSequence arrived = Walking(0x0026, pose: MotherBrainBodyPose.Walking);
        arrived.DrainedByBaby.StepPainfulWalking();
        AssertEqual(0x0010 - 1, arrived.DrainedByBaby.PainfulWalkingPause?.CartridgeCount, "a mid-step body at the target has arrived");
        Console.WriteLine("Mother Brain walk backwards: pose gates the $30 limit, not the target.");
    }

    /// <summary>Runs the focused backward-walk pose gate check.</summary>
    [VerificationCommand("--mother-brain-walk-backwards-pose")]
    internal static int MotherBrainWalkBackwardsPoseCommand()
    {
        VerifyMotherBrainWalkBackwardsPose();
        return 0;
    }
}
