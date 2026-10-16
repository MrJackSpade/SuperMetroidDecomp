namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>Typed view of the room scroll-zone buffer in WRAM (#629).</summary>
[CpuBusBoundary("domain view: room scroll-zone WRAM")]
public readonly struct RoomScrollMemory(ISnesMemoryBus bus)
{
    /// <summary>The 50 scroll-zone bytes.</summary>
    public WorkRamByteTable Zones => new(bus, RoomScrollWorkRamAddresses.Zones);
}
