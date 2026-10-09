using System.Buffers.Binary;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

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
        ArgumentNullException.ThrowIfNull(bus);
        var blends = new Dictionary<string, PaletteRgb5[]>();
        foreach (byte id in RoomFxPaletteBlendDefinitions.Ids)
        {
            ushort? calculatedThird = RoomFxPaletteBlendDefinitions.CalculatedThirdColor(id);
            byte[] source = RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus),
                RoomFxPaletteBlendDefinitions.SourceAddress(id),
                (calculatedThird.HasValue ? 2 : RoomFxRomData.Layer3.PaletteBlendColorCount) * sizeof(ushort));
            var colors = new PaletteRgb5[RoomFxRomData.Layer3.PaletteBlendColorCount];
            for (int index = 0; index < colors.Length; index++)
            {
                ushort word = index == 2 && calculatedThird.HasValue ? calculatedThird.Value
                    : BinaryPrimitives.ReadUInt16LittleEndian(source.AsSpan(index * sizeof(ushort)));
                int red = index < 2 ? RoomFxPaletteBlendDefinitions.CalculatedPairRed(id, index == 0) ?? (word & 31) : word & 31;
                int green = index < 2 ? RoomFxPaletteBlendDefinitions.CalculatedPairGreen(id, red, index == 0) ?? ((word >> 5) & 31) : RoomFxPaletteBlendDefinitions.CalculatedThirdGreen(id) ?? ((word >> 5) & 31);
                colors[index] = new PaletteRgb5
                {
                    Red = red,
                    Green = green,
                    Blue = index < 2 ? RoomFxPaletteBlendDefinitions.CalculatedPairBlue(id, red, green, index == 0) ?? ((word >> 10) & 31) : RoomFxPaletteBlendDefinitions.CalculatedThirdBlue(id, red) ?? ((word >> 10) & 31),
                };
            }
            blends.Add(RoomFxPaletteBlendDefinitions.Key(id), colors);
        }
        return RoomFxPaletteBlendCatalog.Write(new RoomFxPaletteBlendDocument
        {
            Version = RoomFxPaletteBlendDefinitions.Version,
            Blends = blends,
            CeresHazeBlue = RoomFxPaletteBlendDefinitions.StockCeresHazeBlue,
            CeresHazeRed = RoomFxPaletteBlendDefinitions.StockCeresHazeRed,
        });
    }
}
