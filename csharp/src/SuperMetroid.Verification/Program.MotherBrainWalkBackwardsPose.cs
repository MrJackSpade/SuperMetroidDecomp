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

internal static class MotherBrainWalkBackwardsPoseVerification
{
    // #1269: $A9:C647 tests the target, then the body pose, and only a standing body
    // reaches the $30 arena limit. The port tested the limit regardless of pose, so in the
    // 100% movie a mid-step body at X $29 finished the painful backward walk two frames
    // before native and the neck sped up early.
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
        AssertEqual((ushort)0, midStep.PainfulWalkingFunctionTimer, "a mid-step body short of the target has not arrived");

        MotherBrainRainbowBeamAttackSequence standing = Walking(0x0029, pose: MotherBrainBodyPose.Standing);
        standing.StepPainfulWalking();
        AssertEqual((ushort)0x0010, standing.PainfulWalkingFunctionTimer, "a standing body inside the $30 limit has arrived");

        MotherBrainRainbowBeamAttackSequence arrived = Walking(0x0026, pose: MotherBrainBodyPose.Walking);
        arrived.StepPainfulWalking();
        AssertEqual((ushort)0x0010, arrived.PainfulWalkingFunctionTimer, "a mid-step body at the target has arrived");
        Console.WriteLine("Mother Brain walk backwards: pose gates the $30 limit, not the target.");
    }

    [VerificationCommand("--mother-brain-walk-backwards-pose")]
    internal static int MotherBrainWalkBackwardsPoseCommand()
    {
        VerifyMotherBrainWalkBackwardsPose();
        return 0;
    }
}
