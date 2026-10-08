namespace SuperMetroid.Core.Rooms;

/// <summary>Development-tool members of <see cref="RoomHeaderPointers"/>; never linked by player hosts.</summary>
internal static class RoomHeaderPointersTooling
{
    /// <summary>Crateria save station at $8F:93D5.</summary>
    public const ushort CrateriaSaveStation = 0x93d5;
    /// <summary><c>RoomHeader_XrayScope</c>: Brinstar room $22 at $8F:A2CE, with two bomb-activated shutters.</summary>
    public const ushort BrinstarShutterRoom = 0xa2ce;
    /// <summary><c>kRoom_a408</c>: Brinstar room $28 at $8F:A408, with a shallow-water floor below a low ceiling.</summary>
    public const ushort BrinstarShallowWaterRoom = 0xa408;
}
