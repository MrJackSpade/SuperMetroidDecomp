using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Exports the ten authored Hyper Beam palettes without exposing phase logic.</summary>
public static class SamusHyperBeamColorExtractor
{
    /// <summary>Exports the ten full-body Hyper Beam palette images in the compiled frame-selection order.</summary>
    /// <param name="bus">Non-null import-capable cartridge source for bank-$9B rows descending from $A360 in $20-byte steps.</param>
    /// <returns>A new UTF-8 JSON buffer with ten ordered sixteen-color RGB5 frames, including color zero and channels 0..31.</returns>
    /// <remarks>This body-palette resource is distinct from Hyper-shot flashes and the eight-color palette-FX program. Bit 15 is omitted; native phase selection and timing are not exported.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="bus"/> lacks cartridge import access.</exception>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        Ensure.NotNull(bus);
        var frames = new PaletteRgb5[SamusHyperBeamColorFormat.FrameCount][];
        for (int frame = 0; frame < frames.Length; frame++)
        {
            int address = SamusSourceDefinitions.HyperBeamPalettes.Value - frame * SamusSourceDefinitions.HyperBeamPaletteStride;
            byte[] source = RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), address,
                SamusHyperBeamColorFormat.ColorsPerFrame * sizeof(ushort));
            frames[frame] = new PaletteRgb5[SamusHyperBeamColorFormat.ColorsPerFrame];
            for (int index = 0; index < frames[frame].Length; index++)
            {
                ushort word = unchecked(SnesLittleEndian.Word(source[index * 2], source[index * 2 + 1]));
                frames[frame][index] = new PaletteRgb5
                {
                    Red = word & 31,
                    Green = (word >> 5) & 31,
                    Blue = (word >> 10) & 31,
                };
            }
        }
        return SamusHyperBeamColorFile.Write(new SamusHyperBeamColorDocument
        {
            Version = SamusHyperBeamColorFormat.Version,
            Frames = frames,
        });
    }
}
