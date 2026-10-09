using System.Threading;

namespace SuperMetroid.Desktop;

/// <summary>
/// Owns the fatal-error contract shared by the playable game and room-viewer executables.
/// A developer build must preserve the complete managed exception in its console and keep
/// that console alive long enough for a person to read or copy it without Visual Studio.
/// </summary>
public static class UnhandledExceptionConsole
{
    /// <summary>Ensures only the first concurrent fatal reporter writes a stack trace and owns the console prompt.</summary>
    private static int fatalErrorIsBeingReported;
    /// <summary>Optional sink that may handle a WinForms callback exception without terminating the message loop.</summary>
    private static Action<Exception>? recoverableUiErrorReporter;
    /// <summary>Callback used to publish diagnostic artifacts after printing a fatal error and before waiting to exit.</summary>
    private static Action? fatalDiagnosticCheckpoint;

    /// <summary>
    /// Routes WinForms event-handler and background-thread failures through the same visible,
    /// blocking console report. This must be called before WinForms creates its first window.
    /// </summary>
    public static void InstallWinFormsHandlers()
    {
        // Without CatchException, WinForms may replace a useful stack trace with its modal
        // ThreadException dialog. The desktop hosts intentionally use the console subsystem.
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += HandleUiThreadException;

        // Exceptions escaping non-UI threads bypass Application.ThreadException. The CLR
        // raises this notification immediately before terminating the process, which still
        // gives us an opportunity to print and wait for acknowledgment.
        AppDomain.CurrentDomain.UnhandledException += HandleBackgroundThreadException;
    }

    /// <summary>
    /// Installs an opt-in nonfatal sink for exceptions escaping WinForms callbacks.
    /// Background-thread and startup failures remain fatal because the CLR or message loop
    /// has already committed to termination by the time their outer boundary observes them.
    /// </summary>
    public static void SetRecoverableUiErrorReporter(Action<Exception>? reporter) =>
        Volatile.Write(ref recoverableUiErrorReporter, reporter);

    /// <summary>Publishes the active session's diagnostic ZIP before waiting or CLR termination.</summary>
    internal static void SetFatalDiagnosticCheckpoint(Action? checkpoint) =>
        Volatile.Write(ref fatalDiagnosticCheckpoint, checkpoint);

    /// <summary>
    /// Prints a fatal exception, flushes stderr, and waits for Enter before returning failure.
    /// Use this from a top-level catch block around startup and the WinForms message loop.
    /// </summary>
    public static int ReportAndWait(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        // A fatal exception can provoke cleanup failures on other threads. Only the first
        // failure owns the console prompt; subsequent reporters return instead of interleaving
        // multiple stack traces and competing to consume the same Enter key.
        if (Interlocked.Exchange(ref fatalErrorIsBeingReported, 1) != 0)
            return 1;

        ReportAndWait(exception, Console.Error, WaitForConsoleAcknowledgment);
        return 1;
    }

    /// <summary>
    /// Test seam that preserves the production ordering: diagnostic, flush, then wait.
    /// Keeping this in the desktop assembly lets the smoke test prove the contract without
    /// blocking an automated verification run on real console input.
    /// </summary>
    internal static void ReportAndWait(
        Exception exception,
        TextWriter error,
        Action waitForAcknowledgment)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentNullException.ThrowIfNull(error);
        ArgumentNullException.ThrowIfNull(waitForAcknowledgment);

        error.WriteLine();
        error.WriteLine("FATAL: Super Metroid stopped because of an unhandled exception.");
        error.WriteLine(exception.ToString());
        error.WriteLine();
        error.WriteLine("Press Enter to exit.");
        error.Flush();

        Volatile.Read(ref fatalDiagnosticCheckpoint)?.Invoke();

        waitForAcknowledgment();
    }

    /// <summary>Offers a WinForms callback failure to the recoverable sink, or reports it as fatal and exits the UI thread.</summary>
    /// <param name="sender">The WinForms event source; the exception policy does not depend on its identity.</param>
    /// <param name="eventArguments">The callback exception, replaced with an aggregate if the recoverable reporter also fails.</param>
    private static void HandleUiThreadException(object? sender, ThreadExceptionEventArgs eventArguments)
    {
        Action<Exception>? reporter = Volatile.Read(ref recoverableUiErrorReporter);
        if (reporter is not null)
        {
            try
            {
                reporter(eventArguments.Exception);
                return;
            }
            catch (Exception reportingException)
            {
                eventArguments = new ThreadExceptionEventArgs(
                    new AggregateException(
                        "A WinForms callback and its recoverable error reporter both failed.",
                        eventArguments.Exception,
                        reportingException));
            }
        }
        Environment.ExitCode = ReportAndWait(eventArguments.Exception);
        Application.ExitThread();
    }

    /// <summary>Prints an unhandled background-thread failure and leaves process termination to the CLR.</summary>
    /// <param name="sender">The object whose background thread raised the unhandled-exception notification.</param>
    /// <param name="eventArguments">The escaped exception object and CLR termination-state metadata.</param>
    private static void HandleBackgroundThreadException(object? sender, UnhandledExceptionEventArgs eventArguments)
    {
        Exception exception = eventArguments.ExceptionObject as Exception ??
            new InvalidOperationException(
                $"An unhandled non-Exception object escaped a background thread: " +
                $"{eventArguments.ExceptionObject}");
        Environment.ExitCode = ReportAndWait(exception);
    }

    /// <summary>Waits for interactive console acknowledgment; redirected input reaches EOF without blocking CI.</summary>
    private static void WaitForConsoleAcknowledgment()
    {
        // A normal interactive launch owns a console input stream and waits here. Redirected
        // automation has no person capable of acknowledging the prompt, so reading to EOF is
        // the only meaningful equivalent and prevents CI from hanging indefinitely.
        try
        {
            Console.ReadLine();
        }
        catch (IOException exception)
        {
            Console.Error.WriteLine($"Console input was unavailable; exiting now: {exception.Message}");
        }
        catch (InvalidOperationException exception)
        {
            Console.Error.WriteLine($"Console input was unavailable; exiting now: {exception.Message}");
        }
    }
}
