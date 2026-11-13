using SuperMetroid.AssetExtraction.Lists;

namespace SuperMetroid.Verification.Lists;

/// <summary>The strided lists the checks compare.</summary>
internal static class ListChecks
{
    /// <summary>The entry the third item of a list of stride two is at.</summary>
    internal static int Third => new StridedList(2).Third;
}
