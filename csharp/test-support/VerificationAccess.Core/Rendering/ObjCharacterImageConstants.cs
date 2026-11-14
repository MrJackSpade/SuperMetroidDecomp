using SuperMetroid.Core.Assets;
using System.Collections.Immutable;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Artwork;

using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Core.Rendering;

/// <summary>Cartridge values and helpers of <see cref="ObjCharacterImage"/> that only verification reads.</summary>
internal static class ObjCharacterImageConstants
{
    /// <summary>The values of OAM's nine-bit X: the upper half lies left of the screen.</summary>
    internal const int XRange = 512;
    /// <summary>The values of OAM's eight-bit Y.</summary>
    internal const int YRange = 256;

    /// <summary>The depth among a gameplay room's layers each record priority draws at, from priority zero up (#1514).</summary>
    private static readonly ActorDepth[] DepthOfPriority =
        [ActorDepth.BehindRoom, ActorDepth.BehindRoom, ActorDepth.BehindFrontParts, ActorDepth.InFront];

    /// <summary>Images pictured from OAM so far, which keeps each one's name its own.</summary>
    private static int pictured;

    /// <summary>
    /// The records this thread last pictured, over the screen memory and object selection it pictured them from (#1757): a
    /// record's picture depends only on its characters there, its size, flips, palette row and priority, so a check that
    /// pictures many frames over one screen memory pictures each record shape once.
    /// </summary>
    [ThreadStatic]
    private static PicturedRecords? lastPictured;

    /// <summary>The pictures of records drawn over one screen memory and object selection, by record shape.</summary>
    /// <param name="Vram">The screen memory the records' characters were read from.</param>
    /// <param name="ObjectSelection">The screen's OBSEL value.</param>
    private sealed record PicturedRecords(byte[] Vram, int ObjectSelection)
    {
        /// <summary>Each record shape's picture.</summary>
        internal Dictionary<RecordShape, SpriteSheetFrame> Frames { get; } = [];
    }

    /// <summary>What a record's picture depends on besides the screen memory: everything but its position.</summary>
    /// <param name="TileNumber">The record's first character.</param>
    /// <param name="Palette">The record's palette row.</param>
    /// <param name="Priority">The record's priority tier.</param>
    /// <param name="FlipX">Whether the record is mirrored horizontally.</param>
    /// <param name="FlipY">Whether the record is mirrored vertically.</param>
    /// <param name="IsLarge">Whether the record has the larger size.</param>
    private readonly record struct RecordShape(int TileNumber, int Palette, int Priority, bool FlipX, bool FlipY, bool IsLarge);
    /// <summary>
    /// The draws <paramref name="oam"/>'s finalized records stand for (#1392): one pre-coloured image per record, in
    /// record order so record 0 is frontmost, each pixel carrying the record's palette row and the colour its
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
        ReadOnlySpan<byte> memory = vram.Bytes;
        if (lastPictured is not { } records || records.ObjectSelection != objectSelection || !memory.SequenceEqual(records.Vram))
            lastPictured = records = new PicturedRecords(memory.ToArray(), objectSelection);
        var draws = ImmutableArray.CreateBuilder<SpriteDraw>();
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
            var shape = new RecordShape(entry.TileNumber, entry.Palette, entry.Priority, entry.FlipX, entry.FlipY, entry.IsLarge);
            if (!records.Frames.TryGetValue(shape, out SpriteSheetFrame? frame))
            {
                frame = Picture(composition, part, cells, size, left, top, corner, index, sizes);
                records.Frames.Add(shape, frame);
            }
            draws.Add(new ImageSpriteDraw(new SpriteSource(SpriteSourceKind.FrontendObjects, index), frame,
                new Position16<ScreenPixels>(unchecked((ushort)left), unchecked((ushort)top)), SpriteMirror.None));
            part += cells;
        }
        return draws.ToImmutable();
    }

    /// <summary>
    /// Pictures the record whose cells are <paramref name="cells"/> parts of <paramref name="composition"/> from
    /// <paramref name="part"/>, with its top-left at (<paramref name="left"/>, <paramref name="top"/>): each pixel carries
    /// the record's palette row and the colour its characters name, with the record's flips applied.
    /// </summary>
    private static SpriteSheetFrame Picture(CharacterSpriteDraw composition, int part, int cells, Extent16<ScreenPixels> size,
        int left, int top, SpriteScreenPart corner, int index, ScreenObjectSizes sizes)
    {
        var standard = new RgbColor[ImageColourGroup.HalfColours];
        RgbaPixel black = default(RgbColor).ToRgbaPixel();
        var cellParts = ImmutableArray.CreateBuilder<SpriteFramePart>(cells);
        var pixels = new RgbaPixel[size.X * size.Y];
        var indices = new byte[pixels.Length];
        for (int cell = 0; cell < cells; cell++)
        {
            SpriteScreenPart placed = composition.Place(part + cell);
            SpriteFramePart cellPart = composition.Frame.Parts[part + cell];
            cellParts.Add(cellPart with { Offset = new(checked((short)(cellPart.Offset.X - left)), checked((short)(cellPart.Offset.Y - top))) });
            for (ushort row = 0; row < placed.Size.Y; row++)
            for (ushort column = 0; column < placed.Size.X; column++)
            {
                int character = placed.CharacterAt(new(column, row), out Position16<CharacterPixels> at);
                int colour = composition.Artwork.ColorIndex(character, at);
                if (colour == 0)
                    continue;
                int pixel = (placed.Position.Y - top + row) * size.X + placed.Position.X - left + column;
                indices[pixel] = (byte)(placed.Palette * 16 + colour);
                pixels[pixel] = black;
            }
        }
        // Each ink is the record's own palette row and colour, with no palette bits ORed in, installed as an installed
        // sheet's manifest would record it (#1512, #1544).
        // A record is drawn whole at the depth its priority places it among a gameplay room's layers (#1514), blending as the
        // import decides from its colours (#1633).
        SpriteSheetFrame frame = ComposedPictures.InstallWhole($"frontend OAM record {index} ({Interlocked.Increment(ref pictured)})",
            new(size.X, size.Y), pixels, indices, SpriteImageColours.Sprite, standard) with { Depth = DepthOfPriority[corner.Priority] };
        SpriteImage image = frame.Sheet;
        // The picture stands for the record's cells, as a composed sheet frame stands for its composition.
        SuperMetroid.TestSupport.RepositoryInstallation.Verification.SheetFrameSources.Register(frame,
            new SpriteFrame(image.Name, cellParts.MoveToImmutable()), composition.Artwork, sizes);
        return frame;
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
            int objectY = entry.Position.Y >= ScreenGeometry.ScreenHeightPixels ? entry.Position.Y - YRange : entry.Position.Y;
            var flip = (entry.FlipX ? PictureFlip.Horizontal : 0) | (entry.FlipY ? PictureFlip.Vertical : 0);
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
