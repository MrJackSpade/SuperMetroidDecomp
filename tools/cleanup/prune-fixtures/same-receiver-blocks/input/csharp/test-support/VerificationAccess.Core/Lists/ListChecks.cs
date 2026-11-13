using SuperMetroid.AssetExtraction.Lists;

namespace SuperMetroid.Verification.Lists;

/// <summary>The marks the checks compare.</summary>
internal static class ListChecks
{
    /// <summary>The checked layout's doubled and tripled marks.</summary>
    internal static int Marks => new ListLayout([4, 12]).DoubleMark + new ListLayout([4, 12]).TripleMark;
}
