using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Input;
using static SuiteTimingVerification;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static RetailRuntimeFixtureVerification;

internal static class AcidStatueFirstEntryVerification
{
    internal static void VerifyAcidStatueFirstEntry()
    {
        const ushort sourceRoom = 0xb236;
        const ushort destinationRoom = 0xb1e5;
        const ushort drainedAcidY = 0x02d2;

        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.System.SetEvent(EventNumber.LowerNorfairChozoLoweredAcid);
        runtime.LoadCartridgeRoomForDebug(sourceRoom);

        RoomLevelData level = runtime.LevelData!;
        int doorX = -1;
        int doorY = -1;
        byte behavior = 0;
        for (int y = 0; y < level.Size.Y && doorX < 0; y++)
        for (int x = 0; x < level.Size.X; x++)
        {
            RoomCollisionBlock block = level.GetCollisionBlock(new(checked((ushort)x), checked((ushort)y)));
            if (block.CollisionType != RoomCollisionType.DoorBlock)
                continue;
            CartridgeDoorHeader? candidate = level.ResolveDoorCollision(bus, block.Behavior, SamusPoseId.FacingRightNormalPose, false).Door;
            if (candidate?.DestinationRoomPointer != destinationRoom)
                continue;
            doorX = x;
            doorY = y;
            behavior = block.Behavior;
            break;
        }
        AssertTrue(doorX >= 0, "Main Hall contains the retail door into the Acid Statue room");

        runtime.LoadCartridgeRoomForDebug(
            sourceRoom,
            camera: new Position16<RoomPixels>((ushort)(doorX / 16 * 256), (ushort)(doorY / 16 * 256)));
        SamusState samus = runtime.Samus!;
        samus.InputLocked = false;
        samus.PoseId = SamusPoseId.FacingRightNormalPose;
        samus.Position = samus.Position.WithWhole(new((ushort)(doorX * 16 + 8), (ushort)(doorY * 16 + 8)));
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        runtime.RunNmi(0, true);
        runtime.LevelData!.ResolveDoorCollision(bus, behavior, samus.Pose, true);

        var transition = new DoorTransitionState();
        var audio = new CartridgeAudioState();
        transition.Begin(runtime);
        AssertTrue(runtime.System.HasEvent(EventNumber.LowerNorfairChozoLoweredAcid),
            "completed Chozo event survives source-room staging");
        int destinationFrames = 0;
        bool sawDestinationFade = false;
        Rgba32[]? lastDestinationFadePixels = null;
        int lastDestinationFadeSurfaceRow = 0;
        for (int frame = 0; transition.IsActive && frame < 500; frame++)
        {
            DoorTransitionPhase phase = transition.Phase;
            transition.Step(runtime, audio, 0, SuperMetroid.Core.Runtime.LagFreeDoorLoaderProgress.Instance);
            ushort activeRoom = runtime.ActiveRoom!.Pointer;
            RoomLayer3FxRenderSnapshot? displayed = runtime.DisplayedRoomLayer3Fx;
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
                lastDestinationFadeSurfaceRow = displayed.Value.WaterSurfaceScreenY;
            }

            destinationFrames++;
        }

        AssertTrue(!transition.IsActive, "Acid Statue room transition completes");
        AssertEqual(destinationRoom, runtime.ActiveRoom!.Pointer, "transition reaches the reported room");
        AssertTrue(sawDestinationFade, "transition captures the presented destination fade");
        // High acid ($00C8) would cover this whole screen. Just above the drained surface
        // the room's dark background must show; just below it the drained acid is drawn.
        AssertTrue(lastDestinationFadeSurfaceRow is > 8 and < 216, $"drained acid surface lies on the captured screen (row {lastDestinationFadeSurfaceRow})");
        Rgba32 background = new(66, 0, 24);
        AssertEqual(background, lastDestinationFadePixels![(lastDestinationFadeSurfaceRow - 8) * 256 + 16],
            "first-entry fade shows the dark room above the drained surface, not the high-acid flash");
        AssertTrue(lastDestinationFadePixels[(lastDestinationFadeSurfaceRow + 8) * 256 + 16] != background,
            "first-entry fade draws the drained acid below its surface");
        ushort transitionEndY = runtime.RoomLayer3Fx.CurrentYPosition;
        for (int frame = 0; frame < 8; frame++)
        {
            runtime.StepFrame(0);
            Rgba32[] pixels = SoftwareLayeredSnapshotRenderer.Render(
                GameplayDisplayCapture.TryCaptureFrame(runtime)!);

        }
        AssertEqual(drainedAcidY, runtime.RoomLayer3Fx.CurrentYPosition,
            $"completed Chozo event restores the drained acid height (transition ended at ${transitionEndY:X4})");
        Console.WriteLine($"Acid Statue first entry: checked {destinationFrames} destination transition frames.");
    }

    [VerificationCommand("--acid-statue-first-entry")]
    internal static int AcidStatueFirstEntryCommand()
    {
        Suite(nameof(VerifyAcidStatueFirstEntry), () => VerifyAcidStatueFirstEntry());
        return 0;
    }
}
