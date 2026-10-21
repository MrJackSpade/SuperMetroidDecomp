using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.RenderVerification.Device;
using SuperMetroid.Rendering.Direct3D11.Device;
using SuperMetroid.Rendering.Direct3D11.Frames;
namespace SuperMetroid.RenderVerification.Rooms;

using SuperMetroid.TestSupport.FrameCapture;
using SuperMetroid.TestSupport.RenderFixtures;
using SuperMetroid.TestSupport.RepositoryInstallation;

/// <summary>Compares captured Ceres door transitions with direct retail-frame rendering.</summary>
internal static class RetailDoorCaptureTests
{
    /// <summary>Exercises both directions between Ceres' final hallway and dead scientist room.</summary>
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        foreach (bool left in new[] { false, true })
        {
            var runtime = RepositoryInstallation.CreateRuntime();
            runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug); runtime.FrameBoundary.BeginAcceptedFrame(0);
            runtime.RoomLoader.InitializeStartingCeresRoom(); runtime.RoomLoader.InitializeCeresStartSamus();
            RoomHeaderDefinition destination = left ? RoomHeaderDefinitions.DeadScientist : RoomHeaderDefinitions.Room58Escape;
            ushort door = left ? DoorPointers.CeresDeadScientistFromFinalHallway : DoorPointers.CeresFinalHallwayFromDeadScientist;
            runtime.LoadCartridgeRoomForDebug(left ? RoomHeaderDefinitions.Room58Escape : RoomHeaderDefinitions.DeadScientist,
                new Position16<RoomPixels>(left ? DoorTransitionAuditPositions.Origin : DoorTransitionAuditPositions.SecondScreenCameraX, 0));
            runtime.Player.IntroducedSamus.Kinematics.Position = runtime.Player.IntroducedSamus.Kinematics.Position.WithX(SnesFixedPosition.FromRaw(left ? DoorTransitionAuditPositions.LeftDoorSamusXFixed : DoorTransitionAuditPositions.SecondScreenRightDoorSamusXFixed));
            runtime.Player.IntroducedSamus.Kinematics.Position = runtime.Player.IntroducedSamus.Kinematics.Position.WithY(SnesFixedPosition.FromRaw(DoorTransitionAuditPositions.CeresCorridorDoorSamusYFixed));
            SwapchainTests.VerifyBlockedDoor(device, renderer, runtime, door, destination, left ? "left" : "right");
        }
    }

    /// <summary>Advances a door through scrolling and destination load, comparing each captured frame.</summary>
    internal static void VerifyTransition(D3D11RenderDevice device, D3D11FrameRenderer renderer, SuperMetroidRuntime runtime, RetailDoorCase doorCase, string context, Action<RenderFrameSnapshot>? publish = null)
    {
            PublishDoor(runtime, doorCase.Door);
            var transition = new DoorTransitionState();
            var audio = new CartridgeAudioState();
            transition.Begin(runtime);
            int frames = 0, scrollFrames = 0;
            while (transition.IsActive && frames < 320)
            {
                if (transition.Phase == DoorTransitionPhase.WaitForDoorOpeningScroll) scrollFrames++;
                transition.Step(runtime, audio, 0, SuperMetroid.Core.Rooms.Doors.LagFreeDoorLoaderProgress.Instance);
                var expected = FrontendScenePixelsAccess.Pixels(runtime.PublishDisplayFrame());
                var packet = new RenderFrameSnapshot(new(++frames, 1, (ushort)frames), GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame()));
                publish?.Invoke(packet);
                PixelComparison.Verify(packet, expected, renderer.RenderForReadback(packet),
                    $"{device.Kind}: door {doorCase.Door:X4}, {context}, frame={frames}, phase={transition.Phase}");
            }
            if (transition.IsActive || scrollFrames < 60 || runtime.Room.ActiveDoor?.DestinationRoom != doorCase.Destination)
                throw new InvalidOperationException($"Door fixture missed complete scrolling/destination: {context}, scroll={scrollFrames}.");
            Console.WriteLine($"{device.Kind}: {context} retail door: {frames} exact frames, {scrollFrames} scroll frames, destination verified.");
    }

    /// <summary>Finds the authored collision block and publishes its door through production collision handling.</summary>
    internal static void PublishDoor(SuperMetroidRuntime runtime, ushort pointer)
    {
        // Same staged approach as the existing visual regression: discover the real
        // authored door via its collision block, then run the production dispatcher.
        var level = runtime.Room.LoadedLevelData;
        for (int index = 0; index < level.ForegroundEntries.Length; index++)
        {
            var block = level.GetCollisionBlockByIndex(index);
            if (block.CollisionType != RoomCollisionType.DoorBlock) continue;
            var door = level.ResolveDoorCollision(block.Bts.Value, runtime.Player.IntroducedSamus.Pose.ForRoomCollision(), publishDoorSideEffects: false);
            if (door.Pointer != pointer) continue;
            level.ResolveDoorCollision(block.Bts.Value, runtime.Player.IntroducedSamus.Pose.ForRoomCollision(), publishDoorSideEffects: true);
            return;
        }
        throw new InvalidOperationException($"No authored collision for door {pointer:X4}.");
    }
}
