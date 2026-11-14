using SuperMetroid.Core;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
namespace SuperMetroid.Verification.Samus.Projectiles;

/// <summary>
/// Confirms that firing cancels a pending resume of the charging-beam sound where the original does (#1689): the
/// uncharged and charged fire routines and the Hyper Beam's clear the request right after queueing the shot's own sound,
/// a charged release the fire routines reject clears it right before queueing the charge-silencing sound, and the
/// grapple's firing clears it right after queueing the grapple's firing sound. Each case's oracle is the original's
/// event order: the update's sound requests are the firing's own and nothing else, and the post-draw sound pass that
/// follows, with Shoot still held, requests no resumed charging sound.
/// </summary>
internal static class ResumeChargingSoundCancelVerification
{
    /// <summary>Samus's position for the standing shots and the grapple.</summary>
    private static readonly FixedPosition<RoomPixels> StandingPosition = FixedPosition<RoomPixels>.FromWhole(new(128, 128));

    /// <summary>Samus's position for the jump-transition releases.</summary>
    private static readonly FixedPosition<RoomPixels> JumpingPosition = FixedPosition<RoomPixels>.FromWhole(new(211, 288));

    /// <summary>Runs every firing path's check.</summary>
    internal static void VerifyFiringCancelsResumeChargingSound()
    {
        Suite(nameof(VerifyControlResumes), VerifyControlResumes);
        Suite(nameof(VerifyUnchargedShotsCancel), VerifyUnchargedShotsCancel);
        Suite(nameof(VerifyChargedReleasesCancel), VerifyChargedReleasesCancel);
        Suite(nameof(VerifyHyperBeamAfterSpinCancels), VerifyHyperBeamAfterSpinCancels);
        Suite(nameof(VerifyGrappleFiringCancels), VerifyGrappleFiringCancels);
        Console.WriteLine("Resume-charging sound: uncharged, charged, rejected-release, Hyper Beam and grapple firing each cancel a pending resume in the original's order.");
    }

    /// <summary>Without a shot, the pending resume plays on the post-draw pass: the fixture observes the resume.</summary>
    private static void VerifyControlResumes()
    {
        var fixture = new Fixture(SamusPoseId.FacingRightNormalPose, StandingPosition, SamusBeamFlags.None);
        fixture.Samus.ResumeChargingBeamSoundPending = true;
        fixture.PostDraw(ControllerButton.X);
        AssertTrue(fixture.Requests.SoundRequests.SequenceEqual([new SoundRequest(SamusSound.ResumeChargingBeam, SoundRequestPriority.High)]),
            "with nothing fired, the pending resume requests the charging sound on the post-draw pass");
    }

    /// <summary>The uncharged fire routine, reached without Charge Beam and on Charge Beam's first held update.</summary>
    private static void VerifyUnchargedShotsCancel()
    {
        foreach (SamusBeamFlags beams in new[] { SamusBeamFlags.None, SamusBeamFlags.Charge })
        {
            var fixture = new Fixture(SamusPoseId.FacingRightNormalPose, StandingPosition, beams);
            fixture.Samus.ResumeChargingBeamSoundPending = true;
            fixture.Fire(new ProjectileButtonInput { ControllerInput = ControllerButton.X, ControllerNewInput = ControllerButton.X });
            fixture.AssertShotCancelled($"uncharged shot with {beams}");
        }
    }

