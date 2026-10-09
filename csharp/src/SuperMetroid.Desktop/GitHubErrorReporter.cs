using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Channels;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Desktop;

/// <summary>Debugger context captured at a recoverable desktop-host error boundary.</summary>
/// <param name="Boundary">Name of the host boundary that caught the exception.</param>
/// <param name="FrameNumber">Optional emulated frame number associated with the failure.</param>
/// <param name="GameState">Optional readable game-state identity at capture time.</param>
/// <param name="Phase">Optional finer-grained coroutine or transition phase.</param>
/// <param name="ControllerInput">Optional controller word consumed for the failing frame.</param>
/// <param name="RoomPointer">Optional bank-$8F room header pointer.</param>
/// <param name="RoomStatePointer">Optional bank-$8F room-state pointer.</param>
/// <param name="DoorPointer">Optional bank-$83 door pointer involved in the current transition.</param>
/// <param name="InputRecordingPath">Optional path to the replay recording associated with the failure.</param>
/// <param name="FrameDiagnostics">Optional textual diagnostics captured from the failed frame.</param>
public sealed record GitHubErrorContext(
    string Boundary,
    ushort? FrameNumber = null,
    string? GameState = null,
    string? Phase = null,
    ushort? ControllerInput = null,
    ushort? RoomPointer = null,
    ushort? RoomStatePointer = null,
    ushort? DoorPointer = null,
    string? InputRecordingPath = null,
    string? FrameDiagnostics = null);

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
    /// <summary>GitHub owner/repository target shared by issue lookup and report submission.</summary>
    private readonly string repository;
    /// <summary>Authenticated client used by the background worker to find, comment on, or create issues.</summary>
    private readonly IGitHubIssueClient issueClient;
    /// <summary>Single-reader channel preserving report order without blocking the emulation thread.</summary>
    private readonly Channel<QueuedError> queue = Channel.CreateUnbounded<QueuedError>(
        new UnboundedChannelOptions { SingleReader = true, SingleWriter = false });
    /// <summary>Per-session occurrence totals used to throttle repeated reports with the same context.</summary>
    private readonly Dictionary<string, int> sessionOccurrences = new(StringComparer.Ordinal);
    /// <summary>Protects occurrence tracking when recoverable boundaries report concurrently.</summary>
    private readonly object fingerprintLock = new();
    /// <summary>Background consumer that drains the queue and performs GitHub operations.</summary>
    private readonly Task worker;
    /// <summary>Atomically indicates that disposal has begun and further reports must be rejected.</summary>
    private int disposed;

    /// <summary>Starts a single-reader reporting queue backed by the authenticated GitHub CLI.</summary>
    /// <param name="repository">Nonblank GitHub repository identifier, in owner/repository form, receiving queued reports.</param>
    public GitHubErrorReporter(string repository)
        : this(repository, new GhCliGitHubIssueClient())
    {
    }

    /// <summary>Creates a reporter with an explicitly supplied client, primarily for host integration.</summary>
    /// <param name="repository">Nonblank GitHub repository identifier receiving reports.</param>
    /// <param name="issueClient">Client that performs issue lookup, comments, and creation.</param>
    internal GitHubErrorReporter(string repository, IGitHubIssueClient issueClient)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(repository);
        this.repository = repository;
        this.issueClient = issueClient ?? throw new ArgumentNullException(nameof(issueClient));
        worker = ProcessQueueAsync();
    }

    /// <summary>
    /// Prints the complete exception immediately and asynchronously queues its first
    /// occurrence. Returns the stable ID even when this session already saw the failure.
    /// </summary>
    public string Report(Exception exception, GitHubErrorContext context)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentNullException.ThrowIfNull(context);
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

    /// <summary>Creates a stable issue key from exception identity and, for ordinary failures, room context.</summary>
    /// <param name="exception">Failure whose type, message, and managed call identities contribute to the key.</param>
    /// <param name="context">Optional room context included unless the exception declares room-independent identity.</param>
    /// <returns>A compact <c>SMERR-</c> identifier suitable for duplicate issue lookup.</returns>
    internal static string CreateFingerprint(Exception exception, GitHubErrorContext? context = null)
    {
        ArgumentNullException.ThrowIfNull(exception);
        var identity = new StringBuilder();
        AppendExceptionIdentity(identity, exception);
        // Unknown failures are conservatively room-scoped. Only an explicit cartridge
        // dispatch identity may declare that room/block location does not change the bug.
        if (exception is not CartridgeDispatchException { IsRoomIndependent: true } && context is not null)
            identity.Append($"\nroom:{context.RoomPointer:X4}/state:{context.RoomStatePointer:X4}");
        byte[] digest = SHA256.HashData(Encoding.UTF8.GetBytes(identity.ToString()));
        return $"SMERR-{Convert.ToHexString(digest.AsSpan(0, 8))}";
    }

    /// <summary>Appends stable exception and call-chain identity, recursively including inner exceptions.</summary>
    /// <param name="identity">Builder receiving the normalized identity components.</param>
    /// <param name="exception">Exception whose identity is appended.</param>
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

    /// <summary>Consumes reports sequentially, updating an existing fingerprint issue or creating a new one.</summary>
    private async Task ProcessQueueAsync()
    {
        await foreach (QueuedError queued in queue.Reader.ReadAllAsync().ConfigureAwait(false))
        {
            if (queued.FlushCompletion is TaskCompletionSource completion)
            {
                completion.TrySetResult();
                continue;
            }

            try
            {
                string? existingUrl = await issueClient.FindByFingerprintAsync(
                    repository,
                    queued.Fingerprint!).ConfigureAwait(false);
                if (existingUrl is not null)
                {
                    await issueClient.CommentAsync(repository, existingUrl,
                        BuildBody(queued.Fingerprint!, queued.Exception!, queued.Context!)).ConfigureAwait(false);
                    Console.Error.WriteLine(
                        $"GitHub error [{queued.Fingerprint}] already exists: {existingUrl}");
                    continue;
                }

                string title = BuildTitle(queued.Fingerprint!, queued.Exception!);
                string body = BuildBody(queued.Fingerprint!, queued.Exception!, queued.Context!);
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

    /// <summary>Builds the bounded issue title with the stable fingerprint and one-line exception description.</summary>
    /// <param name="fingerprint">Stable report identifier included for later duplicate searches.</param>
    /// <param name="exception">Failure supplying the exception type and message.</param>
    /// <returns>Title truncated to GitHub's supported issue-title length.</returns>
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

    /// <summary>Formats runtime context and the full exception into the report's Markdown body.</summary>
    /// <param name="fingerprint">Stable identifier embedded in the report for duplicate detection.</param>
    /// <param name="exception">Failure whose complete details appear in the exception section.</param>
    /// <param name="context">Captured host and emulated-game context for the failed boundary.</param>
    /// <returns>Markdown report containing build identity, runtime context, and exception details.</returns>
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
        AppendHexContext(body, "Controller input", context.ControllerInput);
        AppendBankContext(body, "Room", 0x8F, context.RoomPointer);
        AppendBankContext(body, "Room state", 0x8F, context.RoomStatePointer);
        AppendBankContext(body, "Door", 0x83, context.DoorPointer);
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

    /// <summary>Adds a context bullet for a present 16-bit value, formatted as four hexadecimal digits.</summary>
    /// <param name="body">Markdown builder receiving the optional bullet.</param>
    /// <param name="name">Human-readable field label.</param>
    /// <param name="value">Optional word to format.</param>
    private static void AppendHexContext(StringBuilder body, string name, ushort? value)
    {
        if (value is ushort word)
            body.Append("- ").Append(name).Append(": $").AppendLine(word.ToString("X4"));
    }

    /// <summary>Adds a bank-qualified address bullet when the optional address is present.</summary>
    /// <param name="body">Markdown builder receiving the optional bullet.</param>
    /// <param name="name">Human-readable address label.</param>
    /// <param name="bank">Cartridge bank displayed before the address word.</param>
    /// <param name="value">Optional low-word address to include.</param>
    private static void AppendBankContext(
        StringBuilder body,
        string name,
        byte bank,
        ushort? value)
    {
        if (value is ushort address)
        {
            body.Append("- ").Append(name).Append(": $")
                .Append(bank.ToString("X2")).Append(':').AppendLine(address.ToString("X4"));
        }
    }

    /// <summary>Work item consumed by the report queue, including a marker-only item used to flush it.</summary>
    /// <param name="Fingerprint">Stable duplicate-detection key, absent for a flush marker.</param>
    /// <param name="Exception">Failure to report, absent for a flush marker.</param>
    /// <param name="Context">Captured failure context, absent for a flush marker.</param>
    /// <param name="FlushCompletion">Completion source signaled when the worker reaches a flush marker.</param>
    private sealed record QueuedError(
        string? Fingerprint,
        Exception? Exception,
        GitHubErrorContext? Context,
        TaskCompletionSource? FlushCompletion)
    {
        /// <summary>Creates a report work item; ordinary reports always carry all three payload values.</summary>
        /// <param name="fingerprint">Stable identifier used to locate or create the issue.</param>
        /// <param name="exception">Captured failure to include in the report.</param>
        /// <param name="context">Runtime context captured at the recoverable boundary.</param>
        public QueuedError(string fingerprint, Exception exception, GitHubErrorContext context)
            : this(fingerprint, exception, context, null)
        {
        }
    }
}

/// <summary>Remote issue operations used by the reporter's background queue worker.</summary>
internal interface IGitHubIssueClient
{
    /// <summary>Posts the report body as a comment on an existing issue.</summary>
    /// <param name="repository">Owner/repository target containing the issue.</param>
    /// <param name="issueUrl">URL of the issue to update.</param>
    /// <param name="body">Markdown comment body.</param>
    /// <returns>A task completed when the comment operation finishes.</returns>
    Task CommentAsync(string repository, string issueUrl, string body);

    /// <summary>Searches the repository for an issue containing the stable fingerprint.</summary>
    /// <param name="repository">Owner/repository target to search.</param>
    /// <param name="fingerprint">Stable identifier embedded in the issue body or title.</param>
    /// <returns>The existing issue URL, or <see langword="null"/> when no match is found.</returns>
    Task<string?> FindByFingerprintAsync(string repository, string fingerprint);

    /// <summary>Creates a new issue for a failure that has no existing fingerprint match.</summary>
    /// <param name="repository">Owner/repository target receiving the issue.</param>
    /// <param name="title">Issue title prepared by the reporter.</param>
    /// <param name="body">Markdown body containing the captured diagnostic report.</param>
    /// <returns>URL of the newly created issue.</returns>
    Task<string> CreateAsync(string repository, string title, string body);
}
