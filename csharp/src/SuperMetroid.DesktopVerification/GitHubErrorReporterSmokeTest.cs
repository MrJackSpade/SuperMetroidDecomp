using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

namespace SuperMetroid.Desktop;

/// <summary>Deterministic audit of stable fingerprints and both deduplication layers.</summary>
public static class GitHubErrorReporterSmokeTest
{
    /// <summary>Checks stable error fingerprints, session/remote deduplication, captured failure context, and audio recovery using an in-memory issue client without contacting GitHub.</summary>
    public static GitHubErrorReporterSmokeTestResult Run()
    {
        VerifyAudioRecovery();
        string frameDiagnostics = VerifyFrameFailureSnapshot();
        var client = new RecordingIssueClient();
        Exception repeated = CaptureFixtureException("door callback $8F:B9A2 is untranslated");
        Exception existing = CaptureFixtureException("PLM instruction $84:BA6F is untranslated");

        string repeatedFingerprint;
        string duplicateFingerprint;
        using (var reporter = new GitHubErrorReporter("owner/private-repository", client))
        {
            var context = new GitHubErrorContext(
                "smoke-test frame boundary",
                FrameNumber: 42,
                GameState: "$08 MainGameplay",
                Phase: "Gameplay",
                ControllerInput: 0x0080,
                RoomPointer: 0x96BA,
                DoorPointer: 0x8BB6,
                InputRecordingPath: "fixture.smrec",
                FrameDiagnostics: frameDiagnostics);
            Require(GitHubErrorReporter.CreateFingerprint(repeated, context) ==
                GitHubErrorReporter.CreateFingerprint(repeated, context with { FrameDiagnostics = "different frame and handler" }),
                "Diagnostic state changes must not split error fingerprints.");
            client.ExistingFingerprints.Add(GitHubErrorReporter.CreateFingerprint(existing, context));
            repeatedFingerprint = reporter.Report(repeated, context);
            duplicateFingerprint = reporter.Report(repeated, context);
            _ = reporter.Report(existing, context);
            var firstDispatch = new SuperMetroid.Core.Hardware.CartridgeDispatchException(
                "grapple/type-C/bts-45", "block 12 (2,3)", true);
            var movedDispatch = new SuperMetroid.Core.Hardware.CartridgeDispatchException(
                "grapple/type-C/bts-45", "block 99 (4,5)", true);
            var otherDispatch = new SuperMetroid.Core.Hardware.CartridgeDispatchException(
                "grapple/type-C/bts-4F", "block 12 (2,3)", true);
            var otherRoom = context with { RoomPointer = 0xda60, InputRecordingPath = "new-run.smrec" };
            Require(GitHubErrorReporter.CreateFingerprint(firstDispatch, context) == GitHubErrorReporter.CreateFingerprint(movedDispatch, otherRoom),
                "Room-independent dispatch split by block/room location.");
            Require(GitHubErrorReporter.CreateFingerprint(firstDispatch, context) != GitHubErrorReporter.CreateFingerprint(otherDispatch, context),
                "Different BTS failures merged.");
            Require(GitHubErrorReporter.CreateFingerprint(existing, context) != GitHubErrorReporter.CreateFingerprint(existing, otherRoom),
                "Unclassified room-sensitive failure merged across rooms.");
            client.ExistingFingerprints.Add(GitHubErrorReporter.CreateFingerprint(firstDispatch, context));
            reporter.Report(firstDispatch, context);
            reporter.Report(firstDispatch, context with { FrameNumber = 43 });
            reporter.Report(movedDispatch, otherRoom);
            reporter.FlushAsync().GetAwaiter().GetResult();
        }

        Require(repeatedFingerprint == duplicateFingerprint,
            "Repeated exception did not retain one stable fingerprint.");
        Require(client.FindCalls.Count == 4,
            $"Expected four distinct occurrence lookups, got {client.FindCalls.Count}.");
        Require(client.Comments.Count == 3 && client.Comments.Any(body => body.Contains("new-run.smrec")),
            "Existing ticket did not receive new occurrence context, or repeated frames spammed comments.");
        Require(client.CreatedIssues.Count == 1,
            $"Expected one created issue after remote deduplication, got {client.CreatedIssues.Count}.");
        CreatedIssue created = client.CreatedIssues[0];
        Require(created.Title.Contains(repeatedFingerprint, StringComparison.Ordinal),
            "Created issue title omitted its stable fingerprint.");
        Require(created.Body.Contains($"supermetroid-error-id:{repeatedFingerprint}",
                StringComparison.Ordinal),
            "Created issue body omitted its machine-readable fingerprint marker.");
        Require(created.Body.Contains("$8F:96BA", StringComparison.Ordinal) &&
                created.Body.Contains("$83:8BB6", StringComparison.Ordinal) &&
                created.Body.Contains(repeated.ToString(), StringComparison.Ordinal),
            "Created issue omitted frame context or the complete exception.");
        Require(created.Body.Contains(frameDiagnostics, StringComparison.Ordinal),
            "Created issue omitted the exact before/failed-frame diagnostic payload.");

        return new GitHubErrorReporterSmokeTestResult(
            repeatedFingerprint,
            client.FindCalls.Count,
            client.CreatedIssues.Count);
    }

