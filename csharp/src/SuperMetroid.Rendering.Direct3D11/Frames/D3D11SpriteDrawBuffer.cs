using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Spatial;
using Vortice.Direct3D11;

namespace SuperMetroid.Rendering.Direct3D11.Frames;

/// <summary>
/// The shaders' <c>SpriteDraws</c> buffer (#1408, #1512): one frame's whole-frame sprite draws, each an instanced quad that
/// Sprites.hlsl draws from its image in the image texture. Word 0 holds the record count, then six words per draw in
/// front-to-back order (position, extent, flags with the draw's depth among the room's layers (#1514), the image's place in
/// the texture, the first colour word, the first corpse-column word), then, for each draw whose shading changes its picture's colours, the colour of each of its
/// picture's inks under that shading, then, for each rotting corpse's draw (#1531), the first word of each of its image's
/// character columns' table of rows, then each of those tables: the image row each of the image's rows shows in the
/// column, or <see cref="ClearedRow"/> where the rot has cleared it. Which of a sprite's pixels blend
/// is what its image says of each pixel (#1544). The GPU buffer grows to fit the largest frame seen.
/// </summary>
internal sealed class D3D11SpriteDrawBuffer : IDisposable
{
    /// <summary>Words of the header holding the record count.</summary>
    private const int HeaderWords = 1;
    /// <summary>Words per packed record: position, extent, flags, the image's place, first colour word, first corpse-column word.</summary>
    private const int RecordWords = 6;
    /// <summary>A rotting-row word naming a row the rot has cleared, which draws nothing.</summary>
    private const uint ClearedRow = uint.MaxValue;
    /// <summary>Allocation for a frame with no or few draws; larger frames grow it.</summary>
    private const int InitialWords = 1024;
    /// <summary>Flags bit mirroring a record horizontally.</summary>
    private const uint FlipXFlag = 1;
    /// <summary>Flags bit mirroring a record vertically.</summary>
    private const uint FlipYFlag = 2;
    /// <summary>Flags bit marking a draw whose pixels show its shading's ink colours rather than its image's own.</summary>
    private const uint RecolourFlag = 1 << 9;
    /// <summary>Flags bit marking a draw whose shading blends over every part (#1512).</summary>
    private const uint ShadingBlendsFlag = 1 << 10;
    /// <summary>Where the flags keep the draw's depth among the room's layers (#1514), two bits; Sprites.hlsl's SpriteDepthShift.</summary>
    private const int DepthShift = 3;

    /// <summary>Device that owns the buffer.</summary>
    private readonly ID3D11Device device;

    /// <summary>The ink colours of each shaded draw the last <see cref="Pack"/> packed; reused across frames.</summary>
    private readonly List<RgbColor[]?> shadedInks = [];

    /// <summary>Creates the initial buffer on the render owner's device.</summary>
    internal D3D11SpriteDrawBuffer(ID3D11Device device)
    {
        this.device = Ensure.NotNull(device);
        Create(InitialWords);
    }

    /// <summary>CPU words uploaded whole each frame; its length is the GPU buffer's.</summary>
    internal uint[] Upload { get; private set; }
    /// <summary>The GPU buffer.</summary>
    internal ID3D11Buffer Buffer { get; private set; }
    /// <summary>Shader view bound at register t1.</summary>
    internal ID3D11ShaderResourceView View { get; private set; }
    /// <summary>The draws the last <see cref="Pack"/> packed.</summary>
    internal int Count { get; private set; }

    /// <summary>Adds the image each draw shows to <paramref name="needed"/>.</summary>
    /// <exception cref="InvalidDataException">A draw is not an image draw, or names no shading.</exception>
    internal static void Collect(ReadOnlySpan<SpriteDraw> draws, ISet<SpriteSheetFrame> needed)
    {
        foreach (SpriteDraw draw in draws)
            needed.Add(Shaded(draw).Draw.Image);
    }

    /// <summary>
    /// Packs the draws in the order given, each at its depth among the room's layers (#1514), growing the buffer first when
    /// they do not fit.
    /// </summary>
    /// <param name="draws">The frame's draws, each naming its shading.</param>
    /// <param name="depths">The depth each draw is packed at.</param>
    /// <param name="atlas">The texture holding every image the draws show.</param>
    /// <exception cref="InvalidDataException">A draw is not an image draw, or names no shading.</exception>
    /// <exception cref="ArgumentException">The depths are not one for each draw.</exception>
    internal void Pack(ReadOnlySpan<SpriteDraw> draws, ReadOnlySpan<ActorDepth> depths, D3D11ImageAtlas atlas)
    {
        if (depths.Length != draws.Length)
            throw new ArgumentException("Each packed sprite draw needs its depth.", nameof(depths));
        int words = HeaderWords + draws.Length * RecordWords;
        shadedInks.Clear();
        foreach (SpriteDraw draw in draws)
        {
            (ImageSpriteDraw image, PictureShading shading) = Shaded(draw);
            // A draw in its picture's standard colours shows the image's pre-coloured pixels and packs no colours.
            RgbColor[]? inks = shading.IsStandard ? null : shading.InkColours(image.Image.Sheet);
            shadedInks.Add(inks);
            words += (inks?.Length ?? 0) + CorpseRotWords(image);
        }
        if (words > Upload.Length)
        {
            Dispose();
            Create(Math.Max(words, Upload.Length * 2));
        }
        Span<uint> upload = Upload;
        upload[0] = (uint)draws.Length;
        Count = draws.Length;
        int record = HeaderWords;
        int colourWords = HeaderWords + draws.Length * RecordWords;
        // Each rotting corpse's columns and tables of rows follow every draw's colours.
        int rowWords = colourWords;
        foreach (RgbColor[]? inks in shadedInks)
            rowWords += inks?.Length ?? 0;
        for (int index = 0; index < draws.Length; index++)
        {
            var image = (ImageSpriteDraw)draws[index];
            int colours = 0;
            if (shadedInks[index] is { } inks)
            {
                colours = colourWords;
                for (int ink = 0; ink < inks.Length; ink++)
                {
                    RgbaPixel shown = inks[ink].ToRgbaPixel();
                    upload[colourWords + ink] = D3D11ConstantPacking.Bytes(shown.R, shown.G, shown.B, byte.MaxValue);
                }
                colourWords += inks.Length;
            }
            int rows = 0;
            if (CorpseRotOf(image) is { } rot)
            {
                rows = rowWords;
                int tables = rowWords + rot.Columns;
                for (int column = 0; column < rot.Columns; column++)
                    upload[rowWords + column] = (uint)(tables + rot.TableOf(column) * rot.Rows);
                for (int table = 0; table < rot.Tables; table++)
                {
                    for (int row = 0; row < rot.Rows; row++)
                        upload[tables + table * rot.Rows + row] = rot.Shown(table, row) is int shown ? (uint)shown : ClearedRow;
                }
                rowWords += CorpseRotWords(image);
            }
            PackImage(upload.Slice(record, RecordWords), image, depths[index], atlas.Place(image.Image), colours, rows);
            record += RecordWords;
        }
    }

