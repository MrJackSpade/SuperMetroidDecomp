using SuperMetroid.Core.Assets;
using SuperMetroid.Core;
using System.Buffers.Binary;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>Exports the eight retail room-FX palette blends without exposing selector mechanics.</summary>
public static class RoomFxPaletteBlendExtractor
{
    /// <summary>Imports the eight three-color room-FX blends and supplies the compiled stock Ceres haze tints.</summary>
    /// <param name="bus">Non-null cartridge import address space supplying the independent native blend-color words.</param>
    /// <returns>New UTF-8 JSON bytes containing RGB5 triplets in CGRAM 25..27 write order and separate blue and red fixed-color haze tints.</returns>
    /// <remarks>Combines imported channels with calculated stock components, keeping channels in 0..31; selectors, CGRAM destinations, and fade timing remain compiled.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        Ensure.NotNull(bus);
        var blends = new Dictionary<string, PaletteRgb5[]>();
        foreach (RoomFxPaletteBlend id in RoomFxPaletteBlendRules.Ids)
        {
            RgbColor? calculatedThird = RoomFxPaletteBlendRules.CalculatedThirdColor(id);
            byte[] source = RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus),
                RoomFxLayer3TilemapSources.PaletteBlend(id),
                (calculatedThird.HasValue ? 2 : RoomFxPaletteBlendDefinitions.ColorCount) * sizeof(ushort));
            var colors = new PaletteRgb5[RoomFxPaletteBlendDefinitions.ColorCount];
            for (int index = 0; index < colors.Length; index++)
            {
                RgbColor stored = index == 2 && calculatedThird.HasValue ? calculatedThird.Value
                    : RgbColor.FromWord(BinaryPrimitives.ReadUInt16LittleEndian(source.AsSpan(index * RgbColor.ByteCount)));
                int red = index < 2 ? RoomFxPaletteBlendRules.CalculatedPairRed(id, index == 0) ?? stored.Red : stored.Red;
                int green = index < 2 ? RoomFxPaletteBlendRules.CalculatedPairGreen(id, red, index == 0) ?? stored.Green : RoomFxPaletteBlendRules.CalculatedThirdGreen(id) ?? stored.Green;
                colors[index] = new PaletteRgb5
                {
                    Red = red,
                    Green = green,
                    Blue = index < 2 ? RoomFxPaletteBlendRules.CalculatedPairBlue(id, red, green, index == 0) ?? stored.Blue : RoomFxPaletteBlendRules.CalculatedThirdBlue(id, red) ?? stored.Blue,
                };
            }
            blends.Add(RoomFxPaletteBlendRules.Key(id), colors);
        }
        return RoomFxPaletteBlendFile.Write(new RoomFxPaletteBlendDocument
        {
            Version = RoomFxPaletteBlendDefinitions.Version,
            Blends = blends,
            CeresHazeBlue = PaletteRgb5.From(RoomFxPaletteBlendRules.StockCeresHazeBlue),
            CeresHazeRed = PaletteRgb5.From(RoomFxPaletteBlendRules.StockCeresHazeRed),
        });
    }
}
