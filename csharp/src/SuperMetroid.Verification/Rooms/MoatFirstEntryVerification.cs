using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Assets;
namespace SuperMetroid.Verification.Rooms;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Fx;
using SuperMetroid.TestSupport.FrameRendering;

/// <summary>Checks first-entry Moat water pixels through both reciprocal doors and pause history.</summary>
internal static class MoatFirstEntryVerification
{
    /// <summary>Stages each reciprocal doorway (#370) and compares transition and first-gameplay pixels with and without pause.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    internal static void VerifyMoatFirstEntry()
    {

        var unpausedFrames = new Dictionary<UnpausedFrame, RgbaPixel[]>();
        var unpausedTransitionFrames = new Dictionary<UnpausedFrame, RgbaPixel[]>();
        foreach (ushort source in new ushort[] { 0x948c, 0x93fe })
        foreach (bool pauseBeforeEntry in new[] { false, true })
        {
            // Each history starts with fresh mutable WRAM as well as fresh owners.
            var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
            var runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true);
            runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
            runtime.RoomLoader.InitializeStartingCeresRoom();
            runtime.RoomLoader.InitializeCeresStartSamus();
            runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.Get(source));
            var level = runtime.Room.LoadedLevelData;
            int doorX = -1, doorY = -1;
            int behavior = 0;
            for (int y = 0; y < level.Size.Y && doorX < 0; y++)
            for (int x = 0; x < level.Size.X; x++)
            {
                var block = level.GetCollisionBlock(new(checked((ushort)x), checked((ushort)y)));
                if (block.CollisionType != RoomCollisionType.DoorBlock) continue;
                var door = level.ResolveDoorCollision(block.Bts.Value, SamusPoseId.FacingRightNormalPose.ForRoomCollision(), false);
                if (door.Door?.DestinationRoom != RoomHeaderDefinitions.Moat) continue;
                doorX = x; doorY = y; behavior = block.Bts.Value;
                break;
            }
            AssertTrue(doorX >= 0, "retail neighbor has a reciprocal Moat door");
            runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.Get(source),
                camera: new Position16<RoomPixels>((ushort)(doorX / 16 * 256), (ushort)(doorY / 16 * 256)));
            var samus = runtime.Player.IntroducedSamus;
            samus.Control.Unlock();
            samus.PoseId = SamusPoseId.FacingRightNormalPose;
            samus.Position = samus.Position.WithWhole(new((ushort)(doorX * 16 + 8), (ushort)(doorY * 16 + 8)));
            SamusPoseMetadata.RefreshCollisionRadii(samus);
            SamusPresentation.InitializeAnimation(samus);
            runtime.FrameBoundary.BeginAcceptedFrame(0);
            if (pauseBeforeEntry)
            {
                byte[] gameplayBeforePause = runtime.Vram.Bytes.ToArray();
                var room = runtime.Room.LoadedRoom;
                var pause = CreateRetailPauseFixture(new PauseMenuEntry { Samus = samus, System = runtime.System, AreaIndex = room.AreaIndex, RoomMapPosition = room.MapPosition, GameplayLayer3 = runtime.AcceptedLayer3Plane });
                // Use the real menu owner, including a map -> equipment transition.
                // This isolates VRAM ownership rather than claiming a complete host
                // pause-state replay. Unpause's beam publication is applied below.
                pause.Step(ControllerButton.R,
                    ControllerButton.R);
                for (int frame = 0; frame < 64; frame++) pause.Step(0, 0);
                AssertEqual(PauseScreenMode.Equipment, pause.ScreenMode, "pause fixture reaches equipment graphics");
                _ = pause.Render();
                AssertTrue(runtime.Vram.Bytes.SequenceEqual(gameplayBeforePause),
                    "pause graphics do not mutate gameplay VRAM");
                runtime.SamusPhases.UpdateGameplayBeamTilesAndPalette(samus.EquippedBeams);
                runtime.FrameBoundary.BeginAcceptedFrame(0);
            }
            runtime.Room.LoadedLevelData.ResolveDoorCollision(behavior, samus.Pose.ForRoomCollision(), true);
            var transition = new DoorTransitionState();
            var audio = new AudioQueues();
            transition.Begin(runtime);
            int transitionFrames = 0;
            for (int frame = 0; transition.IsActive && frame < 400; frame++)
            {
                var phase = transition.Phase;
                transition.Step(runtime, audio, 0, SuperMetroid.Core.Compatibility.InstantRoomLoad.Instance);
                // SuperMetroidGame publishes gameplay immediately after this same
                // transition step. Capture the IRQ scroll and palette fade too,
                // not just the already-completed destination used by older probes.
                var snapshot = GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame());
                var pixels = DisplayRendering.Render(snapshot);
                if (!pauseBeforeEntry) unpausedTransitionFrames[new UnpausedFrame(source, frame)] = pixels;
                else AssertTrue(pixels.SequenceEqual(unpausedTransitionFrames[new UnpausedFrame(source, frame)]),
                    $"pause/equipment history leaves identical transition pixels at {phase}, frame {frame}");

                transitionFrames++;
            }
            AssertTrue(!transition.IsActive, "staged first-entry door transition completes");
            AssertEqual(RoomHeaderDefinitions.Moat, runtime.Room.LoadedRoom.Definition, "first entry reached reported room");
            for (int frame = 0; frame < 4; frame++)
            {
                runtime.StepFrame(0);
                var snapshot = GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame());
                var pixels = DisplayRendering.Render(snapshot);
                if (!pauseBeforeEntry) unpausedFrames[new UnpausedFrame(source, frame)] = pixels;
                else AssertTrue(pixels.SequenceEqual(unpausedFrames[new UnpausedFrame(source, frame)]),
                    "pause/equipment history leaves identical first-entry water and scene pixels");

            }
            Console.WriteLine($"Moat first entry from {source:X4}, pause={pauseBeforeEntry}: FX={runtime.RoomLayer3Fx.Type}, BG3 characters={runtime.Display.GameplayHudCharacterBaseWord:X4}, camera={runtime.Room.LoadedCamera.PixelPosition.X}/{runtime.Room.LoadedCamera.PixelPosition.Y}. Captured {transitionFrames} transition frames and four gameplay frames; visual diagnosis remains required.");
        }
    }

    /// <summary>Runs the staged Moat first-entry visual comparison.</summary>
    [VerificationCommand("--moat-first-entry")]
    internal static int MoatFirstEntryCommand()
    {
        Suite(nameof(VerifyMoatFirstEntry), () => VerifyMoatFirstEntry());
        return 0;
    }
}
