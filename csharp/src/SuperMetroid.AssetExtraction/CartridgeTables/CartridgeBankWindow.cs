using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction;

/// <summary>
/// Resolves a bank-local pointer read from cartridge data against the bank of a cataloged
/// cartridge window (#629). Native data stores 16-bit pointers whose bank is implied by the
/// table that owns them; codecs name that bank through a <see cref="CpuBusCell"/>.
/// </summary>
[CpuBusAccess]
internal static class CartridgeBankWindow
{
    /// <summary>The CPU address of bank-local <paramref name="offset"/> in the bank holding <paramref name="window"/>.</summary>
    internal static int At(CpuBusCell window, int offset) => (window.Address & 0xff0000) | offset;
}
