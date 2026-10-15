using SuperMetroid.Core;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Channels;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Desktop.Diagnostics;

/// <summary>
/// Queues recoverable failures to one private GitHub repository without blocking emulation.
/// </summary>
/// <remarks>
/// The fingerprint uses exception identities, messages, and managed method identities but
/// deliberately excludes source paths and line numbers. Moving unchanged code therefore
/// does not manufacture a new issue, while a different cartridge address in an exception
/// message remains a distinct defect. Unclassified errors also include room/state identity;
/// typed cartridge dispatch errors can explicitly share a semantic key across rooms.
/// The queue is single-reader so two novel failures can
/// never race each other through the remote duplicate check.
/// </remarks>
public sealed class GitHubErrorReporter : IDisposable
{
    /// <summary>Private owner/repository destination for automatically captured errors.</summary>
    private readonly string repository;
    /// <summary>Remote lookup, comment, and creation operations used by the single worker.</summary>
    private readonly IGitHubIssueClient issueClient;
    /// <summary>Nonblocking multiwriter queue processed in report order by one remote worker.</summary>
    internal readonly Channel<QueuedWork> queue = Channel.CreateUnbounded<QueuedWork>(
        new UnboundedChannelOptions { SingleReader = true, SingleWriter = false });
    /// <summary>Counts repeated failures per fingerprint and context after excluding frame/input changes.</summary>
    private readonly Dictionary<string, int> sessionOccurrences = new(StringComparer.Ordinal);
    /// <summary>Serializes occurrence throttling across reporting threads.</summary>
    private readonly Lock fingerprintLock = new();
    /// <summary>Drains queued errors and prevents duplicate-check races between novel failures.</summary>
    private readonly Task worker;
    /// <summary>Atomic gate that stops queueing new reports after disposal begins.</summary>
    private int disposed;

    /// <summary>Starts a single-reader reporting queue backed by the authenticated GitHub CLI.</summary>
    /// <param name="repository">Nonblank GitHub repository identifier, in owner/repository form, receiving queued reports.</param>
    public GitHubErrorReporter(string repository)
        : this(repository, new GhCliGitHubIssueClient())
    {
    }

