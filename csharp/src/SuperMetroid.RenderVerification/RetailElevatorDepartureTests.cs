using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Rendering.Direct3D11;

/// <summary>Focused downward ride, staged on the carrier rather than reached by a multi-room trace.</summary>
internal static class RetailElevatorDepartureTests
{
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug); runtime.RunNmi(0, true);
        runtime.InitializeStartingCeresRoom(); runtime.InitializeCeresStartSamus();
        runtime.InitializePostCeresZebesRoom();
        var samus = runtime.Samus!;
        samus.InputLocked = false;
        samus.Pose = SamusPoseIds.FacingRightNormalPose;
        samus.InitializeAnimation(bus);
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.BlueBrinstarElevatorRoom);
        var actor = runtime.Enemies.Slots.First(slot => slot.EnemyDefinitionPointer == RoomEnemySystem.ElevatorDefinition);
        samus.XPosition = actor.XPosition;
        samus.YPosition = unchecked((ushort)(actor.YPosition - ElevatorCaptureFixtureDefinitions.SamusAttachmentOffset));
        samus.Kinematics.YSpeed = samus.Kinematics.YSubspeed = 0;
        runtime.Enemies.PublishElevatorDoorContact();
        int samples = 0, departure = 0, scrolling = 0, returning = 0;
        void Compare(string phase)
        {
            var expected = SuperMetroidRuntimeFrameRenderer.Render(runtime);
            var packet = new RenderFrameSnapshot(new(++samples, 1, (ushort)samples),
                GameplayDisplayCapture.TryCaptureFrame(runtime) ?? throw new InvalidOperationException("Missing elevator capture."));
            packet = RenderFrameSnapshotCodec.Deserialize(RenderFrameSnapshotCodec.Serialize(packet));
            PixelComparison.Verify(packet, expected, renderer.RenderForReadback(packet),
                $"{device.Kind}: downward elevator {phase}, sample={samples}");
        }
        runtime.StepFrame((ushort)SnesButton.Down);
        Compare("departure start");
        if (runtime.Enemies.LastElevatorEvent != ElevatorFrameEvent.DepartureStarted)
            throw new InvalidOperationException("Downward elevator did not accept the input edge.");
        while (!runtime.HasPendingDoorTransition && departure < 240)
        {
            runtime.StepFrame(0); departure++; Compare("departure");
        }
        if (!runtime.HasPendingDoorTransition) throw new InvalidOperationException("Elevator never reached its exit.");
        var transition = new DoorTransitionState();
        var audio = new CartridgeAudioState();
        transition.Begin(runtime);
        int doorFrames = 0;
        while (transition.IsActive && doorFrames++ < 512)
        {
            if (transition.Phase == DoorTransitionPhase.WaitForDoorOpeningScroll) scrolling++;
            transition.Step(runtime, audio, 0); Compare(transition.Phase.ToString());
        }
        if (transition.IsActive || scrolling < 12 || runtime.ActiveRoom?.Pointer != RoomHeaderPointers.MorphBallRoom)
            throw new InvalidOperationException("Downward elevator did not complete the authored transition.");
        while (runtime.Enemies.ElevatorStatus != ElevatorActorStatus.Inactive && returning < 1200)
        {
            runtime.StepFrame(0); returning++; Compare("arrival");
        }
        if (returning == 0 || runtime.Enemies.ElevatorStatus != ElevatorActorStatus.Inactive || samus.InputLocked)
            throw new InvalidOperationException("Downward arrival did not restore control.");
        for (int tick = 0; tick < 8; tick++) { runtime.StepFrame(0); Compare("settled"); }
        Console.WriteLine($"{device.Kind}: downward elevator {samples} exact frames, {departure} departure, " +
            $"{scrolling} scroll and {returning} return frames; control released.");
    }
}

internal static class ElevatorCaptureFixtureDefinitions
{
    /// <summary>Native elevator attachment uses a 26-pixel Samus center offset above the carrier; matches the existing ElevatorAudit fixture.</summary>
    internal const int SamusAttachmentOffset = 26;
}
