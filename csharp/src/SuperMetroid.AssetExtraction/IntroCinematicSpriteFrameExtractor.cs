using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Read one native on-screen OAM frame into visual-only editable fields.</summary>
internal static class IntroCinematicSpriteFrameExtractor
{
    /// <summary>Decodes the requested native OAM spritemap into visual-only editable parts.</summary>
    /// <param name="bus">Cartridge address space containing the spritemap bank.</param>
    /// <param name="pointer">Bank-relative pointer to the count-prefixed OAM entries.</param>
    /// <param name="expectedParts">Required OAM entry count for this authored frame.</param>
    /// <param name="name">Frame identity included in malformed-data errors.</param>
    /// <returns>Sprite parts with decoded offsets, tile coordinates, size, priority, and flips.</returns>
    internal static SpriteVisualPart[] Extract(ISnesAddressSpace bus,
        ushort pointer, int expectedParts, string name)
    {
        int source = (int)new SnesAddress(IntroCinematicRomData.Banks.Spritemaps, pointer);
        int count = bus.ReadCartridgeByte(source) | bus.ReadCartridgeByte(source + 1) << 8;
        if (count != expectedParts)
            throw new InvalidDataException(
                $"Opening sprite frame {name} has {count} OAM parts, expected {expectedParts}.");
        var parts = new SpriteVisualPart[count];
        for (int index = 0; index < count; index++)
        {
            int entry = source + 2 + index * 5;
            var x = new SnesSpritemapXWord((ushort)(bus.ReadCartridgeByte(entry) |
                bus.ReadCartridgeByte(entry + 1) << 8));
            byte y = bus.ReadCartridgeByte(entry + 2);
            var attributes = new SnesObjAttributeWord((ushort)(bus.ReadCartridgeByte(entry + 3) |
                bus.ReadCartridgeByte(entry + 4) << 8));
            parts[index] = new SpriteVisualPart
            {
                OffsetX = x.SignedOffset,
                OffsetY = unchecked((sbyte)y),
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
