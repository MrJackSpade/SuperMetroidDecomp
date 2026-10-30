namespace SuperMetroid.Core.Rooms.Plms;
/// <summary>Verification access to <see cref="InstalledRoomPlmKraidVisuals"/> members production does not use.</summary>
internal static class InstalledRoomPlmKraidVisualsAccess
{
    /// <summary>Builds the Kraid room-art fixture from its explicit compiled visual parts.</summary>
    extension(InstalledRoomPlmKraidVisuals)
    {
        /// <summary>Builds stock Kraid PLM art from the appearances of its draws.</summary>
        internal static InstalledRoomPlmKraidVisuals Stock() => new(
            KraidRoomPlmDraws.All.Select(draw =>
                new RoomPlmVisualEntry(
                    KraidRoomPlmDraws.VisualId(draw.Draw),
                    draw.Appearances())));
    }
}
