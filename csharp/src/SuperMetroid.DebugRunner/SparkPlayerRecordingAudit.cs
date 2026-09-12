using System.Security.Cryptography;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>
/// Replays #564's production recording unchanged, then tests the aim-up chord
/// at each recorded windup. Counterfactual input is explicitly not hardware evidence.
/// </summary>
internal static class SparkPlayerRecordingAudit
{
    public static int Run(string recordingPath, string romPath)
    {
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(recordingPath))) !=
            "A33F0651FE74F5519EE4507674BF75D5FA3975E4B7AAD4DD5DA1F0650AB685D9")
            throw new InvalidDataException("Use #564's preserved production 0.2.1 recording.");
        var recording = ControllerInputRecording.Read(recordingPath);
        if (!SHA256.HashData(File.ReadAllBytes(romPath)).AsSpan().SequenceEqual(recording.RomSha256))
            throw new InvalidDataException("Recording and replay ROM differ.");
        foreach (int substitutionStart in new[] { -1, 1688, 2156, 3013, 3722, 4081, 4645 })
        {
            var bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
            recording.InitialSaveRam.CopyTo(bus.SaveRam);
            var game = new SuperMetroidGame(bus, recording.GameOptions, renderGameplayFrames: false);
            var ports = new byte[4];
            var launches = new List<(int Frame, ShinesparkPhase Phase)>();
            bool launched = false, completed = false, movedBothAxes = false;
            bool launchLeft = false;
            for (int frame = 0; frame < recording.ControllerInputs.Length; frame++)
            {
                var runtime = game.RuntimeForVerification;
                var before = runtime?.Samus;
                var phase = before?.Shinespark.Phase ?? ShinesparkPhase.Inactive;
                int x = before?.XPosition ?? 0, y = before?.YPosition ?? 0;
                ushort input = recording.ControllerInputs[frame];
                if (substitutionStart >= 0 && frame >= substitutionStart)
                    input = launched ? (ushort)0 : (ushort)(runtime!.ControllerBindings.Jump | runtime.ControllerBindings.AimUp);
                var result = game.Step(input);
                foreach (var command in result.AudioCommands)
                    if (command.Kind == CartridgeAudioCommandKind.WritePort) ports[command.Port] = command.Value;
                game.SetAudioAcknowledgements(new(ports[0], ports[1], ports[2], ports[3]));
                runtime = game.RuntimeForVerification;
                var samus = runtime?.Samus;
                if (samus is null) continue;
                var next = samus.Shinespark.Phase;
                if (phase == ShinesparkPhase.Windup && next is ShinesparkPhase.Horizontal or ShinesparkPhase.Vertical or ShinesparkPhase.Diagonal)
                {
                    launches.Add((frame, next));
                    Console.WriteLine($"substitution={substitutionStart}, frame={frame}, phase={next}, input={input:X4}, jump={runtime!.ControllerBindings.Jump:X4}, aimUp={runtime.ControllerBindings.AimUp:X4}, xy={samus.XPosition}/{samus.YPosition}");
                    if (substitutionStart >= 0 && frame >= substitutionStart)
                    {
                        if (next != ShinesparkPhase.Diagonal)
                            throw new InvalidDataException("Recorded setup did not accept the aim-up chord as diagonal.");
                        launched = true;
                        launchLeft = samus.Pose == SamusPoseIds.ShinesparkDiagonalLeftPose;
                    }
                }
                if (launched && samus.XPosition != x && samus.YPosition < y)
                {
                    if ((samus.XPosition < x) != launchLeft)
                        throw new InvalidDataException("Diagonal travel moved against its launch facing.");
                    movedBothAxes = true;
                }
                if (launched && next == ShinesparkPhase.Inactive)
                {
                    completed = true;
                    Console.WriteLine($"substitution={substitutionStart}, completed={frame}, movedBothAxes={movedBothAxes}");
                    break;
                }
                if (substitutionStart >= 0 && frame > substitutionStart + 300)
                    throw new InvalidDataException("Substituted launch did not complete within 300 frames.");
            }
            if (substitutionStart < 0)
            {
                (int, ShinesparkPhase)[] expected = [(1694, ShinesparkPhase.Horizontal),
                    (2162, ShinesparkPhase.Vertical), (3017, ShinesparkPhase.Vertical),
                    (3722, ShinesparkPhase.Vertical), (4081, ShinesparkPhase.Vertical), (4645, ShinesparkPhase.Vertical)];
                if (!launches.SequenceEqual(expected))
                    throw new InvalidDataException("The unchanged recording no longer reproduces its six launch selections.");
            }
            else if (!launched || !completed || !movedBothAxes)
                throw new InvalidDataException("Counterfactual did not launch, travel diagonally, and finish.");
        }
        Console.WriteLine("Six original launches and six explicitly substituted diagonal controls passed.");
        return 0;
    }
}
