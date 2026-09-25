using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Read one native on-screen OAM frame into visual-only editable fields.</summary>
internal static class IntroCinematicSpriteFrameExtractor
{
    internal static SpriteVisualPart[] Extract(ISnesAddressSpace bus,
        ushort pointer, int expectedParts, string name)
    {
        int source = (int)new SnesAddress(IntroCinematicRomData.Banks.Spritemaps, pointer);
        int count = bus.ReadByte(source) | bus.ReadByte(source + 1) << 8;
        if (count != expectedParts)
            throw new InvalidDataException(
                $"Opening sprite frame {name} has {count} OAM parts, expected {expectedParts}.");
        var parts = new SpriteVisualPart[count];
        for (int index = 0; index < count; index++)
        {
            int entry = source + 2 + index * 5;
            var x = new SnesSpritemapXWord((ushort)(bus.ReadByte(entry) |
                bus.ReadByte(entry + 1) << 8));
            byte y = bus.ReadByte(entry + 2);
            var attributes = new SnesObjAttributeWord((ushort)(bus.ReadByte(entry + 3) |
                bus.ReadByte(entry + 4) << 8));
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
