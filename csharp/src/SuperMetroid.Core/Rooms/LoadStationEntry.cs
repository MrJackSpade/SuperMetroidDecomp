using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rooms;

/// <summary>One fourteen-byte entry consumed by <c>LoadFromLoadStation</c> at $80:C437.</summary>
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
