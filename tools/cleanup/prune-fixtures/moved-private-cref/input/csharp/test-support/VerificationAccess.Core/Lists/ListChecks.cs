using SuperMetroid.AssetExtraction.Lists;

namespace SuperMetroid.Verification.Lists;

/// <summary>The gauge readings the checks compare.</summary>
internal static class ListChecks
{
    /// <summary>A gauge's level before scaling.</summary>
    internal static int Raw => new ListGauge(3).Raw;
}
