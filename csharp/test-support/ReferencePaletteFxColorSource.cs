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
    public bool TryReadColor(ushort pointer, out ushort color)
    {
        int address = RoomFxRomData.Banks.PaletteFx | pointer;
        color = unchecked((ushort)(source.ReadByte(address) | source.ReadByte(address + 1) << 8));
        return true;
    }
}
