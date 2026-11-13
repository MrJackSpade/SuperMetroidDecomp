using SuperMetroid.AssetExtraction.Audio;
using System.Diagnostics;
using SuperMetroid.Diagnostics.Configuration;
using SuperMetroid.Desktop;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Desktop.Audio;
using SuperMetroid.Desktop.Diagnostics;
using SuperMetroid.Desktop.Window;
using SuperMetroid.AssetExtraction.Audio.Synthesis;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

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

    /// <summary>
    /// The extracted audio catalog the repository installation's clips are rendered from: the audio's import data, which
    /// no installation holds (#1693), extracted from the installation's cartridge copy once per process.
    /// </summary>
    internal static ExtractedAudioAssetCatalog LoadAudioAssets() => InstalledFixtures.Audio;

    /// <summary>The installed pre-rendered clips (#1305) found from the working or executable directory, which a game binds.</summary>
    internal static ClipAudioLibrary LoadAudioClips() =>
        InstalledAudioClips.Load(AudioClipDirectoryLocator.FindAudioDirectory()).Clips;

    /// <summary>A standalone synthesizing session over the extracted audio catalog, for fixtures that drive the cartridge audio types directly.</summary>
    internal static SynthesizingAudioSession CreateAudioSession() =>
        AudioSessionAccess.CreateSynthesizing(LoadAudioAssets());

    /// <summary>
    /// A game control over the installation placed beside a ROM path (see <see cref="InstallationDirectoryName"/>),
    /// or over an explicit extracted data directory.
    /// </summary>
    internal static PlayableGameControl CreateGameControl(
        string romPath,
        HostOptions hostOptions,
        IHostFrameDriver? driver = null,
        GitHubErrorReporter? errorReporter = null,
        string? dataDirectory = null) =>
        new(InstallationFor(romPath, dataDirectory), hostOptions, driver, errorReporter);

    /// <summary>Directory beside a ROM where <see cref="InstallationFor"/> opens the installation a check placed there.</summary>
    internal const string InstallationDirectoryName = "SuperMetroid-installed";

    /// <summary>
    /// Uses an explicit extracted directory or opens the complete installation a check copied beside a ROM; a check
    /// never imports the ROM here, and a missing or incomplete copy fails.
    /// </summary>
    internal static GameInstallation InstallationFor(string romPath, string? dataDirectory = null)
    {
        if (dataDirectory is not null)
            return new GameInstallation(Path.GetFullPath(dataDirectory));
        string fullRomPath = Path.GetFullPath(romPath);
        string root = Path.Combine(Path.GetDirectoryName(fullRomPath)
            ?? throw new InvalidOperationException("Cartridge path has no parent directory."), InstallationDirectoryName);
        return GameAssetInstaller.TryOpenExtractedContent(root) ?? throw new InvalidOperationException(
            $"No complete installation was placed beside {fullRomPath} at {root}.");
    }

}

namespace SuperMetroid.Desktop
{
}
