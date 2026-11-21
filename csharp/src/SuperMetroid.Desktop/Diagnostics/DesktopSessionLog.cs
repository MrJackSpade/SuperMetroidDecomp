using System.IO.Compression;
using System.Reflection;
using System.Text;

namespace SuperMetroid.Desktop.Diagnostics;

/// <summary>
/// Captures the Windows host's console output without requiring shell redirection or
/// GitHub credentials. The caller supplies the executable directory, never the working
/// directory or the installed player-data directory.
/// </summary>
internal sealed class DesktopSessionLog : IDisposable
{
    /// <summary>Serializes both console streams and archive checkpoints around one journal.</summary>
    private readonly Lock gate = new();
    /// <summary>Unwrapped stdout used for the terminal and diagnostic fallback.</summary>
    private readonly TextWriter originalOutput;
    /// <summary>Unwrapped stderr used when the journal itself fails.</summary>
    private readonly TextWriter originalError;
    /// <summary>Durable UTF-8 interim log shared for read access while the game runs.</summary>
    private readonly StreamWriter journal;
    /// <summary>Stops further journal writes after a diagnostic IO failure.</summary>
    private bool journalFailed;
    /// <summary>Prevents writes and repeated archive cleanup after disposal.</summary>
    private bool disposed;

    /// <summary>Creates a uniquely named live journal and ZIP path beside the executable.</summary>
    private DesktopSessionLog(string executableDirectory)
    {
        originalOutput = Console.Out;
        originalError = Console.Error;
        string directory = Path.Combine(Path.GetFullPath(executableDirectory), "logs");
        Directory.CreateDirectory(directory);
        // PID plus a random suffix also isolates simultaneous launches and rapid restarts.
        string name = $"SuperMetroid-{DateTime.UtcNow:yyyyMMdd-HHmmss-fff}-{Environment.ProcessId}-{Guid.NewGuid():N}";
        LiveLogPath = Path.Combine(directory, name + ".log");
        ArchivePath = Path.Combine(directory, name + ".zip");
        journal = new StreamWriter(new FileStream(LiveLogPath, FileMode.CreateNew,
            FileAccess.Write, FileShare.Read), new UTF8Encoding(false)) { AutoFlush = true };
    }

    /// <summary>Durable interim text remains here if the process is forcibly terminated.</summary>
    internal string LiveLogPath { get; }

    /// <summary>The tester-shareable archive, finalized at exit or a fatal-error checkpoint.</summary>
    internal string ArchivePath { get; }

    /// <summary>Installs one ordered, thread-safe tee for stdout and stderr before startup.</summary>
    internal static DesktopSessionLog Start(string executableDirectory)
    {
        var session = new DesktopSessionLog(executableDirectory);
        Console.SetOut(new SessionConsoleWriter(session, session.originalOutput));
        Console.SetError(new SessionConsoleWriter(session, session.originalError));
        Assembly? entry = Assembly.GetEntryAssembly();
        Console.WriteLine($"Session started (UTC): {DateTimeOffset.UtcNow:O}");
        Console.WriteLine($"Game version: {entry?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? entry?.GetName().Version?.ToString() ?? "unknown"}");
        Console.WriteLine($"Executable directory: {Path.GetFullPath(executableDirectory)}");
        Console.WriteLine($"Runtime: {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}; {System.Runtime.InteropServices.RuntimeInformation.OSDescription}");
        Console.WriteLine($"Session diagnostic ZIP: {session.ArchivePath}");
        return session;
    }

    /// <summary>
    /// Publishes an independently readable ZIP while the live journal is still open.
    /// Called before the fatal prompt waits, including background-thread termination.
    /// Diagnostic IO failures remain visible but cannot replace the original exception.
    /// </summary>
    internal void Checkpoint()
    {
        lock (gate)
        {
            if (disposed || journalFailed) return;
            TryArchive();
        }
    }

    /// <summary>Archives the final journal, restores console streams, and removes interim text after success.</summary>
    public void Dispose()
    {
        lock (gate)
        {
            if (disposed) return;
            bool archived = false;
            try
            {
                if (!journalFailed)
                {
                    journal.WriteLine($"Session ended (UTC): {DateTimeOffset.UtcNow:O}; exit code: {Environment.ExitCode}");
                    archived = TryArchive();
                }
            }
            catch (Exception error)
            {
                ReportLogFailure(error);
            }
            finally
            {
                disposed = true;
                Console.SetOut(originalOutput);
                Console.SetError(originalError);
                try { journal.Dispose(); }
                catch (Exception error) { ReportLogFailure(error); }
            }
            // Delete only this session's interim text, and only after ZIP publication succeeded.
            // An interrupted or unsuccessful archive leaves the original diagnostic intact.
            if (archived)
            {
                try { File.Delete(LiveLogPath); }
                catch (Exception error) { ReportLogFailure(error); }
            }
        }
    }

    /// <summary>Flushes the live log to disk and atomically publishes a ZIP containing session.log.</summary>
    private bool TryArchive()
    {
        string temporary = ArchivePath + ".tmp";
        try
        {
            journal.Flush();
            ((FileStream)journal.BaseStream).Flush(flushToDisk: true);
            using (var output = new FileStream(temporary, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (var zip = new ZipArchive(output, ZipArchiveMode.Create, leaveOpen: true))
                {
                    ZipArchiveEntry entry = zip.CreateEntry("session.log", CompressionLevel.Fastest);
                    using Stream destination = entry.Open();
                    using var source = new FileStream(LiveLogPath, FileMode.Open,
                        FileAccess.Read, FileShare.ReadWrite);
                    source.CopyTo(destination);
                }
                output.Flush(flushToDisk: true);
            }
            File.Move(temporary, ArchivePath, overwrite: true);
            return true;
        }
        catch (Exception error)
        {
            ReportLogFailure(error);
            return false;
        }
    }

    /// <summary>Writes a console segment to the journal first, then forwards it to its original stream.</summary>
    internal void Write(TextWriter console, ReadOnlySpan<char> text)
    {
        lock (gate)
        {
            if (!disposed && !journalFailed)
            {
                try { journal.Write(text); }
                catch (Exception error)
                {
                    journalFailed = true;
                    ReportLogFailure(error);
                }
            }
            console.Write(text);
        }
    }

    /// <summary>Flushes journal and original stream in the same order as their writes.</summary>
    internal void Flush(TextWriter console)
    {
        lock (gate)
        {
            if (!disposed && !journalFailed)
            {
                try { journal.Flush(); }
                catch (Exception error)
                {
                    journalFailed = true;
                    ReportLogFailure(error);
                }
            }
            console.Flush();
        }
    }

    /// <summary>Reports logging failure directly to unwrapped stderr to avoid a broken tee loop.</summary>
    private void ReportLogFailure(Exception error)
    {
        // Bypass the tee: writing another error through a failed journal would recurse.
        originalError.WriteLine($"SESSION LOG FAILURE: preserve any available text at {LiveLogPath}");
        originalError.WriteLine(error.ToString());
        originalError.Flush();
    }
}
