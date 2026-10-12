using SuperMetroid.Core;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Channels;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Desktop;

/// <summary>Debugger context captured at a recoverable desktop-host error boundary.</summary>
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
    /// <summary>Stores repository.</summary>
    private readonly string repository;
    /// <summary>Stores issue client.</summary>
    private readonly IGitHubIssueClient issueClient;
    /// <summary>Stores queue.</summary>
    private readonly Channel<QueuedError> queue = Channel.CreateUnbounded<QueuedError>(
        new UnboundedChannelOptions { SingleReader = true, SingleWriter = false });
    /// <summary>Stores session occurrences.</summary>
    private readonly Dictionary<string, int> sessionOccurrences = new(StringComparer.Ordinal);
    /// <summary>Stores fingerprint lock.</summary>
    private readonly Lock fingerprintLock = new();
    /// <summary>Stores worker.</summary>
    private readonly Task worker;
    /// <summary>Stores disposed.</summary>
    private int disposed;

    /// <summary>Starts a single-reader reporting queue backed by the authenticated GitHub CLI.</summary>
    /// <param name="repository">Nonblank GitHub repository identifier, in owner/repository form, receiving queued reports.</param>
    public GitHubErrorReporter(string repository)
        : this(repository, new GhCliGitHubIssueClient())
    {
    }

    /// <summary>Initializes a new git hub error reporter instance.</summary>
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

    /// <summary>Creates fingerprint.</summary>
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

    /// <summary>Append exception identity.</summary>
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

    /// <summary>Processes queue async.</summary>
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

    /// <summary>Builds title.</summary>
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

    /// <summary>Builds body.</summary>
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

    /// <summary>Append hex context.</summary>
    private static void AppendHexContext(StringBuilder body, string name, ushort? value)
    {
        if (value is ushort word)
            body.Append("- ").Append(name).Append(": $").AppendLine(word.ToString("X4"));
    }

    /// <summary>Append bank context.</summary>
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

    /// <summary>Represents queued error.</summary>
    private sealed record QueuedError(
        string? Fingerprint,
        Exception? Exception,
        GitHubErrorContext? Context,
        TaskCompletionSource? FlushCompletion)
    {
        /// <summary>Initializes a new queued error instance.</summary>
        public QueuedError(string fingerprint, Exception exception, GitHubErrorContext context)
            : this(fingerprint, exception, context, null)
        {
        }
    }
}

/// <summary>Represents igit hub issue client.</summary>
internal interface IGitHubIssueClient
{
    /// <summary>Comment async.</summary>
    Task CommentAsync(string repository, string issueUrl, string body);
    /// <summary>Finds by fingerprint async.</summary>
    Task<string?> FindByFingerprintAsync(string repository, string fingerprint);

    /// <summary>Creates async.</summary>
    Task<string> CreateAsync(string repository, string title, string body);
}
