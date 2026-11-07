using SuperMetroid.AssetExtraction.Rooms;

namespace SuperMetroid.Core.Rooms.RoomObjects;
/// <summary>Verification access to <see cref="InstalledRoomObjectKraidVisuals"/> members production does not use.</summary>
internal static class InstalledRoomObjectKraidVisualsAccess
{
    /// <summary>Builds the Kraid room-art fixture from its explicit compiled visual parts.</summary>
    extension(InstalledRoomObjectKraidVisuals)
    {
        /// <summary>Builds stock Kraid PLM art from the appearances of its draws.</summary>
        internal static InstalledRoomObjectKraidVisuals Stock() => RoomObjectKraidVisualFiles.Create(
            RoomObjectKraidVisualFiles.All.Select(draw =>
                new RoomObjectVisualEntry(
                    RoomObjectKraidVisualFiles.VisualId(draw.Draw),
                    draw.Appearances())));
    }
}
