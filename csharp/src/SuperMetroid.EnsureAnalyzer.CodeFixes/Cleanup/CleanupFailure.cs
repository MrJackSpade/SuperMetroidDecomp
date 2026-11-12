using Microsoft.CodeAnalysis;

namespace SuperMetroid.EnsureAnalyzer.CodeFixes.Cleanup;

/// <summary>The failure of a cleanup fix that cannot resolve a finding, naming the finding's site and the reason.</summary>
public sealed class CleanupFailure : InvalidOperationException
{
    /// <summary>Creates the failure for <paramref name="diagnostic"/>.</summary>
    public CleanupFailure(Diagnostic diagnostic, string reason)
        : base($"{Site(diagnostic.Location)}: {diagnostic.Id} cannot be fixed without a choice: {reason}")
    {
    }

    /// <summary>Creates the failure for a site that is not a finding's own location.</summary>
    public CleanupFailure(Location location, string id, string reason)
        : base($"{Site(location)}: {id} cannot be fixed without a choice: {reason}")
    {
    }

    /// <summary>A location as <c>path(line,column)</c>.</summary>
    internal static string Site(Location location)
    {
        FileLinePositionSpan span = location.GetLineSpan();
        return $"{span.Path}({span.StartLinePosition.Line + 1},{span.StartLinePosition.Character + 1})";
    }
}
