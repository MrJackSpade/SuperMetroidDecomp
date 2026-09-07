using SuperMetroid.Core.Game;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyGrappleSounds()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var air = new RoomLevelData(16, 16, new ushort[256], new byte[256], new ushort[256], new byte[8]);
        var anchors = new RoomLevelData(16, 16,
            Enumerable.Repeat((ushort)((int)RoomCollisionType.GrappleBlock << 12), 256).ToArray(),
            new byte[256], new ushort[256], new byte[8]);
        var samus = new SamusState { Pose = SamusPoseIds.FacingRightNormalPose, XPosition = 128, YPosition = 128 };
        samus.RefreshCollisionRadii(bus);
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
            samus.Pose = SamusPoseIds.GrappleSwingRightPose;
            samus.RefreshCollisionRadii(bus);
            samus.Grapple.Phase = phase;
            if (phase == GrapplePhase.CancelPending)
                SamusGrappleMovement.CompleteFiringCancellation(bus, air, samus);
            else
                SamusGrappleMovement.Step(bus, air, samus, 0, 0);
            AssertEqual(new SamusSoundRequest(SoundEffectId.FromCartridge(SoundEffectLibrary.Library1, 7), 15),
                samus.LiquidPhysics.SoundRequests.Single(), $"{phase} emits native grapple stop command once");
        }
        Console.WriteLine("  Grapple sounds: fire, silent extension, attachment, cancellation, swing release and drop queues agree.");
        foreach (SamusSoundRequest request in new[] { SamusGrappleRomData.Sounds.Fire, SamusGrappleRomData.Sounds.Attach, SamusGrappleRomData.Sounds.Stop })
        {
            var audio = new CartridgeAudioState();
            audio.AdvanceFrame(bus, default);
            audio.QueueSound(request.SoundEffect, request.MaximumQueued);
            AssertEqual(CartridgeAudioCommand.WritePort(AudioRomData.Apu.FirstSoundPort, request.SoundEffect.Value),
                audio.AdvanceFrame(bus, default).Single(), "grapple command reaches the actual APU sound port");
        }
    }
}
