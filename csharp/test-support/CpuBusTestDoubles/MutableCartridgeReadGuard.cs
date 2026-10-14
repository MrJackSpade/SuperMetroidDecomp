using SuperMetroid.Core.Hardware;

/// <summary>
/// A <see cref="CartridgeReadGuard"/> whose read rules also apply to WRAM/SRAM reads (#629): a WRAM/SRAM
/// read no rule rejects is served by the source's mutable memory.
/// </summary>
[CpuBusBoundary("exact bus test: guard double over a mutable bus that forbids, observes and counts reads")]
internal sealed class MutableCartridgeReadGuard(ISnesAddressSpace source) : CartridgeReadGuard(source)
{
    /// <inheritdoc />
    public override byte ReadWorkRamByte(int address)
    {
        Check(address);
        return base.ReadWorkRamByte(address);
    }

    /// <inheritdoc />
    public override byte ReadSaveRamByte(int address)
    {
        Check(address);
        return base.ReadSaveRamByte(address);
    }
}
