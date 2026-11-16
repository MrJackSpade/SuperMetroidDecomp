using SuperMetroid.Core.Frontend;
using SuperMetroid.Diagnostics.Configuration;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Diagnostics.Recording;
using SuperMetroid.Desktop.Window;
namespace SuperMetroid.DesktopVerification.Startup;
using SuperMetroid.Core;

/// <summary>
/// Exercises the real Windows host with a complete extracted installation but
/// no installed ROM. Only a test-owned copy is moved; the source file is intact.
/// </summary>
internal static class RomlessDesktopStartupTest
{
    /// <summary>Copies the repository installation into a temporary directory, removes that copy's
    /// ROM, and verifies host startup, frame advance, debugger state, and replay
    /// recover the same pixels without cartridge data.</summary>
    [PrivateInstallation("Checks that OpenOrRepair accepts a complete extracted installation whose ROM has been removed, as the Windows host's startup does.")]
    public static void Run()
    {
        string tempRoot = Path.GetFullPath(Path.Combine("csharp", "test-temp"));
        string root = Path.GetFullPath(Path.Combine(tempRoot,
            "romless-desktop-" + Guid.NewGuid().ToString("N")));
        if (!root.StartsWith(tempRoot + Path.DirectorySeparatorChar,
                StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("ROM-less desktop test escaped test-temp.");
        try
        {
            PrivateInstallationCopy.CopyInstallation(RepositoryInstallation.Installation.Root, root);
            var installation = new GameInstallation(root);
            File.Move(installation.RomPath, Path.Combine(root, "held-rom.smc"));
            if (GameAssetInstaller.OpenOrRepair(root) is null)
                throw new InvalidOperationException(
                    "Windows startup did not accept valid extracted content without a ROM.");
            using var host = new PlayableGameControl(installation,
                new HostOptions
                {
                    AudioEnabled = false,
                });
            var game = host.game ?? throw new InvalidOperationException("Host has no game.");
            long startFrame = game.FrameNumber;
            for (int frame = 0; frame < 6; frame++)
                host.StepFrame(0);
            if (game.FrameNumber != startFrame + 6)
                throw new InvalidOperationException(
                    "Windows host did not advance six title frames without its ROM copy.");

            long savedFrame = game.FrameNumber;
            var savedPixels = game.PublishedPixels();
            host.SaveDebuggerState(0);
            if (!File.Exists(Path.Combine(root, "debug-states",
                    "SuperMetroid-debug-slot-0.smstate")))
                throw new InvalidOperationException(
                    "Windows host did not save a debugger state without its ROM copy.");
            host.StepFrame(0);
            host.LoadDebuggerState(0).GetAwaiter().GetResult();
            game = host.game ?? throw new InvalidOperationException("Restored host has no game.");
            if (game.FrameNumber != savedFrame)
                throw new InvalidOperationException(
                    "Windows debugger state did not restore the zero-ROM frame exactly.");
            host.StepFrame(0);
            if (game.FrameNumber != savedFrame + 1)
                throw new InvalidOperationException(
                    "Windows host did not resume frames after zero-ROM state loading.");

            // Disposal flushes the reset-time input journal. Replay must recover
            // the same stock pixels from its SRAM seed and exact input words,
            // without falling back to the intentionally absent ROM file.
            host.Dispose();
            string[] recordings = Directory.GetFiles(
                Path.Combine(root, "input-recordings"), "*.smrec");
            ControllerInputRecording recordedReset = recordings
                .Select(ControllerInputRecording.Read)
                .OrderByDescending(recording => recording.ControllerInputs.Length)
                .FirstOrDefault() ?? throw new InvalidOperationException(
                    "Windows host did not flush a replayable input recording.");
            if (recordedReset.ControllerInputs.Length < 7)
                throw new InvalidOperationException(
                    "Windows host recording omitted the reset or six title frames.");
            using var replayHost = new PlayableGameControl(installation,
                new HostOptions
                {
                    AudioEnabled = false,
                },
                driver: new RecordingFrameDriver(recordedReset));
            for (int frame = 0; frame < 6; frame++)
                replayHost.StepFrame(null);
            var replayGame = replayHost.game ?? throw new InvalidOperationException(
                    "Replay host has no game.");
            if (replayGame.FrameNumber != savedFrame ||
                !replayGame.PublishedPixels().SequenceEqual(savedPixels))
                throw new InvalidOperationException(
                    "Zero-ROM Windows input replay diverged from the captured frame.");
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
        Console.WriteLine(
            "PASS Windows host boots extracted content without a ROM copy, saves/loads state, and replays exact pixels.");
    }
}
