using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.AssetExtraction.Frontend.Intro;

/// <summary>Read one native on-screen OAM frame into visual-only editable fields.</summary>
internal static class IntroCinematicSpriteFrameExtractor
{
    /// <summary>Decodes a bank-$8C opening spritemap and checks its authored OAM part count before importing editable visual attributes.</summary>
    internal static SpriteVisualPart[] Extract(ISnesAddressSpace bus,
        int pointer, int expectedParts, string name)
    {
        int source = (int)SnesAddress.InBank(IntroCinematicSourceDefinitions.SpritemapBank, pointer);
        int count = CartridgeImportSource.Require(bus).ReadCartridgeWord(source);
        if (count != expectedParts)
            throw new InvalidDataException(
                $"Opening sprite frame {name} has {count} OAM parts, expected {expectedParts}.");
        var parts = new SpriteVisualPart[count];
        for (int index = 0; index < count; index++)
        {
            int entry = source + 2 + index * 5;
            var x = new SnesSpritemapXWord(CartridgeImportSource.Require(bus).ReadCartridgeWord(entry));
            var y = new SnesSpritemapYByte(bus.ReadCartridgeByte(entry + 2));
            var attributes = new SnesObjAttributeWord(CartridgeImportSource.Require(bus).ReadCartridgeWord(entry + 3));
            parts[index] = new SpriteVisualPart
            {
                OffsetX = x.SignedOffset,
                OffsetY = y.SignedOffset,
                TileColumn = attributes.TileNumber % IntroCinematicSpriteCompiler.TileColumns,
                TileRow = attributes.TileNumber / IntroCinematicSpriteCompiler.TileColumns,
                Size = x.IsLarge ? 16 : 8,
                Priority = attributes.Priority,
                // The native generic loader replaces source palette bits with its
                // owning actor's current palette. Null retains that inheritance.
                Palette = null,
                FlipX = attributes.FlipHorizontally,
                FlipY = attributes.FlipVertically,
            };
        }
        return parts;
    }
}
