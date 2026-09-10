using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>One authored exposed missile PLM; every item definition/instruction remains retail.</summary>
internal sealed class RemoteItemPopulation(ISnesAddressSpace inner, byte column, byte row = 8) : ISnesAddressSpace
{
    public byte ReadByte(int address)
    {
        int offset = address - RemoteItemFixtureData.PopulationAddress;
        ReadOnlySpan<byte> population = [(byte)(RoomPlmHeaders.ExposedMissileTank & 255),
            (byte)(RoomPlmHeaders.ExposedMissileTank >> 8), column, row, 0, 0, 0, 0];
        return (uint)offset < population.Length ? population[offset] : inner.ReadByte(address);
    }
    public void WriteByte(int address, byte value) => inner.WriteByte(address, value);
}

/// <summary>Synthetic list identity; not a replacement for item definition ROM data.</summary>
internal static class RemoteItemFixtureData
{
    /// <summary>Bank-$8F scratch overlay containing one six-byte PLM record and a terminator.</summary>
    public const ushort PopulationPointer = 0xf000;
    /// <summary>Fixed-bank address of the authored population overlay.</summary>
    public const int PopulationAddress = 0x8f0000 | PopulationPointer;
}
