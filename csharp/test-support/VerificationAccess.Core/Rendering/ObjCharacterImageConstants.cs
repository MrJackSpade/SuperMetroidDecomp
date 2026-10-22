using SuperMetroid.Core.Assets;
using System.Collections.Immutable;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rendering;

/// <summary>Cartridge values and helpers of <see cref="ObjCharacterImage"/> that only verification reads.</summary>
internal static class ObjCharacterImageConstants
{
    /// <summary>The values of OAM's nine-bit X: the upper half lies left of the screen.</summary>
    internal const int XRange = 512;
    /// <summary>The values of OAM's eight-bit Y.</summary>
    internal const int YRange = 256;
    /// <summary>The draws <paramref name="oam"/>'s finalized records stand for: none, or one draw of all their cells.</summary>
    /// <param name="oam">The screen's finalized OAM storage.</param>
    /// <param name="vram">The screen's VRAM, which holds the characters the records name.</param>
    /// <param name="objectSelection">The screen's OBSEL value.</param>
    public static ImmutableArray<SpriteDraw> Draws(OamBuffer oam, SnesVram vram, int objectSelection) =>
        Draw(oam, vram, objectSelection) is { } draw ? [draw] : [];
    /// <summary>The one draw of every finalized record's cells, or null when there is no record.</summary>
    internal static SpriteDraw? Draw(OamBuffer oam, SnesVram vram, int objectSelection)
    {
        Ensure.NotNull(oam);
        Ensure.NotNull(vram);
        byte obsel = checked((byte)objectSelection);
        if (oam.LastFinalizedSpriteCount == 0)
            return null;
        SpriteArtwork page = ObjCharacterImage.Page(vram.Bytes, obsel);
        ScreenObjectSizes sizes = ObjectSelectionCodec.Sizes(obsel);
        int smallWidth = sizes.Small.X;
        int smallHeight = sizes.Small.Y;
        int largeWidth = sizes.Large.X;
        int largeHeight = sizes.Large.Y;
        var parts = ImmutableArray.CreateBuilder<SpriteFramePart>();
        for (int index = 0; index < oam.LastFinalizedSpriteCount; index++)
        {
            OamEntry entry = oam.GetEntry(index);
            int width = entry.IsLarge ? largeWidth : smallWidth;
            int height = entry.IsLarge ? largeHeight : smallHeight;
            // OAM X is a signed/modular 9-bit coordinate and Y wraps above the visible field.
            int objectX = entry.Position.X >= XRange / 2 ? entry.Position.X - XRange : entry.Position.X;
            int objectY = entry.Position.Y >= SnesPpuLayout.ScreenHeightPixels ? entry.Position.Y - YRange : entry.Position.Y;
            var flip = (entry.FlipX ? SnesTileFlipFlags.Horizontal : 0) | (entry.FlipY ? SnesTileFlipFlags.Vertical : 0);
            int columns = width / SpriteArtwork.CharacterPixels;
            int rows = height / SpriteArtwork.CharacterPixels;
            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    // Names occupy a sixteen-wide grid; the ninth name-select bit stays fixed when the low byte wraps.
                    int name = (entry.TileNumber & ObjCharacterImage.NameSelectBit) | unchecked((byte)(entry.TileNumber + row * 16 + column));
                    int shownColumn = entry.FlipX ? columns - 1 - column : column;
                    int shownRow = entry.FlipY ? rows - 1 - row : row;
                    parts.Add(new SpriteFramePart(
                        new Offset16<ScreenPixels>(checked((short)(objectX + shownColumn * SpriteArtwork.CharacterPixels)),
                            checked((short)(objectY + shownRow * SpriteArtwork.CharacterPixels))),
                        new Extent16<ScreenPixels>(SpriteArtwork.CharacterPixels, SpriteArtwork.CharacterPixels),
                        name, flip, entry.Palette, entry.Priority));
                }
            }
        }
        var frame = new SpriteFrame($"frontend OAM, {oam.LastFinalizedSpriteCount} records", parts.ToImmutable());
        return new CharacterSpriteDraw(new SpriteSource(SpriteSourceKind.FrontendObjects), frame, page, default, 0, 0, 0);
    }
}
