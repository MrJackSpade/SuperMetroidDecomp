using SuperMetroid.AssetExtraction.Lists;

namespace SuperMetroid.Verification.Lists;

/// <summary>The names the checks compare.</summary>
internal static class ListChecks
{
    /// <summary>The checked layout.</summary>
    private static readonly ListLayout layout = new([4, 12]);

    /// <summary>The checked layout's name and its length.</summary>
    internal static string Name => $"{layout.Name} ({layout.NameLength})";
}
