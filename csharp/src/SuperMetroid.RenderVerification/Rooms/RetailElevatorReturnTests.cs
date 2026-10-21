using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Rendering.Direct3D11.Device;
using SuperMetroid.Rendering.Direct3D11.Frames;
namespace SuperMetroid.RenderVerification.Rooms;

using SuperMetroid.TestSupport.FrameCapture;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.Core.Progression;

/// <summary>Room-local upward elevator handoff using the existing Blue Brinstar regression route.</summary>
internal static class RetailElevatorReturnTests
{
    /// <summary>Checks the upward return and the companion departure through exact captured frame comparisons.</summary>
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        RunUpward(device, renderer);
        RetailElevatorDepartureTests.Run(device, renderer);
    }

    /// <summary>Follows the Morph Ball room elevator door to Blue Brinstar through arrival and settled camera control.</summary>
    private static void RunUpward(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = RepositoryInstallation.CreateRuntime();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        runtime.RoomLoader.InitializeStartingCeresRoom(); runtime.RoomLoader.InitializeCeresStartSamus();
        var samus = runtime.Player.IntroducedSamus;
        samus.CollectedItems |= SamusEquipmentFlags.MorphBall;
        samus.EquippedItems |= SamusEquipmentFlags.MorphBall;
        samus.MaxMissiles = samus.Missiles = 5;
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.MorphBall);
        // This persistent native word is carried from the preceding downward ride.
        // Stage only that source state; the door coroutine and destination actor run normally.
        runtime.Enemies.ElevatorBehavior.SetElevatorStatusForDebugging((ushort)ElevatorActorStatus.Departing);
        RetailDoorCaptureTests.PublishDoor(runtime, DoorPointers.BlueBrinstarElevatorFromMorphBall);
        var transition = new DoorTransitionState();
        var audio = new CartridgeAudioState();
        transition.Begin(runtime);
        int samples = 0, scrolling = 0, arrivalFrames = 0;
        var cameraPositions = new HashSet<ushort>();
        var actorPositions = new HashSet<ushort>();
        void Compare(string phase)
        {
            var expected = FrontendScenePixelsAccess.Pixels(runtime.PublishDisplayFrame());
            var capture = GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame())
                ?? throw new InvalidOperationException("Elevator return omitted its display capture.");
            var packet = new RenderFrameSnapshot(new(++samples, 1, (ushort)samples), capture);
            PixelComparison.Verify(packet, expected, renderer.RenderForReadback(packet),
                $"{device.Kind}: upward elevator {phase}, sample={samples}");
        }
        while (transition.IsActive && samples < 512)
        {
            if (transition.Phase == DoorTransitionPhase.WaitForDoorOpeningScroll) scrolling++;
            transition.Step(runtime, audio, 0, SuperMetroid.Core.Compatibility.InstantRoomLoad.Instance);
            Compare(transition.Phase.ToString());
        }
        if (transition.IsActive || scrolling < 12 ||
            runtime.Room.ActiveRoom?.Definition != RoomHeaderDefinitions.ElevToBlueBrinstar)
            throw new InvalidOperationException("Elevator fixture missed its vertical door transition.");
        while (runtime.Enemies.ElevatorBehavior.ElevatorStatus != ElevatorActorStatus.Inactive && arrivalFrames < 1200)
        {
            runtime.StepFrame(0);
            arrivalFrames++;
            cameraPositions.Add(runtime.Room.LoadedCamera.PixelPosition.Y);
            var actor = runtime.Enemies.SlotTable.Slots.First(slot => slot.DefinitionId == EnemyDefinitionId.Elevator);
            actorPositions.Add(actor.Position.Y.Whole);
            Compare("return");
        }
        if (runtime.Enemies.ElevatorBehavior.ElevatorStatus != ElevatorActorStatus.Inactive || samus.Control.AlphaLocked ||
            cameraPositions.Count < 2 || actorPositions.Count < 2)
            throw new InvalidOperationException("Elevator fixture missed camera/actor movement or control release.");
        for (int tick = 0; tick < 8; tick++) { runtime.StepFrame(0); Compare("settled"); }
        // Preserve the existing cartridge-derived regression endpoint, not a forced
        // screen origin: front-facing Samus selects the room's up-scroller target.
        ushort expectedCameraY = unchecked((ushort)(samus.Position.Y.Whole - runtime.Room.ActiveRoom.UpScroller));
        if (runtime.Room.LoadedCamera.PixelPosition.Y != expectedCameraY || runtime.Room.LoadedCamera.IdealPosition.Y != expectedCameraY ||
            runtime.BackgroundScroll.Layer1Position.Y != expectedCameraY ||
            runtime.BackgroundScroll.Bg1Scroll.Y != unchecked((ushort)(expectedCameraY + runtime.BackgroundScroll.Bg1Offset.Y)))
            throw new InvalidOperationException("Settled elevator camera and BG1 disagree with the room up-scroller target.");
        Console.WriteLine($"{device.Kind}: upward elevator {samples} exact frames, {scrolling} scroll frames, " +
            $"{arrivalFrames} return frames, {cameraPositions.Count} camera positions; control released.");
    }
}
