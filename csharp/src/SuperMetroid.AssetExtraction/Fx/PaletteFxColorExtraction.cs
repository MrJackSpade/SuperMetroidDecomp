using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>Reads a palette-FX colour family's colours from the cartridge, row by row.</summary>
internal static class PaletteFxColorExtraction
{
    /// <summary>Every colour of <paramref name="rows"/>, read from its bank-$8D colour word.</summary>
    internal static PaletteRgb5[][] ReadFrames(ISnesAddressSpace bus, PaletteFxColorRows rows)
    {
        var frames = new PaletteRgb5[rows.FrameCount][];
        for (int frame = 0; frame < rows.FrameCount; frame++)
        {
            frames[frame] = new PaletteRgb5[rows.ColorsPerFrame];
            for (int color = 0; color < rows.ColorsPerFrame; color++)
            {
                ushort bgr555 = RomDataReader.ReadWordFixedBank(
                    CartridgeImportSource.Require(bus),
                    RoomFxBankSourceDefinitions.PaletteFx | PaletteFxRecordSources.ColorAddress(rows.Color(frame, color)));
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
