using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using System.Reflection;
using SuperMetroid.Core.Game;

/// <summary>Checks how Mother Brain's painful backward walk admits its target and arena limit.</summary>
internal static class MotherBrainWalkBackwardsPoseVerification
{
    /// <summary>
    /// Confirms a mid-step body short of the target ignores the $30 arena limit, while a
    /// standing body at that position finishes the walk as native does.
    /// </summary>
    internal static void VerifyMotherBrainWalkBackwardsPose()
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;

        MotherBrainRainbowBeamAttackSequence Walking(ushort x, MotherBrainBodyPose pose)
        {
            var sequence = new MotherBrainRainbowBeamAttackSequence();
            sequence.PainfulWalkingForward = false;
            sequence.PainfulWalkingAnimationDelay = (ushort)0x0002;
            sequence.Body.Position = sequence.Body.Position.WithX(x);
            sequence.Body.Pose = pose;
            return sequence;
        }

        MotherBrainRainbowBeamAttackSequence midStep = Walking(0x0029, pose: MotherBrainBodyPose.Walking);
        midStep.StepPainfulWalking();
        AssertEqual((ushort)0, midStep.PainfulWalkingFunctionTimer.RawValue, "a mid-step body short of the target has not arrived");

        MotherBrainRainbowBeamAttackSequence standing = Walking(0x0029, pose: MotherBrainBodyPose.Standing);
        standing.StepPainfulWalking();
        AssertEqual((ushort)0x0010, standing.PainfulWalkingFunctionTimer.RawValue, "a standing body inside the $30 limit has arrived");

        MotherBrainRainbowBeamAttackSequence arrived = Walking(0x0026, pose: MotherBrainBodyPose.Walking);
        arrived.StepPainfulWalking();
        AssertEqual((ushort)0x0010, arrived.PainfulWalkingFunctionTimer.RawValue, "a mid-step body at the target has arrived");
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
