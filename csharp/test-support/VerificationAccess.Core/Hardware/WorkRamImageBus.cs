namespace SuperMetroid.Core.Hardware;

/// <summary>The runtime's WRAM image as a test bus, for primitives that read and write cells through one.</summary>
/// <param name="image">The image read and written.</param>
[CpuBusBoundary("exact bus test: WRAM cell primitives over the runtime image")]
internal sealed class WorkRamImageBus(SuperMetroidAddressSpace image) : ISnesMemoryBus
{
    /// <inheritdoc />
    public byte ReadWorkRamByte(int cpuAddress) => image.ReadWorkRamByte(cpuAddress);

    /// <inheritdoc />
    public void WriteByte(int address, byte value) => image.WriteByte(address, value);
}
