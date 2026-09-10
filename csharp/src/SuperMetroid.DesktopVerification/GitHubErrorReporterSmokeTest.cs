namespace SuperMetroid.Desktop;

/// <summary>Deterministic audit of stable fingerprints and both deduplication layers.</summary>
public static class GitHubErrorReporterSmokeTest
{
    public static GitHubErrorReporterSmokeTestResult Run()
    {
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
                InputRecordingPath: "fixture.smrec");
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

        return new GitHubErrorReporterSmokeTestResult(
            repeatedFingerprint,
            client.FindCalls.Count,
            client.CreatedIssues.Count);
    }

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

    private static void Require(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException(message);
    }

    private sealed class RecordingIssueClient : IGitHubIssueClient
    {
        public List<string> Comments { get; } = [];
        public Task CommentAsync(string repository, string issueUrl, string body)
        {
            Comments.Add(body);
            return Task.CompletedTask;
        }
        public HashSet<string> ExistingFingerprints { get; } = new(StringComparer.Ordinal);

        public List<string> FindCalls { get; } = [];

        public List<CreatedIssue> CreatedIssues { get; } = [];

        public Task<string?> FindByFingerprintAsync(string repository, string fingerprint)
        {
            FindCalls.Add(fingerprint);
            string? result = ExistingFingerprints.Contains(fingerprint)
                ? $"https://github.example/{repository}/issues/1"
                : null;
            return Task.FromResult(result);
        }

        public Task<string> CreateAsync(string repository, string title, string body)
        {
            CreatedIssues.Add(new CreatedIssue(title, body));
            return Task.FromResult($"https://github.example/{repository}/issues/2");
        }
    }

    private sealed record CreatedIssue(string Title, string Body);
}

public readonly record struct GitHubErrorReporterSmokeTestResult(
    string Fingerprint,
    int RemoteLookups,
    int IssuesCreated);