    /// <summary>Starts the same reporting worker with an injected issue client for focused host checks.</summary>
    internal GitHubErrorReporter(string repository, IGitHubIssueClient issueClient)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(repository);
        this.repository = repository;
        this.issueClient = Ensure.NotNull(issueClient);
        worker = ProcessQueueAsync();
    }

    /// <summary>
    /// Prints the complete exception immediately and asynchronously queues its first
    /// occurrence. Returns the stable ID even when this session already saw the failure.
    /// </summary>
    public string Report(Exception exception, GitHubErrorContext context)
    {
        Ensure.NotNull(exception);
        Ensure.NotNull(context);
        ObjectDisposedException.ThrowIf(Volatile.Read(ref disposed) != 0, this);

        string fingerprint = CreateFingerprint(exception, context);
        // A repeated frame is throttled, but a new room/recording occurrence must retain
        // its context even when the underlying dispatch defect has the same identity.
        string occurrenceKey = fingerprint + (context with { FrameNumber = null, ControllerInput = null }).ToString();
        lock (fingerprintLock)
        {
            if (sessionOccurrences.TryGetValue(occurrenceKey, out int occurrences))
            {
                occurrences++;
                sessionOccurrences[occurrenceKey] = occurrences;
                // A dispatcher which retries the same unsupported operation every frame
                // must not turn the console into a 60-Hz bottleneck. The first failure is
                // complete and later milestones remain visibly loud without flooding it.
                if (occurrences == 2 || occurrences % 600 == 0)
                {
                    Console.Error.WriteLine(
                        $"Recoverable error [{fingerprint}] has occurred {occurrences} times; " +
                        "the existing GitHub report remains authoritative.");
                }
                return fingerprint;
            }
            sessionOccurrences.Add(occurrenceKey, 1);
        }

        Console.Error.WriteLine();
        Console.Error.WriteLine(
            $"RECOVERABLE ERROR [{fingerprint}] at {context.Boundary}; " +
            "the host will attempt the next frame.");
        Console.Error.WriteLine(exception);
        Console.Error.WriteLine($"Room $8F:{context.RoomPointer:X4}, state $8F:{context.RoomStatePointer:X4}, " +
            $"door $83:{context.DoorPointer:X4}, recording {context.InputRecordingPath}");
        Console.Error.WriteLine(
            "The failed frame may have partially mutated emulated state. " +
            "A repeated throw can stall progress until the translation is fixed.");
        Console.Error.Flush();

        if (!queue.Writer.TryWrite(new QueuedError(fingerprint, exception, context)))
        {
            throw new InvalidOperationException(
                $"GitHub error-report queue rejected new report [{fingerprint}].");
        }
        Console.Error.WriteLine($"Queued GitHub error report [{fingerprint}] for {repository}.");
        return fingerprint;
    }

    /// <summary>Stops accepting reports and synchronously waits for the existing queue to finish; repeated calls do nothing.</summary>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref disposed, 1) != 0)
            return;
        queue.Writer.TryComplete();
        worker.GetAwaiter().GetResult();
    }

    /// <summary>Hashes exception and managed-method identity, adding room/state unless a typed dispatch error declares itself room-independent.</summary>
    internal static string CreateFingerprint(Exception exception, GitHubErrorContext? context = null)
    {
        Ensure.NotNull(exception);
        var identity = new StringBuilder();
        AppendExceptionIdentity(identity, exception);
        // Unknown failures are conservatively room-scoped. Only an explicit cartridge
        // dispatch identity may declare that room/block location does not change the bug.
        if (exception is not CartridgeDispatchException { IsRoomIndependent: true } && context is not null)
            identity.Append($"\nroom:{context.RoomPointer:X4}/state:{context.RoomStatePointer:X4}");
        byte[] digest = SHA256.HashData(Encoding.UTF8.GetBytes(identity.ToString()));
        return $"SMERR-{Convert.ToHexString(digest.AsSpan(0, 8))}";
    }

    /// <summary>Uses a typed dispatch key or recursively records exception type, message, managed stack methods, and inner errors without source locations.</summary>
    private static void AppendExceptionIdentity(StringBuilder identity, Exception exception)
    {
        if (exception is CartridgeDispatchException dispatch)
        {
            identity.Append(dispatch.GetType().FullName).Append('\n').Append(dispatch.DispatchIdentity);
            return;
        }
        identity.Append(exception.GetType().FullName)
            .Append('\n')
            .Append(exception.Message)
            .Append('\n');

        foreach (StackFrame frame in new StackTrace(exception, fNeedFileInfo: false).GetFrames())
        {
            MethodBase? method = frame.GetMethod();
            identity.Append(method?.DeclaringType?.FullName)
                .Append('.')
                .Append(method?.Name)
                .Append('\n');
        }

        if (exception.InnerException is Exception inner)
        {
            identity.Append("-- inner --\n");
            AppendExceptionIdentity(identity, inner);
        }
    }

    /// <summary>Processes reports serially, comments on matching fingerprints, and prints remote failures without recursively reporting them.</summary>
    private async Task ProcessQueueAsync()
    {
        await foreach (QueuedWork work in queue.Reader.ReadAllAsync().ConfigureAwait(false))
        {
            if (work is QueueFlush flush)
            {
                flush.Completion.TrySetResult();
                continue;
            }
            if (work is not QueuedError queued)
                throw new InvalidOperationException($"Undefined queued GitHub error work {work}.");

            try
            {
                string? existingUrl = await issueClient.FindByFingerprintAsync(
                    repository,
                    queued.Fingerprint).ConfigureAwait(false);
                if (existingUrl is not null)
                {
                    await issueClient.CommentAsync(repository, existingUrl,
                        BuildBody(queued.Fingerprint, queued.Exception, queued.Context)).ConfigureAwait(false);
                    Console.Error.WriteLine(
                        $"GitHub error [{queued.Fingerprint}] already exists: {existingUrl}");
                    continue;
                }

                string title = BuildTitle(queued.Fingerprint, queued.Exception);
                string body = BuildBody(queued.Fingerprint, queued.Exception, queued.Context);
                string createdUrl = await issueClient.CreateAsync(repository, title, body)
                    .ConfigureAwait(false);
                Console.Error.WriteLine(
                    $"Created GitHub error [{queued.Fingerprint}]: {createdUrl}");
            }
            catch (Exception reportingException)
            {
                // Reporting is explicitly a diagnostic side channel. Losing access to GitHub
                // must itself be loud, but must not recursively enqueue another GitHub issue.
                Console.Error.WriteLine(
                    $"FAILED TO FILE GITHUB ERROR [{queued.Fingerprint}] in {repository}:");
                Console.Error.WriteLine(reportingException);
                Console.Error.Flush();
            }
        }
    }

    /// <summary>Formats the searchable auto-error marker and compresses whitespace to fit GitHub's 240-character title limit.</summary>
    private static string BuildTitle(string fingerprint, Exception exception)
    {
        string message = string.Join(
            ' ',
            exception.Message.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        string prefix = $"[auto-error:{fingerprint}] {exception.GetType().Name}: ";
        int remaining = Math.Max(0, 240 - prefix.Length);
        if (message.Length > remaining)
            message = message[..Math.Max(0, remaining - 1)] + "…";
        return prefix + message;
    }

    /// <summary>Records build IDs, capture context, optional frame diagnostics, and the full exception for a new issue or occurrence comment.</summary>
    private static string BuildBody(
        string fingerprint,
        Exception exception,
        GitHubErrorContext context)
    {
        var body = new StringBuilder();
        body.AppendLine($"Core build ID: {typeof(CartridgeDispatchException).Module.ModuleVersionId}");
        body.AppendLine($"Host build ID: {typeof(GitHubErrorReporter).Module.ModuleVersionId}");
        body.Append("<!-- supermetroid-error-id:")
            .Append(fingerprint)
            .AppendLine(" -->")
            .AppendLine("Automatically captured by the opt-in desktop error reporter.")
            .AppendLine()
            .AppendLine("The host caught this at a recoverable boundary and attempted the next frame. " +
                "Because CLR exceptions cannot resume at the throwing instruction, emulated state may " +
                "already be partially mutated and a repeated failure may still stall progress.")
            .AppendLine()
            .AppendLine("## Runtime context")
            .AppendLine()
            .Append("- Captured UTC: ").AppendLine(DateTimeOffset.UtcNow.ToString("O"))
            .Append("- Boundary: ").AppendLine(context.Boundary)
            .Append("- Build: ").AppendLine(
                typeof(GitHubErrorReporter).Assembly.GetName().Version?.ToString() ?? "unknown")
            .Append("- Runtime: ").AppendLine(RuntimeInformation.FrameworkDescription)
            .Append("- OS: ").AppendLine(RuntimeInformation.OSDescription);
        AppendHexContext(body, "Frame", context.FrameNumber);
        if (context.GameState is not null)
            body.Append("- Game state: ").AppendLine(context.GameState);
        if (context.Phase is not null)
            body.Append("- Phase: ").AppendLine(context.Phase);
        // The report shows the controller word as the journal records it.
        AppendHexContext(body, "Controller input", (ushort?)context.ControllerInput);
        AppendBankContext(body, "Room", ReportedPointerBank.RoomHeaders, context.RoomPointer);
        AppendBankContext(body, "Room state", ReportedPointerBank.RoomHeaders, context.RoomStatePointer);
        AppendBankContext(body, "Door", ReportedPointerBank.DoorHeaders, context.DoorPointer);
        if (context.InputRecordingPath is not null)
            body.Append("- Input recording: `").Append(context.InputRecordingPath).AppendLine("`");
        if (context.FrameDiagnostics is not null)
            body.AppendLine().AppendLine("## Frame failure state").AppendLine()
                .AppendLine("```text").AppendLine(context.FrameDiagnostics).AppendLine("```");

        body.AppendLine()
            .AppendLine("## Exception")
            .AppendLine()
            .AppendLine("```text")
            .AppendLine(exception.ToString())
            .AppendLine("```");
        return body.ToString();
    }

    /// <summary>Adds an optional 16-bit runtime field in four-digit hexadecimal form.</summary>
    private static void AppendHexContext(StringBuilder body, string name, ushort? value)
    {
        if (value is ushort word)
            body.Append("- ").Append(name).Append(": $").AppendLine(word.ToString("X4"));
    }

    /// <summary>Adds an optional room or door pointer with its owning cartridge bank.</summary>
    private static void AppendBankContext(
        StringBuilder body,
        string name,
        ReportedPointerBank bank,
        ushort? value)
    {
        if (value is ushort address)
        {
            body.Append("- ").Append(name).Append(": $")
                .Append(((byte)bank).ToString("X2")).Append(':').AppendLine(address.ToString("X4"));
        }
    }

    /// <summary>One item the serial worker consumes: an exception occurrence or a flush marker.</summary>
    internal abstract record QueuedWork;

    /// <summary>One queued exception occurrence.</summary>
    internal sealed record QueuedError(string Fingerprint, Exception Exception, GitHubErrorContext Context) : QueuedWork;

    /// <summary>A marker the worker completes once every earlier item has been processed.</summary>
    internal sealed record QueueFlush(TaskCompletionSource Completion) : QueuedWork;
}
