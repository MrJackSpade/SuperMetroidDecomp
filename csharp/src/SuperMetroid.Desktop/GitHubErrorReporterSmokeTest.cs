namespace SuperMetroid.Desktop;

/// <summary>Deterministic audit of stable fingerprints and both deduplication layers.</summary>
public static class GitHubErrorReporterSmokeTest
{
    public static GitHubErrorReporterSmokeTestResult Run()
    {
        var client = new RecordingIssueClient();
        Exception repeated = CaptureFixtureException("door callback $8F:B9A2 is untranslated");
        Exception existing = CaptureFixtureException("PLM instruction $84:BA6F is untranslated");
        string existingFingerprint = GitHubErrorReporter.CreateFingerprint(existing);
        client.ExistingFingerprints.Add(existingFingerprint);

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
            repeatedFingerprint = reporter.Report(repeated, context);
            duplicateFingerprint = reporter.Report(repeated, context);
            _ = reporter.Report(existing, context);
            reporter.FlushAsync().GetAwaiter().GetResult();
        }

        Require(repeatedFingerprint == duplicateFingerprint,
            "Repeated exception did not retain one stable fingerprint.");
        Require(client.FindCalls.Count == 2,
            $"Expected two distinct remote lookups, got {client.FindCalls.Count}.");
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
