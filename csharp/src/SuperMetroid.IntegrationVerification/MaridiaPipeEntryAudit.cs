using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;

/// <summary>Read-only replay of the preserved #391 state; never touches player slots.</summary>
internal static class MaridiaPipeEntryAudit
{
    public static int Run(bool fromNorth = false, bool freshOrigin = false, bool incomingDoor = false)
    {
        var loaded = DebuggerFixtureLoader.Load("maridia-041b-pipe-entry", 0);
        var game = loaded.Game;
        var audio = new CartridgeAudioRenderer(ExtractedAudioAssetCatalog.Load("standalone-assets/audio"), loaded.AudioPlayer!);
        string output = "csharp/test-temp/issue-391-pipe" + (fromNorth ? "-north" : "") + (freshOrigin ? "-fresh-origin" : "") + (incomingDoor ? "-door" : "");
        var runtime = game.RuntimeForVerification ?? throw new InvalidDataException("Pipe fixture lacks runtime.");
        if (fromNorth)
        {
            if (freshOrigin)
            {
                // Diagnostic control only: a fresh room setup must not inherit an
                // unrelated saved room's unfinished circular tilemap coordinate origin.
                runtime.BackgroundScroll.Bg1XOffset = runtime.BackgroundScroll.Bg1YOffset = 0;
                runtime.BackgroundScroll.Bg2XOffset = runtime.BackgroundScroll.Bg2YOffset = 0;
            }
            // Stage the preceding tube through its real incoming setup callback,
            // then let the frontend own the single tube -> Oasis room boundary.
            var door = SuperMetroid.Core.Rooms.CartridgeDoorHeader.Load(loaded.AddressSpace,
                SuperMetroid.Core.Rooms.DoorPointers.MaridiaElevatubeFromNorth);
            if (incomingDoor)
                MaridiaPipeIncomingDoorSeed.Apply(runtime, loaded.AddressSpace, door);
            else
            {
                runtime.LoadCartridgeRoomThroughDoorForVerification(door);
                runtime.Samus!.XPosition = 128;
                runtime.Samus.YPosition = 64;
            }
        }
        Directory.CreateDirectory(output);
        var pixels = new Rgba32[FrontendFrame.Width * FrontendFrame.Height];
        for (int frame = 0; frame < (incomingDoor ? 620 : 420); frame++)
        {
            if (frame % 30 == 0)
                Console.WriteLine($"frame={frame} state={game.GameState} room={runtime.ActiveRoom?.Identity} header={runtime.ActiveRoom?.Pointer:X4} main={runtime.ActiveRoom?.State.MainCodePointer:X4} Samus={runtime.Samus!.XPosition}/{runtime.Samus.YPosition} camera={runtime.Camera!.XPosition}/{runtime.Camera.YPosition} pose={runtime.Samus.Pose:X2}");
            if (fromNorth && !incomingDoor && frame == 120 && runtime.Camera!.YPosition < 800)
                throw new InvalidDataException("#391: elevatube carried Samus past Y=1000 while camera failed to follow the post-scroll room-main displacement.");
            game.SetAudioAcknowledgements(audio.ReadAcknowledgements());
            // Leave the small central ledge, then let gravity carry the preserved
            // player state through the shaft without inventing position writes.
            ushort input = fromNorth ? (ushort)0 : frame < 30 ? (ushort)SuperMetroid.Core.Input.SnesButton.Right :
                frame is >= 100 and < 123 ? (ushort)SuperMetroid.Core.Input.SnesButton.Left : (ushort)0;
            var result = game.StepCaptured(input, frame + 1, 1);
            audio.RenderFrame(result.Frame.AudioCommands);
            if ((fromNorth || frame % 30 == 0) && result.Snapshot is { } snapshot)
            {
                if (fromNorth) MaridiaPipeTerrainAudit.Observe(runtime, snapshot, frame);
                SoftwareFrameSnapshotRenderer.Render(snapshot, pixels);
                PngWriter.WriteRgba($"{output}/frame-{frame:D3}.png", snapshot.Width, snapshot.Height, pixels);
            }
        }
        Console.WriteLine("Captured preserved pipe-entry sequence; image inspection and cartridge comparison are required before claiming a fix.");
        return 0;
    }
}
