using SuperMetroid.Core.Game;

/// <summary>Verification view of private palette-FX slot occupancy.</summary>
internal static class PaletteFxAccess
{
    extension(RoomPaletteFxSystem fx)
    {
        /// <summary>Gets the number of active native palette-FX slots.</summary>
        internal int ActiveCount => PrivateState.Field<Array>(fx, "slots").Cast<object>()
            .Count(slot => PrivateState.Property<ushort>(slot, "Id") != 0);
    }
}
