using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Escape.Artwork;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Areas.Escape;

/// <summary>Composes the escape timer's pre-coloured sheet from the cartridge's timer spritemaps, characters and colours.</summary>
[CpuBusBoundary("codec: ROM escape-timer spritemaps, characters and palette row → pre-coloured timer sheet")]
internal static class EscapeTimerSheetComposer
{
    /// <summary>OBJ character $1E0, where the timer's first character lands; timer artwork character 0.</summary>
    private const int FirstCharacter = 0x1e0;

    /// <summary>The OBJ palette row the timer is drawn with.</summary>
    internal const int PaletteRow = 5;

    /// <summary>The most parts a timer spritemap may hold.</summary>
    private const int MaximumParts = 128;

    /// <summary>Composes the label and the ten digits in glyph order, coloured as a room load leaves the sprite rows.</summary>
    internal static ComposedSpriteSheet Compose(ISnesAddressSpace bus)
    {
        Ensure.NotNull(bus);
        var frames = new List<KeyValuePair<string, SpriteFrame>>();
        foreach (EscapeTimerGlyph glyph in EscapeTimerPresentationDefinitions.Glyphs)
        {
            string name = EscapeTimerPresentationDefinitions.FrameName(glyph);
            int spritemap = glyph == EscapeTimerGlyph.Label
                ? EscapeCartridgeTables.TimerLabelSpritemap
                : EscapeCartridgeTables.TimerSpritemapBank | (EscapeCartridgeTables.FirstTimerDigitSpritemap +
                    EscapeTimerPresentationDefinitions.DigitValue(glyph) * EscapeCartridgeTables.TimerDigitSpritemapStride);
            frames.Add(KeyValuePair.Create(name, new SpriteFrame(name, ReadSpritemap(bus, spritemap))));
        }
        EscapeTimerTileAtlas tiles = EscapeTimerTileAtlas.Load(new MemoryStream(EscapeTimerTileAtlasExtractor.Extract(bus), writable: false));
        return SpriteSheetComposer.Compose(frames, tiles.Artwork, GameplayBasePaletteFiles.RoomLoadSpriteColours(bus), PaletteRow, 0);
    }

    /// <summary>Reads one timer spritemap's parts, with characters counted from the timer artwork's first.</summary>
    private static System.Collections.Immutable.ImmutableArray<SpriteFramePart> ReadSpritemap(ISnesAddressSpace bus, int address)
    {
        int count = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address);
        if (count > MaximumParts)
            throw new InvalidDataException($"Escape timer spritemap ${address:X6} exceeds OAM capacity.");
        var parts = new SpriteFramePart[count];
        for (int index = 0; index < count; index++)
        {
            int source = address + sizeof(ushort) + index * 5;
            var x = new SnesSpritemapXWord(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), source));
            var attributes = new SnesObjAttributeWord(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), source + 3));
            ushort size = x.IsLarge ? (ushort)16 : (ushort)8;
            parts[index] = new SpriteFramePart(
                new Offset16<ScreenPixels>(checked((short)x.SignedOffset), unchecked((sbyte)bus.ReadCartridgeByte(source + 2))),
                new Extent16<ScreenPixels>(size, size), attributes.TileNumber - FirstCharacter,
                (attributes.FlipHorizontally ? SnesTileFlipFlags.Horizontal : SnesTileFlipFlags.None) |
                (attributes.FlipVertically ? SnesTileFlipFlags.Vertical : SnesTileFlipFlags.None),
                null, attributes.Priority);
        }
        return [.. parts];
    }
}
