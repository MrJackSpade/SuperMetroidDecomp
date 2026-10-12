using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using static GrapplePoseRefireVerification;
using static SuiteTimingVerification;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Spatial;
using static RetailRuntimeFixtureVerification;

internal static class GrappleSoundsVerification
{
    internal static void VerifyGrappleSounds()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var air = new RoomLevelData(new Extent16<RoomBlocks>(16, 16), new RoomLevelPlanes(new ushort[256], new byte[256], new ushort[256]), new byte[8]);
        var anchors = new RoomLevelData(new Extent16<RoomBlocks>(16, 16), new RoomLevelPlanes(Enumerable.Repeat((ushort)((int)RoomCollisionType.GrappleBlock << 12), 256).ToArray(), new byte[256], new ushort[256]), new byte[8]);
        var samus = new SamusState { Pose = SamusPoseId.FacingRightNormalPose, Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)) };
        samus.RefreshCollisionRadii(bus);
        PrepareRetailSamusFixture(samus);
        SamusGrappleMovement.BeginFiring(bus, samus);
        AssertEqual(new SamusSoundRequest(SoundEffectId.FromCartridge(SoundEffectLibrary.Library1, 5), 1),
            samus.LiquidPhysics.SoundRequests.Single(), "grapple firing starts native library-one sound five once");
        samus.LiquidPhysics.BeginFrameSoundRequests();
        SamusGrappleMovement.StepFiring(bus, air, samus, (ushort)SnesButton.X);
        AssertEqual(0, samus.LiquidPhysics.SoundRequests.Count, "extending does not restart grapple sound");
        SamusGrappleMovement.StepFiring(bus, anchors, samus, (ushort)SnesButton.X);
        AssertEqual(new SamusSoundRequest(SoundEffectId.FromCartridge(SoundEffectLibrary.Library1, 6), 6),
            samus.LiquidPhysics.SoundRequests.Single(), "accepted connection replaces firing sound with attached sound");
        foreach (GrapplePhase phase in new[] { GrapplePhase.CancelPending, GrapplePhase.ReleaseFromSwing, GrapplePhase.Dropped })
        {
            samus.LiquidPhysics.BeginFrameSoundRequests();
            samus.Pose = SamusPoseId.GrappleSwingRightPose;
            samus.RefreshCollisionRadii(bus);
            samus.Grapple.Phase = phase;
            if (phase == GrapplePhase.CancelPending)
                SamusGrappleMovement.CompleteFiringCancellation(bus, air, samus);
            else
                SamusGrappleMovement.Step(new SamusTerrain { Bus = bus, Level = air }, samus, 0, 0);
            AssertEqual(new SamusSoundRequest(SoundEffectId.FromCartridge(SoundEffectLibrary.Library1, 7), 15),
                samus.LiquidPhysics.SoundRequests.Single(), $"{phase} emits native grapple stop command once");
        }
        Console.WriteLine("  Grapple sounds: fire, silent extension, attachment, cancellation, swing release and drop queues agree.");
        foreach (SamusSoundRequest request in new[] { SamusGrappleRomData.Sounds.Fire, SamusGrappleRomData.Sounds.Attach, SamusGrappleRomData.Sounds.Stop })
        {
            var audio = new CartridgeAudioState();
            audio.AdvanceFrame(bus, default);
            audio.QueueSound(request.SoundEffect, request.MaximumQueued);
            AssertEqual(CartridgeAudioCommand.WritePort(ApuPort.SoundLibrary1, request.SoundEffect.Value),
                audio.AdvanceFrame(bus, default).Single(), "grapple command reaches the actual APU sound port");
        }
    }

    [VerificationCommand("--grapple-sound-refire-fixtures")]
    internal static int GrappleSoundRefireFixturesCommand()
    {
     VerifyGrappleSounds(); VerifyGrapplePoseRefire(); return 0; 
    }

    [VerificationCommand("--grapple-sounds")]
    internal static int GrappleSoundsCommand()
    {
        Suite(nameof(VerifyGrappleSounds), () => VerifyGrappleSounds());
        return 0;
    }
}
