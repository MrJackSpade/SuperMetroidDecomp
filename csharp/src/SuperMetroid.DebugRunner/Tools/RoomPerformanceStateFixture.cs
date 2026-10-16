using System.Diagnostics;
using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Hardware;
namespace SuperMetroid.DebugRunner.Tools;
using SuperMetroid.Diagnostics.Android;

/// <summary>Representative retail-room fixtures for Android performance issues #361-363.</summary>
internal static class RoomPerformanceStateFixture
{
    /// <summary>Captures a save, map, or surface room state and measures 600 post-warmup gameplay frames.</summary>
    public static int Export(string scene, string destination)
    {
        if (scene is not ("save" or "map" or "surface"))
            throw new ArgumentException("Expected save, map, or surface.", nameof(scene));
        string root = Path.GetFullPath(destination);
        if (Directory.Exists(root)) throw new IOException($"Fixture directory already exists: {root}");
        Directory.CreateDirectory(root);
        using var data = new AndroidSessionData(root, Path.GetFullPath("Super Metroid.smc"),
            Path.GetFullPath("standalone-assets/audio"));
        data.Game.Session.SetupSelectedGame();
        var runtime = data.Game.Session.GameplayRuntime;
        bool surface = scene == "surface";
        ushort camera = surface ? (ushort)1024 : (ushort)0;
        if (scene == "map")
        {
            // The actual map-room entrance runs its cartridge setup and door PLM;
            // this does not simulate an entire route merely to benchmark one room.
            var door = SuperMetroid.AssetExtraction.Rooms.CartridgeDoorHeaderImporter.Load(data.Bus, RoomPerformanceFixtureDefinitions.CrateriaMapEntranceDoor);
            runtime.LoadCartridgeRoomThroughDoorForVerification(door, new Position16<RoomPixels>(camera, camera));
        }
        else
        {
            ushort room = surface ? RoomHeaderPointers.LandingSite : RoomHeaderPointersTooling.CrateriaSaveStation;
            runtime.LoadCartridgeRoomForDebug(room, new Position16<RoomPixels>(camera, camera));
        }
        runtime.Player.IntroducedSamus.InputLocked = true;
        runtime.Player.IntroducedSamus.Position = runtime.Player.IntroducedSamus.Position.WithWhole(new((ushort)(camera + (surface ? 128 : 64)), (ushort)(camera + 160)));
        data.Game.GameState = SuperMetroidGameState.MainGameplay;
        double stepMs = 0, renderMs = 0, audioMs = 0;
        var pcm = new short[CartridgeAudioSession.PcmSamplesPerFrame];
        Rgba32[]? pixels = null;
        RenderFrameSnapshot? lastSnapshot = null;
        for (long frame = 1; frame <= 720; frame++)
        {
            data.Record(0);
            long start = Stopwatch.GetTimestamp();
            var result = data.Game.StepCaptured(0, frame, data.Generation);
            lastSnapshot = result.Snapshot;
            long stepped = Stopwatch.GetTimestamp();
            pixels = SoftwareFrameSnapshotRenderer.Render(result.Snapshot);
            long rendered = Stopwatch.GetTimestamp();
            data.Game.CopyAudioFrame(pcm);
            long mixed = Stopwatch.GetTimestamp();
            // Exclude asset/driver/JIT warmup from the ten-second measured slice.
            if (frame <= 120) continue;
            stepMs += Stopwatch.GetElapsedTime(start, stepped).TotalMilliseconds;
            renderMs += Stopwatch.GetElapsedTime(stepped, rendered).TotalMilliseconds;
            audioMs += Stopwatch.GetElapsedTime(rendered, mixed).TotalMilliseconds;
        }
        if (data.Game.GameState != SuperMetroidGameState.MainGameplay)
            throw new InvalidDataException($"Performance fixture unexpectedly left gameplay: {data.Game.GameState}");
        PngWriterTooling.WriteRgba(Path.Combine(root, "room.png"), new Extent16<ScreenPixels>(FrontendFrame.Width, FrontendFrame.Height), pixels);
        Console.WriteLine(data.SaveSlot(9));
        Console.WriteLine($"{scene}: room ${(byte?)data.Game.GameplayActiveAreaIndex:X2}/${data.Game.GameplayActiveRoomIndex:X2}; " +
            $"600 measured Windows frames: step (including audio synthesis)={stepMs / 600:F3}ms render={renderMs / 600:F3}ms audio copy={audioMs / 600:F3}ms.");
        if (lastSnapshot?.Layers is { } layers)
        {
            foreach (RenderLayer layer in layers.Layers)
            {
                var isolated = new LayeredRenderSnapshot(layers.Memory, [layer], layers.ObjectSelection, layers.Brightness);
                // These isolated costs include snapshot-memory setup and backdrop
                // creation, so they locate expensive operations but are not additive.
                for (int i = 0; i < 10; i++) SoftwareLayeredSnapshotRenderer.Render(isolated);
                long start = Stopwatch.GetTimestamp();
                for (int i = 0; i < 100; i++) SoftwareLayeredSnapshotRenderer.Render(isolated);
                Console.WriteLine($"  Isolated {layer.GetType().Name}: {Stopwatch.GetElapsedTime(start).TotalMilliseconds / 100:F3}ms");
            }
        }
        return 0;
    }
}
