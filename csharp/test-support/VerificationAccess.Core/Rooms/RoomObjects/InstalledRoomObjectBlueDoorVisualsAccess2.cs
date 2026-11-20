using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to <see cref="InstalledRoomObjectBlueDoorVisuals"/> members production does not use.</summary>
internal static class InstalledRoomObjectBlueDoorVisualsAccess2
{
    /// <summary>Exposes <see cref="InstalledRoomObjectBlueDoorVisuals"/> state that only verification reads.</summary>
    extension(InstalledRoomObjectBlueDoorVisuals self)
    {
        /// <summary>The picture <paramref name="draw"/> shows; the closed cap drawn solid shows its facing's closed cap.</summary>
        /// <exception cref="InvalidDataException">The installation lacks the frame.</exception>
        internal RoomObjectFrameImage Image(RoomObjectDraw draw) => self.Images.Image(BlueDoorCaps.VisualSource(draw));
    }
}