    /// <summary>Builds before/after gameplay snapshots and checks that the reported failure payload preserves both states and replay context.</summary>
    /// <returns>Formatted diagnostics for the captured frame failure.</returns>
    private static string VerifyFrameFailureSnapshot()
    {
        var bus = SuperMetroidAddressSpace.CreateWithoutCartridge();
        var colors = GameplayBasePaletteCatalog.Load(new MemoryStream(GameplayBasePaletteCatalog.Write(
            new GameplayBasePaletteDocument(GameplayBasePaletteFormat.Version,
                Enumerable.Range(0, SnesCgram.ColorCount).Select(_ => new PaletteRgb5 { Red = 0, Green = 0, Blue = 0 }).ToArray(),
                Enumerable.Range(0, GameplayBasePaletteFormat.SpriteColorCount).Select(_ => new PaletteRgb5 { Red = 0, Green = 0, Blue = 0 }).ToArray()))));
        var runtime = new SuperMetroidRuntime(bus, initialPaletteArt: colors);
        var samus = new SamusState { Pose = SamusPoseIds.MovingRightNormalPose, XPosition = 100, YPosition = 200 };
        typeof(SuperMetroidRuntime).GetProperty(nameof(runtime.Samus))!.SetValue(runtime, samus);
        typeof(SamusDrainedState).GetProperty(nameof(samus.Drained.GetUpHandler))!
            .SetValue(samus.Drained, DrainedGetUpHandler.UnableToStand);
        samus.SetAnimationFrameFromSpecialHandler(8, 1);
        samus.LiquidPhysics.FxType = RoomFxType.Acid;
        samus.LiquidPhysics.LavaAcidYPosition = 100;
        var game = new SuperMetroidGame(bus);
        typeof(SuperMetroidGame).GetField("runtime", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(game, runtime);
        string before = game.CaptureGameplayFailureContext();
        samus.SetAnimationFrameFromSpecialHandler(18, 1);
        string after = game.CaptureGameplayFailureContext();
        Require(before.Contains("animationFrame=8;") && !before.Contains("animationFrame=18;"),
            "Before-frame snapshot changed with mutable Samus state.");
        Require(after.Contains("animationFrame=18;") && after.Contains("animationTimer=1;") &&
            after.Contains("drainedHandler=UnableToStand;") && after.Contains("pose=$09;") && after.Contains("fx=Acid;"),
            "Failed-frame snapshot omitted the reported invalid animation and its installed handler.");
        string diagnostics = GameplayFrameFailureDiagnostics.Format(before, after, 0x0880, "fixture.smrec", 70000);
        Require(diagnostics.Contains("Build:") && diagnostics.Contains("Core build ID:") &&
            diagnostics.Contains("Host build ID:") && diagnostics.Contains("Attempted input: $0880") &&
            diagnostics.Contains("zero-based): 70000") && diagnostics.Contains("fixture.smrec") &&
            diagnostics.Contains("Before frame:") && diagnostics.Contains("may be partially mutated"),
            "Failure diagnostic omitted build/replay provenance or snapshot ordering.");
        return diagnostics;
    }

    /// <summary>Checks that repeated audio failures are reported once and do not prevent later frame attempts.</summary>
    private static void VerifyAudioRecovery()
    {
        var recovery = new AudioFrameRecovery();
        var client = new RecordingIssueClient();
        using var reporter = new GitHubErrorReporter("owner/private-repository", client);
        var context = new GitHubErrorContext("audio rendering/submission after completed gameplay frame");
        int completedFrames = 0;
        for (int frame = 0; frame < 3; frame++)
        {
            recovery.Run(() => throw new InvalidDataException("fixture missing PCM source"),
                error => reporter.Report(error, context));
            completedFrames++;
        }
        reporter.FlushAsync().GetAwaiter().GetResult();
        Require(completedFrames == 3 && client.CreatedIssues.Count == 1 && client.FindCalls.Count == 1,
            "Audio errors must preserve frame completion and deduplicate GitHub reports.");
        recovery.Run(() => throw new InvalidDataException("reporting disabled"), null);
        recovery.Run(() => throw new InvalidDataException("transport failure"),
            _ => throw new InvalidOperationException("fixture reporter failure"));
        bool resumed = false;
        recovery.Run(() => resumed = true, null);
        Require(resumed, "Audio must resume attempts after a failed frame or report.");
        Console.WriteLine("PASS audio recovery: repeated failure, reporting disabled, failed reporter, deduplication and subsequent successful frame.");
    }

    /// <summary>Captures a deterministic invalid-data exception with the supplied fixture message.</summary>
    /// <param name="message">Diagnostic text stored in the exception.</param>
    /// <returns>The thrown exception instance, including its captured stack trace.</returns>
    private static Exception CaptureFixtureException(string message)
    {
        try
        {
            throw new InvalidDataException(message);
        }
        catch (Exception exception)
        {
            return exception;
        }
    }

    /// <summary>Fails the smoke test when a required reporter invariant is false.</summary>
    /// <param name="condition">Invariant that must hold.</param>
    /// <param name="message">Failure description used by the thrown exception.</param>
    private static void Require(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException(message);
    }

    /// <summary>In-memory issue client that records reporter operations without contacting GitHub.</summary>
    private sealed class RecordingIssueClient : IGitHubIssueClient
    {
        /// <summary>Comment bodies submitted for existing issues.</summary>
        public List<string> Comments { get; } = [];
        /// <summary>Records an issue-comment request and completes without external I/O.</summary>
        /// <param name="repository">Repository identifier supplied by the reporter.</param>
        /// <param name="issueUrl">Existing issue receiving the comment.</param>
        /// <param name="body">Comment text to record.</param>
        /// <returns>A completed task after recording the request.</returns>
        public Task CommentAsync(string repository, string issueUrl, string body)
        {
            Comments.Add(body);
            return Task.CompletedTask;
        }
        /// <summary>Fingerprints treated as already present in the remote issue tracker.</summary>
        public HashSet<string> ExistingFingerprints { get; } = new(StringComparer.Ordinal);

        /// <summary>Fingerprint lookups requested by the reporter in call order.</summary>
        public List<string> FindCalls { get; } = [];

        /// <summary>Issue title and body pairs created by the reporter.</summary>
        public List<CreatedIssue> CreatedIssues { get; } = [];

        /// <summary>Records a fingerprint lookup and returns a fixture issue URL only for configured existing fingerprints.</summary>
        /// <param name="repository">Repository identifier supplied by the reporter.</param>
        /// <param name="fingerprint">Stable identifier to look up.</param>
        /// <returns>The fake issue URL when the fingerprint is configured as existing; otherwise <see langword="null"/>.</returns>
        public Task<string?> FindByFingerprintAsync(string repository, string fingerprint)
        {
            FindCalls.Add(fingerprint);
            string? result = ExistingFingerprints.Contains(fingerprint)
                ? $"https://github.example/{repository}/issues/1"
                : null;
            return Task.FromResult(result);
        }

        /// <summary>Records an issue creation request and returns a deterministic fixture URL.</summary>
        /// <param name="repository">Repository identifier supplied by the reporter.</param>
        /// <param name="title">Issue title to record.</param>
        /// <param name="body">Issue body to record.</param>
        /// <returns>A completed task containing the fake issue URL.</returns>
        public Task<string> CreateAsync(string repository, string title, string body)
        {
            CreatedIssues.Add(new CreatedIssue(title, body));
            return Task.FromResult($"https://github.example/{repository}/issues/2");
        }
    }

    /// <summary>Issue content captured by the fake client when the reporter creates a new issue.</summary>
    /// <param name="Title">Title supplied in the create request.</param>
    /// <param name="Body">Body supplied in the create request.</param>
    private sealed record CreatedIssue(string Title, string Body);
}

/// <summary>Fingerprint and fake-client operation counts after the reporter queue has been drained.</summary>
/// <param name="Fingerprint">Stable identifier of the repeated fixture exception.</param>
/// <param name="RemoteLookups">Recorded fingerprint lookups across distinct queued occurrences, not actual network requests.</param>
/// <param name="IssuesCreated">Issue creations recorded by the fake client after deduplication.</param>
public readonly record struct GitHubErrorReporterSmokeTestResult(
    string Fingerprint,
    int RemoteLookups,
    int IssuesCreated);