    /// <summary>
    /// Packs one image draw: its screen rectangle, mirroring, recolouring and depth, its image's place in the texture, for a
    /// recoloured draw its colours' first word, and for a rotting corpse's draw its columns' first word; zero names none,
    /// since the header holds word zero.
    /// </summary>
    private static void PackImage(Span<uint> record, ImageSpriteDraw draw, ActorDepth depth, uint place, int colourWords, int rowWords)
    {
        Offset16<ScreenPixels> corner = draw.Corner;
        SpriteSheetFrame frame = draw.Image;
        record[0] = D3D11ConstantPacking.Halves(unchecked((ushort)corner.X), unchecked((ushort)corner.Y));
        record[1] = D3D11ConstantPacking.Halves(frame.Size.X, frame.Size.Y);
        record[2] = ((draw.Mirror & SpriteMirror.Horizontal) != 0 ? FlipXFlag : 0) | ((draw.Mirror & SpriteMirror.Vertical) != 0 ? FlipYFlag : 0)
            | (colourWords != 0 ? RecolourFlag : 0) | (ShadingBlends(draw) ? ShadingBlendsFlag : 0)
            | D3D11FrameRenderer.DepthWord(depth) << DepthShift;
        record[3] = place;
        record[4] = (uint)colourWords;
        record[5] = (uint)rowWords;
    }

    /// <summary>
    /// How a rotting corpse's <paramref name="draw"/> shows its image (#1531): one table of rows for each of its image's
    /// character columns, each as tall as its image; none for any other draw.
    /// </summary>
    /// <exception cref="InvalidDataException">The columns do not cover the image.</exception>
    private static CorpseRotPicture? CorpseRotOf(ImageSpriteDraw draw)
    {
        if (draw.CorpseRot is not { } rot)
            return null;
        return rot.Rows == draw.Image.Size.Y && rot.Columns * CorpseRotPicture.ColumnPixels == draw.Image.Size.X ? rot
            : throw new InvalidDataException($"Sprite draw {draw} shows {rot.Columns} rotting columns of {rot.Rows} rows over an image {draw.Image.Size.X} by {draw.Image.Size.Y}.");
    }

    /// <summary>The words a rotting corpse's <paramref name="draw"/> packs: a word for each column, then each table; none for any other draw.</summary>
    private static int CorpseRotWords(ImageSpriteDraw draw) => CorpseRotOf(draw) is { } rot ? rot.Columns + rot.Tables * rot.Rows : 0;

    /// <summary>Whether a shade of <paramref name="draw"/>'s shading blends over the draw's every part.</summary>
    private static bool ShadingBlends(ImageSpriteDraw draw)
    {
        if (draw.Shading is not { } shading)
            return false;
        foreach (PictureShade shade in shading.Shades)
        {
            if (draw.Image.Sheet.Shades.TryGetValue(shade, out ShadeInks? inks) && inks.Blends)
                return true;
        }
        return false;
    }

    /// <summary><paramref name="draw"/> as an image draw and the shading it names.</summary>
    /// <exception cref="InvalidDataException">The draw is not an image draw, or names no shading.</exception>
    private static (ImageSpriteDraw Draw, PictureShading Shading) Shaded(SpriteDraw draw) => draw switch
    {
        ImageSpriteDraw { Shading: { } shading } image => (image, shading),
        ImageSpriteDraw image => throw new InvalidDataException($"Sprite draw {image} reaches the Direct3D 11 sprite buffer naming no shading."),
        _ => throw new InvalidDataException($"The Direct3D 11 sprite buffer cannot pack a {draw.GetType().Name}."),
    };

    /// <summary>Allocates the GPU buffer, its view and the CPU upload words.</summary>
    [System.Diagnostics.CodeAnalysis.MemberNotNull(nameof(Buffer), nameof(View), nameof(Upload))]
    private void Create(int words)
    {
        Buffer = device.CreateBuffer(new BufferDescription(checked((uint)(words * sizeof(uint))), BindFlags.ShaderResource,
            ResourceUsage.Default, CpuAccessFlags.None, ResourceOptionFlags.BufferStructured, sizeof(uint)));
        View = device.CreateShaderResourceView(Buffer);
        Upload = new uint[words];
    }

    /// <summary>Releases the GPU buffer and its view.</summary>
    public void Dispose()
    {
        View.Dispose();
        Buffer.Dispose();
    }
}
