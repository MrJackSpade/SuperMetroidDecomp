using SuperMetroid.Core.Rendering;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Escape.Artwork;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Areas.Escape;

/// <summary>Composes the escape timer's pre-coloured sheet from the cartridge's timer spritemaps, characters and colours.</summary>
internal static class EscapeTimerSheetComposer
{
    /// <summary>The OBJ palette row the timer is drawn with.</summary>
    internal const int PaletteRow = 5;

    /// <summary>The most parts a timer spritemap may hold.</summary>
    private const int MaximumParts = 128;

    /// <summary>Composes the label and the ten digits in glyph order, coloured as a room load leaves the sprite rows.</summary>
    internal static ComposedSpriteSheet Compose(ISnesAddressSpace bus)
    {
        Ensure.NotNull(bus);
        EscapeTimerTileAtlas tiles = EscapeTimerCharacters.Read(new MemoryStream(EscapeTimerTileAtlasExtractor.Extract(bus), writable: false));
        var frames = new List<SheetFrameSource>();
        foreach (EscapeTimerGlyph glyph in EscapeTimerPresentationDefinitions.Glyphs)
        {
            string name = EscapeTimerPresentationDefinitions.FrameName(glyph);
            int spritemap = glyph == EscapeTimerGlyph.Label
                ? (int)EscapeSourceDefinitions.TimerLabelSpritemap
                : (int)EscapeSourceDefinitions.TimerSpritemapBank.AddWithinBank(EscapeSourceDefinitions.FirstTimerDigitSpritemap +
                    EscapeTimerPresentationDefinitions.DigitValue(glyph) * EscapeSourceDefinitions.TimerDigitSpritemapStride);
            frames.Add(new(name, new SpriteFrame(name, ReadSpritemap(bus, spritemap)), tiles.Artwork));
        }
        return SpriteSheetComposer.Compose(new SpriteSheetSources(frames, GameplayBasePaletteFiles.RoomLoadSpriteColours(bus), ObjectSelectionCodec.Sizes(GameplayRenderDefinitions.ObjectSelection)), PaletteRow, 0);
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
            Extent16<ScreenPixels> size = x.IsLarge ? new(16, 16) : new(8, 8);
            parts[index] = new SpriteFramePart(
                new Offset16<ScreenPixels>(checked((short)x.SignedOffset), unchecked((sbyte)bus.ReadCartridgeByte(source + 2))),
                size, attributes.TileNumber - EscapeTextArtworkDefinitions.TimerFirstCharacter,
                (attributes.FlipHorizontally ? TileFlip.Horizontal : TileFlip.None) |
                (attributes.FlipVertically ? TileFlip.Vertical : TileFlip.None),
                null, attributes.Priority);
        }
        return [.. parts];
    }
}
