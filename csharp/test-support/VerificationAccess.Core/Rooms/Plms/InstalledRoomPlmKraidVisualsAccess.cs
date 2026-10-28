namespace SuperMetroid.Core.Rooms.Plms;
/// <summary>Verification access to <see cref="InstalledRoomPlmKraidVisuals"/> members production does not use.</summary>
internal static class InstalledRoomPlmKraidVisualsAccess
{
    /// <summary>Builds the Kraid room-art fixture from its explicit compiled visual parts.</summary>
    extension(InstalledRoomPlmKraidVisuals)
    {
        /// <summary>Builds stock Kraid PLM art from the visual bits of its physical draw words.</summary>
        internal static InstalledRoomPlmKraidVisuals Stock() => new(
            KraidRoomPlmDrawDefinitions.All.Select(draw =>
                new RoomPlmVisualEntry(
                    KraidRoomPlmDraws.VisualId(draw.Draw),
                    draw.VisualWords())));
    }
}
