using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.AssetExtraction.Fx;

using SuperMetroid.AssetExtraction.CartridgeTables;
namespace SuperMetroid.TestSupport.VerificationOracles;

/// <summary>
/// Explicit diagnostic-only color oracle for existing constructed/reference fixtures.
/// This file is linked only into verification/debug tools, never gameplay Core.
/// Production owners supply their installed presentation catalogs instead.
/// </summary>
internal sealed class ReferencePaletteFxColorSource(ISnesAddressSpace source) : IPaletteFxColorSource
{
    /// <summary>Reads a BGR555 word from the palette-FX bank and reports it available to the diagnostic fixture.</summary>
    public bool TryReadColor(PaletteFxColorKey key, out RgbColor color)
    {
        int address = RoomFxBankSourceDefinitions.PaletteFx | PaletteFxRecordSources.ColorAddress(key);
        color = RgbColor.FromWord(CartridgeImportSource.Require(source).ReadCartridgeWord(address));
        return true;
    }
}
