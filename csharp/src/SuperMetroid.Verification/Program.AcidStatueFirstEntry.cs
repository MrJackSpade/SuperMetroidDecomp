using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifyAcidStatueFirstEntry()
    {
        const ushort sourceRoom = 0xb236;
        const ushort destinationRoom = 0xb1e5;
        const ushort drainedAcidY = 0x02d2;
        const string output = "csharp/test-temp/issue-612-acid-entry";
        Directory.CreateDirectory(output);

        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = new SuperMetroidRuntime(bus, playerInvincibilityEnabled: true);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.System.SetEvent(EventNumber.LowerNorfairChozoLoweredAcid);
        runtime.LoadCartridgeRoomForDebug(sourceRoom);

        RoomLevelData level = runtime.LevelData!;
        int doorX = -1;
        int doorY = -1;
        byte behavior = 0;
        for (int y = 0; y < level.HeightInBlocks && doorX < 0; y++)
        for (int x = 0; x < level.WidthInBlocks; x++)
        {
            RoomCollisionBlock block = level.GetCollisionBlock(x, y);
            if (block.CollisionType != RoomCollisionType.DoorBlock)
                continue;
            CartridgeDoorHeader candidate = level.ResolveDoorCollision(bus, block.Behavior, 1, false);
            if (candidate.DestinationRoomPointer != destinationRoom)
                continue;
            doorX = x;
            doorY = y;
            behavior = block.Behavior;
            break;
        }
        AssertTrue(doorX >= 0, "Main Hall contains the retail door into the Acid Statue room");

        runtime.LoadCartridgeRoomForDebug(
            sourceRoom,
            cameraX: (ushort)(doorX / 16 * 256),
            cameraY: (ushort)(doorY / 16 * 256));
        SamusState samus = runtime.Samus!;
        samus.InputLocked = false;
        samus.PoseId = SamusPoseId.FacingRightNormalPose;
        samus.XPosition = (ushort)(doorX * 16 + 8);
        samus.YPosition = (ushort)(doorY * 16 + 8);
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        runtime.RunNmi(0, true);
        runtime.LevelData!.ResolveDoorCollision(bus, behavior, samus.Pose, true);

        var transition = new DoorTransitionState();
        var audio = new CartridgeAudioState();
        transition.Begin(runtime);
        using var trace = new StreamWriter(Path.Combine(output, "trace.csv"));
        AssertTrue(runtime.System.HasEvent(EventNumber.LowerNorfairChozoLoweredAcid),
            "completed Chozo event survives source-room staging");
        trace.WriteLine("frame,phase,room,event,liveType,liveY,displayedType,displayedY");
        int destinationFrames = 0;
        bool sawDestinationFade = false;
        Rgba32[]? lastDestinationFadePixels = null;
        for (int frame = 0; transition.IsActive && frame < 500; frame++)
        {
            DoorTransitionPhase phase = transition.Phase;
            transition.Step(runtime, audio, 0);
            ushort activeRoom = runtime.ActiveRoom!.Pointer;
            RoomLayer3FxRenderSnapshot? displayed = runtime.DisplayedRoomLayer3Fx;
            trace.WriteLine($"{frame},{phase},{activeRoom:X4},{runtime.System.HasEvent(EventNumber.LowerNorfairChozoLoweredAcid)},{runtime.RoomLayer3Fx.Type},{runtime.RoomLayer3Fx.CurrentYPosition:X4},{displayed?.Type},{displayed?.CurrentYPosition:X4}");
            if (activeRoom != destinationRoom)
                continue;
            LayeredRenderSnapshot? snapshot = GameplayDisplayCapture.TryCaptureFrame(runtime);
            if (snapshot is null)
                continue;
            Rgba32[] pixels = SoftwareLayeredSnapshotRenderer.Render(snapshot);
            if (phase is DoorTransitionPhase.HandleTransition or DoorTransitionPhase.FadeInDestinationPalette)
            {
                sawDestinationFade = true;
                AssertEqual(RoomFxType.Acid, displayed!.Value.Type,
                    "destination fade retains the room's acid FX type");
                AssertEqual(drainedAcidY, displayed.Value.CurrentYPosition,
                    "destination fade presents the event-restored acid height");
                lastDestinationFadePixels = pixels;
            }
            PngWriter.WriteRgba(
                Path.Combine(output, $"destination-{destinationFrames:D3}-{phase}.png"),
                256,
                224,
                pixels);
            destinationFrames++;
        }

        AssertTrue(!transition.IsActive, "Acid Statue room transition completes");
        AssertEqual(destinationRoom, runtime.ActiveRoom!.Pointer, "transition reaches the reported room");
        AssertTrue(sawDestinationFade, "transition captures the presented destination fade");
        AssertEqual(new Rgba32(66, 0, 24), lastDestinationFadePixels![216 * 256 + 16],
            "first-entry bottom edge contains the drained dark surface, not the bright high-acid flash");
        ushort transitionEndY = runtime.RoomLayer3Fx.CurrentYPosition;
        for (int frame = 0; frame < 8; frame++)
        {
            runtime.StepFrame(0);
            trace.WriteLine($"{500 + frame},Gameplay,{runtime.ActiveRoom!.Pointer:X4},{runtime.System.HasEvent(EventNumber.LowerNorfairChozoLoweredAcid)},{runtime.RoomLayer3Fx.Type},{runtime.RoomLayer3Fx.CurrentYPosition:X4},{runtime.DisplayedRoomLayer3Fx?.Type},{runtime.DisplayedRoomLayer3Fx?.CurrentYPosition:X4}");
            Rgba32[] pixels = SoftwareLayeredSnapshotRenderer.Render(
                GameplayDisplayCapture.TryCaptureFrame(runtime)!);
            PngWriter.WriteRgba(Path.Combine(output, $"gameplay-{frame:D3}.png"), 256, 224, pixels);
        }
        AssertEqual(drainedAcidY, runtime.RoomLayer3Fx.CurrentYPosition,
            $"completed Chozo event restores the drained acid height (transition ended at ${transitionEndY:X4})");
        Console.WriteLine($"Acid Statue first entry: captured {destinationFrames} destination transition frames in {output}.");
    }
}
