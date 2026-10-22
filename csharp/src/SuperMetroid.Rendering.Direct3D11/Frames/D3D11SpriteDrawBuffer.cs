using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Spatial;
using Vortice.Direct3D11;

namespace SuperMetroid.Rendering.Direct3D11.Frames;

/// <summary>
/// The shader's <c>SpriteDraws</c> buffer: one frame's whole-frame sprite draws packed as Objects.hlsli reads them.
/// Word 0 holds the record count, then four words per record in front-to-back order: a character draw's parts, or one
/// record per image draw. Then each bound artwork's characters and each drawn sheet frame's pixels, once each, so a
/// large sheet costs only the frames a scene shows. The GPU buffer grows to fit the largest frame seen.
/// </summary>
internal sealed class D3D11SpriteDrawBuffer : IDisposable
{
    /// <summary>Words of the header holding the record count.</summary>
    private const int HeaderWords = 1;
    /// <summary>Words per packed record: position, extent, flags, first character or pixel word.</summary>
    private const int RecordWords = 4;
    /// <summary>Words per four-bit planar character.</summary>
    private const int CharacterWords = SpriteArtwork.CharacterByteCount / sizeof(uint);
    /// <summary>Allocation for a frame with no or few draws; larger frames grow it.</summary>
    private const int InitialWords = 1024;
    /// <summary>Flags bit mirroring a record horizontally.</summary>
    private const uint FlipXFlag = 1;
    /// <summary>Flags bit mirroring a record vertically.</summary>
    private const uint FlipYFlag = 2;
    /// <summary>Flags position of a character part's three-bit OBJ palette.</summary>
    private const int PaletteShift = 2;
    /// <summary>Flags position of the two-bit priority.</summary>
    private const int PriorityShift = 5;
    /// <summary>Flags bit marking an image record, whose last word points at pre-coloured pixels.</summary>
    private const uint ImageFlag = 1 << 7;
    /// <summary>Flags bit marking an image that takes part in colour blending.</summary>
    private const uint BlendsFlag = 1 << 8;
    /// <summary>Flags bit marking an image whose uploaded words are palette indices, because a palette effect changed a row it uses.</summary>
    private const uint RecolourFlag = 1 << 9;
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
    /// <summary>The GPU buffer.</summary>
    internal ID3D11Buffer Buffer { get; private set; }
    /// <summary>Shader view bound at register t1.</summary>
    internal ID3D11ShaderResourceView View { get; private set; }

    /// <summary>Packs a frame's draws in emission order, growing the buffer first when they do not fit.</summary>
    /// <param name="draws">The frame's draws.</param>
    /// <param name="cgram">The frame's 256 colours, against which each image frame's standard colours are checked.</param>
    internal void Pack(ReadOnlySpan<SpriteDraw> draws, ReadOnlySpan<Bgr555> cgram)
    {
        int records = 0;
        foreach (SpriteDraw draw in draws)
            records += draw switch
            {
                CharacterSpriteDraw character => character.Frame.Parts.Length,
                ImageSpriteDraw => 1,
                _ => throw Unsupported(draw),
            };
        int words = HeaderWords + records * RecordWords;
        var artworkWords = new Dictionary<SpriteArtwork, int>(ReferenceEqualityComparer.Instance);
        var imageWords = new Dictionary<SpriteSheetFrame, int>(ReferenceEqualityComparer.Instance);
        var recoloured = new HashSet<SpriteSheetFrame>(ReferenceEqualityComparer.Instance);
        foreach (SpriteDraw draw in draws)
        {
            if (draw is CharacterSpriteDraw character && artworkWords.TryAdd(character.Artwork, words))
                words += character.Artwork.CharacterCount * CharacterWords;
            else if (draw is ImageSpriteDraw image && imageWords.TryAdd(image.Image, words))
            {
                words += image.Image.Size.X * image.Image.Size.Y;
                if (!image.Image.ShowsStandardColours(cgram))
                    recoloured.Add(image.Image);
            }
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
            if (draw is ImageSpriteDraw image)
            {
                PackImage(upload.Slice(record, RecordWords), image, imageWords[image.Image], recoloured.Contains(image.Image));
                record += RecordWords;
                continue;
            }
            var character = (CharacterSpriteDraw)draw;
            int artwork = artworkWords[character.Artwork];
            for (int index = 0; index < character.Frame.Parts.Length; index++)
            {
                SpriteScreenPart part = character.Place(index);
                upload[record] = D3D11ConstantPacking.Halves(unchecked((ushort)part.Position.X), unchecked((ushort)part.Position.Y));
                upload[record + 1] = D3D11ConstantPacking.Halves(part.Size.X, part.Size.Y);
                upload[record + 2] = (part.FlipX ? FlipXFlag : 0) | (part.FlipY ? FlipYFlag : 0)
                    | (uint)part.Palette << PaletteShift | (uint)part.Priority << PriorityShift;
                upload[record + 3] = (uint)(artwork + part.Character * CharacterWords);
                record += RecordWords;
            }
        }
        foreach ((SpriteArtwork artwork, int offset) in artworkWords)
            System.Runtime.InteropServices.MemoryMarshal.Cast<byte, uint>(artwork.Characters)
                .CopyTo(upload[offset..]);
        foreach ((SpriteSheetFrame frame, int offset) in imageWords)
        {
            ReadOnlySpan<Rgba32> pixels = frame.Sheet.Pixels;
            ReadOnlySpan<byte> indices = frame.Sheet.PaletteIndices;
            bool indexed = recoloured.Contains(frame);
            int target = offset;
            for (int row = 0; row < frame.Size.Y; row++)
            {
                int source = (frame.Corner.Y + row) * frame.Sheet.Size.X + frame.Corner.X;
                for (int column = 0; column < frame.Size.X; column++, target++)
                {
                    Rgba32 pixel = pixels[source + column];
                    upload[target] = indexed ? indices[source + column] : D3D11ConstantPacking.Bytes(pixel.R, pixel.G, pixel.B, pixel.A);
                }
            }
        }
    }

    /// <summary>Packs one image draw: its screen rectangle, mirroring, priority, blending, recolouring and its frame's uploaded words.</summary>
    private static void PackImage(Span<uint> record, ImageSpriteDraw draw, int frameWords, bool recolour)
    {
        Offset16<ScreenPixels> corner = draw.Corner;
        SpriteSheetFrame frame = draw.Image;
        record[0] = D3D11ConstantPacking.Halves(unchecked((ushort)corner.X), unchecked((ushort)corner.Y));
        record[1] = D3D11ConstantPacking.Halves(frame.Size.X, frame.Size.Y);
        record[2] = ((draw.Mirror & SpriteMirror.Horizontal) != 0 ? FlipXFlag : 0) | ((draw.Mirror & SpriteMirror.Vertical) != 0 ? FlipYFlag : 0)
            | (uint)draw.Priority << PriorityShift | ImageFlag | (frame.Blends ? BlendsFlag : 0) | (recolour ? RecolourFlag : 0) | (uint)frame.Size.X << StrideShift;
        record[3] = (uint)frameWords;
    }

    /// <summary>The error for a kind of draw this buffer cannot pack.</summary>
    private static InvalidDataException Unsupported(SpriteDraw draw) =>
        new($"The Direct3D 11 sprite buffer cannot pack a {draw.GetType().Name}.");

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
