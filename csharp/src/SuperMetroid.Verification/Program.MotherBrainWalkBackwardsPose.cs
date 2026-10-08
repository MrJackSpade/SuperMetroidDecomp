using System.Reflection;
using SuperMetroid.Core.Game;

internal static partial class Program
{
    // #1269: $A9:C647 tests the target, then the body pose, and only a standing body
    // reaches the $30 arena limit. The port tested the limit regardless of pose, so in the
    // 100% movie a mid-step body at X $29 finished the painful backward walk two frames
    // before native and the neck sped up early.
    private static void VerifyMotherBrainWalkBackwardsPose()
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        MethodInfo step = typeof(MotherBrainRainbowBeamAttackSequence).GetMethod("StepPainfulWalking", flags)!;
        PropertyInfo forward = typeof(MotherBrainRainbowBeamAttackSequence).GetProperty(
            nameof(MotherBrainRainbowBeamAttackSequence.PainfulWalkingForward))!;

        MotherBrainRainbowBeamAttackSequence Walking(ushort x, ushort pose)
        {
            var sequence = new MotherBrainRainbowBeamAttackSequence();
            forward.SetValue(sequence, false);
            typeof(MotherBrainRainbowBeamAttackSequence).GetProperty(
                    nameof(MotherBrainRainbowBeamAttackSequence.PainfulWalkingAnimationDelay))!
                .SetValue(sequence, (ushort)0x0002);
            sequence.Body.XPosition = x;
            sequence.Body.Pose = pose;
            return sequence;
        }

        MotherBrainRainbowBeamAttackSequence midStep = Walking(0x0029, pose: 1);
        step.Invoke(midStep, null);
        AssertEqual((ushort)0, midStep.PainfulWalkingFunctionTimer, "a mid-step body short of the target has not arrived");

        MotherBrainRainbowBeamAttackSequence standing = Walking(0x0029, pose: 0);
        step.Invoke(standing, null);
        AssertEqual((ushort)0x0010, standing.PainfulWalkingFunctionTimer, "a standing body inside the $30 limit has arrived");

        MotherBrainRainbowBeamAttackSequence arrived = Walking(0x0026, pose: 1);
        step.Invoke(arrived, null);
        AssertEqual((ushort)0x0010, arrived.PainfulWalkingFunctionTimer, "a mid-step body at the target has arrived");
        Console.WriteLine("Mother Brain walk backwards: pose gates the $30 limit, not the target.");
    }
}
