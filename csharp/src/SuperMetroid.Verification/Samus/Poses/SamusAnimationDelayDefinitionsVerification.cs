using SuperMetroid.Core.Samus.Drawing;
using static SuperMetroid.Verification.Samus.Movement.RunningCadenceVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
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
            IReadOnlyList<SamusAnimationSequence> expected = SamusAnimationProgramCodec.Decode(cartridge, pose);
            IReadOnlyList<SamusAnimationSequence> actual = SamusAnimationDelays.ProgramForPose(pose).Sequences;
            AssertEqual(expected.Count, actual.Count, $"pose {pose} animation run count");
            for (int run = 0; run < expected.Count; run++)
            {
                AssertEqual(expected[run].FirstFrame, actual[run].FirstFrame, $"pose {pose} run {run} first frame");
                AssertSequenceEqual(expected[run].Delays, actual[run].Delays, $"pose {pose} run {run} delays");
                AssertEqual(expected[run].End, actual[run].End, $"pose {pose} run {run} instruction");
            }
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
