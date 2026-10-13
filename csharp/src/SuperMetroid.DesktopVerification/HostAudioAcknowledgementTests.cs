using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Desktop;
using static DesktopVerificationSupport;

/// <summary>
/// The desktop host with audio disabled advances the SFX handshake exactly as a plain Core
/// session does (#1303): no host audio call feeds or withholds acknowledgements.
/// </summary>
internal static class HostAudioAcknowledgementTests
{
    private const int FileSelectCursorMoves = 6;

    internal static async Task Verify()
    {
        using var directoryScratch = new TestScratchDirectory("desktop-audio-acknowledgements");
        string directory = directoryScratch.Root;
        Directory.CreateDirectory(directory);
        string rom = Path.Combine(directory, "Super Metroid.smc");
        File.Copy(Path.GetFullPath("Super Metroid.smc"), rom);
        var options = new SuperMetroidGameOptions { AudioEnabled = false, Renderer = RendererSelection.Software };
        using var form = new Form { ClientSize = new(900, 760), ShowInTaskbar = false };
        using var control = DesktopAccess.CreateGameControl(rom, options);
        form.Controls.Add(control);
        _ = form.Handle;
        _ = control.Handle;
        control.SetPlaying(false);
        int cursorRequests = 0;
        try
        {
            await control.InitializeRendererAsync();
            Check(control.audioDevice is null, "an audio-disabled host opens no device");
            // The host has run its reset frame from the same SRAM; replay that frame in Core.
            var bus = DesktopAccess.InstallationFor(rom).OpenRuntimeAddressSpace();
            control.addressSpace.SaveRam.CopyTo(bus.SaveRam);
            SuperMetroidGame reference = InstalledFixtures.CreateGame(bus, options);
            reference.Step(0);
            AssertSameAudio(control.game, reference, "reset frame");

            int moves = 0;
            for (int tick = 0; tick < 2000 && moves < FileSelectCursorMoves; tick++)
            {
                ushort input = 0;
                if (reference.GameState != SuperMetroidGameState.FileSelectMenus)
                    input = tick % 47 == 0 ? (ushort)SnesButton.Start : (ushort)0;
                else if (tick % 20 == 0)
                {
                    input = (ushort)SnesButton.Down;
                    moves++;
                }
                control.StepFrame(input);
                FrontendFrame frame = reference.Step(input);
                cursorRequests += frame.AudioCommands.Count(command =>
                    command.Kind == CartridgeAudioCommandKind.WritePort && command.InputPort == ApuPort.SoundLibrary1 &&
                    command.Value == SoundEffectLibrary1Sounds.MenuCursor.Value);
                AssertSameAudio(control.game, reference, $"update {tick}");
            }
            Check(moves == FileSelectCursorMoves, "the fixture reaches the file-select cursor");
            Check(cursorRequests > 0, "the file-select cursor sends its library-1 sound");
        }
        finally { await control.StopRendererAsync(); }
        Console.WriteLine($"  Audio-disabled desktop host: SFX queues and acknowledgements match Core across {cursorRequests} cursor sound writes.");
    }

    private static void AssertSameAudio(SuperMetroidGame host, SuperMetroidGame reference, string context)
    {
        Check(host.FrameNumber == reference.FrameNumber && host.GameState == reference.GameState,
            $"host and Core cadence diverged at {context}");
        Check(host.AudioAcknowledgementsForVerification == reference.AudioAcknowledgementsForVerification,
            $"host acknowledgements diverged at {context}");
        Check(host.AudioForVerification.HasQueuedSounds == reference.AudioForVerification.HasQueuedSounds,
            $"host queued-sound state diverged at {context}");
        for (int library = 0; library < 3; library++)
            Check(host.AudioForVerification.SoundQueueForVerification(library) ==
                reference.AudioForVerification.SoundQueueForVerification(library),
                $"host library {library + 1} dispatcher diverged at {context}");
    }
}
