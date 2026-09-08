using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using System.Security.Cryptography;

/// <summary>Replays a player session through the frontend, checking the exact #489 trajectories.</summary>
internal static class MotherBrainRecordingAudit
{
    public static int Run(string path, string romPath)
    {
        var recording = ControllerInputRecording.Read(path);
        if (!SHA256.HashData(File.ReadAllBytes(romPath)).AsSpan().SequenceEqual(recording.RomSha256))
            throw new InvalidDataException("The replay ROM does not match the recorded revision.");
        var bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        recording.InitialSaveRam.CopyTo(bus.SaveRam);
        var game = new SuperMetroidGame(bus, recording.GameOptions, renderGameplayFrames: false);
        var ports = new byte[4];
        int minStandingY = int.MaxValue, maxStandingY = 0, maxBattleCamera = 0, battleFrames = 0;
        for (int frame = 0; frame < recording.ControllerInputs.Length; frame++)
        {
            var result = game.Step(recording.ControllerInputs[frame]);
            foreach (var command in result.AudioCommands)
                if (command.Kind == CartridgeAudioCommandKind.WritePort) ports[command.Port] = command.Value;
            game.SetAudioAcknowledgements(new(ports[0], ports[1], ports[2], ports[3]));
            var runtime = game.RuntimeForVerification;
            if (runtime is not null && frame is 1228 or 1300 or 1400 or 1500)
            {
                string directory = "csharp/test-temp/mother-brain-recording";
                Directory.CreateDirectory(directory);
                PngWriter.WriteRgba(Path.Combine(directory, $"{frame}.png"), 256, 224,
                    SuperMetroidRuntimeFrameRenderer.Render(runtime));
                Console.WriteLine($"Transition {frame}: room={runtime.ActiveRoom?.Pointer:X4} camera={runtime.Camera!.XPosition},{runtime.Camera.YPosition}");
            }
            if (runtime?.Enemies.MotherBrain is { HasBg2ScrollOverride: true } active && active.Body.YPosition < 200)
            {
                battleFrames++;
                maxBattleCamera = Math.Max(maxBattleCamera, runtime.Camera!.XPosition);
                if (active.Pose == SuperMetroid.Core.Game.MotherBrainBodyPose.Standing)
                {
                    minStandingY = Math.Min(minStandingY, active.Body.YPosition);
                    maxStandingY = Math.Max(maxStandingY, active.Body.YPosition);
                }
            }
            if (frame % 120 == 0 && runtime?.Enemies.MotherBrain is { } brain)
                Console.WriteLine($"{frame}: {brain.Function} pose={brain.Pose} xy={brain.Body.XPosition},{brain.Body.YPosition} list={brain.Body.CurrentInstruction:X4} camera={runtime.Camera!.XPosition},{runtime.Camera.YPosition} samus={runtime.Samus!.XPosition},{runtime.Samus.YPosition}");
        }
        Console.WriteLine($"Battle frames={battleFrames}, standing Y={minStandingY}..{maxStandingY}, maximum camera X={maxBattleCamera}.");
        if (battleFrames > 0 && (minStandingY != 150 || maxStandingY != 150 || maxBattleCamera != 0))
            throw new InvalidDataException("Recorded Mother Brain fight drifted or failed to lock its camera.");
        return 0;
    }
}
