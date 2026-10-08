namespace SuperMetroid.Core.Hardware;

/// <summary>A checked, lossless 24-bit SNES CPU-bus address.</summary>
/// <remarks>
/// A native long address consists of an eight-bit bank and a sixteen-bit offset. Keeping
/// those pieces typed prevents host integer addition from accidentally carrying into the
/// bank when a 65C816 routine advances only its sixteen-bit pointer register.
/// </remarks>
public readonly record struct SnesAddress
{
    private const int MaximumValue = 0x00ff_ffff;
    private const ushort UpperLoRomWindowMask = 0x8000;

    /// <summary>Creates an address from its exact native bank and offset fields.</summary>
    public SnesAddress(byte bank, ushort offset)
    {
        Value = (bank << 16) | offset;
    }

    private SnesAddress(int checkedValue)
    {
        Value = checkedValue;
    }

    /// <summary>The packed 24-bit value, exposed only for explicit bus-boundary conversion.</summary>
    public int Value { get; }

    /// <summary>Native eight-bit data/program bank.</summary>
    public byte Bank => (byte)(Value >> 16);

    /// <summary>Native sixteen-bit address within <see cref="Bank"/>.</summary>
    public ushort Offset => (ushort)Value;

    /// <summary>Whether the address lies in a bank's cartridge-backed upper LoROM window.</summary>
    public bool IsUpperLoRomWindow => (Offset & UpperLoRomWindowMask) != 0;

    /// <summary>Validates and preserves an already-packed CPU-bus address.</summary>
    public static SnesAddress FromBusAddress(int address)
    {
        if ((uint)address > MaximumValue)
            throw new ArgumentOutOfRangeException(nameof(address), address, "SNES addresses are 24-bit values.");
        return new SnesAddress(address);
    }

    /// <summary>
    /// Adds to only the native sixteen-bit offset, wrapping at the bank boundary while
    /// retaining the original bank exactly.
    /// </summary>
    public SnesAddress AddWithinBank(int byteCount) =>
        new(Bank, unchecked((ushort)(Offset + byteCount)));

    /// <summary>
    /// Advances through consecutive upper LoROM storage. Unlike <see cref="AddWithinBank"/>,
    /// crossing <c>$xx:FFFF</c> continues at <c>$(xx+1):8000</c>.
    /// </summary>
    public SnesAddress NextLoRomByte()
    {
        if (!IsUpperLoRomWindow)
            throw new InvalidOperationException($"${Bank:X2}:${Offset:X4} is outside an upper LoROM window.");
        return Offset == ushort.MaxValue
            ? new SnesAddress(unchecked((byte)(Bank + 1)), UpperLoRomWindowMask)
            : new SnesAddress(Bank, (ushort)(Offset + 1));
    }

    /// <summary>Returns the packed 24-bit CPU-bus value at an explicit bus boundary.</summary>
    /// <param name="address">The native bank-and-offset address to convert.</param>
    public static explicit operator int(SnesAddress address) => address.Value;

    /// <summary>Formats the address as a hexadecimal native bank and offset, such as <c>$80:8000</c>.</summary>
    public override string ToString() => $"${Bank:X2}:${Offset:X4}";
}
