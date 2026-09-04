using System.Diagnostics;
using System.Text.Json;

namespace SuperMetroid.Desktop;

/// <summary>Minimal authenticated GitHub issue client backed by the installed <c>gh</c> CLI.</summary>
internal sealed class GhCliGitHubIssueClient : IGitHubIssueClient
{
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
        return new ProcessResult(output, error);
    }

    private sealed record ProcessResult(string StandardOutput, string StandardError);
}
