using static SuperMetroid.Verification.Samus.Grapple.GrapplePoseRefireVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
namespace SuperMetroid.Verification.Samus.Grapple;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Audio;

/// <summary>Checks Grapple fire, attachment, and stop commands through movement and audio publication.</summary>
internal static class GrappleSoundsVerification
{
    /// <summary>Confirms extension stays silent, release paths stop once, and sounds reach the correct APU port.</summary>
    internal static void VerifyGrappleSounds()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var air = new RoomLevelData(new Extent16<RoomBlocks>(16, 16), new RoomLevelPlanes(new ushort[256], new byte[256], new ushort[256]), new byte[8]);
        var anchors = new RoomLevelData(new Extent16<RoomBlocks>(16, 16), new RoomLevelPlanes(Enumerable.Repeat((ushort)((int)RoomCollisionType.GrappleBlock << 12), 256).ToArray(), new byte[256], new ushort[256]), new byte[8]);
        var samus = new SamusState { Pose = SamusPoseId.FacingRightNormalPose, Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)) };
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        PrepareRetailSamusFixture(samus);
        SamusGrappleMovement.BeginFiring(samus);
        AssertEqual(new SoundRequest(Library1Sound.GrappleStart, 1),
            samus.LiquidPhysics.SoundRequests.Single(), "grapple firing starts native library-one sound five once");
        samus.LiquidPhysics.BeginFrameSoundRequests();
        SamusGrappleMovement.StepFiring(air, samus, SnesButton.X);
        AssertEqual(0, samus.LiquidPhysics.SoundRequests.Count, "extending does not restart grapple sound");
        SamusGrappleMovement.StepFiring(anchors, samus, SnesButton.X);
        AssertEqual(new SoundRequest(Library1Sound.Grappling, 6),
            samus.LiquidPhysics.SoundRequests.Single(), "accepted connection replaces firing sound with attached sound");
        foreach (GrapplePhase phase in new[] { GrapplePhase.CancelPending, GrapplePhase.ReleaseFromSwing, GrapplePhase.Dropped })
        {
            samus.LiquidPhysics.BeginFrameSoundRequests();
            samus.Pose = SamusPoseId.GrappleSwingRightPose;
            SamusPoseMetadata.RefreshCollisionRadii(samus);
            samus.Grapple.Phase = phase;
            if (phase == GrapplePhase.CancelPending)
                SamusGrappleMovement.CompleteFiringCancellation(air, samus);
            else
                SamusGrappleMovement.Step(new SamusTerrain { Level = air }, samus, 0, 0);
            AssertEqual(new SoundRequest(Library1Sound.GrappleEnd, 15),
                samus.LiquidPhysics.SoundRequests.Single(), $"{phase} emits native grapple stop command once");
        }
        Console.WriteLine("  Grapple sounds: fire, silent extension, attachment, cancellation, swing release and drop queues agree.");
        foreach (SoundRequest request in new[] { GrappleBeamDefinitions.Sounds.Fire, GrappleBeamDefinitions.Sounds.Attach, GrappleBeamDefinitions.Sounds.Stop })
        {
            var audio = new AudioQueues();
            audio.AdvanceFrame(default);
            audio.QueueSound(request.SoundEffect, request.MaximumQueued.Requests);
            AssertEqual(AudioRequest.ForSound(SoundEffectLibrary.Library1, request.SoundEffect.Value),
                audio.AdvanceFrame(default).Single(), "grapple command reaches the actual APU sound port");
        }
    }

    /// <summary>Runs Grapple sound checks together with pose-refire behavior.</summary>
    [VerificationCommand("--grapple-sound-refire-fixtures")]
    internal static int GrappleSoundRefireFixturesCommand()
    {
     VerifyGrappleSounds(); VerifyGrapplePoseRefire(); return 0; 
    }

    /// <summary>Runs the focused Grapple sound verification command.</summary>
    [VerificationCommand("--grapple-sounds")]
    internal static int GrappleSoundsCommand()
    {
        Suite(nameof(VerifyGrappleSounds), () => VerifyGrappleSounds());
        return 0;
    }
}
