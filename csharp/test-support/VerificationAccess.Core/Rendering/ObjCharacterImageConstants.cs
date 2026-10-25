using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Assets;
using System.Collections.Immutable;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.Core.Rendering;

/// <summary>Cartridge values and helpers of <see cref="ObjCharacterImage"/> that only verification reads.</summary>
internal static class ObjCharacterImageConstants
{
    /// <summary>The values of OAM's nine-bit X: the upper half lies left of the screen.</summary>
    internal const int XRange = 512;
    /// <summary>The values of OAM's eight-bit Y.</summary>
    internal const int YRange = 256;

    /// <summary>Images pictured from OAM so far, which keeps each one's name its own.</summary>
    private static int pictured;
    /// <summary>
    /// The draws <paramref name="oam"/>'s finalized records stand for (#1392): one pre-coloured image per record, in
    /// record order so record 0 is frontmost, each pixel carrying the record's palette row, priority and the colour its
    /// characters name, with the record's flips applied. The images' standard colours are black, so a screen whose
    /// colours differ shows each pixel through its palette index, as its characters show it.
    /// </summary>
    /// <param name="oam">The screen's finalized OAM storage.</param>
    /// <param name="vram">The screen's VRAM, which holds the characters the records name.</param>
    /// <param name="objectSelection">The screen's OBSEL value.</param>
    public static ImmutableArray<SpriteDraw> Draws(OamBuffer oam, SnesVram vram, int objectSelection)
    {
        if (Composition(oam, vram, objectSelection) is not { } composition)
            return [];
        var draws = ImmutableArray.CreateBuilder<SpriteDraw>();
        var standard = new SpriteImagePalette(SpriteImageColours.Sprite, [.. new RgbColor[SpriteImage.SpriteColourCount]]);
        RgbaPixel black = default(RgbColor).ToRgbaPixel();
        ScreenObjectSizes sizes = ObjectSelectionCodec.Sizes(checked((byte)objectSelection));
        int part = 0;
        for (int index = 0; index < oam.LastFinalizedSpriteCount; index++)
        {
            OamEntry entry = oam.GetEntry(index);
            Extent16<ScreenPixels> size = entry.IsLarge ? sizes.Large : sizes.Small;
            int cells = size.X / SpriteArtwork.CharacterPixels * (size.Y / SpriteArtwork.CharacterPixels);
            // The record's cells are its parts in the composition; the first cell sits at the record's corner.
            SpriteScreenPart corner = composition.Place(part);
            int left = int.MaxValue, top = int.MaxValue;
            for (int cell = 0; cell < cells; cell++)
            {
                SpriteScreenPart placed = composition.Place(part + cell);
                left = Math.Min(left, placed.Position.X);
                top = Math.Min(top, placed.Position.Y);
            }
            var cellParts = ImmutableArray.CreateBuilder<SpriteFramePart>(cells);
            var pixels = new RgbaPixel[size.X * size.Y];
            var indices = new byte[pixels.Length];
            var priorities = new byte[pixels.Length];
            var partRows = new byte[pixels.Length];
            for (int cell = 0; cell < cells; cell++)
            {
                SpriteScreenPart placed = composition.Place(part + cell);
                SpriteFramePart cellPart = composition.Frame.Parts[part + cell];
                cellParts.Add(cellPart with { Offset = new(checked((short)(cellPart.Offset.X - left)), checked((short)(cellPart.Offset.Y - top))) });
                for (ushort row = 0; row < placed.Size.Y; row++)
                for (ushort column = 0; column < placed.Size.X; column++)
                {
                    int character = placed.CharacterAt(new(column, row), out Position16<CharacterPixels> at);
                    byte colour = composition.Artwork.ColorIndex(character, at);
                    if (colour == 0)
                        continue;
                    int pixel = (placed.Position.Y - top + row) * size.X + placed.Position.X - left + column;
                    indices[pixel] = (byte)(placed.Palette * 16 + colour);
                    priorities[pixel] = (byte)placed.Priority;
                    partRows[pixel] = (byte)(placed.Palette | SpriteImage.FixedPartRow);
                    pixels[pixel] = black;
                }
            }
            var image = new SpriteImage($"frontend OAM record {index} ({Interlocked.Increment(ref pictured)})", new((ushort)size.X, (ushort)size.Y),
                pixels, indices, standard, priorities, partRows);
            var frame = new SpriteSheetFrame(image, default, new((ushort)size.X, (ushort)size.Y), default,
                SpritePaletteRowSet.Rows[corner.Palette], []);
            // The picture stands for the record's cells, as a composed sheet frame stands for its composition.
            SuperMetroid.TestSupport.RepositoryInstallation.Verification.SheetFrameSources.Register(frame,
                new SpriteFrame(image.Name, cellParts.MoveToImmutable()), composition.Artwork, sizes);
            draws.Add(new ImageSpriteDraw(new SpriteSource(SpriteSourceKind.FrontendObjects, index), frame,
                new Position16<ScreenPixels>(unchecked((ushort)left), unchecked((ushort)top)), SpriteMirror.None));
            part += cells;
        }
        return draws.ToImmutable();
    }

    /// <summary>The composition of every finalized record's cells, frontmost first, or null when there is no record.</summary>
    internal static CharacterSpriteDraw? Composition(OamBuffer oam, SnesVram vram, int objectSelection)
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
            var flip = (entry.FlipX ? TileFlip.Horizontal : 0) | (entry.FlipY ? TileFlip.Vertical : 0);
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
