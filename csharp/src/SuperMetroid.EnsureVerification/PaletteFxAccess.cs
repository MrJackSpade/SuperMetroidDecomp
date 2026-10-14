using SuperMetroid.Core.Fx;

/// <summary>Verification view of palette-FX slot occupancy.</summary>
internal static class PaletteFxAccess
{
    /// <summary>Reads palette-FX slot occupancy from the production system during verification.</summary>
    extension(RoomPaletteFxSystem fx)
    {
        /// <summary>Counts slots whose private native effect ID is nonzero.</summary>
        internal int ActiveCount => fx.slots.Count(slot => slot.Id != 0);
    }
}
