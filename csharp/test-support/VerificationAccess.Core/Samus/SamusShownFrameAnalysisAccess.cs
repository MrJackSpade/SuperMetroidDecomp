using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Verification access to <see cref="SamusShownFrameAnalysis"/> members production does not use.</summary>
internal static class SamusShownFrameAnalysisAccess
{
    /// <summary>Exposes <see cref="SamusShownFrameAnalysis"/> state that only verification reads.</summary>
    extension(SamusShownFrameAnalysis self)
    {
        /// <summary>The picture frames of <paramref name="pose"/>, by their places in its animation's layout, in order.</summary>
        internal IEnumerable<int> Pictures(SamusPoseId pose) => self.pictured.TryGetValue(pose, out SortedSet<int>? frames) ? frames : [];
    }
}
