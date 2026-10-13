using SuperMetroid.Core.Game;

/// <summary>Verification view of palette-FX slot occupancy.</summary>
internal static class PaletteFxAccess
{
    extension(RoomPaletteFxSystem fx)
    {
        internal int ActiveCount => fx.slots.Count(slot => slot.Id != 0);
    }
}
