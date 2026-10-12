namespace SuperMetroid.Desktop;

/// <summary>
/// Audio is a host side channel: a managed playback failure must not discard a completed
/// gameplay/display frame. Reporting remains explicit; no fabricated samples are submitted.
/// </summary>
internal sealed class AudioFrameRecovery
{
    private readonly HashSet<string> logged = [];

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
                // A broken diagnostic transport must not turn an audio-only error into
                // a fatal game error. Preserve both diagnostics locally and say so.
                if (logged.Add("report:" + identity))
                {
                    Console.Error.WriteLine("Audio error could not be queued to GitHub:");
                    Console.Error.WriteLine(reportingException);
                }
            }
        }
    }
}
