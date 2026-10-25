using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Spatial;
using Vortice.Direct3D11;

namespace SuperMetroid.Rendering.Direct3D11.Frames;

/// <summary>
/// The shaders' <c>SpriteDraws</c> buffer (#1408): one frame's whole-frame sprite draws, each an instanced quad that
/// Sprites.hlsl draws from its image in the image texture. Word 0 holds the record count, then five words per draw in
/// front-to-back order (position, extent, flags, the image's place in the texture, the first colour word), then each
/// recoloured draw's colours (#1405). The GPU buffer grows to fit the largest frame seen.
/// </summary>
internal sealed class D3D11SpriteDrawBuffer : IDisposable
{
    /// <summary>Words of the header holding the record count.</summary>
    private const int HeaderWords = 1;
    /// <summary>Words per packed record: position, extent, flags, the image's place, first colour word.</summary>
    private const int RecordWords = 5;
    /// <summary>
    /// Words of a recoloured draw's colours: a word of one blending bit per key, with the part-row keying flag above
    /// them, then each key's sixteen colours.
    /// </summary>
    private const int ColourWords = 1 + DrawColours.Keys * DrawColours.RowColours;
    /// <summary>Colour header bit: the draw's pixels take their part row's colours rather than their ink row's.</summary>
    private const uint FollowsPartRowsFlag = 1 << 16;
    /// <summary>Allocation for a frame with no or few draws; larger frames grow it.</summary>
    private const int InitialWords = 1024;
    /// <summary>Flags bit mirroring a record horizontally.</summary>
    private const uint FlipXFlag = 1;
    /// <summary>Flags bit mirroring a record vertically.</summary>
    private const uint FlipYFlag = 2;
    /// <summary>Flags bit marking a draw whose pixels show its colours rather than its image's own.</summary>
    private const uint RecolourFlag = 1 << 9;

    /// <summary>Device that owns the buffer.</summary>
    private readonly ID3D11Device device;

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
    /// <exception cref="InvalidDataException">A draw is not an image draw, or its colours were not resolved.</exception>
    internal static void Collect(ReadOnlySpan<SpriteDraw> draws, ISet<SpriteSheetFrame> needed)
    {
        foreach (SpriteDraw draw in draws)
        {
            if (draw is not ImageSpriteDraw { Colours: not null } image)
                throw Unsupported(draw);
            needed.Add(image.Image);
        }
    }

    /// <summary>Packs the draws in emission order, growing the buffer first when they do not fit.</summary>
    /// <param name="draws">The frame's draws, each with its colours resolved.</param>
    /// <param name="atlas">The texture holding every image the draws show.</param>
    internal void Pack(ReadOnlySpan<SpriteDraw> draws, D3D11ImageAtlas atlas)
    {
        int words = HeaderWords + draws.Length * RecordWords;
        foreach (SpriteDraw draw in draws)
        {
            if (!((ImageSpriteDraw)draw).Colours!.IsOwn)
                words += ColourWords;
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
        foreach (SpriteDraw draw in draws)
        {
            var image = (ImageSpriteDraw)draw;
            int colours = 0;
            if (!image.Colours!.IsOwn)
            {
                colours = colourWords;
                PackColours(upload.Slice(colourWords, ColourWords), image);
                colourWords += ColourWords;
            }
            PackImage(upload.Slice(record, RecordWords), image, atlas.Place(image.Image), colours);
            record += RecordWords;
        }
    }

    /// <summary>
    /// Packs one image draw: its screen rectangle, mirroring and recolouring, its image's place in the texture and, for
    /// a recoloured draw, its colours' first word.
    /// </summary>
    private static void PackImage(Span<uint> record, ImageSpriteDraw draw, uint place, int colourWords)
    {
        Offset16<ScreenPixels> corner = draw.Corner;
        SpriteSheetFrame frame = draw.Image;
        record[0] = D3D11ConstantPacking.Halves(unchecked((ushort)corner.X), unchecked((ushort)corner.Y));
        record[1] = D3D11ConstantPacking.Halves(frame.Size.X, frame.Size.Y);
        record[2] = ((draw.Mirror & SpriteMirror.Horizontal) != 0 ? FlipXFlag : 0) | ((draw.Mirror & SpriteMirror.Vertical) != 0 ? FlipYFlag : 0)
            | (colourWords != 0 ? RecolourFlag : 0);
        record[3] = place;
        record[4] = (uint)colourWords;
    }

    /// <summary>
    /// Packs a recoloured draw's colours: one blending bit per key and the part-row keying flag, then each key's sixteen
    /// colours, its image's own where the key shows them.
    /// </summary>
    private static void PackColours(Span<uint> words, ImageSpriteDraw draw)
    {
        DrawColours colours = draw.Colours!;
        ReadOnlySpan<RgbColor> own = draw.Image.Sheet.StandardColours;
        uint header = colours.FollowsPartRows ? FollowsPartRowsFlag : 0;
        int keys = colours.FollowsPartRows ? DrawColours.Keys : SpriteImage.MaximumPaletteRow + 1;
        for (int key = 0; key < keys; key++)
        {
            if (colours.Blends(key))
                header |= 1u << key;
            for (int colour = 0; colour < DrawColours.RowColours; colour++)
            {
                RgbaPixel shown = (colours.ShowsOwnColours(key)
                    ? own[key * DrawColours.RowColours + colour]
                    : colours.Colour(key, colour)).ToRgbaPixel();
                words[1 + key * DrawColours.RowColours + colour] = D3D11ConstantPacking.Bytes(shown.R, shown.G, shown.B, byte.MaxValue);
            }
        }
        words[0] = header;
    }

    /// <summary>The error for a kind of draw this buffer cannot pack.</summary>
    private static InvalidDataException Unsupported(SpriteDraw draw) => draw is ImageSpriteDraw
        ? new("A sprite draw reaches the Direct3D 11 sprite buffer once its frame has resolved its colours.")
        : new($"The Direct3D 11 sprite buffer cannot pack a {draw.GetType().Name}.");

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
