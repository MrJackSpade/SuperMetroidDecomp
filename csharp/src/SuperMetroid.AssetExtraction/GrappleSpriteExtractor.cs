using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Extracts attribute words, not the adjacent instruction timers or control flow.</summary>
public static class GrappleSpriteExtractor
{
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        GrappleSpriteStyle Read(int address)
        {
            var attributes = new SnesObjAttributeWord(RomDataReader.ReadWordFixedBank(bus, address));
            return new()
            {
                TileColumn = attributes.TileNumber % ProjectileSpriteDefinitions.TileColumns,
                TileRow = attributes.TileNumber / ProjectileSpriteDefinitions.TileColumns,
                Palette = attributes.PaletteIndex, Priority = attributes.Priority,
                FlipX = attributes.FlipHorizontally, FlipY = attributes.FlipVertically,
            };
        }
        return GrappleSpriteCatalog.Write(new()
        {
            Version = GrappleSpriteDefinitions.Version,
            Endpoint = Read(GrappleSpriteDefinitions.EndpointAttributeAddress),
            Segments = GrappleSpriteDefinitions.SegmentAttributeAddresses.ToArray().Select(Read).ToArray(),
        });
    }
}
