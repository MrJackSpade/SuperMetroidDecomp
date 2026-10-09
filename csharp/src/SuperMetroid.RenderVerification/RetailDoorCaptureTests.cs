using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Rendering.Direct3D11;

/// <summary>Captures and compares retail Ceres door-transition frames through the Direct3D11 renderer.</summary>
internal static class RetailDoorCaptureTests
{
    /// <summary>Runs the leftward and rightward retail hallway door captures.</summary>
    /// <param name="device">Rendering device used to label the captured sequence.</param>
    /// <param name="renderer">Renderer whose readback is compared with the runtime frame.</param>
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        foreach (bool left in new[] { false, true })
        {
            var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
            var runtime = RepositoryInstallation.CreateRuntime(bus);
            runtime.InitializeHud(HudSnapshot.CeresDebug); runtime.RunNmi(0, true);
            runtime.InitializeStartingCeresRoom(); runtime.InitializeCeresStartSamus();
            ushort destination = left ? RoomHeaderPointers.CeresDeadScientistRoom : RoomHeaderPointers.CeresFinalHallway;
            ushort door = left ? DoorPointers.CeresDeadScientistFromFinalHallway : DoorPointers.CeresFinalHallwayFromDeadScientist;
            runtime.LoadCartridgeRoomForDebug(left ? RoomHeaderPointers.CeresFinalHallway : RoomHeaderPointers.CeresDeadScientistRoom,
                left ? DoorTransitionAuditPositions.Origin : DoorTransitionAuditPositions.SecondScreenCameraX, 0);
            runtime.Samus!.Kinematics.SetXFixed(left ? DoorTransitionAuditPositions.LeftDoorSamusXFixed : DoorTransitionAuditPositions.SecondScreenRightDoorSamusXFixed);
            runtime.Samus.Kinematics.SetYFixed(DoorTransitionAuditPositions.CeresCorridorDoorSamusYFixed);
            SwapchainTests.VerifyBlockedDoor(device, renderer, runtime, bus, door, destination, left ? "left" : "right");
        }
    }

    /// <summary>Advances a retail door transition to its destination while comparing each rendered frame.</summary>
    /// <param name="device">Rendering device associated with the capture.</param>
    /// <param name="renderer">Renderer used for frame readback.</param>
    /// <param name="runtime">Initialized runtime containing the source room and Samus state.</param>
    /// <param name="bus">Cartridge address space used to resolve and publish the authored door.</param>
    /// <param name="door">Native door pointer that the fixture must open.</param>
    /// <param name="destination">Expected destination room pointer after scrolling completes.</param>
    /// <param name="context">Label included in comparison errors and completion output.</param>
    /// <param name="publish">Optional callback receiving each serialized frame snapshot.</param>
    internal static void VerifyTransition(D3D11RenderDevice device, D3D11FrameRenderer renderer,
        SuperMetroidRuntime runtime, ISnesAddressSpace bus, ushort door, ushort destination, string context,
        Action<RenderFrameSnapshot>? publish = null)
    {
            PublishDoor(runtime, bus, door);
            var transition = new DoorTransitionState();
            var audio = new CartridgeAudioState();
            transition.Begin(runtime);
            int frames = 0, scrollFrames = 0;
            while (transition.IsActive && frames < 320)
            {
                if (transition.Phase == DoorTransitionPhase.WaitForDoorOpeningScroll) scrollFrames++;
                transition.Step(runtime, audio, 0, SuperMetroid.Core.Runtime.LagFreeDoorLoaderProgress.Instance);
                var expected = SuperMetroidRuntimeFrameRenderer.Render(runtime);
                var packet = new RenderFrameSnapshot(new(++frames, 1, (ushort)frames), GameplayDisplayCapture.TryCaptureFrame(runtime)!);
                packet = RenderFrameSnapshotCodec.Deserialize(RenderFrameSnapshotCodec.Serialize(packet));
                publish?.Invoke(packet);
                PixelComparison.Verify(packet, expected, renderer.RenderForReadback(packet),
                    $"{device.Kind}: door {door:X4}, {context}, frame={frames}, phase={transition.Phase}");
            }
            if (transition.IsActive || scrollFrames < 60 || runtime.ActiveDoor?.DestinationRoomPointer != destination)
                throw new InvalidOperationException($"Door fixture missed complete scrolling/destination: {context}, scroll={scrollFrames}.");
            Console.WriteLine($"{device.Kind}: {context} retail door: {frames} exact frames, {scrollFrames} scroll frames, destination verified.");
    }

    /// <summary>Finds a room collision for the requested door and publishes its production door side effects.</summary>
    /// <param name="runtime">Runtime whose loaded room contains the door collision.</param>
    /// <param name="bus">Cartridge address space used to resolve each collision's door pointer.</param>
    /// <param name="pointer">Native door pointer to locate and publish.</param>
    /// <exception cref="InvalidOperationException">No room collision resolves to the requested door.</exception>
    internal static void PublishDoor(SuperMetroidRuntime runtime, ISnesAddressSpace bus, ushort pointer)
    {
        // Same staged approach as the existing visual regression: discover the real
        // authored door via its collision block, then run the production dispatcher.
        var level = runtime.LevelData!;
        for (int index = 0; index < level.ForegroundEntries.Length; index++)
        {
            var block = level.GetCollisionBlockByIndex(index);
            if (block.CollisionType != RoomCollisionType.DoorBlock) continue;
            var door = level.ResolveDoorCollision(bus, block.Behavior, runtime.Samus!.Pose, publishDoorSideEffects: false);
            if (door.Pointer != pointer) continue;
            level.ResolveDoorCollision(bus, block.Behavior, runtime.Samus.Pose, publishDoorSideEffects: true);
            return;
        }
        throw new InvalidOperationException($"No authored collision for door {pointer:X4}.");
    }
}
