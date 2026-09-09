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
    // #370 diagnostic: stage each reciprocal doorway, never preload the destination.
    // A completed transition is only fixture setup, not proof the water pixels are correct.
    private static void VerifyMoatFirstEntry()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        const string output = "csharp/test-temp/issue-370-moat";
        Directory.CreateDirectory(output);
        var unpausedFrames = new Dictionary<(ushort Source, int Frame), Rgba32[]>();
        foreach (ushort source in new ushort[] { 0x948c, 0x93fe })
        foreach (bool pauseBeforeEntry in new[] { false, true })
        {
            var runtime = new SuperMetroidRuntime(bus, playerInvincibilityEnabled: true);
            runtime.InitializeHud(HudSnapshot.CeresDebug);
            runtime.InitializeStartingCeresRoom();
            runtime.InitializeCeresStartSamus();
            runtime.LoadCartridgeRoomForDebug(source);
            var level = runtime.LevelData!;
            int doorX = -1, doorY = -1;
            byte behavior = 0;
            for (int y = 0; y < level.HeightInBlocks && doorX < 0; y++)
            for (int x = 0; x < level.WidthInBlocks; x++)
            {
                var block = level.GetCollisionBlock(x, y);
                if (block.CollisionType != RoomCollisionType.DoorBlock) continue;
                var door = level.ResolveDoorCollision(bus, block.Behavior, 1, false);
                if (door.DestinationRoomPointer != 0x95ff) continue;
                doorX = x; doorY = y; behavior = block.Behavior;
                break;
            }
            AssertTrue(doorX >= 0, "retail neighbor has a reciprocal Moat door");
            runtime.LoadCartridgeRoomForDebug(source,
                cameraX: (ushort)(doorX / 16 * 256), cameraY: (ushort)(doorY / 16 * 256));
            var samus = runtime.Samus!;
            samus.InputLocked = false;
            samus.PoseId = SamusPoseId.FacingRightNormalPose;
            samus.XPosition = (ushort)(doorX * 16 + 8);
            samus.YPosition = (ushort)(doorY * 16 + 8);
            samus.RefreshCollisionRadii(bus);
            samus.InitializeAnimation(bus);
            runtime.RunNmi(0, true);
            if (pauseBeforeEntry)
            {
                byte[] gameplayBeforePause = runtime.Vram.Bytes.ToArray();
                var room = runtime.ActiveRoom!;
                var pause = new PauseMenuState(bus, samus, runtime.System, room.AreaIndex,
                    room.MapX, room.MapY, gameplayVram: runtime.Vram);
                // Use the real menu owner, including a map -> equipment transition.
                // This isolates VRAM ownership rather than claiming a complete host
                // pause-state replay. Unpause's beam publication is applied below.
                pause.Step((ushort)SuperMetroid.Core.Input.SnesButton.R,
                    (ushort)SuperMetroid.Core.Input.SnesButton.R);
                for (int frame = 0; frame < 64; frame++) pause.Step(0, 0);
                AssertEqual(1, pause.ScreenMode, "pause fixture reaches equipment graphics");
                _ = pause.Render();
                AssertTrue(runtime.Vram.Bytes.SequenceEqual(gameplayBeforePause),
                    "pause graphics do not mutate gameplay VRAM");
                runtime.QueueGameplayBeamTilesAndLoadPalette(samus.EquippedBeams);
                runtime.RunNmi(0, true);
            }
            runtime.LevelData!.ResolveDoorCollision(bus, behavior, samus.Pose, true);
            var transition = new DoorTransitionState();
            var audio = new CartridgeAudioState();
            transition.Begin(runtime);
            for (int frame = 0; transition.IsActive && frame < 400; frame++)
                transition.Step(runtime, audio, 0);
            AssertTrue(!transition.IsActive, "staged first-entry door transition completes");
            AssertEqual((ushort)0x95ff, runtime.ActiveRoom!.Pointer, "first entry reached reported room");
            for (int frame = 0; frame < 4; frame++)
            {
                runtime.StepFrame(0);
                var snapshot = GameplayDisplayCapture.TryCaptureFrame(runtime)!;
                var pixels = SoftwareLayeredSnapshotRenderer.Render(snapshot);
                if (!pauseBeforeEntry) unpausedFrames[(source, frame)] = pixels;
                else AssertTrue(pixels.SequenceEqual(unpausedFrames[(source, frame)]),
                    "pause/equipment history leaves identical first-entry water and scene pixels");
                PngWriter.WriteRgba($"{output}/from-{source:X4}-pause-{pauseBeforeEntry}-frame-{frame}.png", 256, 224,
                    pixels);
            }
            Console.WriteLine($"Moat first entry from {source:X4}, pause={pauseBeforeEntry}: FX={runtime.RoomLayer3Fx.Type}, BG3 characters={runtime.GameplayHudCharacterBaseWord:X4}, camera={runtime.Camera!.XPosition}/{runtime.Camera.YPosition}. Captured four visible frames; visual diagnosis remains required.");
        }
    }
}
