using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
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
                    AssertTrue(actual.TryStepAt(gap) is null, $"pose {pose} frame {gap} is no step");
                }
                for (int index = 0; index < run.Delays.Count; index++, nextFrame++)
                    AssertEqual<SamusAnimationProgramStep?>(new SamusAnimationDelay(run.Delays[index]), actual.TryStepAt(nextFrame),
                        $"pose {pose} frame {nextFrame} delay");
                AssertEqual<SamusAnimationProgramStep?>(run.End, actual.TryStepAt(nextFrame), $"pose {pose} frame {nextFrame} instruction");
                nextFrame++;
            }
            int past = nextFrame;
            AssertTrue(actual.TryStepAt(past) is null, $"pose {pose} has no step past its program");
        }

        // The three numbers past the pose records name no program: no gameplay path gives Samus one of them.
        foreach (SamusPoseId adjacent in new[] { SamusPoseId.AdjacentCodePoseFD, SamusPoseId.AdjacentCodePoseFE, SamusPoseId.AdjacentCodePoseFF })
            AssertThrows<ArgumentOutOfRangeException>(() => SamusAnimationDelays.ProgramForPose(adjacent),
                $"pose {adjacent} has no animation program");

        VerifyAnimationContinuation(cartridge);

        var flashbackSamus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence())
        {
            Pose = SamusPoseId.FacingLeftNormalPose,
        };
        SamusPresentation.InitializeAnimation(flashbackSamus);
        AssertEqual(SamusPoseId.FacingLeftNormalPose, flashbackSamus.AnimatedPose,
            "Mother Brain flashback pose starts its typed animation program");
        Console.WriteLine("Samus pose animation: all 253 typed programs match the cartridge's runs, delays and instructions.");
    }

    /// <summary>
    /// Decodes where the cartridge lays each pose's program out and requires the installed continuation to read past
    /// every program into the program laid out next, shifted by the distance between their starts, and past the last
    /// program into nothing. The Mother Brain glitch movie's update 1947 is the recorded case: the right-facing drained
    /// standing pose at frame 11 plays the left-facing program's frame 3 (#1473).
    /// </summary>
    private static void VerifyAnimationContinuation(IImportCartridgeSource cartridge)
    {
        var continuation = InstalledFixtures.AnimationContinuation;
        int[] starts = [.. Enumerable.Range(0, SamusAnimationProgramCodec.PoseCount)
            .Select(number => SamusAnimationProgramCodec.ProgramStart(cartridge, number))];
        for (int number = 0; number < SamusAnimationProgramCodec.PoseCount; number++)
        {
            SamusPoseId pose = SamusPoseNumbering.FromNumber(number);
            int start = starts[number];
            int[] later = [.. starts.Where(other => other > start)];
            if (later.Length == 0)
            {
                AssertTrue(continuation.ReadPast(pose, ushort.MaxValue) is null, $"pose {pose}'s program is laid out last");
                continue;
            }
            int next = later.Min();
            int shift = next - start;
            AssertTrue(continuation.ReadPast(pose, shift - 1) is null, $"pose {pose} reads nothing short of the next program");
            var read = continuation.ReadPast(pose, shift)
                ?? throw new InvalidOperationException($"Pose {pose} reads no program past its own.");
            AssertEqual(0, read.Frame, $"pose {pose} reads the next program's first frame");
            AssertEqual(shift, read.Shift, $"pose {pose}'s next program starts {shift} frames later");
            SamusPoseId laidOutNext = SamusPoseNumbering.FromNumber(Array.IndexOf(starts, next));
            AssertTrue(ReferenceEquals(SamusAnimationDelays.ProgramForPose(laidOutNext), SamusAnimationDelays.ProgramForPose(read.Program)),
                $"pose {pose} reads the program laid out after its own");
        }
        var drained = continuation.ReadPast(SamusPoseId.DrainedStandingRightPose, 11)
            ?? throw new InvalidOperationException("The right-facing drained standing pose reads no program past its own.");
        AssertTrue(ReferenceEquals(SamusAnimationDelays.ProgramForPose(SamusPoseId.DrainedStandingLeftPose),
            SamusAnimationDelays.ProgramForPose(drained.Program)), "drained standing right reads into drained standing left");
        AssertEqual(3, drained.Frame, "Mother Brain glitch movie update 1947 plays the left program's frame 3");
        AssertEqual(8, drained.Shift, "the left drained standing program starts eight frames later");
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
