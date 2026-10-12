using SuperMetroid.Core.Game;

/// <summary>Verification view of private palette-FX slot occupancy.</summary>
internal static class PaletteFxAccess
{
    /// <summary>Reads palette-FX slot occupancy from the production system during verification.</summary>
    extension(RoomPaletteFxSystem fx)
    {
        /// <summary>Counts slots whose private native effect ID is nonzero.</summary>
        internal int ActiveCount => PrivateState.Field<Array>(fx, "slots").Cast<object>()
            .Count(slot => PrivateState.Property<ushort>(slot, "Id") != 0);
    }
}
