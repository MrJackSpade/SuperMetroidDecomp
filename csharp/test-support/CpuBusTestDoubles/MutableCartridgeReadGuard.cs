using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.TestSupport.CpuBusTestDoubles;

/// <summary>
/// A <see cref="CartridgeReadGuard"/> whose read rules also apply to WRAM/SRAM reads (#629): a WRAM/SRAM
/// read no rule rejects is served by the source's mutable memory.
/// </summary>
internal sealed class MutableCartridgeReadGuard(ISnesAddressSpace source) : CartridgeReadGuard(source)
{
    /// <inheritdoc />
    public override byte ReadWorkRamByte(int address)
    {
        Check(address);
        return base.ReadWorkRamByte(address);
    }
}
