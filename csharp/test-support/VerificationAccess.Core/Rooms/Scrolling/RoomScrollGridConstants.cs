
namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>Cartridge values of <see cref="RoomScrollGrid"/> that only verification reads.</summary>
internal static class RoomScrollGridConstants
{
    /// <summary>24-bit WRAM address of the first room scroll-zone byte, $7E:CD20.</summary>
    public const int WorkRamAddress = 0x7ecd20;

    /// <summary>Exposes the literal scroll-grid WRAM address to verification callers of the domain type.</summary>
    extension(RoomScrollGrid)
    {
        /// <inheritdoc cref="RoomScrollGridConstants.WorkRamAddress"/>
        internal static int WorkRamAddress => RoomScrollGridConstants.WorkRamAddress;
    }
}
