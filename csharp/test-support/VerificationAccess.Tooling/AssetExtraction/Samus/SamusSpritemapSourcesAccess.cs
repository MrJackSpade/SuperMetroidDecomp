using System.Reflection;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Verification access to the whole per-pose base tables of <see cref="SamusSpritemapSources"/>, which only checks read (#1595).</summary>
internal static class SamusSpritemapSourcesAccess
{
    /// <summary>Reads each pose's first upper-half and lower-half table entries.</summary>
    extension(SamusSpritemapSources self)
    {
        /// <summary>Each pose with a record's first upper-half table entry.</summary>
        internal IReadOnlyDictionary<SamusPoseId, int> TopBases => Bases(self, "topBases");

        /// <summary>Each pose with a record's first lower-half table entry, including poses sharing the upper/lower composition.</summary>
        internal IReadOnlyDictionary<SamusPoseId, int> BottomBases => Bases(self, "bottomBases");
    }

    /// <summary>The sources' per-pose base table held in <paramref name="field"/>.</summary>
    private static Dictionary<SamusPoseId, int> Bases(SamusSpritemapSources sources, string field) =>
        typeof(SamusSpritemapSources).GetField(field, BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(sources) as Dictionary<SamusPoseId, int>
            ?? throw new InvalidOperationException($"SamusSpritemapSources no longer keeps a per-pose base table in {field}.");
}
