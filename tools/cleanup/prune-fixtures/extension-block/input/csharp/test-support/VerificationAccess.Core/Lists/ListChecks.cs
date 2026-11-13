using SuperMetroid.AssetExtraction.Lists;

namespace SuperMetroid.Verification.Lists;

/// <summary>The identities the checks compare.</summary>
internal static class ListChecks
{
    /// <summary>The checked layout.</summary>
    private static readonly ListLayout layout = new([4, 12]);

    /// <summary>The identity of the checked layout's second list.</summary>
    internal static int EntryIdentity => new ListEntry(layout, 1).Identity;
}