    /// <summary>
    /// The charged fire routine and its rejection tail, both reached by a jump transition's forced release while Shoot
    /// is still held, so the post-draw pass would resume the charge if the request survived.
    /// </summary>
    private static void VerifyChargedReleasesCancel()
    {
        var fired = new Fixture(SamusPoseId.NormalJumpGunExtendedLeftPose, JumpingPosition, SamusBeamFlags.Charge | SamusBeamFlags.Wave);
        fired.Charge.FlareCounter = BeamChargeDefinitions.FullyChargedCounter;
        fired.Samus.PendingTransitionShot = new SamusTransitionShot(SamusProjectileDirection.Left);
        fired.Samus.ResumeChargingBeamSoundPending = true;
        fired.Fire(new ProjectileButtonInput { ControllerInput = ControllerButton.X, ControllerNewInput = ControllerButton.None });
        fired.AssertShotCancelled("charged release");

        // A running cooldown rejects the release; with the charge past the charging sound's start, the rejection clears
        // the request and then silences the charge.
        var rejected = new Fixture(SamusPoseId.NormalJumpGunExtendedLeftPose, JumpingPosition, SamusBeamFlags.Charge | SamusBeamFlags.Wave);
        rejected.Charge.FlareCounter = BeamChargeDefinitions.FullyChargedCounter;
        rejected.Samus.PendingTransitionShot = new SamusTransitionShot(SamusProjectileDirection.Left);
        rejected.Bombs.SetSharedCooldown(5);
        rejected.Samus.ResumeChargingBeamSoundPending = true;
        rejected.Fire(new ProjectileButtonInput { ControllerInput = ControllerButton.X, ControllerNewInput = ControllerButton.None });
        AssertTrue(!rejected.Samus.ResumeChargingBeamSoundPending, "the rejected release clears the pending resume");
        rejected.PostDraw(ControllerButton.X);
        AssertTrue(rejected.Requests.SoundRequests.SequenceEqual([new SoundRequest(SamusSound.Silence, SoundRequestPriority.Essential)]),
            "the rejected release requests only the charge-silencing sound, and no resume follows it");
    }

    /// <summary>
    /// The ticket's case: Samus leaves a spin jump holding Shoot with the Hyper Beam's flare showing, which requests the
    /// resume for the next post-draw pass, and fires the Hyper Beam on that next update.
    /// </summary>
    private static void VerifyHyperBeamAfterSpinCancels()
    {
        var fixture = new Fixture(SamusPoseId.FacingRightNormalPose, StandingPosition, SamusBeamFlags.Wave | SamusBeamFlags.Plasma);
        fixture.Samus.HyperBeamEnabled = true;
        fixture.Fire(new ProjectileButtonInput { ControllerInput = ControllerButton.X, ControllerNewInput = ControllerButton.X });
        AssertTrue(fixture.Charge.HyperBeamFlare, "the first Hyper Beam shot starts its flare");

        fixture.Requests.BeginUpdate();
        SamusPostDrawAudio.Step(fixture.Samus, SamusMovementType.SpinJumping, ControllerButton.X);
        AssertTrue(fixture.Samus.ResumeChargingBeamSoundPending, "leaving the spin with Shoot held and the flare showing requests the resume");

        fixture.Requests.BeginUpdate();
        fixture.Bombs.SetSharedCooldown(0);
        fixture.Fire(new ProjectileButtonInput { ControllerInput = ControllerButton.X, ControllerNewInput = ControllerButton.None });
        fixture.AssertShotCancelled("Hyper Beam shot after the spin");
    }

    /// <summary>The grapple's firing, which queues its firing sound and then clears the request.</summary>
    private static void VerifyGrappleFiringCancels()
    {
        var fixture = new Fixture(SamusPoseId.FacingRightNormalPose, StandingPosition, SamusBeamFlags.None);
        PrepareRetailSamusFixture(fixture.Samus);
        fixture.Samus.ResumeChargingBeamSoundPending = true;
        SamusGrappleMovement.BeginFiring(fixture.Samus, ControllerButton.X);
        AssertEqual(GrapplePhase.Firing, fixture.Samus.Grapple.Phase, "the grapple fires");
        AssertTrue(!fixture.Samus.ResumeChargingBeamSoundPending, "the grapple's firing clears the pending resume");
        fixture.PostDraw(ControllerButton.X);
        AssertTrue(fixture.Requests.SoundRequests.SequenceEqual([GrappleBeamDefinitions.Sounds.Fire]),
            "the grapple requests only its firing sound, and no resume follows it");
    }

