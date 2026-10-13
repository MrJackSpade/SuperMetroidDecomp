using System.Diagnostics;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Desktop;

/// <summary>Verification views of private desktop audio and error-reporting state.</summary>
internal static class DesktopAccess
{
    extension(WaveOutAudioDevice device)
    {
        internal int PreparedBufferCountForVerification
        {
            get
            {
                lock (device.deviceGate)
                    return device.slots.Count(slot => slot.Prepared);
            }
        }

        internal int NativeQueuedBufferCountForVerification
        {
            get
            {
                lock (device.deviceGate)
                    return device.slots.Count(slot => slot.Prepared && !slot.IsDone);
            }
        }

        /// <summary>Waits until the worker completes every accepted PCM frame, within the device's own buffer-return bound.</summary>
        internal void WaitForPendingSubmissions()
        {
            long target = device.enqueuedFrames;
            Stopwatch elapsed = Stopwatch.StartNew();
            while (device.completedFrames < target)
            {
                device.ThrowWorkerFailure();
                if (elapsed.ElapsedMilliseconds >= WaveOutAudioPolicy.BufferReturnTimeoutMilliseconds)
                    throw new TimeoutException(
                        $"waveOut worker completed {device.completedFrames} of " +
                        $"{target} accepted PCM frames within the bounded audit interval.");
                Thread.Sleep(1);
            }
            device.ThrowWorkerFailure();
        }
    }

    extension(GitHubErrorReporter reporter)
    {
        /// <summary>Queues a flush marker behind every pending report and waits for the worker to reach it.</summary>
        internal async Task FlushAsync()
        {
            var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var marker = new GitHubErrorReporter.QueuedError(null, null, null, completion);
            if (!reporter.queue.Writer.TryWrite(marker))
                throw new ObjectDisposedException(nameof(GitHubErrorReporter));
            await completion.Task.ConfigureAwait(false);
        }
    }

    /// <summary>The extracted audio found from the working or executable directory.</summary>
    internal static ExtractedAudioAssetCatalog LoadAudioAssets(string? audioDirectory = null) =>
        ExtractedAudioAssetCatalog.Load(audioDirectory ?? ExtractedAudioAssetLocator.FindAudioDirectory());

    /// <summary>A standalone renderer over that audio, for fixtures that drive the cartridge audio types directly.</summary>
    internal static CartridgeAudioRenderer CreateAudioRenderer(string? audioDirectory = null) =>
        new(LoadAudioAssets(audioDirectory));

    /// <summary>
    /// A game control over the installation beside a ROM path, importing it on first use, or over
    /// an explicit extracted data directory.
    /// </summary>
    internal static PlayableGameControl CreateGameControl(
        string romPath,
        SuperMetroidGameOptions gameOptions,
        ControllerInputRecording? replay = null,
        GitHubErrorReporter? errorReporter = null,
        string? audioDirectory = null,
        string? dataDirectory = null) =>
        new(InstallationFor(romPath, dataDirectory), gameOptions, replay, errorReporter, audioDirectory);

    internal static GameInstallation InstallationFor(string romPath, string? dataDirectory = null)
    {
        if (dataDirectory is not null)
            return new GameInstallation(Path.GetFullPath(dataDirectory));
        string fullRomPath = Path.GetFullPath(romPath);
        string root = Path.Combine(Path.GetDirectoryName(fullRomPath)
            ?? throw new InvalidOperationException("Cartridge path has no parent directory."), "SuperMetroid-installed");
        return GameAssetInstaller.OpenOrRepair(root) ?? GameAssetInstaller.Install(fullRomPath, root);
    }

}

namespace SuperMetroid.Desktop
{
    /// <summary>Finds extracted audio beside the working or executable directory for verification hosts.</summary>
    internal static class ExtractedAudioAssetLocator
    {
        internal static string FindAudioDirectory()
        {
            foreach (string start in new[] { Environment.CurrentDirectory, AppContext.BaseDirectory })
            {
                for (DirectoryInfo? directory = new(Path.GetFullPath(start)); directory is not null; directory = directory.Parent)
                {
                    // The repository verification installation (csharp/.cache), then a hand extraction.
                    foreach (string nested in new[]
                    {
                        Path.Combine(directory.FullName, "csharp", ".cache", "verification-installed-content", "game", "audio"),
                        Path.Combine(directory.FullName, "standalone-assets", "audio"),
                    })
                    {
                        if (File.Exists(Path.Combine(nested, SuperMetroid.Core.Audio.ExtractedAudioAssetCatalog.ManifestFileName)))
                            return nested;
                    }
                    // Published builds copy the contents under an adjacent audio directory.
                    string adjacent = Path.Combine(directory.FullName, "audio");
                    if (File.Exists(Path.Combine(adjacent, SuperMetroid.Core.Audio.ExtractedAudioAssetCatalog.ManifestFileName)))
                        return adjacent;
                }
            }
            throw new DirectoryNotFoundException(
                "Could not find standalone-assets/audio/audio-manifest.json from the working or executable directory. " +
                "Run SuperMetroid.DebugRunner assets audio standalone-assets/raw standalone-assets/audio first.");
        }
    }
}
