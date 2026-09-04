using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Channels;

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
    string? InputRecordingPath = null);

/// <summary>
/// Queues recoverable failures to one private GitHub repository without blocking emulation.
/// </summary>
/// <remarks>
/// The fingerprint uses exception identities, messages, and managed method identities but
/// deliberately excludes source paths and line numbers. Moving unchanged code therefore
/// does not manufacture a new issue, while a different cartridge address in an exception
/// message remains a distinct defect. The queue is single-reader so two novel failures can
/// never race each other through the remote duplicate check.
/// </remarks>
public sealed class GitHubErrorReporter : IDisposable
{
    private readonly string repository;
    private readonly IGitHubIssueClient issueClient;
    private readonly Channel<QueuedError> queue = Channel.CreateUnbounded<QueuedError>(
        new UnboundedChannelOptions { SingleReader = true, SingleWriter = false });
    private readonly Dictionary<string, int> sessionOccurrences = new(StringComparer.Ordinal);
    private readonly object fingerprintLock = new();
    private readonly Task worker;
    private int disposed;

    public GitHubErrorReporter(string repository)
        : this(repository, new GhCliGitHubIssueClient())
    {
    }

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

        string fingerprint = CreateFingerprint(exception);
        lock (fingerprintLock)
        {
            if (sessionOccurrences.TryGetValue(fingerprint, out int occurrences))
            {
                occurrences++;
                sessionOccurrences[fingerprint] = occurrences;
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
            sessionOccurrences.Add(fingerprint, 1);
        }

        Console.Error.WriteLine();
        Console.Error.WriteLine(
            $"RECOVERABLE ERROR [{fingerprint}] at {context.Boundary}; " +
            "the host will attempt the next frame.");
        Console.Error.WriteLine(exception);
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

    /// <summary>Waits until every report queued so far has completed its remote check.</summary>
    internal async Task FlushAsync()
    {
        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        if (!queue.Writer.TryWrite(new QueuedError(completion)))
            throw new ObjectDisposedException(nameof(GitHubErrorReporter));
        await completion.Task.ConfigureAwait(false);
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref disposed, 1) != 0)
            return;
        queue.Writer.TryComplete();
        worker.GetAwaiter().GetResult();
    }

    internal static string CreateFingerprint(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        var identity = new StringBuilder();
        AppendExceptionIdentity(identity, exception);
        byte[] digest = SHA256.HashData(Encoding.UTF8.GetBytes(identity.ToString()));
        return $"SMERR-{Convert.ToHexString(digest.AsSpan(0, 8))}";
    }

    private static void AppendExceptionIdentity(StringBuilder identity, Exception exception)
    {
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

    private static string BuildBody(
        string fingerprint,
        Exception exception,
        GitHubErrorContext context)
    {
        var body = new StringBuilder();
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

        body.AppendLine()
            .AppendLine("## Exception")
            .AppendLine()
            .AppendLine("```text")
            .AppendLine(exception.ToString())
            .AppendLine("```");
        return body.ToString();
    }

    private static void AppendHexContext(StringBuilder body, string name, ushort? value)
    {
        if (value is ushort word)
            body.Append("- ").Append(name).Append(": $").AppendLine(word.ToString("X4"));
    }

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

    private sealed record QueuedError(
        string? Fingerprint,
        Exception? Exception,
        GitHubErrorContext? Context,
        TaskCompletionSource? FlushCompletion)
    {
        public QueuedError(string fingerprint, Exception exception, GitHubErrorContext context)
            : this(fingerprint, exception, context, null)
        {
        }

        public QueuedError(TaskCompletionSource completion)
            : this(null, null, null, completion)
        {
        }
    }
}

internal interface IGitHubIssueClient
{
    Task<string?> FindByFingerprintAsync(string repository, string fingerprint);

    Task<string> CreateAsync(string repository, string title, string body);
}
