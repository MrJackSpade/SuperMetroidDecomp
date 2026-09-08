using System.Diagnostics;
using System.Reflection;
using SuperMetroid.Android;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>Representative retail-room fixtures for Android performance issues #361-363.</summary>
internal static class RoomPerformanceStateFixture
{
    public static int Export(string scene, string destination)
    {
        if (scene is not ("save" or "map" or "surface"))
            throw new ArgumentException("Expected save, map, or surface.", nameof(scene));
        string root = Path.GetFullPath(destination);
        if (Directory.Exists(root)) throw new IOException($"Fixture directory already exists: {root}");
        Directory.CreateDirectory(root);
        using var data = new AndroidSessionData(root, Path.GetFullPath("Super Metroid.smc"),
            Path.GetFullPath("standalone-assets/audio"));
        const BindingFlags fields = BindingFlags.Instance | BindingFlags.NonPublic;
        typeof(SuperMetroidGame).GetMethod("SetupSelectedGame", fields)!.Invoke(data.Game, null);
        var runtime = (SuperMetroidRuntime)typeof(SuperMetroidGame).GetField("runtime", fields)!.GetValue(data.Game)!;
        bool surface = scene == "surface";
        ushort camera = surface ? (ushort)1024 : (ushort)0;
        if (scene == "map")
        {
            // The actual map-room entrance runs its cartridge setup and door PLM;
            // this does not simulate an entire route merely to benchmark one room.
            var door = CartridgeDoorHeader.Load(data.Bus, RoomPerformanceFixtureDefinitions.CrateriaMapEntranceDoor);
            typeof(SuperMetroidRuntime).GetMethod("LoadCartridgeRoomThroughDoorForVerification", fields)!
                .Invoke(runtime, [door, camera, camera]);
        }
        else
        {
            ushort room = surface ? RoomHeaderPointers.LandingSite : RoomHeaderPointers.CrateriaSaveStation;
            typeof(SuperMetroidRuntime).GetMethod("LoadCartridgeRoomForDebug", fields)!
                .Invoke(runtime, [room, camera, camera]);
        }
        runtime.Samus!.InputLocked = true;
        runtime.Samus.XPosition = (ushort)(camera + (surface ? 128 : 64));
        runtime.Samus.YPosition = (ushort)(camera + 160);
        typeof(SuperMetroidGame).GetProperty(nameof(SuperMetroidGame.GameState))!
            .SetValue(data.Game, SuperMetroidGameState.MainGameplay);
        double stepMs = 0, renderMs = 0, audioMs = 0;
        Rgba32[]? pixels = null;
        for (long frame = 1; frame <= 720; frame++)
        {
            data.Game.SetAudioAcknowledgements(data.Audio.ReadAcknowledgements());
            data.Record(0);
            long start = Stopwatch.GetTimestamp();
            var result = data.Game.StepCaptured(0, frame, data.Generation);
            long stepped = Stopwatch.GetTimestamp();
            pixels = result.Snapshot is { } snapshot ? SoftwareFrameSnapshotRenderer.Render(snapshot) : result.Frame.Pixels;
            long rendered = Stopwatch.GetTimestamp();
            data.Audio.RenderFrame(result.Frame.AudioCommands);
            long mixed = Stopwatch.GetTimestamp();
            // Exclude asset/driver/JIT warmup from the ten-second measured slice.
            if (frame <= 120) continue;
            stepMs += Stopwatch.GetElapsedTime(start, stepped).TotalMilliseconds;
            renderMs += Stopwatch.GetElapsedTime(stepped, rendered).TotalMilliseconds;
            audioMs += Stopwatch.GetElapsedTime(rendered, mixed).TotalMilliseconds;
        }
        if (data.Game.GameState != SuperMetroidGameState.MainGameplay)
            throw new InvalidDataException($"Performance fixture unexpectedly left gameplay: {data.Game.GameState}");
        PngWriter.WriteRgba(Path.Combine(root, "room.png"), FrontendFrame.Width, FrontendFrame.Height, pixels!);
        Console.WriteLine(data.SaveSlot(9));
        Console.WriteLine($"{scene}: room ${(byte?)data.Game.GameplayActiveAreaIndex:X2}/${data.Game.GameplayActiveRoomIndex:X2}; " +
            $"600 measured Windows frames: step={stepMs / 600:F3}ms render={renderMs / 600:F3}ms audio={audioMs / 600:F3}ms.");
        return 0;
    }
}

/// <summary>Cartridge entry identities used only by the bounded performance fixtures.</summary>
internal static class RoomPerformanceFixtureDefinitions
{
    /// <summary>Bank-$83 door $8BDA enters the Crateria map station through its native setup.</summary>
    internal const ushort CrateriaMapEntranceDoor = 0x8bda;
}
