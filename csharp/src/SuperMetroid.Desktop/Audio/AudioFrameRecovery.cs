using SuperMetroid.Desktop.Diagnostics;

namespace SuperMetroid.Desktop.Audio;

/// <summary>
/// Audio is a host side channel: a managed playback failure must not discard a completed
/// gameplay/display frame. Reporting remains explicit; no fabricated samples are submitted.
/// </summary>
internal sealed class AudioFrameRecovery
{
    /// <summary>Fingerprints already logged audio and reporting failures to avoid repeating console diagnostics.</summary>
    private readonly HashSet<string> logged = [];

    /// <summary>
    /// Runs audio submission without losing a completed game frame on a recoverable
    /// failure. Logs each fingerprint once and attempts an optional report; failure
    /// of that diagnostic transport is logged locally instead of becoming fatal.
    /// </summary>
    internal void Run(Action renderAndSubmit, Action<Exception>? report)
    {
        try
        {
            renderAndSubmit();
        }
        catch (Exception exception) when (exception is not OutOfMemoryException)
        {
            string identity = GitHubErrorReporter.CreateFingerprint(exception);
            if (logged.Add(identity))
            {
                Console.Error.WriteLine($"RECOVERABLE AUDIO ERROR [{identity}]: gameplay continues; this audio frame was not completed.");
                Console.Error.WriteLine(exception);
            }
            if (report is null) return;
            try
            {
                report(exception);
            }
            catch (Exception reportingException) when (reportingException is not OutOfMemoryException)
            {
                if (logged.Add("report:" + identity))
                {
                    Console.Error.WriteLine("Audio error could not be queued to GitHub:");
                    Console.Error.WriteLine(reportingException);
                }
            }
        }
    }
}
