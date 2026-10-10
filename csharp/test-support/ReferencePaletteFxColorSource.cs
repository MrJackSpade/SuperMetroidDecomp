using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>
/// Explicit diagnostic-only color oracle for existing constructed/reference fixtures.
/// This file is linked only into verification/debug tools, never gameplay Core.
/// Production owners supply their installed presentation catalogs instead.
/// </summary>
internal sealed class ReferencePaletteFxColorSource(ISnesAddressSpace source) : IPaletteFxColorSource
{
    /// <summary>Reads the reference palette-FX word at a bank-local byte pointer.</summary>
    /// <param name="pointer">The byte offset within the palette-FX bank.</param>
    /// <param name="color">Receives the little-endian 16-bit word stored at that offset.</param>
    /// <returns><see langword="true"/> after reading the word from the configured address space.</returns>
    public bool TryReadColor(ushort pointer, out ushort color)
    {
        int address = RoomFxRomData.Banks.PaletteFx | pointer;
        color = unchecked((ushort)(source.ReadByte(address) | source.ReadByte(address + 1) << 8));
        return true;
    }
}
