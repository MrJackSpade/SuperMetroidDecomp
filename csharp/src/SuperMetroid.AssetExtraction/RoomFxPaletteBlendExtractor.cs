using System.Buffers.Binary;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Exports the eight retail room-FX palette blends without exposing selector mechanics.</summary>
public static class RoomFxPaletteBlendExtractor
{
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
                colors[index] = new PaletteRgb5
                {
                    Red = word & 31,
                    Green = (word >> 5) & 31,
                    Blue = (word >> 10) & 31,
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
