namespace SuperMetroid.Core.Game;

/// <summary>Typed view of the room scroll-zone buffer in WRAM (#629).</summary>
[CpuBusBoundary("domain view: room scroll-zone WRAM")]
public readonly struct RoomScrollMemory(ISnesMemoryBus bus)
{
    /// <summary>The 50 scroll-zone bytes.</summary>
    public WorkRamByteTable Zones => new(bus, RoomScrollWorkRam.Zones);

    /// <summary>
    /// The byte a bank-$80 scroll calculation reads at <paramref name="index"/> from the first
    /// scroll-zone byte with an absolute-indexed load. Past the 50-byte allocation it is whatever
    /// adjacent WRAM holds (index $32 is the first explored-map byte at <c>$7E:CD52</c>), and the
    /// index carries linearly into the following bank as the 65C816 does.
    /// </summary>
    public byte ReadIndexed(int index)
    {
        if ((uint)index > 0xffff)
            throw new ArgumentOutOfRangeException(nameof(index), index, "A 16-bit index register cannot select this byte.");
        return bus.ReadWorkRamByte(RoomScrollWorkRam.Zones.Address + index);
    }
}
