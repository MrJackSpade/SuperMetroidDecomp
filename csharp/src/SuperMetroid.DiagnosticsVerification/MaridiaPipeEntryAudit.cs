using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using System.Reflection;

/// <summary>Read-only replay of the preserved #391 state; never touches player slots.</summary>
internal static class MaridiaPipeEntryAudit
{
    public static int Run()
    {
        var loaded = DebuggerFixtureLoader.Load("maridia-041b-pipe-entry", 0);
        var game = loaded.Game;
        var audio = new CartridgeAudioRenderer(ExtractedAudioAssetCatalog.Load("standalone-assets/audio"), loaded.AudioPlayer!);
        const string output = "csharp/test-temp/issue-391-pipe";
        Directory.CreateDirectory(output);
        var pixels = new Rgba32[FrontendFrame.Width * FrontendFrame.Height];
        for (int frame = 0; frame < 240; frame++)
        {
            var runtime = (SuperMetroidRuntime)typeof(SuperMetroidGame)
                .GetField("runtime", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(game)!;
            if (frame % 30 == 0)
                Console.WriteLine($"frame={frame} state={game.GameState} room={runtime.ActiveRoom?.Identity} Samus={runtime.Samus!.XPosition}/{runtime.Samus.YPosition} camera={runtime.Camera!.XPosition}/{runtime.Camera.YPosition} pose={runtime.Samus.Pose:X2}");
            game.SetAudioAcknowledgements(audio.ReadAcknowledgements());
            // Leave the small central ledge, then let gravity carry the preserved
            // player state through the shaft without inventing position writes.
            ushort input = frame < 30 ? (ushort)SuperMetroid.Core.Input.SnesButton.Right : (ushort)0;
            var result = game.StepCaptured(input, frame + 1, 1);
            audio.RenderFrame(result.Frame.AudioCommands);
            if (frame % 30 == 0 && result.Snapshot is { } snapshot)
            {
                SoftwareFrameSnapshotRenderer.Render(snapshot, pixels);
                PngWriter.WriteRgba($"{output}/frame-{frame:D3}.png", snapshot.Width, snapshot.Height, pixels);
            }
        }
        Console.WriteLine("Captured preserved pipe-entry sequence; image inspection and cartridge comparison are required before claiming a fix.");
        return 0;
    }
}
