using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Poses;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
namespace SuperMetroid.Verification.Samus.Abilities;
using SuperMetroid.Verification.Harness;

/// <summary>
/// Checks what the suit pickup leaves for the shinespark (<see cref="SuitPickupSparkCarryover"/>) and on the count Crystal
/// Flash's drain and Draygon's escape share (<see cref="SharedEscapeAndDrainCount"/>).
/// </summary>
internal static class SuitPickupCarryoverVerification
{
    /// <summary>Upper bound on stage calls for one stage, far above the longest stage, so a stalled stage fails.</summary>
    private const int MaximumCallsPerStage = 300;

    /// <summary>
    /// Drives a Varia transformation through every stage and checks, after each stage call, that the stage number
    /// and the light-beam word reach the shinespark's acceleration pair, and the stage number Crystal Flash's ammunition
    /// timer and Draygon's escape presses, exactly:
    /// the scanline count while the beam grows, during the reveal and while it is wiped, and the edge columns, left
    /// in the low byte, while it is a column or centre line. The dissipation's first call wraps the left column from
    /// 255 to 7 within the eight-bit window column.
    /// </summary>
    internal static void VerifySuitPickupCarryover()
    {
        SamusState samus = PrepareRetailSamusFixture(new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())
        {
            Pose = SamusPoseId.FacingRightNormalPose,
            Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)),
        });
        var pickup = new SamusSuitPickupState();
        var cgram = new SamusColours();
        pickup.Begin(samus, new Position16<RoomPixels>(0, 0), SamusSuitPickupKind.Varia);
        AssertCarryover(samus, 0, 0, "setup");

        // Two instruction-list calls precede the first stage call and leave the carryover untouched.
        pickup.Step(samus, cgram);
        pickup.Step(samus, cgram);
        AssertCarryover(samus, 0, 0, "instruction-list setup");

        // Stage zero: the word is the grown scanline count, until the stage hands over the narrow beam's edges.
        StepThrough(pickup, samus, cgram, SamusSuitPickupStage.LightBeamAppears,
            context => AssertCarryover(samus, 0, checked((ushort)pickup.Beam.Progress), context));
        AssertCarryover(samus, 1, EdgeWord(SuitPickupBeamDefinitions.NarrowBeamEdges), "uniform widening starts");
        pickup.Step(samus, cgram);
        AssertCarryover(samus, 1, LittleEndianBytes.Word(119, 121), "first uniform widening call");

        // Columns: the word is the edge pair, left column in the low byte, until the curved stage reaches full width.
        StepThrough(pickup, samus, cgram, SamusSuitPickupStage.UniformWidening,
            context => AssertCarryover(samus, 1, BeamEdgeWord(pickup), context));
        AssertCarryover(samus, 2, EdgeWord(SuitPickupBeamDefinitions.CurvedWideningStart), "curved widening starts");
        StepThrough(pickup, samus, cgram, SamusSuitPickupStage.CurvedWidening,
            context => AssertCarryover(samus, 2, BeamEdgeWord(pickup), context));
        AssertCarryover(samus, 3, 0, "full-width beam clears the word for the reveal");
        pickup.Step(samus, cgram);
        AssertEqual(SamusSuitPickupStage.LightBeamShrinks, pickup.Stage, "the reveal hands over to the shrinking stage");
        AssertCarryover(samus, 4, 0, "reveal");

        // Stage four: the word is the wipe's scanline count, until the stage hands over the dissipation edges.
        StepThrough(pickup, samus, cgram, SamusSuitPickupStage.LightBeamShrinks,
            context => AssertCarryover(samus, 4, checked((ushort)pickup.Beam.Progress), context));
        AssertCarryover(samus, 5, EdgeWord(SuitPickupBeamDefinitions.DissipationStart), "dissipation starts");
        pickup.Step(samus, cgram);
        AssertEqual(7, pickup.Beam.Left.X, "the first dissipation call wraps the left column from 255 to 7");
        AssertCarryover(samus, 5, LittleEndianBytes.Word(7, 240), "first dissipation call");
        StepThrough(pickup, samus, cgram, SamusSuitPickupStage.LightBeamDissipates,
            context => AssertCarryover(samus, 5, BeamEdgeWord(pickup), context));
        AssertCarryover(samus, 6, BeamEdgeWord(pickup), "the last centre line awaits cleanup");
        pickup.Step(samus, cgram);
        AssertTrue(!pickup.IsActive, "cleanup ends the transformation");
        AssertCarryover(samus, 0, 0, "cleanup");
        Console.WriteLine("Suit pickup carryover: every stage leaves its number and exact light-beam word for the spark, and its number for Crystal Flash and Draygon's escape.");
    }

    /// <summary>Runs stage calls while <paramref name="stage"/> stays current, checking the carryover after each call that keeps it.</summary>
    private static void StepThrough(SamusSuitPickupState pickup, SamusState samus, SamusColours cgram,
        SamusSuitPickupStage stage, Action<string> checkWithinStage)
    {
        AssertEqual(stage, pickup.Stage, $"{stage} is current before its calls");
        for (int call = 1; call <= MaximumCallsPerStage; call++)
        {
            pickup.Step(samus, cgram);
            if (pickup.Stage != stage)
                return;
            checkWithinStage($"{stage} call {call}");
        }
        throw new InvalidOperationException($"Suit pickup stage {stage} did not end within {MaximumCallsPerStage} calls.");
    }

    /// <summary>The published beam's edge columns as one word, left column in the low byte.</summary>
    private static ushort BeamEdgeWord(SamusSuitPickupState pickup) =>
        LittleEndianBytes.Word(checked((byte)pickup.Beam.Left.X), checked((byte)pickup.Beam.Right.X));

    /// <summary>An authored edge pair as the word the carryover leaves, left column in the low byte (#1468).</summary>
    private static ushort EdgeWord(SuitPickupBeamEdges edges) =>
        LittleEndianBytes.Word(checked((byte)edges.Left.X), checked((byte)edges.Right.X));

    /// <summary>Checks that the spark acceleration, Crystal Flash ammunition timer and escape presses hold what the stage left.</summary>
    private static void AssertCarryover(SamusState samus, ushort stageNumber, ushort lightBeamWord, string context)
    {
        AssertEqual(SignedSixteenSixteen.FromParts(unchecked((short)stageNumber), lightBeamWord),
            samus.SparkCarryover.SparkAcceleration, $"{context}: spark acceleration holds the stage number and light-beam word");
        AssertEqual(stageNumber, samus.EscapeAndDrainCount.AmmoCountdown.RawValue,
            $"{context}: Crystal Flash ammunition timer holds the stage number");
        AssertEqual(stageNumber, samus.DraygonGrabbed.EscapeButtonCounter,
            $"{context}: Draygon escape presses hold the stage number");
    }

    /// <summary>Runs the suit pickup carryover check.</summary>
    [VerificationCommand("--suit-pickup-carryover")]
    internal static int SuitPickupCarryoverCommand()
    {
        Suite(nameof(VerifySuitPickupCarryover), () => VerifySuitPickupCarryover());
        return 0;
    }
}
