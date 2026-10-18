using SuperMetroid.Core.Fx;

namespace SuperMetroid.EnsureVerification.PaletteFx;

/// <summary>Verification view of palette-FX slot occupancy.</summary>
internal static class PaletteFxAccess
{
    /// <summary>Reads palette-FX slot occupancy from the production system during verification.</summary>
    extension(RoomPaletteFxSystem fx)
    {
        /// <summary>Counts slots that run an object.</summary>
        internal int ActiveCount => fx.slots.Count(slot => slot.Object is not null);
    }
}
