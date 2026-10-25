namespace SuperMetroid.AssetExtraction.Cartridge;

/// <summary>Address geometry of the SNES 65C816 CPU bus.</summary>
internal static class SnesCpuAddressLayout
{
    /// <summary>The 24-bit CPU address bus wraps after $FF:FFFF, independently of cartridge mapping.</summary>
    public const int AddressMask = 0xffffff;

    /// <summary>The bank byte of a 24-bit address, kept in place.</summary>
    public const int BankMask = 0xff0000;

    /// <summary>The sixteen-bit offset of an address within its bank.</summary>
    public const int OffsetMask = 0xffff;

    /// <summary>Bits below the bank byte: the width of a within-bank offset.</summary>
    public const int OffsetBits = 16;
}
