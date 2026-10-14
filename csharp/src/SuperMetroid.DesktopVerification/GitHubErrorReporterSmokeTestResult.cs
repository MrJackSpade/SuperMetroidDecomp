namespace SuperMetroid.Desktop;

/// <summary>Fingerprint and fake-client operation counts after the reporter queue has been drained.</summary>
/// <param name="Fingerprint">Stable identifier of the repeated fixture exception.</param>
/// <param name="RemoteLookups">Recorded fingerprint lookups across distinct queued occurrences, not actual network requests.</param>
/// <param name="IssuesCreated">Issue creations recorded by the fake client after deduplication.</param>
public readonly record struct GitHubErrorReporterSmokeTestResult(
    string Fingerprint,
    int RemoteLookups,
    int IssuesCreated);
