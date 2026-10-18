using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.AssetExtraction.Fx;

namespace SuperMetroid.TestSupport.VerificationOracles;

/// <summary>
/// Explicit diagnostic-only color oracle for existing constructed/reference fixtures.
/// This file is linked only into verification/debug tools, never gameplay Core.
/// Production owners supply their installed presentation catalogs instead.
/// </summary>
internal sealed class ReferencePaletteFxColorSource(ISnesAddressSpace source) : IPaletteFxColorSource
{
    /// <summary>Reads a BGR555 word from the palette-FX bank and reports it available to the diagnostic fixture.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for palette-FX color words")]
    public bool TryReadColor(PaletteFxColorKey key, out Bgr555 color)
    {
        int address = RoomFxRomData.Banks.PaletteFx | PaletteFxRecordSources.ColorAddress(key);
        color = Bgr555.FromWord(CartridgeImportSource.Require(source).ReadCartridgeWord(address));
        return true;
    }
}
