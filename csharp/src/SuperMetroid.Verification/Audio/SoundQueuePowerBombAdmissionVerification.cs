using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
namespace SuperMetroid.Verification.Audio;

/// <summary>
/// Checks that the sound queues' own admission turns every sound away while a power bomb explodes, whoever requests it
/// (#1620): the original's three sound-queue routines each test the explosion after the queue's occupancy, so no requester
/// tests it itself, and its music queue routines do not test it.
/// </summary>
internal static class SoundQueuePowerBombAdmissionVerification
{
    /// <summary>
    /// During an exploding power bomb, the producers that used to queue without the power-bomb test (the death's spin-end
    /// sound, Crystal Flash's silence, the X-ray's start and end, the health warning) each make their request and the queue
    /// turns it away, exactly as it turns away an ordinary producer's landing sound; music still joins its queue. Once the
    /// explosion ends, the same landing sound joins its queue.
    /// </summary>
    internal static void VerifyExplodingPowerBombTurnsEverySoundAway()
    {
        var gameplay = new CheckGameplaySoundState { PowerBombExploding = true };
        AudioQueues audio = AudioQueues.Standalone(gameplay);
        GameplayAudioRequests requests = GameplayAudioRequests.Reaching(audio);

        // Dying while spin jumping ends the spin's sound as the death begins.
        SamusState spinning = SamusAt(SamusPoseId.SpinJumpRightPose, requests);
        spinning.DeathSequence.Begin(spinning, cameraPosition: new(0, 0));
        AssertTurnedAway(audio, requests, SamusSound.SpinJumpEnd, "the death's spin-end sound");

        // The X-ray's setup queues its start; releasing Run after the warm-up and the widening tears it down, queueing its end.
        SamusState scoping = SamusAt(SamusPoseId.FacingRightNormalPose, requests);
        AssertTrue(scoping.Xray.TryBegin(scoping, previousMovementType: SamusMovementType.Standing,
                conditions: new XrayBeginConditions { GameState = SuperMetroidGameState.MainGameplay }),
            "the X-ray's setup is accepted");
        AssertTurnedAway(audio, requests, SamusSound.XrayStart, "the X-ray's start");
        for (int update = 0; update < 8 + 1 + 27; update++)
            scoping.Xray.StepBeam(scoping, ControllerButton.B);
        AssertEqual(XrayBeamPhase.Full, scoping.Xray.BeamPhase, "the X-ray reaches its full beam");
        for (int update = 0; update < 4; update++)
            scoping.Xray.StepBeam(scoping, 0);
        AssertTrue(!scoping.Xray.IsActive, "releasing Run tears the X-ray down");
        AssertTurnedAway(audio, requests, SamusSound.XrayEnd, "the X-ray's end");

        // Crystal Flash's raise ends with its essential-priority silence of the room library.
        const ControllerButton chord = ControllerButton.Down | ControllerButton.L | ControllerButton.R | ControllerButton.X;
        SamusState flashing = SamusAt(SamusPoseId.FacingRightNormalPose, requests);
        flashing.Health = 1;
        flashing.MaxHealth = 99;
        flashing.Missiles = flashing.SuperMissiles = flashing.PowerBombs = 10;
        AssertTrue(flashing.CrystalFlash.TryBegin(flashing, chord), "Crystal Flash is accepted");
        for (int frame = 0; frame < 10; frame++)
            flashing.CrystalFlash.Step(flashing, (ushort)frame);
        AssertEqual(CrystalFlashPhase.DrainingAmmo, flashing.CrystalFlash.Phase, "Crystal Flash's raise completes");
        AssertTurnedAway(audio, requests, RoomSound.Silence, "Crystal Flash's silence");

        // The health warning starts below 31 energy and stops at 31 or more; its latch changes either way.
        var warning = new SamusHealthWarningState();
        warning.Update(30, requests);
        AssertTrue(warning.IsActive, "the warning's latch is set although its start was turned away");
        AssertTurnedAway(audio, requests, RoomSound.LowHealthBeep, "the health warning's start");

        // An ordinary producer's sound is turned away the same way.
        SamusState landing = SamusAt(SamusPoseId.FacingRightNormalPose, requests);
        landing.LiquidPhysics.CinematicActive = false;
        landing.LiquidPhysics.HandleLandingSoundEffectsAndGraphics(landing,
            SamusMovementType.Standing, SamusPoseId.FacingRightNormalPose, new UnsignedSixteenSixteen(5, 0));
        AssertTurnedAway(audio, requests, RoomSound.HardLanding, "a landing's sound");

        // Music is not tested against the explosion.
        requests.QueuePermanentItemFanfare();
        AssertTrue(audio.HasQueuedMusic, "an exploding power bomb leaves music to join its queue");

        // Once the explosion ends, the same landing joins its queue.
        gameplay.PowerBombExploding = false;
        landing.LiquidPhysics.HandleLandingSoundEffectsAndGraphics(landing,
            SamusMovementType.Standing, SamusPoseId.FacingRightNormalPose, new UnsignedSixteenSixteen(5, 0));
        AssertTrue(audio.WaitingSounds(SoundEffectLibrary.Room).Contains(RoomSound.HardLanding),
            "with no explosion the landing's sound joins its queue");
        Console.WriteLine("  Power-bomb sound admission: every producer's sound, ordinary or not, is turned away in the queue; music is not.");
    }

    /// <summary>A standing-ready Samus in <paramref name="pose"/>, her sounds joining <paramref name="requests"/>.</summary>
    private static SamusState SamusAt(SamusPoseId pose, GameplayAudioRequests requests)
    {
        var samus = new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(),
            SamusCompatibility.ForNewSamus(), new BeamChargeState())
        {
            Pose = pose,
            Position = FixedPosition<RoomPixels>.FromWhole(new(0x0120, 0x0080)),
        };
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);
        samus.BindAudioRequests(requests);
        return samus;
    }

    /// <summary>
    /// Asserts that <paramref name="sound"/> was requested this check, admitted by its queue's occupancy, and turned away:
    /// none of its library's waiting requests is it.
    /// </summary>
    private static void AssertTurnedAway(AudioQueues audio, GameplayAudioRequests requests, SoundEffectId sound, string what)
    {
        AssertTrue(requests.SoundRequests.Any(request => request.SoundEffect == sound), $"{what} is requested");
        AssertTrue(!audio.WaitingSounds(sound.Library).Contains(sound),
            $"{what} is turned away while a power bomb explodes");
    }

    /// <summary>Runs the power-bomb sound admission check from the verification command line.</summary>
    [VerificationCommand("--sound-queue-power-bomb-admission")]
    internal static int SoundQueuePowerBombAdmissionCommand()
    {
        Suite(nameof(VerifyExplodingPowerBombTurnsEverySoundAway), () => VerifyExplodingPowerBombTurnsEverySoundAway());
        return 0;
    }
}
