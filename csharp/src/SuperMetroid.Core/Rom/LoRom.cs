namespace SuperMetroid.Core.Rom;

/// <summary>
/// Converts between 24-bit SNES CPU addresses and offsets in an unheadered LoROM image.
/// Super Metroid uses FastROM timing, but FastROM does not change this physical layout.
/// </summary>
public static class LoRom
{
    /// <summary>
    /// Converts a CPU-visible address such as <c>$8F:91F8</c> to an offset in the ROM file.
    /// Banks <c>$80-$FF</c> mirror <c>$00-$7F</c>; only the upper 32 KiB of an ordinary
    /// LoROM bank is backed by ROM, hence the <c>$8000</c> lower-bound check.
    /// </summary>
    public static int ToFileOffset(int address)
    {
        int bank = (address >> 16) & 0xff;
        int offsetInBank = address & 0xffff;
        if (offsetInBank < 0x8000)
            throw new ArgumentOutOfRangeException(nameof(address), $"${bank:X2}:{offsetInBank:X4} is outside the ordinary LoROM window.");

        // Masking bit 7 collapses the high-bank FastROM mirror onto the same file bytes.
        return (bank & 0x7f) * 0x8000 + (offsetInBank & 0x7fff);
    }

    /// <summary>
    /// Produces the conventional high-bank spelling of a ROM offset. Many offsets have
    /// several CPU-visible mirrors; returning one canonical form keeps logs deterministic.
    /// </summary>
    public static int ToCanonicalAddress(int fileOffset)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(fileOffset);
        return ((0x80 + fileOffset / 0x8000) << 16) | 0x8000 | (fileOffset % 0x8000);
    }
}
