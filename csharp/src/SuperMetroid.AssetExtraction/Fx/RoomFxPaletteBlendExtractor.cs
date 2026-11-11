using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core;
using System.Buffers.Binary;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Fx.Artwork;

namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>Exports the eight retail room-FX palette blends without exposing selector mechanics.</summary>
public static class RoomFxPaletteBlendExtractor
{
    /// <summary>Imports the eight three-color room-FX blends.</summary>
    /// <param name="bus">Non-null cartridge import address space supplying the independent native blend-color words.</param>
    /// <returns>New UTF-8 JSON bytes containing RGB5 triplets in CGRAM 25..27 write order.</returns>
    /// <remarks>Each selection's three colour words are copied as the cartridge stores them; selectors, CGRAM destinations, and fade timing remain compiled.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        Ensure.NotNull(bus);
        var blends = new Dictionary<string, PaletteRgb5[]>();
        foreach (RoomFxPaletteBlend id in RoomFxPaletteBlendDocumentRules.Ids)
        {
            byte[] source = RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus),
                RoomFxLayer3TilemapSources.PaletteBlend(id), RoomFxPaletteBlendDefinitions.ColorCount * sizeof(ushort));
            var colors = new PaletteRgb5[RoomFxPaletteBlendDefinitions.ColorCount];
            for (int index = 0; index < colors.Length; index++)
            {
                RgbColor stored = RgbColor.FromWord(BinaryPrimitives.ReadUInt16LittleEndian(source.AsSpan(index * ColourWordCodec.ByteCount)));
                colors[index] = new PaletteRgb5
                {
                    Red = stored.FadeLevels.Red,
                    Green = stored.FadeLevels.Green,
                    Blue = stored.FadeLevels.Blue,
                };
            }
            blends.Add(RoomFxPaletteBlendDocumentRules.Key(id), colors);
        }
        return RoomFxPaletteBlendFile.Write(new RoomFxPaletteBlendDocument
        {
            Version = RoomFxPaletteBlendDefinitions.Version,
            Blends = blends,
        });
    }
}
