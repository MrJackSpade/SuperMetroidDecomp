namespace SuperMetroid.Core.Hardware;

/// <summary>Address geometry of the SNES 65C816 CPU bus.</summary>
internal static class SnesCpuAddressLayout
{
    /// <summary>The 24-bit CPU address bus wraps after $FF:FFFF, independently of cartridge mapping.</summary>
    public const int AddressMask = 0xffffff;
}
