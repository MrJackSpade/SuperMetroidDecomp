using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rooms;

/// <summary>One fourteen-byte entry consumed by <c>LoadFromLoadStation</c> at $80:C437.</summary>
/// <param name="RoomPointer">Pointer to the room header that the station loads.</param>
/// <param name="DoorPointer">Pointer to the destination door entry used for the room arrival.</param>
/// <param name="CameraX">Horizontal camera position recorded for the station destination.</param>
/// <param name="CameraY">Vertical camera position recorded for the station destination.</param>
/// <param name="SamusYOffset">Vertical offset from <paramref name="CameraY"/> used to place Samus.</param>
/// <param name="SamusXOffset">Horizontal offset added after the camera's 128-pixel center adjustment to place Samus.</param>
public sealed record LoadStationEntry(
    ushort RoomPointer,
    ushort DoorPointer,
    ushort CameraX,
    ushort CameraY,
    ushort SamusYOffset,
    ushort SamusXOffset)
{
    /// <summary>World X assigned by the native load-station routine.</summary>
    public ushort SamusX => unchecked((ushort)(CameraX + 128 + SamusXOffset));

    /// <summary>World Y assigned by the native load-station routine.</summary>
    public ushort SamusY => unchecked((ushort)(CameraY + SamusYOffset));

}
