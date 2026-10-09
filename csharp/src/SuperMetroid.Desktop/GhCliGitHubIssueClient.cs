using System.Diagnostics;
using System.Text.Json;

namespace SuperMetroid.Desktop;

/// <summary>Minimal authenticated GitHub issue client backed by the installed <c>gh</c> CLI.</summary>
internal sealed class GhCliGitHubIssueClient : IGitHubIssueClient
{
    /// <summary>Posts a comment to the specified issue using the GitHub CLI.</summary>
    /// <param name="repository">GitHub repository in owner/name form.</param>
    /// <param name="issueUrl">URL of the issue that will receive the comment.</param>
    /// <param name="body">Comment text supplied to the CLI on standard input.</param>
    public async Task CommentAsync(string repository, string issueUrl, string body)
    {
        await RunAsync(body, "issue", "comment", issueUrl, "--repo", repository,
            "--body-file", "-").ConfigureAwait(false);
    }
    /// <summary>Searches all issues in a repository for a title containing the supplied fingerprint.</summary>
    /// <param name="repository">GitHub repository in owner/name form.</param>
    /// <param name="fingerprint">Text to locate in an issue title.</param>
    /// <returns>The URL of the first matching issue, or <see langword="null"/> when no issue matches.</returns>
    public async Task<string?> FindByFingerprintAsync(string repository, string fingerprint)
    {
        ProcessResult result = await RunAsync(
            standardInput: null,
            "issue", "list",
            "--repo", repository,
            "--state", "all",
            "--search", $"{fingerprint} in:title",
            "--limit", "1",
            "--json", "url").ConfigureAwait(false);

        using JsonDocument document = JsonDocument.Parse(result.StandardOutput);
        JsonElement issues = document.RootElement;
        if (issues.GetArrayLength() == 0)
            return null;
        return issues[0].GetProperty("url").GetString()
            ?? throw new InvalidDataException("GitHub returned an issue without a URL.");
    }

    /// <summary>Creates an issue in the repository and returns the URL reported by the CLI.</summary>
    /// <param name="repository">GitHub repository in owner/name form.</param>
    /// <param name="title">Title for the new issue.</param>
    /// <param name="body">Issue body supplied to the CLI on standard input.</param>
    /// <returns>The absolute URL of the created issue.</returns>
    /// <exception cref="InvalidDataException">The CLI output is not an absolute issue URL.</exception>
    public async Task<string> CreateAsync(string repository, string title, string body)
    {
        ProcessResult result = await RunAsync(
            body,
            "issue", "create",
            "--repo", repository,
            "--title", title,
            "--body-file", "-").ConfigureAwait(false);
        string url = result.StandardOutput.Trim();
        if (!Uri.TryCreate(url, UriKind.Absolute, out _))
            throw new InvalidDataException($"GitHub issue creation returned no URL: {url}");
        return url;
    }

    /// <summary>Runs a <c>gh</c> command with argument-list escaping and optional standard input.</summary>
    /// <param name="standardInput">Text to pipe to the command, or <see langword="null"/> when no input is needed.</param>
    /// <param name="arguments">Arguments passed to the GitHub CLI after the executable name.</param>
    /// <returns>Captured standard output from the completed command.</returns>
    /// <exception cref="InvalidOperationException">The CLI cannot start or exits with a nonzero status.</exception>
    private static async Task<ProcessResult> RunAsync(
        string? standardInput,
        params string[] arguments)
    {
        var startInfo = new ProcessStartInfo("gh")
        {
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardInput = standardInput is not null,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WindowStyle = ProcessWindowStyle.Hidden,
        };
        foreach (string argument in arguments)
            startInfo.ArgumentList.Add(argument);

        using Process process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Failed to start the GitHub CLI.");
        Task<string> outputTask = process.StandardOutput.ReadToEndAsync();
        Task<string> errorTask = process.StandardError.ReadToEndAsync();
        if (standardInput is not null)
        {
            await process.StandardInput.WriteAsync(standardInput).ConfigureAwait(false);
            process.StandardInput.Close();
        }
        await process.WaitForExitAsync().ConfigureAwait(false);
        string output = await outputTask.ConfigureAwait(false);
        string error = await errorTask.ConfigureAwait(false);
        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"GitHub CLI exited with code {process.ExitCode}: {error.Trim()}");
        }
        return new ProcessResult(output);
    }

    /// <summary>Captures the text emitted on standard output by a completed GitHub CLI command.</summary>
    /// <param name="StandardOutput">Unmodified standard-output text returned by the process.</param>
    private sealed record ProcessResult(string StandardOutput);
}
