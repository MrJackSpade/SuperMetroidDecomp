using SuperMetroid.Core.Game;

/// <summary>Verification view of private palette-FX slot occupancy.</summary>
internal static class PaletteFxAccess
{
    /// <summary>Provides extension members for <c>RoomPaletteFxSystem</c>.</summary>
    extension(RoomPaletteFxSystem fx)
    {
        /// <summary>Gets or sets active count.</summary>
        internal int ActiveCount => PrivateState.Field<Array>(fx, "slots").Cast<object>()
            .Count(slot => PrivateState.Property<ushort>(slot, "Id") != 0);
    }
}
