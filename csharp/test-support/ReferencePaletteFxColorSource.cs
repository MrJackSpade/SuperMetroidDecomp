using SuperMetroid.Core.Rom;
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
    [CpuBusBoundary("exact bus test: retail ROM oracle for palette-FX color words")]
    public bool TryReadColor(ushort pointer, out Bgr555 color)
    {
        int address = RoomFxRomData.Banks.PaletteFx | pointer;
        color = Bgr555.FromWord(CartridgeImportSource.Require(source).ReadCartridgeWord(address));
        return true;
    }
}
