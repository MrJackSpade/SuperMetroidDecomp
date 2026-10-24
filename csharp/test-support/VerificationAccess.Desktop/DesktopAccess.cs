using System.Diagnostics;
using SuperMetroid.Diagnostics.Configuration;
using SuperMetroid.Desktop;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Diagnostics.Recording;
using SuperMetroid.Desktop.Audio;
using SuperMetroid.Desktop.Diagnostics;
using SuperMetroid.Desktop.Window;
using SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>Verification views of private desktop audio and error-reporting state.</summary>
internal static class DesktopAccess
{
    /// <summary>Exposes waveOut buffer ownership and worker progress so audio verification can distinguish queued samples from completed playback.</summary>
    extension(WaveOutAudioDevice device)
    {
        /// <summary>Counts buffers prepared for waveOut while holding the device's state gate.</summary>
        internal int PreparedBufferCountForVerification
        {
            get
            {
                lock (device.deviceGate)
                    return device.slots.Count(slot => slot.Prepared);
            }
        }

        /// <summary>Counts prepared buffers that waveOut has not yet marked done.</summary>
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

    /// <summary>Lets verification wait for the error reporter's queued work by inserting a completion marker after pending reports.</summary>
    extension(GitHubErrorReporter reporter)
    {
        /// <summary>Queues a flush marker behind every pending report and waits for the worker to reach it.</summary>
        internal async Task FlushAsync()
        {
            var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var marker = new GitHubErrorReporter.QueueFlush(completion);
            if (!reporter.queue.Writer.TryWrite(marker))
                throw new ObjectDisposedException(nameof(GitHubErrorReporter));
            await completion.Task.ConfigureAwait(false);
        }
    }

    /// <summary>The extracted audio found from the working or executable directory.</summary>
    internal static ExtractedAudioAssetCatalog LoadAudioAssets(string? audioDirectory = null) =>
        ExtractedAudioAssetCatalog.Load(audioDirectory ?? ExtractedAudioAssetLocator.FindAudioDirectory());

    /// <summary>The installed pre-rendered clips beside that audio (#1305), which a game binds.</summary>
    internal static ClipAudioCatalog LoadAudioClips(string? audioDirectory = null) =>
        ClipAudioCatalog.Load(audioDirectory ?? ExtractedAudioAssetLocator.FindAudioDirectory());

    /// <summary>A standalone synthesizing session over that audio, for fixtures that drive the cartridge audio types directly.</summary>
    internal static SynthesizingAudioSession CreateAudioSession(string? audioDirectory = null) =>
        AudioSessionAccess.CreateSynthesizing(LoadAudioAssets(audioDirectory));

    /// <summary>
    /// A game control over the installation beside a ROM path, importing it on first use, or over
    /// an explicit extracted data directory.
    /// </summary>
    internal static PlayableGameControl CreateGameControl(
        string romPath,
        HostOptions hostOptions,
        ControllerInputRecording? replay = null,
        GitHubErrorReporter? errorReporter = null,
        string? audioDirectory = null,
        string? dataDirectory = null) =>
        new(InstallationFor(romPath, dataDirectory), hostOptions, replay, errorReporter, audioDirectory);

    /// <summary>Directory beside a ROM where <see cref="InstallationFor"/> opens or creates its installation.</summary>
    internal const string InstallationDirectoryName = "SuperMetroid-installed";

    /// <summary>Uses an explicit extracted directory or repairs and opens the installation beside a ROM.</summary>
    internal static GameInstallation InstallationFor(string romPath, string? dataDirectory = null)
    {
        if (dataDirectory is not null)
            return new GameInstallation(Path.GetFullPath(dataDirectory));
        string fullRomPath = Path.GetFullPath(romPath);
        string root = Path.Combine(Path.GetDirectoryName(fullRomPath)
            ?? throw new InvalidOperationException("Cartridge path has no parent directory."), InstallationDirectoryName);
        return GameAssetInstaller.OpenOrRepair(root) ?? GameAssetInstaller.Install(fullRomPath, root);
    }

}

namespace SuperMetroid.Desktop
{
}
