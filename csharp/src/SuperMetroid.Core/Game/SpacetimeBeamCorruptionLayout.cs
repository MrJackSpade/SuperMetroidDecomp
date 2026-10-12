using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>Addresses and loop bounds reached by the malformed SpaceTime Beam callback (#629).</summary>
[CpuBusBoundary("catalog: SpaceTime Beam corrupted palette copy")]
public static class SpacetimeBeamCorruptionLayout
{
    /// <summary>
    /// Direct-page long pointer $00-$02 consumed by <c>LDA [$00],Y</c>. Gameplay uses direct
    /// page zero, so these CPU-bus addresses are the low-WRAM mirror of <c>$7E:0000-$7E:0002</c>.
    /// </summary>
    public static readonly CpuBusCell SourceLongPointer = CpuBusCell.WramBytes(nameof(SourceLongPointer), 0x000000, 3,
        "24-bit long pointer (u8[3] little-endian)");

    /// <summary>
    /// $7E:C1C0, the sprite-palette-six destination embedded in the interrupted
    /// <c>STA.l $7EC1C0,X</c> at $90:AD14. The inherited X runs the copy past this palette.
    /// </summary>
    public static readonly CpuBusCell SpritePaletteSix = CpuBusCell.WramBytes(nameof(SpritePaletteSix), 0x7ec1c0,
        PaletteByteCount, "BGR555[16] (u16[] little-endian)");

    /// <summary>$90:AD1C compares inherited Y against $0020 after incrementing it.</summary>
    public const ushort PaletteByteCount = 0x0020;

    /// <summary>Bytes in one bank-$93 projectile animation record.</summary>
    public const ushort AnimationRecordByteCount = 8;
}

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
        int sourcePointer = SnesLittleEndian.Word(pointer.Read(0), pointer.Read(1)) | pointer.Read(2) << 16;
        int sourceAddress = (sourcePointer + sourceIndex) & SnesCpuAddressLayout.AddressMask;
        int destinationAddress = (SpacetimeBeamCorruptionLayout.SpritePaletteSix.Address + destinationIndex) & SnesCpuAddressLayout.AddressMask;

        bus.WriteByte(destinationAddress, ReadSourceByte(sourceAddress));
        bus.WriteByte(
            (destinationAddress + 1) & SnesCpuAddressLayout.AddressMask,
            ReadSourceByte((sourceAddress + 1) & SnesCpuAddressLayout.AddressMask));
        return SaveMirrorMemory.Overlaps(destinationAddress, destinationAddress + 1);
    }

    /// <summary>read source byte.</summary>
    private byte ReadSourceByte(int address) =>
        SnesLiveDataRead.TryRead(bus, address, "SpaceTime source", out byte value)
            ? value
            : SpacetimeBeamCopyDefinitions.ReadCartridgeByte(address);
}
