using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Fx.PaletteFx;

namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>Reads a palette-FX colour family's colours from the cartridge, frame by frame.</summary>
internal static class PaletteFxColorExtraction
{
    /// <summary>Every colour of <paramref name="colorFrames"/>, read from its bank-$8D colour word.</summary>
    internal static PaletteRgb5[][] ReadFrames(ISnesAddressSpace bus, PaletteFxColorFrames colorFrames)
    {
        var frames = new PaletteRgb5[colorFrames.FrameCount][];
        for (int frame = 0; frame < colorFrames.FrameCount; frame++)
        {
            frames[frame] = new PaletteRgb5[colorFrames.ColorsPerFrame];
            for (int color = 0; color < colorFrames.ColorsPerFrame; color++)
            {
                int bgr555 = RomDataReader.ReadWordFixedBank(
                    CartridgeImportSource.Require(bus),
                    RoomFxBankSourceDefinitions.PaletteFx | PaletteFxRecordSources.ColorAddress(colorFrames.Color(frame, color)));
                frames[frame][color] = new PaletteRgb5
                {
                    Red = bgr555 & 31,
                    Green = bgr555 >> 5 & 31,
                    Blue = bgr555 >> 10 & 31,
                };
            }
        }
        return frames;
    }
}
