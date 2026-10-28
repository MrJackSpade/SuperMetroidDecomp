
namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The retail scroll programs: ordered room-scroll writes by program.</summary>
internal static partial class RoomPlmScrollProgramDefinitions
{
    /// <summary>
    /// The smallest byte that ends a scroll program: the program reads a byte as signed and stops at a negative one,
    /// so every byte from this one upward ends it and every smaller byte is a scroll index.
    /// </summary>
    internal const byte FirstEndMarker = 0x80;
}
