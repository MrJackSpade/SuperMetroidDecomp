using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Drawing;
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
using SuperMetroid.Core.Gameplay;
namespace SuperMetroid.Verification.Fx;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms;
using SuperMetroid.TestSupport.FrameRendering;

/// <summary>Checks the first Acid Statue room transition after the Lower Norfair acid-drain event.</summary>
internal static class AcidStatueFirstEntryVerification
{
    /// <summary>Follows the retail door and checks drained acid height and pixels during destination fade.</summary>
    internal static void VerifyAcidStatueFirstEntry()
    {
        RoomDefinition sourceRoom = RoomDefinitions.MainHall;
        RoomDefinition destinationRoom = RoomDefinitions.AcidStatue;
        const ushort drainedAcidY = 0x02d2;
        _ = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.NewGame);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus(runtime.Room.LoadedScope);
        runtime.System.SetEvent(StoryEvent.LowerNorfairChozoLoweredAcid);
        runtime.LoadCartridgeRoomForDebug(sourceRoom);

        RoomLevelData level = runtime.Room.LoadedLevelData;
        int doorX = -1;
        int doorY = -1;
        for (int y = 0; y < level.Size.Y && doorX < 0; y++)
        for (int x = 0; x < level.Size.X; x++)
        {
            RoomCollisionBlock block = level.GetCollisionBlock(new(checked((ushort)x), checked((ushort)y)));
            if (block.CollisionType != RoomCollisionType.DoorBlock)
                continue;
            DoorDefinition? candidate = level.DoorExitAt(block).Door;
            if (candidate?.DestinationRoom != destinationRoom)
                continue;
            doorX = x;
            doorY = y;
            break;
        }
        AssertTrue(doorX >= 0, "Main Hall contains the retail door into the Acid Statue room");

        runtime.LoadCartridgeRoomForDebug(
            sourceRoom,
            camera: new Position16<RoomPixels>((ushort)(doorX / 16 * 256), (ushort)(doorY / 16 * 256)));
        SamusState samus = runtime.Player.Samus;
        samus.Control.Unlock();
        samus.PoseId = SamusPoseId.FacingRightNormalPose;
        samus.Position = samus.Position.WithWhole(new((ushort)(doorX * 16 + 8), (ushort)(doorY * 16 + 8)));
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        runtime.MeetDoorBlock(runtime.Room.LoadedLevelData.GetCollisionBlock(new(doorX, doorY)), samus.Pose.ForRoomCollision());

        var transition = new DoorTransitionState();
        var audio = AudioQueues.Standalone();
        transition.Begin(runtime);
        AssertTrue(runtime.System.HasEvent(StoryEvent.LowerNorfairChozoLoweredAcid),
            "completed Chozo event survives source-room staging");
        int destinationFrames = 0;
        bool sawDestinationFade = false;
        int lastDestinationFadeSurfaceRow = 0;
        for (int frame = 0; transition.IsActive && frame < 500; frame++)
        {
            DoorTransitionPhase phase = transition.Phase;
            transition.Step(runtime, audio, 0, SuperMetroid.Core.Compatibility.InstantRoomLoad.Instance);
            RoomDefinition activeRoom = runtime.Room.LoadedRoom.Definition;
            RoomFxOverlayRenderSnapshot? displayed = runtime.Display.DisplayedRoomFxOverlay;
            if (activeRoom != destinationRoom)
                continue;
            LayeredScene? snapshot = GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame());
            if (snapshot is null)
                continue;
            // Every destination frame draws (#3036, #3047): its colours and its overlay, as the frame boundary latched them.
            _ = DisplayRendering.Render(snapshot);
            if (phase is DoorTransitionPhase.Running.FinishDoorOpeningScroll or DoorTransitionPhase.Running.FadeInDestinationRoom)
            {
                sawDestinationFade = true;
                AssertEqual(RoomFxType.Acid, (displayed ?? throw new InvalidOperationException("Verification requires displayed.")).Type,
                    "destination fade retains the room's acid FX type");
                AssertEqual(drainedAcidY, displayed.Value.NamedSurface,
                    "destination fade presents the event-restored acid height");
                lastDestinationFadeSurfaceRow = displayed.Value.NamedSurfaceScreenY;
            }

            destinationFrames++;
        }

        AssertTrue(!transition.IsActive, "Acid Statue room transition completes");
        AssertEqual(destinationRoom, runtime.Room.LoadedRoom.Definition, "transition reaches the reported room");
        AssertTrue(sawDestinationFade, "transition captures the presented destination fade");
        // The fade shows the drained surface where it lies below the camera (#3047), as the cartridge's HDMA objects place it on
        // every update of the fade in: the door enters at the room's top, so the drained acid lies below the screen and the
        // fade shows none of it, not the high acid's surface that the room's load first placed.
        int cameraY = (runtime.Room.LoadedScope.Camera.PixelPosition - Position16<RoomPixels>.Zero).Y;
        AssertEqual(drainedAcidY - cameraY, lastDestinationFadeSurfaceRow,
            "first-entry fade places the drained acid's surface below the camera");
        AssertTrue(lastDestinationFadeSurfaceRow >= ScreenGeometry.ScreenSize.Y,
            $"first-entry fade shows no acid: the drained surface lies below the screen (row {lastDestinationFadeSurfaceRow})");
        int transitionEndY = runtime.RoomFxOverlay.NamedSurface;
        for (int frame = 0; frame < 8; frame++)
        {
            runtime.StepFrame(0);
            runtime.PublishDisplayFrame();
        }
        AssertEqual(drainedAcidY, runtime.RoomFxOverlay.NamedSurface,
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
