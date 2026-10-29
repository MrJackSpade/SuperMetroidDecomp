using SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;

namespace SuperMetroid.Core.Frontend.Pause.Artwork;

/// <summary>Verification access to <see cref="PauseReserveTankPresentation"/> members production does not use.</summary>
internal static class PauseReserveTankPresentationAccess
{
    /// <summary>Counts non-null compositions among <paramref name="frames"/>' ten reserve-tank visual phases.</summary>
    private static int StoredCount(PauseReserveTankSourcePresentation.FrameSet frames) =>
        (frames.Full is null ? 0 : 1) + (frames.EndCap is null ? 0 : 1) + (frames.Empty is null ? 0 : 1) + frames.Fills.Count(fill => fill is not null);

    /// <summary>Inspects which reserve-tank frames and anchor components are actually stored.</summary>
    extension(PauseReserveTankPresentation self)
    {
        /// <summary>Counts non-null compositions among the ten reserve-tank visual phases.</summary>
        internal int StoredFrameCount => StoredCount(PauseReserveTankSourcePresentation.Of(self).frames);

        /// <summary>Counts separately supplied X and Y coordinates across reserve-tank anchors.</summary>
        internal int StoredAnchorComponentCount => self.anchorOverrides.Values.Sum(value => (value.X.HasValue ? 1 : 0) + (value.Y.HasValue ? 1 : 0));
    }
}
