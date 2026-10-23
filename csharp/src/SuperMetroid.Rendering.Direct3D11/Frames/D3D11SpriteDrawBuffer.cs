using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Spatial;
using Vortice.Direct3D11;

namespace SuperMetroid.Rendering.Direct3D11.Frames;

/// <summary>
/// The shader's <c>SpriteDraws</c> buffer: one frame's whole-frame sprite draws packed as Objects.hlsli reads them.
/// Word 0 holds the record count, then five words per image draw in front-to-back order, then each drawn sheet frame's
/// pixels, once each, so a large sheet costs only the frames a scene shows, then each indexed draw's colours (#1405).
/// The GPU buffer grows to fit the largest frame seen.
/// </summary>
internal sealed class D3D11SpriteDrawBuffer : IDisposable
{
    /// <summary>Words of the header holding the record count.</summary>
    private const int HeaderWords = 1;
    /// <summary>Words per packed record: position, extent, flags, first pixel word, first colour word.</summary>
    private const int RecordWords = 5;
    /// <summary>
    /// Words of an indexed draw's colours: a word of one blending bit per key, with the part-row keying flag above
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
    /// <summary>Flags bit marking an image that takes part in colour blending.</summary>
    private const int BlendingRow = 4;
    /// <summary>Flags bit marking a draw whose frame uploads palette indices and whose colours follow its frame's pixels.</summary>
    private const uint RecolourFlag = 1 << 9;

    /// <summary>Index word bits 10-13: the pixel's part palette row and its fixed-row flag.</summary>
    private const int PartRowShift = 10;
    /// <summary>Flags position of an image record's frame width, the stride between its uploaded pixel rows.</summary>
    private const int StrideShift = 16;

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
    /// <summary>Each frame's first upload word, by image frame; reused across packs so a frame allocates nothing.</summary>
    private readonly Dictionary<SpriteSheetFrame, int> imageWords = new(ReferenceEqualityComparer.Instance);

    /// <summary>The image frames that upload their palette indices, because a draw recolours them; reused across packs.</summary>
    private readonly HashSet<SpriteSheetFrame> recoloured = new(ReferenceEqualityComparer.Instance);

    /// <summary>The GPU buffer.</summary>
    internal ID3D11Buffer Buffer { get; private set; }
    /// <summary>Shader view bound at register t1.</summary>
    internal ID3D11ShaderResourceView View { get; private set; }

