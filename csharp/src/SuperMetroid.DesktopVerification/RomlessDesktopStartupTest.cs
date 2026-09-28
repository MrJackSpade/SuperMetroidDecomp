using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Desktop;

/// <summary>
/// Exercises the real Windows host with a complete extracted installation but
/// no installed ROM. Only a test-owned copy is moved; the source file is intact.
/// </summary>
internal static class RomlessDesktopStartupTest
{
    public static void Run(string sourceRom)
    {
        string tempRoot = Path.GetFullPath(Path.Combine("csharp", "test-temp"));
        string root = Path.GetFullPath(Path.Combine(tempRoot,
            "romless-desktop-" + Guid.NewGuid().ToString("N")));
        if (!root.StartsWith(tempRoot + Path.DirectorySeparatorChar,
                StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("ROM-less desktop test escaped test-temp.");
        try
        {
            GameInstallation installation = GameAssetInstaller.Install(sourceRom, root);
            File.Move(installation.RomPath, Path.Combine(root, "held-rom.smc"));
            if (GameAssetInstaller.OpenOrRepair(root) is null)
                throw new InvalidOperationException(
                    "Windows startup did not accept valid extracted content without a ROM.");
            using var host = new PlayableGameControl(installation.RomPath,
                new SuperMetroidGameOptions
                {
                    AudioEnabled = false,
                    Renderer = RendererSelection.Software,
                },
                dataDirectory: root);
            var memory = (SuperMetroidAddressSpace)(typeof(PlayableGameControl)
                .GetField("addressSpace", BindingFlags.Instance | BindingFlags.NonPublic)!
                .GetValue(host) ?? throw new InvalidOperationException("Host has no address space."));
            if (!memory.Rom.IsEmpty)
                throw new InvalidOperationException(
                    "Windows host retained cartridge bytes after ROM-less startup.");
            var game = (SuperMetroidGame)(typeof(PlayableGameControl)
                .GetField("game", BindingFlags.Instance | BindingFlags.NonPublic)!
                .GetValue(host) ?? throw new InvalidOperationException("Host has no game."));
            long startFrame = game.FrameNumber;
            MethodInfo step = typeof(PlayableGameControl).GetMethod("StepFrame",
                BindingFlags.Instance | BindingFlags.NonPublic)
                ?? throw new InvalidOperationException("Windows host has no frame step.");
            for (int frame = 0; frame < 6; frame++)
                step.Invoke(host, [(ushort?)0]);
            if (game.FrameNumber != startFrame + 6)
                throw new InvalidOperationException(
                    "Windows host did not advance six title frames without its ROM copy.");

            MethodInfo saveState = typeof(PlayableGameControl).GetMethod(
                "SaveDebuggerState", BindingFlags.Instance | BindingFlags.NonPublic)
                ?? throw new InvalidOperationException("Windows host has no debugger state save.");
            MethodInfo loadState = typeof(PlayableGameControl).GetMethod(
                "LoadDebuggerState", BindingFlags.Instance | BindingFlags.NonPublic)
                ?? throw new InvalidOperationException("Windows host has no debugger state load.");
            long savedFrame = game.FrameNumber;
            var savedPixels = game.CurrentFrame.Pixels.ToArray();
            saveState.Invoke(host, [0]);
            if (!File.Exists(Path.Combine(root, "debug-states",
                    "SuperMetroid-debug-slot-0.smstate")))
                throw new InvalidOperationException(
                    "Windows host did not save a debugger state without its ROM copy.");
            step.Invoke(host, [(ushort?)0]);
            ((Task)(loadState.Invoke(host, [0]) ?? throw new InvalidOperationException(
                "Windows host did not start debugger state loading.")))
                .GetAwaiter().GetResult();
            memory = (SuperMetroidAddressSpace)(typeof(PlayableGameControl)
                .GetField("addressSpace", BindingFlags.Instance | BindingFlags.NonPublic)!
                .GetValue(host) ?? throw new InvalidOperationException("Restored host has no address space."));
            game = (SuperMetroidGame)(typeof(PlayableGameControl)
                .GetField("game", BindingFlags.Instance | BindingFlags.NonPublic)!
                .GetValue(host) ?? throw new InvalidOperationException("Restored host has no game."));
            if (!memory.Rom.IsEmpty || game.FrameNumber != savedFrame)
                throw new InvalidOperationException(
                    "Windows debugger state did not restore the zero-ROM frame exactly.");
            step.Invoke(host, [(ushort?)0]);
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
            using var replayHost = new PlayableGameControl(installation.RomPath,
                new SuperMetroidGameOptions
                {
                    AudioEnabled = false,
                    Renderer = RendererSelection.Software,
                },
                replay: recordedReset,
                dataDirectory: root);
            for (int frame = 0; frame < 6; frame++)
                step.Invoke(replayHost, [(ushort?)null]);
            var replayMemory = (SuperMetroidAddressSpace)(typeof(PlayableGameControl)
                .GetField("addressSpace", BindingFlags.Instance | BindingFlags.NonPublic)!
                .GetValue(replayHost) ?? throw new InvalidOperationException(
                    "Replay host has no address space."));
            var replayGame = (SuperMetroidGame)(typeof(PlayableGameControl)
                .GetField("game", BindingFlags.Instance | BindingFlags.NonPublic)!
                .GetValue(replayHost) ?? throw new InvalidOperationException(
                    "Replay host has no game."));
            if (!replayMemory.Rom.IsEmpty || replayGame.FrameNumber != savedFrame ||
                !replayGame.CurrentFrame.Pixels.AsSpan().SequenceEqual(savedPixels))
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
