using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;

internal static class SpinJumpMissingAudioAudit
{
    public static int Run()
    {
        var loaded = DebuggerFixtureLoader.Load("spin-jump-missing-audio", 0);
        var game = loaded.Game;
        var runtime = game.RuntimeForVerification ?? throw new InvalidDataException("Missing runtime in spin audio fixture.");
        var audio = new CartridgeAudioRenderer(ExtractedAudioAssetCatalog.Load("standalone-assets/audio"), loaded.AudioPlayer!);
        Console.WriteLine($"Preserved spin audio room={runtime.ActiveRoom?.Identity}, Samus={runtime.Samus!.XPosition}/{runtime.Samus.YPosition}, pose={runtime.Samus.Pose:X2}");
        bool heardScrewAttackStart = false;
        for (int frame = 0; frame < 180; frame++)
        {
            ushort input = frame is >= 30 and < 33 ? (ushort)SnesButton.Left :
                frame is >= 33 and < 60 ? (ushort)((ushort)SnesButton.Left | runtime.ControllerBindings.Jump) :
                frame is >= 60 and < 90 ? (ushort)SnesButton.Up : (ushort)0;
            game.SetAudioAcknowledgements(audio.ReadAcknowledgements());
            var next = game.StepCaptured(input, frame + 1, 1);
            audio.RenderFrame(next.Frame.AudioCommands);
            // $91:F624 queues library-one $33 on initial Screw Attack entry. Observe
            // the real frontend/APU handoff, not merely that the airborne pose changed.
            heardScrewAttackStart |= next.Frame.AudioCommands.Contains(CartridgeAudioCommand.WritePort(1, 0x33));
            if (frame % 15 == 0 || next.Frame.AudioCommands.Count != 0)
                Console.WriteLine($"frame={frame} room={runtime.ActiveRoom?.Identity} pose={runtime.Samus.Pose:X2} position={runtime.Samus.XPosition}/{runtime.Samus.YPosition} audio={string.Join(',', next.Frame.AudioCommands)}");
            if (frame == 0 && next.Snapshot is { } snapshot)
            {
                var pixels = new Rgba32[snapshot.Width * snapshot.Height];
                SoftwareFrameSnapshotRenderer.Render(snapshot, pixels);
                Directory.CreateDirectory("csharp/test-temp/issue-480-spin");
                PngWriter.WriteRgba("csharp/test-temp/issue-480-spin/start.png", snapshot.Width, snapshot.Height, pixels);
            }
        }
        if (!heardScrewAttackStart)
            throw new InvalidDataException("The saved-room Screw Attack jump never published its native start sound to APU port one.");
        return 0;
    }
}
