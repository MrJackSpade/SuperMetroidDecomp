using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Samus.Poses;

/// <summary>
/// Confirms #1535's reported chains through the production animation stepper: the Baby's rainbow stop on the right-facing
/// morph and the Hyper Beam freeze on the left-facing unmorph and the right-facing crouch transition read on past the
/// pose's program, cue values included, and end in the transition the game reaches. Every expected value is derived from
/// the cartridge's programs: the ticks are the bytes the read passes, each a delay, and the pose is the one the decoded
/// cue it reaches publishes.
/// </summary>
internal static class SamusCueValuePlaceReadsVerification
{
    /// <summary>The bank holding the animation programs, whose table words name places in it.</summary>
    private const int ProgramBank = 0x910000;

    /// <summary>The lowest program byte the interpreter takes as an instruction; every byte below it is a delay.</summary>
    private const int FirstInstruction = 0x80;

    /// <summary>The window position the programs end at, where the running cadences start.</summary>
    private const int ProgramsEnd = 0xb5d1;

    /// <summary>Drives each reported chain and compares it with the cartridge's bytes.</summary>
    internal static void VerifySamusCueValuePlaceReads(ISnesAddressSpace bus)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(bus);
        VerifyChain(cartridge, SamusPoseId.MorphingTransitionRightPose, "rainbow stop",
            samus => samus.Drained.DisableRainbowAndStartStandingAnimation(samus), SamusPoseId.GlitchBallRightPose);
        VerifyChain(cartridge, SamusPoseId.UnmorphingTransitionLeftPose, "Hyper Beam freeze",
            SamusDrainedState.FreezeForHyperBeamAcquisition, SamusPoseId.DraygonHeldBallLeftPose);
        VerifyChain(cartridge, SamusPoseId.CrouchingTransitionRightPose, "Hyper Beam freeze",
            SamusDrainedState.FreezeForHyperBeamAcquisition, SamusPoseId.FacingRightNormalPose);
        Console.WriteLine("Samus cue-value place reads: the three reported over-read chains end where the cartridge's bytes lead.");
    }

    /// <summary>
    /// Starts <paramref name="pose"/>'s animation, applies <paramref name="write"/>, then steps the animation tick by tick:
    /// no transition may be pending until the read has passed every delay byte after the written frame, and on that tick
    /// the decoded cue's pose, which must be <paramref name="reported"/>, is pending.
    /// </summary>
    private static void VerifyChain(IImportCartridgeSource cartridge, SamusPoseId pose, string writer, Action<SamusState> write,
        SamusPoseId reported)
    {
        var samus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())
        {
            Pose = pose,
            Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)),
        };
        SamusPresentation.InitializeAnimation(samus);
        write(samus);
        AssertEqual(0, samus.AnimationFrameBuffer, $"{pose} at the {writer} animates without a liquid's delay buffer");
        AssertTrue(samus.PendingTransitionalPose is null, $"{pose} at the {writer} has no transition pending");

        // The tick that runs out the written frame's one-tick timer moves to the next frame; each delay byte read after it
        // holds the frame for its ticks, a zero delay for one, before the next tick moves on.
        int place = SamusAnimationProgramCodec.ProgramStart(cartridge, SamusPoseNumbers.Number(pose)) + samus.AnimationFrame + 1;
        int ticks = 1;
        int value;
        while ((value = cartridge.ReadCartridgeByte(ProgramBank | place)) < FirstInstruction)
        {
            ticks += Math.Max(value, 1);
            place++;
            AssertTrue(place < ProgramsEnd, $"{pose} at the {writer} reads within the programs");
        }
        SamusPoseId expected = PublishedPose(DecodedCueAt(cartridge, place));
        AssertEqual(reported, expected, $"{pose} at the {writer} reaches the cue that publishes {reported}");

        for (int tick = 1; tick <= ticks; tick++)
        {
            AssertTrue(samus.PendingTransitionalPose is null, $"{pose} at the {writer} has no transition pending before tick {tick}");
            SamusPresentation.AnimateNoFx(samus, new SamusAnimationStep { Random = new SharedRandom() });
        }
        AssertEqual<SamusPoseId?>(expected, samus.PendingTransitionalPose, $"{pose} at the {writer} publishes {expected} after {ticks} ticks");
    }

    /// <summary>The cue a decoded program holds at window position <paramref name="place"/>.</summary>
    /// <exception cref="InvalidDataException">No program's run ends with a cue there.</exception>
    private static SamusAnimationCue DecodedCueAt(IImportCartridgeSource cartridge, int place)
    {
        for (int number = 0; number < SamusAnimationProgramCodec.PoseCount; number++)
        {
            if (!SamusPoseNumbering.NamesPose(number))
                continue;
            int start = SamusAnimationProgramCodec.ProgramStart(cartridge, number);
            foreach (SamusAnimationSequence run in SamusAnimationProgramCodec.Decode(cartridge, SamusPoseNumbering.FromNumber(number)))
            {
                if (start + run.FirstFrame + run.Delays.Count == place)
                    return run.End;
            }
        }
        throw new InvalidDataException($"No decoded Samus program holds a cue at $91:{place:X4}.");
    }

    /// <summary>The pose <paramref name="cue"/> publishes for a Samus with no items who is not moving vertically.</summary>
    /// <exception cref="InvalidDataException">The cue publishes no pose.</exception>
    private static SamusPoseId PublishedPose(SamusAnimationCue cue) => cue switch
    {
        SamusAnimationTransition transition => transition.Pose,
        SamusAnimationTurnTransition turn => turn.Pose,
        SamusAnimationItemTransition item => item.Unequipped,
        SamusAnimationItemAirborneTransition itemAirborne => itemAirborne.UnequippedGrounded,
        SamusAnimationVerticalSpeedTransition verticalSpeed => verticalSpeed.Grounded,
        _ => throw new InvalidDataException($"Samus animation cue {cue} publishes no pose."),
    };

    /// <summary>Runs the cue-value place reads check against the retail ROM.</summary>
    [VerificationCommand("--samus-cue-value-place-reads")]
    internal static int SamusCueValuePlaceReadsCommand()
    {
        string sourceRom = Path.GetFullPath("Super Metroid.smc");
        CartridgeImportAddressSpace bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(sourceRom);
        Suite(nameof(VerifySamusCueValuePlaceReads), () => VerifySamusCueValuePlaceReads(bus));
        return 0;
    }
}
