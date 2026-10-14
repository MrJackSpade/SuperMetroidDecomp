using SuperMetroid.Core;
using SuperMetroid.Core.Assets;

/// <summary>Verification access to <see cref="PauseReserveTankPresentation"/> members production does not use.</summary>
internal static class PauseReserveTankPresentationAccess
{
    /// <summary>Inspects which reserve-tank frames and anchor components are actually stored.</summary>
    extension(PauseReserveTankPresentation self)
    {
        /// <summary>Counts non-null compositions among the ten reserve-tank visual phases.</summary>
        internal int StoredFrameCount => (self.frames.Full is null ? 0 : 1) + (self.frames.EndCap is null ? 0 : 1) + (self.frames.Empty is null ? 0 : 1) + (self.frames.Fill1 is null ? 0 : 1) + (self.frames.Fill2 is null ? 0 : 1) + (self.frames.Fill3 is null ? 0 : 1) + (self.frames.Fill4 is null ? 0 : 1) + (self.frames.Fill5 is null ? 0 : 1) + (self.frames.Fill6 is null ? 0 : 1) + (self.frames.Fill7 is null ? 0 : 1);

        /// <summary>Counts separately supplied X and Y coordinates across reserve-tank anchors.</summary>
        internal int StoredAnchorComponentCount => self.anchorOverrides.Values.Sum(value => (value.X.HasValue ? 1 : 0) + (value.Y.HasValue ? 1 : 0));
    }
}
