using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Extracts attribute words, not the adjacent instruction timers or control flow.</summary>
public static class GrappleSpriteExtractor
{
    /// <summary>Decodes the endpoint OBJ attributes and four rope-animation attribute words into editable tile, palette, priority, and flip selections.</summary>
    /// <param name="bus">Import-capable cartridge source for endpoint operand $94:B13D and rope operands $94:B18D/B191/B195/B199.</param>
    /// <returns>A new UTF-8 JSON buffer with one endpoint style and four segment styles; tile coordinates use the shared sixteen-column OBJ character grid.</returns>
    /// <remarks>Adjacent duration words, rope layout, connection state, and animation control flow remain compiled and are not serialized.</remarks>
    /// <exception cref="ArgumentException"><paramref name="bus"/> does not provide cartridge import access, including null.</exception>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        GrappleSpriteStyle Read(int address)
        {
            var attributes = new SnesObjAttributeWord(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address));
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
