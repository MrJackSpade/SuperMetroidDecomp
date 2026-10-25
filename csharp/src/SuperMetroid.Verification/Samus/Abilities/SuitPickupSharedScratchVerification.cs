using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Poses;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
namespace SuperMetroid.Verification.Samus.Abilities;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the suit pickup's writes to the scratch words it shares with the shinespark and Crystal Flash.</summary>
internal static class SuitPickupSharedScratchVerification
{
    /// <summary>Upper bound on stage calls for one stage, far above the longest stage, so a stalled stage fails.</summary>
    private const int MaximumCallsPerStage = 300;

    /// <summary>
    /// Drives a Varia transformation through every stage and checks, after each stage call, that the stage number
    /// and the light-beam word reach the shinespark's acceleration pair and Crystal Flash's ammunition timer exactly:
    /// the scanline count while the beam grows, during the reveal and while it is wiped, and the edge columns, left
    /// in the low byte, while it is a column or centre line. The dissipation's first call wraps the left column from
    /// 255 to 7 within the eight-bit window column.
    /// </summary>
    internal static void VerifySuitPickupSharedScratch()
    {
        SamusState samus = PrepareRetailSamusFixture(new SamusState
        {
            Pose = SamusPoseId.FacingRightNormalPose,
            Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)),
        });
        var pickup = new SamusSuitPickupState();
        var cgram = new SamusColourRig();
        pickup.Begin(samus, new Position16<RoomPixels>(0, 0), SamusSuitPickupKind.Varia);
        AssertScratch(samus, 0, 0, "setup");

        // Two instruction-list calls precede the first stage call and leave the scratch untouched.
        pickup.Step(samus, cgram.Samus);
        pickup.Step(samus, cgram.Samus);
        AssertScratch(samus, 0, 0, "instruction-list setup");

        // Stage zero: the word is the grown scanline count, until the stage hands over the narrow beam's edges.
        StepThrough(pickup, samus, cgram, SamusSuitPickupStage.LightBeamAppears,
            context => AssertScratch(samus, 0, checked((ushort)pickup.Beam.Progress), context));
        AssertScratch(samus, 1, SuitPickupBeamDefinitions.NarrowBeamEdges.Word, "uniform widening starts");
        pickup.Step(samus, cgram.Samus);
        AssertScratch(samus, 1, SnesLittleEndian.Word(119, 121), "first uniform widening call");

        // Columns: the word is the edge pair, left column in the low byte, until the curved stage reaches full width.
        StepThrough(pickup, samus, cgram, SamusSuitPickupStage.UniformWidening,
            context => AssertScratch(samus, 1, BeamEdgeWord(pickup), context));
        AssertScratch(samus, 2, SuitPickupBeamDefinitions.CurvedWideningStart.Word, "curved widening starts");
        StepThrough(pickup, samus, cgram, SamusSuitPickupStage.CurvedWidening,
            context => AssertScratch(samus, 2, BeamEdgeWord(pickup), context));
        AssertScratch(samus, 3, 0, "full-width beam clears the word for the reveal");
        pickup.Step(samus, cgram.Samus);
        AssertEqual(SamusSuitPickupStage.LightBeamShrinks, pickup.Stage, "the reveal hands over to the shrinking stage");
        AssertScratch(samus, 4, 0, "reveal");

        // Stage four: the word is the wipe's scanline count, until the stage hands over the dissipation edges.
        StepThrough(pickup, samus, cgram, SamusSuitPickupStage.LightBeamShrinks,
            context => AssertScratch(samus, 4, checked((ushort)pickup.Beam.Progress), context));
        AssertScratch(samus, 5, SuitPickupBeamDefinitions.DissipationStart.Word, "dissipation starts");
        pickup.Step(samus, cgram.Samus);
        AssertEqual(7, pickup.Beam.Left.X, "the first dissipation call wraps the left column from 255 to 7");
        AssertScratch(samus, 5, SnesLittleEndian.Word(7, 240), "first dissipation call");
        StepThrough(pickup, samus, cgram, SamusSuitPickupStage.LightBeamDissipates,
            context => AssertScratch(samus, 5, BeamEdgeWord(pickup), context));
        AssertScratch(samus, 6, BeamEdgeWord(pickup), "the last centre line awaits cleanup");
        pickup.Step(samus, cgram.Samus);
        AssertTrue(!pickup.IsActive, "cleanup ends the transformation");
        AssertScratch(samus, 0, 0, "cleanup");
        Console.WriteLine("Suit pickup shared scratch: every stage publishes its number and exact light-beam word to the spark and Crystal Flash.");
    }

    /// <summary>Runs stage calls while <paramref name="stage"/> stays current, checking the scratch after each call that keeps it.</summary>
    private static void StepThrough(SamusSuitPickupState pickup, SamusState samus, SamusColourRig cgram,
        SamusSuitPickupStage stage, Action<string> checkWithinStage)
    {
        AssertEqual(stage, pickup.Stage, $"{stage} is current before its calls");
        for (int call = 1; call <= MaximumCallsPerStage; call++)
        {
            pickup.Step(samus, cgram.Samus);
            if (pickup.Stage != stage)
                return;
            checkWithinStage($"{stage} call {call}");
        }
        throw new InvalidOperationException($"Suit pickup stage {stage} did not end within {MaximumCallsPerStage} calls.");
    }

    /// <summary>The published beam's edge columns as one word, left column in the low byte.</summary>
    private static ushort BeamEdgeWord(SamusSuitPickupState pickup) =>
        SnesLittleEndian.Word(checked((byte)pickup.Beam.Left.X), checked((byte)pickup.Beam.Right.X));

    /// <summary>Checks that the spark acceleration and Crystal Flash ammunition timer hold the expected scratch words.</summary>
    private static void AssertScratch(SamusState samus, ushort stageNumber, ushort lightBeamWord, string context)
    {
        AssertEqual(SignedSixteenSixteen.FromParts(unchecked((short)stageNumber), lightBeamWord),
            samus.Shinespark.VerticalAcceleration, $"{context}: spark acceleration holds the stage number and light-beam word");
        AssertEqual(stageNumber, samus.CrystalFlash.AmmoDecrementTimer.RawValue,
            $"{context}: Crystal Flash ammunition timer holds the stage number");
    }

    /// <summary>Runs the suit pickup shared-scratch check.</summary>
    [VerificationCommand("--suit-pickup-shared-scratch")]
    internal static int SuitPickupSharedScratchCommand()
    {
        Suite(nameof(VerifySuitPickupSharedScratch), () => VerifySuitPickupSharedScratch());
        return 0;
    }
}