    /// <summary>Packs a frame's draws in emission order, growing the buffer first when they do not fit.</summary>
    /// <param name="draws">The frame's draws, each with its colours resolved.</param>
    /// <exception cref="InvalidDataException">A draw is not an image draw, or its colours were not resolved.</exception>
    internal void Pack(ReadOnlySpan<SpriteDraw> draws)
    {
        foreach (SpriteDraw draw in draws)
        {
            if (draw is not ImageSpriteDraw { Colours: not null })
                throw Unsupported(draw);
        }
        int records = draws.Length;
        int words = HeaderWords + records * RecordWords;
        imageWords.Clear();
        recoloured.Clear();
        foreach (SpriteDraw draw in draws)
        {
            var image = (ImageSpriteDraw)draw;
            if (imageWords.TryAdd(image.Image, words))
                words += image.Image.Size.X * image.Image.Size.Y;
            // A frame a draw shows in other colours, or blending otherwise, uploads its indices.
            if (!image.Colours!.IsOwn)
                recoloured.Add(image.Image);
        }
        int colourWords = words;
        foreach (SpriteDraw draw in draws)
        {
            if (recoloured.Contains(((ImageSpriteDraw)draw).Image))
                words += ColourWords;
        }
        if (words > Upload.Length)
        {
            Dispose();
            Create(Math.Max(words, Upload.Length * 2));
        }
        Span<uint> upload = Upload;
        upload.Clear();
        upload[0] = (uint)records;
        int record = HeaderWords;
        foreach (SpriteDraw draw in draws)
        {
            var image = (ImageSpriteDraw)draw;
            int colours = 0;
            if (recoloured.Contains(image.Image))
            {
                colours = colourWords;
                PackColours(upload.Slice(colourWords, ColourWords), image);
                colourWords += ColourWords;
            }
            PackImage(upload.Slice(record, RecordWords), image, imageWords[image.Image], colours);
            record += RecordWords;
        }
        foreach ((SpriteSheetFrame frame, int offset) in imageWords)
        {
            ReadOnlySpan<Rgba32> pixels = frame.Sheet.Pixels;
            ReadOnlySpan<SpriteImageInk> inks = frame.Sheet.Inks;
            bool indexed = recoloured.Contains(frame);
            int target = offset;
            for (int row = 0; row < frame.Size.Y; row++)
            {
                int source = (frame.Corner.Y + row) * frame.Sheet.Size.X + frame.Corner.X;
                for (int column = 0; column < frame.Size.X; column++, target++)
                {
                    Rgba32 pixel = pixels[source + column];
                    SpriteImageInk ink = inks[source + column];
                    // An index word keeps its priority and part row above the index; a colour word keeps the priority,
                    // plus one, in the alpha byte, whose zero stays transparent, plus four where the pixel's palette
                    // row takes part in colour math.
                    uint priority = (uint)ink.Priority;
                    byte alpha = pixel.A == 0 ? (byte)0
                        : (byte)(priority + 1 + (ink.PaletteIndex / 16 >= BlendingRow ? BlendingRow : 0));
                    upload[target] = indexed
                        ? (uint)ink.PaletteIndex | priority << 8 | (uint)ink.PartRow << PartRowShift
                        : D3D11ConstantPacking.Bytes(pixel.R, pixel.G, pixel.B, alpha);
                }
            }
        }
    }

    /// <summary>
    /// Packs one image draw: its screen rectangle, mirroring and recolouring, its frame's uploaded words and, for an
    /// indexed frame, its colours' first word.
    /// </summary>
    private static void PackImage(Span<uint> record, ImageSpriteDraw draw, int frameWords, int colourWords)
    {
        Offset16<ScreenPixels> corner = draw.Corner;
        SpriteSheetFrame frame = draw.Image;
        record[0] = D3D11ConstantPacking.Halves(unchecked((ushort)corner.X), unchecked((ushort)corner.Y));
        record[1] = D3D11ConstantPacking.Halves(frame.Size.X, frame.Size.Y);
        record[2] = ((draw.Mirror & SpriteMirror.Horizontal) != 0 ? FlipXFlag : 0) | ((draw.Mirror & SpriteMirror.Vertical) != 0 ? FlipYFlag : 0)
            | (colourWords != 0 ? RecolourFlag : 0) | (uint)frame.Size.X << StrideShift;
        record[3] = (uint)frameWords;
        record[4] = (uint)colourWords;
    }

    /// <summary>
    /// Packs an indexed draw's colours: one blending bit per key and the part-row keying flag, then each key's sixteen
    /// colours, its image's own where the key shows them.
    /// </summary>
    private static void PackColours(Span<uint> words, ImageSpriteDraw draw)
    {
        DrawColours colours = draw.Colours!;
        ReadOnlySpan<Bgr555> own = draw.Image.Sheet.StandardColours;
        uint header = colours.FollowsPartRows ? FollowsPartRowsFlag : 0;
        int keys = colours.FollowsPartRows ? DrawColours.Keys : SpriteImage.MaximumPaletteRow + 1;
        for (int key = 0; key < keys; key++)
        {
            if (colours.Blends(key))
                header |= 1u << key;
            for (int colour = 0; colour < DrawColours.RowColours; colour++)
            {
                Rgba32 shown = (colours.ShowsOwnColours(key)
                    ? own[key * DrawColours.RowColours + colour]
                    : colours.Colour(key, colour)).ToRgba32();
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
