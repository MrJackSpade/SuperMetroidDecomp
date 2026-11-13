using SuperMetroid.AssetExtraction.Lists;

namespace SuperMetroid.Verification.Lists;

/// <summary>The bounds the checks compare.</summary>
internal static class ListChecks
{
    /// <summary>The first entry of a list from the second to the fifth.</summary>
    internal static int Start => new ListBounds(2, 5).Start;
}