    /// <summary>One Samus, her projectiles and bombs, sharing one charge and one record of the update's sound requests.</summary>
    private sealed class Fixture
    {
        /// <summary>An empty room the shots fly through without striking anything.</summary>
        private readonly RoomLevelData room = CreateRoom(new(32, 32), new RoomTerrainBlock[1024], new RoomBlockBehavior[1024]);

        /// <summary>Places Samus in <paramref name="pose"/> at <paramref name="position"/> with <paramref name="beams"/> equipped.</summary>
        internal Fixture(SamusPoseId pose, FixedPosition<RoomPixels> position, SamusBeamFlags beams)
        {
            Samus = new SamusState(InstalledFixtures.FramesBeyondAnimation, new UnauthoredBoostCadence(), SamusCompatibility.ForNewSamus(), Charge)
            {
                Pose = pose,
                Position = position,
                EquippedBeams = beams,
            };
            Projectiles = new SamusProjectileSystem(Charge, InstalledFixtures.SamusOverreads, new LeftMuzzleSubpixelCarry(SamusProjectileSystem.SlotCount))
            {
                Animation = { FrameBindings = InstalledFixtures.Projectiles.FrameBindings },
            };
            Projectiles.Earthquakes.BindRoomEarthquake(new RoomEarthquake());
            Samus.BindAudioRequests(Requests);
            Projectiles.BindAudioRequests(Requests);
        }

        /// <summary>The charge Samus and her projectiles share.</summary>
        internal BeamChargeState Charge { get; } = new();

        /// <summary>The update's sound requests, in request order, of Samus and her projectiles alike.</summary>
        internal GameplayAudioRequests Requests { get; } = new();

        /// <summary>Samus.</summary>
        internal SamusState Samus { get; }

        /// <summary>Her shots.</summary>
        internal SamusProjectileSystem Projectiles { get; }

        /// <summary>Her bombs, which carry the shared projectile cooldown.</summary>
        internal SamusBombProjectileSystem Bombs { get; } = CreateBombFixture();

        /// <summary>Runs the projectile producer's firing pass with <paramref name="buttons"/>.</summary>
        internal void Fire(ProjectileButtonInput buttons) =>
            Projectiles.FireFromHud(new SamusTerrain { Level = room }, Samus, buttons, Bombs, projectileProducerEnabled: true);

        /// <summary>Runs the post-draw sound pass with <paramref name="held"/> held, Samus's movement unchanged.</summary>
        internal void PostDraw(ControllerButton held) =>
            SamusPostDrawAudio.Step(Samus, SamusPoseMetadata.ReadMovementType(Samus), held);

        /// <summary>
        /// Asserts the pass fired a shot and cleared the pending resume, and that after the post-draw pass with Shoot held
        /// the update's sound requests are the shot's sound alone.
        /// </summary>
        internal void AssertShotCancelled(string shot)
        {
            SamusProjectileFrameResult result = Projectiles.LastFrameResult;
            AssertTrue(result.QueuedSoundEffect is not null, $"the {shot} fires and requests its sound");
            AssertTrue(!Samus.ResumeChargingBeamSoundPending, $"the {shot} clears the pending resume");
            PostDraw(ControllerButton.X);
            var shotRequest = new SoundRequest(result.QueuedSoundEffect!.Value, result.QueuedSoundMaximum!.Value);
            AssertTrue(Requests.SoundRequests.SequenceEqual([shotRequest]),
                $"the {shot} requests only its own sound, and no resume follows it: [{string.Join(", ", Requests.SoundRequests)}]");
        }
    }

    /// <summary>Runs the firing paths' resume-cancellation checks.</summary>
    [VerificationCommand("--resume-charging-sound-cancel")]
    internal static int ResumeChargingSoundCancelCommand()
    {
        VerifyFiringCancelsResumeChargingSound();
        return 0;
    }
}
