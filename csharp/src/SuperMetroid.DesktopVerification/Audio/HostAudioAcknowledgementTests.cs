using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.DesktopVerification.Harness.DesktopVerificationSupport;
using SuperMetroid.Core.Gameplay;
namespace SuperMetroid.DesktopVerification.Audio;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

/// <summary>
/// The desktop host with audio disabled advances the SFX handshake exactly as a plain Core
/// session does (#1303): no host audio call feeds or withholds acknowledgements.
/// </summary>
internal static class HostAudioAcknowledgementTests
{
    /// <summary>Cursor moves made on the file-select screen, each queuing a menu sound.</summary>
    private const int FileSelectCursorMoves = 6;

    /// <summary>Drives an audio-disabled desktop host and a plain Core session through the same file-select input.</summary>
    internal static async Task Verify()
    {
        using var directoryScratch = new TestScratchDirectory("desktop-audio-acknowledgements");
        string directory = directoryScratch.Root;
        Directory.CreateDirectory(directory);
        string rom = PlaceInstalledRom(directory);
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
            // The host has run its reset frame from the same saves; replay that frame in Core over a copy of them.
            var bus = DesktopAccess.InstallationFor(rom).OpenRuntimeAddressSpace();
            var savesCopy = SuperMetroid.Core.Saves.SaveRamCodec.Decode(SuperMetroid.Core.Saves.SaveRamCodec.Encode(control.game.Saves, InstalledFixtures.Maps), InstalledFixtures.Maps);
            SuperMetroidGame reference = InstalledFixtures.CreateGame(savesCopy, options);
            reference.Step(0);
            AssertSameAudio(control.game, reference, "reset frame");

            int moves = 0;
            for (int tick = 0; tick < 2000 && moves < FileSelectCursorMoves; tick++)
            {
                SnesButton input = 0;
                if (reference.GameState != SuperMetroidGameState.FileSelectMenus)
                    input = tick % 47 == 0 ? SnesButton.Start : SnesButton.None;
                else if (tick % 20 == 0)
                {
                    input = SnesButton.Down;
                    moves++;
                }
                control.StepFrame(input);
                FrontendFrame frame = reference.Step(input);
                cursorRequests += reference.AudioEventsForVerification.Count(audioEvent =>
                    audioEvent is AudioEvent.StartSoundEffect started &&
                    started.SoundEffect == SoundEffectLibrary1Sounds.MenuCursor);
                AssertSameAudio(control.game, reference, $"update {tick}");
            }
            Check(moves == FileSelectCursorMoves, "the fixture reaches the file-select cursor");
            Check(cursorRequests > 0, "the SPC starts the file-select cursor's library-1 sound");
        }
        finally { await control.StopRendererAsync(); }
        Console.WriteLine($"  Audio-disabled desktop host: SFX queues and acknowledgements match Core across {cursorRequests} cursor sound starts.");
    }

    /// <summary>Requires both sessions to hold identical SFX queues and acknowledgements.</summary>
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
