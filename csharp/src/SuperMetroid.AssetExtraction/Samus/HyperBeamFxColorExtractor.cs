using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.AssetExtraction.Fx;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Reads only the authored Hyper Beam palette-FX color words from the validated cartridge.</summary>
[CpuBusBoundary("codec: ROM Hyper Beam palette-FX color words → Hyper Beam FX colors")]
public static class HyperBeamFxColorExtractor
{
    /// <summary>Exports the ten eight-color payloads of Hyper Beam palette-FX program $8D:D900, skipping each duration and terminator word.</summary>
    /// <param name="bus">Cartridge source for RGB5 operands beginning at $8D:D906 with a twenty-byte frame stride.</param>
    /// <returns>A new UTF-8 JSON buffer with ten ordered frames of eight colors, each channel expressed as an integer 0..31.</returns>
    /// <remarks>Only color channels are imported; the two-update holds, entry command, loop, and program pointer domain remain compiled. Native bit 15 is not represented in RGB5 channel output.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        Ensure.NotNull(bus);
        PaletteRgb5[][] frames = PaletteFxColorExtraction.ReadFrames(bus, HyperBeamPaletteFxColorDefinitions.Rows);
        return HyperBeamFxColorCatalog.Write(new() { Version = HyperBeamFxColorFormat.Version, Frames = frames });
    }
}
