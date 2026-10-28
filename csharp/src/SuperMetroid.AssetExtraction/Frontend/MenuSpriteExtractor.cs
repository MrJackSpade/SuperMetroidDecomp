using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.AssetExtraction.Frontend;

/// <summary>Decodes ordered bank-$82 menu parts, preserving native caller-palette inheritance.</summary>
internal static class MenuSpriteExtractor
{
    /// <summary>Decodes an indexed bank-$82 spritemap into bounded OAM parts, leaving palette choice to the caller as native menu rendering does.</summary>
    public static SpriteVisualPart[] Read(ISnesAddressSpace bus, int id)
    {
        int address = MapSourceDefinitions.Menu.ObjectBank | RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), MapSourceDefinitions.Menu.SpritemapPointers + id * 2);
        int count = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address);
        if (count > MapSpriteFormat.MaximumParts) throw new InvalidDataException($"Menu sprite {id:X4} exceeds OAM capacity.");
        var parts = new SpriteVisualPart[count];
        for (int index = 0; index < count; index++)
        {
            int part = address + 2 + index * 5;
            var x = new SnesSpritemapXWord(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), part));
            var attributes = new SnesObjAttributeWord(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), part + 3));
            parts[index] = new() { OffsetX = x.SignedOffset, OffsetY = unchecked((sbyte)bus.ReadCartridgeByte(part + 2)),
                TileColumn = attributes.TileNumber % MapSpriteFormat.TileColumns, TileRow = attributes.TileNumber / MapSpriteFormat.TileColumns,
                Size = x.IsLarge ? 16 : 8, Priority = attributes.Priority, FlipX = attributes.FlipHorizontally, FlipY = attributes.FlipVertically,
                // $81:879F replaces source palette bits with its caller's live palette.
                Palette = null };
        }
        return parts;
    }
}
