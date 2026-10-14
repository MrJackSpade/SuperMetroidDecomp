namespace SuperMetroid.Desktop;

/// <summary>Remote issue operations used to deduplicate and publish captured desktop failures.</summary>
internal interface IGitHubIssueClient
{
    /// <summary>Adds another occurrence to the issue already owning this fingerprint.</summary>
    Task CommentAsync(string repository, string issueUrl, string body);
    /// <summary>Finds a published issue by its stable auto-error marker.</summary>
    Task<string?> FindByFingerprintAsync(string repository, string fingerprint);

    /// <summary>Creates a new issue for a previously unseen fingerprint.</summary>
    Task<string> CreateAsync(string repository, string title, string body);
}
