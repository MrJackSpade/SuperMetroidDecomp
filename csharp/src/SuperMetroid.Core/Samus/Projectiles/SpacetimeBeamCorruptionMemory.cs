using SuperMetroid.Core.Saves;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The live memory the malformed <c>$90:AD16</c> callback copies through when it falls back
/// into LoadBeamPalette's loop at <c>$90:AD12</c> (#629).
/// </summary>
[CpuBusBoundary("domain view: SpaceTime Beam corrupted palette copy")]
internal readonly struct SpacetimeBeamCorruptionMemory(ISnesMemoryBus bus)
{
    /// <summary>
    /// Copies one word from <c>[$00],Y</c> (Y = <paramref name="sourceIndex"/>) to <c>$7E:C1C0,X</c> (X = <paramref name="destinationIndex"/>) with the native 24-bit address
    /// wrap: low byte read then written, then high byte read then written.
    /// </summary>
    /// <returns>True when the written word touched a translated persistent-progression or saved-entry byte.</returns>
    internal bool CopyPaletteWord(ushort destinationIndex, ushort sourceIndex)
    {
        var pointer = new WramBytes(bus, SpacetimeBeamCorruptionLayout.SourceLongPointer);
        int sourcePointer = SnesLittleEndian.Long(pointer.Read(0), pointer.Read(1), pointer.Read(2));
        int sourceAddress = (sourcePointer + sourceIndex) & SnesCpuAddressLayout.AddressMask;
        int destinationAddress = (SpacetimeBeamCorruptionLayout.SpritePaletteSix.Address + destinationIndex) & SnesCpuAddressLayout.AddressMask;

        bus.WriteByte(destinationAddress, ReadSourceByte(sourceAddress));
        bus.WriteByte(
            (destinationAddress + 1) & SnesCpuAddressLayout.AddressMask,
            ReadSourceByte((sourceAddress + 1) & SnesCpuAddressLayout.AddressMask));
        return SaveMirrorMemory.Overlaps(destinationAddress, destinationAddress + 1);
    }

    /// <summary>Reads live mapped data first, then the compiled cartridge byte for the malformed palette copy.</summary>
    private byte ReadSourceByte(int address) =>
        SnesLiveDataRead.TryRead(bus, address, "SpaceTime source", out byte value)
            ? value
            : SpacetimeBeamCopyDefinitions.ReadCartridgeByte(address);
}
