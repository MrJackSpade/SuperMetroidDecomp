using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Input;
using System.Security.Cryptography;

/// <summary>Replays a player session through the frontend, checking the exact #489 trajectories.</summary>
internal static class MotherBrainRecordingAudit
{
    public static int Run(string path, string romPath, bool verifyBeam = false, bool verifyDeath = false)
    {
        var recording = ControllerInputRecording.Read(path);
        if (!SHA256.HashData(File.ReadAllBytes(romPath)).AsSpan().SequenceEqual(recording.RomSha256))
            throw new InvalidDataException("The replay ROM does not match the recorded revision.");
        var bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        recording.InitialSaveRam.CopyTo(bus.SaveRam);
        var game = new SuperMetroidGame(bus, recording.GameOptions, renderGameplayFrames: false);
        var ports = new byte[4];
        int minStandingY = int.MaxValue, maxStandingY = 0, maxBattleCamera = 0, battleFrames = 0;
        var samusRainbowPalettes = new HashSet<string>();
        var beamColors = new HashSet<ushort>();
        int beamSamples = 0;
        var deathChecks = new MotherBrainDeathRecordingChecks();
        // This focused attack slice intentionally stops before the separately tracked
        // death dispatcher. The default audit still executes the complete recording.
        int frameCount = verifyBeam ? Math.Min(13000, recording.ControllerInputs.Length) : recording.ControllerInputs.Length;
        for (int frame = 0; frame < frameCount; frame++)
        {
            var result = game.Step(recording.ControllerInputs[frame]);
            foreach (var command in result.AudioCommands)
                if (command.Kind == CartridgeAudioCommandKind.WritePort) ports[command.Port] = command.Value;
            game.SetAudioAcknowledgements(new(ports[0], ports[1], ports[2], ports[3]));
            var runtime = game.RuntimeForVerification;
            if (verifyDeath && runtime is not null) deathChecks.Observe(runtime, frame);
            if (verifyBeam && runtime?.Enemies.MotherBrain?.RainbowBeamHdma is { Active: true } beam && frame % 30 == 0)
            {
                var snapshot = GameplayDisplayCapture.TryCaptureFrame(runtime)!;
                var pixels = SoftwareLayeredSnapshotRenderer.Render(snapshot);
                var withoutBeam = new LayeredRenderSnapshot(snapshot.Memory,
                    snapshot.Layers.ToArray().Where(layer => layer is not ScanlineColorAddRenderLayer).ToArray(),
                    snapshot.ObjectSelection, snapshot.Brightness);
                var baseline = SoftwareLayeredSnapshotRenderer.Render(withoutBeam);
                int changed = 0;
                for (int index = 0; index < pixels.Length; index++)
                {
                    if (pixels[index] == baseline[index]) continue;
                    int y = index / 256, x = index % 256;
                    ushort window = beam.Windows[y];
                    if (y < 32 || x < (window & 255) || x > (window >> 8))
                        throw new InvalidDataException("Rainbow beam changed a pixel outside its native color window.");
                    changed++;
                }
                if (changed == 0)
                    throw new InvalidDataException($"Active rainbow beam is invisible at recorded frame {frame}.");
                beamSamples++;
                beamColors.Add(beam.Color);
                if (frame == 6840)
                {
                    Directory.CreateDirectory("csharp/test-temp/mother-brain-recording");
                    var packet = new RenderFrameSnapshot(new(frame, 1, runtime.NmiFrameCounter), snapshot);
                    File.WriteAllBytes("csharp/test-temp/mother-brain-recording/beam-6840.smframe",
                        RenderFrameSnapshotCodec.Serialize(packet));
                }
            }
            if (runtime?.Samus?.Drained.RainbowPaletteEnabled == true)
                samusRainbowPalettes.Add(string.Join(',', runtime.Cgram.Colors.Slice(192, 16).ToArray()));
            if (runtime?.Samus?.HyperBeam != 0 && samusRainbowPalettes.Count != 0 && frame % 120 == 0)
                Console.WriteLine($"Samus acquisition rainbow: {samusRainbowPalettes.Count} distinct live palettes.");
            if (runtime is not null && frame is 6840 or 8100 or 11880 or 12000 or 12120)
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
            {
                Console.WriteLine($"{frame}: {brain.Function} form={brain.Form} hp={brain.Head!.Health}/{brain.RainbowBeamSequence?.BrainHealth} baby={brain.BabyMetroid?.Phase} pose={brain.Pose} xy={brain.Body.XPosition},{brain.Body.YPosition} list={brain.Body.CurrentInstruction:X4} camera={runtime.Camera!.XPosition},{runtime.Camera.YPosition} samus={runtime.Samus!.XPosition},{runtime.Samus.YPosition}");
                if (brain.Form == 4)
                    Console.WriteLine($"Hyper={runtime.Samus.HyperBeam} input={recording.ControllerInputs[frame]:X4} count={runtime.Projectiles.ProjectileCounter} cooldown={runtime.BombProjectiles.CooldownTimer} head={brain.Head.XPosition},{brain.Head.YPosition} props={brain.Head.Properties} inv={brain.Head.InvincibilityTimer} shots={string.Join(';', runtime.Projectiles.Slots.Where(p => p.IsActive).Select(p => $"{p.SlotIndex}:{p.Type:X4}/{p.Damage}@{p.XPosition},{p.YPosition}"))}");
            }
        }
        Console.WriteLine($"Battle frames={battleFrames}, standing Y={minStandingY}..{maxStandingY}, maximum camera X={maxBattleCamera}.");
        if (verifyBeam)
        {
            if (beamSamples < 10 || beamColors.Count < 5)
                throw new InvalidDataException($"Insufficient visible rainbow coverage: {beamSamples} frames, {beamColors.Count} colors.");
            Console.WriteLine($"Visible rainbow beam: {beamSamples} sampled frames, {beamColors.Count} native colors; all changes inside native windows.");
        }
        else if (verifyDeath)
        {
            var runtime = game.RuntimeForVerification!;
            deathChecks.VerifyReadyToLeave(runtime);
            ushort room = runtime.ActiveRoom!.Pointer;
            bool leftRoom = false;
            for (int frame = 0; frame < 300; frame++)
            {
                // This tail only exercises the opened boss-room doorway: walk to its
                // ledge, then jump left. Stop at the first completed room transition.
                ushort input = (ushort)((ushort)SnesButton.Left | (frame >= 100 ? runtime.ControllerBindings.Jump : 0));
                var result = game.Step(input);
                foreach (var command in result.AudioCommands)
                    if (command.Kind == CartridgeAudioCommandKind.WritePort) ports[command.Port] = command.Value;
                game.SetAudioAcknowledgements(new(ports[0], ports[1], ports[2], ports[3]));
                if (game.RuntimeForVerification?.ActiveRoom?.Pointer != room)
                {
                    if (game.RuntimeForVerification?.EscapeTimer.IsActive != true)
                        throw new InvalidDataException("Escape timer did not survive leaving Mother Brain room.");
                    Console.WriteLine($"Samus left the opened boss room after {frame + 1} tail frames; escape timer remains active.");
                    leftRoom = true;
                    break;
                }
            }
            if (!leftRoom) throw new InvalidDataException($"Samus could not leave the opened escape door; position {runtime.Samus!.XPosition},{runtime.Samus.YPosition}.");
        }
        if (battleFrames > 0 && (minStandingY != 150 || maxStandingY != 150 || maxBattleCamera != 0))
            throw new InvalidDataException("Recorded Mother Brain fight drifted or failed to lock its camera.");
        return 0;
    }
}
