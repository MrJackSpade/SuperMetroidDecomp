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
                lock (PrivateState.Field<object>(device, "deviceGate"))
                    return Slots(device).Count(slot => PrivateState.Property<bool>(slot, "Prepared"));
            }
        }

        internal int NativeQueuedBufferCountForVerification
        {
            get
            {
                lock (PrivateState.Field<object>(device, "deviceGate"))
                    return Slots(device).Count(slot => PrivateState.Property<bool>(slot, "Prepared")
                        && !PrivateState.Property<bool>(slot, "IsDone"));
            }
        }

        /// <summary>Waits until the worker completes every accepted PCM frame, within the device's own buffer-return bound.</summary>
        internal void WaitForPendingSubmissions()
        {
            long target = PrivateState.Field<long>(device, "enqueuedFrames");
            Stopwatch elapsed = Stopwatch.StartNew();
            while (PrivateState.Field<long>(device, "completedFrames") < target)
            {
                PrivateState.Invoke(device, "ThrowWorkerFailure");
                if (elapsed.ElapsedMilliseconds >= WaveOutAudioPolicy.BufferReturnTimeoutMilliseconds)
                    throw new TimeoutException(
                        $"waveOut worker completed {PrivateState.Field<long>(device, "completedFrames")} of " +
                        $"{target} accepted PCM frames within the bounded audit interval.");
                Thread.Sleep(1);
            }
            PrivateState.Invoke(device, "ThrowWorkerFailure");
        }
    }

    extension(GitHubErrorReporter reporter)
    {
        /// <summary>Queues a flush marker behind every pending report and waits for the worker to reach it.</summary>
        internal async Task FlushAsync()
        {
            var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            object queue = PrivateState.Field<object>(reporter, "queue");
            Type queuedError = queue.GetType().GetGenericArguments()[0];
            object marker = Activator.CreateInstance(queuedError, [null, null, null, completion])!;
            object writer = PrivateState.Property<object>(queue, "Writer");
            if (!(bool)PrivateState.Invoke(writer, "TryWrite", marker)!)
                throw new ObjectDisposedException(nameof(GitHubErrorReporter));
            await completion.Task.ConfigureAwait(false);
        }
    }

    /// <summary>An audio engine over the extracted audio found from the working or executable directory.</summary>
    internal static SpcAudioEngine CreateAudioEngine(string? audioDirectory = null) => new(
        ExtractedAudioAssetCatalog.Load(audioDirectory ?? ExtractedAudioAssetLocator.FindAudioDirectory()),
        new ManagedSpcPlayer());

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

    private static IEnumerable<object> Slots(WaveOutAudioDevice device) =>
        PrivateState.Field<Array>(device, "slots").Cast<object>();
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
                    string nested = Path.Combine(directory.FullName, "standalone-assets", "audio");
                    if (File.Exists(Path.Combine(nested, SuperMetroid.Core.Audio.ExtractedAudioAssetCatalog.ManifestFileName)))
                        return nested;
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
