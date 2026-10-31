using SuperMetroid.Core.Samus.Drawing;
using static SuperMetroid.Verification.Samus.Movement.RunningCadenceVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Samus;
namespace SuperMetroid.Verification.Samus.Poses;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Samus's typed pose animation programs against the cartridge's (#1473).</summary>
internal static class SamusAnimationDelayDefinitionsVerification
{
    /// <summary>
    /// Decodes every pose's program from the cartridge and requires the typed program the pose animates by to hold the
    /// same runs: each run's first frame, its delays and the instruction that ends it.
    /// </summary>
    internal static void VerifySamusAnimationDelayDefinitions(ISnesAddressSpace bus)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(bus);
        for (int number = 0; number < SamusAnimationProgramCodec.PoseCount; number++)
        {
            SamusPoseId pose = SamusPoseNumbering.FromNumber(number);
            SamusAnimationProgram actual = SamusAnimationDelays.ProgramForPose(pose);
            int nextFrame = 0;
            foreach (SamusAnimationSequence run in SamusAnimationProgramCodec.Decode(cartridge, pose))
            {
                // Frame numbers between two runs name no step: they held the previous instruction's operands.
                for (; nextFrame < run.FirstFrame; nextFrame++)
                {
                    int gap = nextFrame;
                    AssertThrows<InvalidDataException>(() => actual.StepAt(gap), $"pose {pose} frame {gap} is no step");
                }
                for (int index = 0; index < run.Delays.Count; index++, nextFrame++)
                    AssertEqual<SamusAnimationProgramStep>(new SamusAnimationDelay(run.Delays[index]), actual.StepAt(nextFrame),
                        $"pose {pose} frame {nextFrame} delay");
                AssertEqual<SamusAnimationProgramStep>(run.End, actual.StepAt(nextFrame), $"pose {pose} frame {nextFrame} instruction");
                nextFrame++;
            }
            int past = nextFrame;
            AssertThrows<InvalidDataException>(() => actual.StepAt(past), $"pose {pose} has no step past its program");
        }

        // The three numbers past the pose records name no program: no gameplay path gives Samus one of them.
        foreach (SamusPoseId adjacent in new[] { SamusPoseId.AdjacentCodePoseFD, SamusPoseId.AdjacentCodePoseFE, SamusPoseId.AdjacentCodePoseFF })
            AssertThrows<ArgumentOutOfRangeException>(() => SamusAnimationDelays.ProgramForPose(adjacent),
                $"pose {adjacent} has no animation program");

        var flashbackSamus = new SamusState
        {
            Pose = SamusPoseId.FacingLeftNormalPose,
        };
        SamusPresentation.InitializeAnimation(flashbackSamus);
        AssertEqual(SamusPoseId.FacingLeftNormalPose, flashbackSamus.AnimatedPose,
            "Mother Brain flashback pose starts its typed animation program");
        Console.WriteLine("Samus pose animation: all 253 typed programs match the cartridge's runs, delays and instructions.");
    }

    /// <summary>Runs pose-program and running-cadence checks against the retail ROM.</summary>
    [VerificationCommand("--mutable-animation-aliases")]
    internal static int MutableAnimationAliasesCommand()
    {
        string sourceRom = Path.GetFullPath("Super Metroid.smc");
        SuperMetroidAddressSpace bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(sourceRom);
        Suite(nameof(VerifySamusAnimationDelayDefinitions), () => VerifySamusAnimationDelayDefinitions(bus));
        Suite(nameof(VerifyRunningCadence), () => VerifyRunningCadence(bus));
        return 0;
    }
}
