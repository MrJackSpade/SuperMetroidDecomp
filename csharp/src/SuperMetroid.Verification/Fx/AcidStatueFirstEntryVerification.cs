using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Gameplay;
namespace SuperMetroid.Verification.Fx;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the first Acid Statue room transition after the Lower Norfair acid-drain event.</summary>
internal static class AcidStatueFirstEntryVerification
{
    /// <summary>Follows the retail door and checks drained acid height and pixels during destination fade.</summary>
    internal static void VerifyAcidStatueFirstEntry()
    {
        const ushort sourceRoom = 0xb236;
        const ushort destinationRoom = 0xb1e5;
        const ushort drainedAcidY = 0x02d2;

        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.System.SetEvent(EventNumber.LowerNorfairChozoLoweredAcid);
        runtime.LoadCartridgeRoomForDebug(sourceRoom);

        RoomLevelData level = runtime.Room.LoadedLevelData;
        int doorX = -1;
        int doorY = -1;
        byte behavior = 0;
        for (int y = 0; y < level.Size.Y && doorX < 0; y++)
        for (int x = 0; x < level.Size.X; x++)
        {
            RoomCollisionBlock block = level.GetCollisionBlock(new(checked((ushort)x), checked((ushort)y)));
            if (block.CollisionType != RoomCollisionType.DoorBlock)
                continue;
            CartridgeDoorHeader? candidate = level.ResolveDoorCollision(block.Behavior, SamusPoseId.FacingRightNormalPose, false).Door;
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
        SamusState samus = runtime.Player.IntroducedSamus;
        samus.InputLocked = false;
        samus.PoseId = SamusPoseId.FacingRightNormalPose;
        samus.Position = samus.Position.WithWhole(new((ushort)(doorX * 16 + 8), (ushort)(doorY * 16 + 8)));
        samus.PoseMetadata.RefreshCollisionRadii();
        samus.Presentation.InitializeAnimation();
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        runtime.Room.LoadedLevelData.ResolveDoorCollision(behavior, samus.Pose, true);

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
            transition.Step(runtime, audio, 0, SuperMetroid.Core.Rooms.Doors.LagFreeDoorLoaderProgress.Instance);
            ushort activeRoom = runtime.Room.LoadedRoom.Pointer;
            RoomLayer3FxRenderSnapshot? displayed = runtime.Display.DisplayedRoomLayer3Fx;
            if (activeRoom != destinationRoom)
                continue;
            LayeredRenderSnapshot? snapshot = GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame());
            if (snapshot is null)
                continue;
            Rgba32[] pixels = SoftwareLayeredSnapshotRenderer.Render(snapshot);
            if (phase is DoorTransitionPhase.HandleTransition or DoorTransitionPhase.FadeInDestinationPalette)
            {
                sawDestinationFade = true;
                AssertEqual(RoomFxType.Acid, (displayed ?? throw new InvalidOperationException("Verification requires displayed.")).Type,
                    "destination fade retains the room's acid FX type");
                AssertEqual(drainedAcidY, displayed.Value.Surface.Y,
                    "destination fade presents the event-restored acid height");
                lastDestinationFadePixels = pixels;
                lastDestinationFadeSurfaceRow = displayed.Value.WaterSurfaceScreenY;
            }

            destinationFrames++;
        }

        AssertTrue(!transition.IsActive, "Acid Statue room transition completes");
        AssertEqual(destinationRoom, runtime.Room.LoadedRoom.Pointer, "transition reaches the reported room");
        AssertTrue(sawDestinationFade, "transition captures the presented destination fade");
        // High acid ($00C8) would cover this whole screen. Just above the drained surface
        // the room's dark background must show; just below it the drained acid is drawn.
        AssertTrue(lastDestinationFadeSurfaceRow is > 8 and < 216, $"drained acid surface lies on the captured screen (row {lastDestinationFadeSurfaceRow})");
        Rgba32 background = new(66, 0, 24);
        AssertEqual(background, (lastDestinationFadePixels ?? throw new InvalidOperationException("Verification requires lastDestinationFadePixels."))[(lastDestinationFadeSurfaceRow - 8) * 256 + 16],
            "first-entry fade shows the dark room above the drained surface, not the high-acid flash");
        AssertTrue(lastDestinationFadePixels[(lastDestinationFadeSurfaceRow + 8) * 256 + 16] != background,
            "first-entry fade draws the drained acid below its surface");
        ushort transitionEndY = runtime.RoomLayer3Fx.Surface.Y;
        for (int frame = 0; frame < 8; frame++)
        {
            runtime.StepFrame(0);
            Rgba32[] pixels = SoftwareLayeredSnapshotRenderer.Render(
                GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame()));

        }
        AssertEqual(drainedAcidY, runtime.RoomLayer3Fx.Surface.Y,
            $"completed Chozo event restores the drained acid height (transition ended at ${transitionEndY:X4})");
        Console.WriteLine($"Acid Statue first entry: checked {destinationFrames} destination transition frames.");
    }

    /// <summary>Runs the focused Acid Statue first-entry transition fixture.</summary>
    [VerificationCommand("--acid-statue-first-entry")]
    internal static int AcidStatueFirstEntryCommand()
    {
        Suite(nameof(VerifyAcidStatueFirstEntry), () => VerifyAcidStatueFirstEntry());
        return 0;
    }
}
