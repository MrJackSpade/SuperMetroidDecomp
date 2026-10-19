using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using Vortice.Direct3D11;

namespace SuperMetroid.Rendering.Direct3D11.Frames;

/// <summary>
/// The shader's <c>SpriteDraws</c> buffer: one frame's whole-frame sprite draws packed as Objects.hlsli reads them.
/// Word 0 holds the part count, then three words per part in front-to-back order, then each bound artwork's
/// characters once. The GPU buffer grows to fit the largest frame seen.
/// </summary>
internal sealed class D3D11SpriteDrawBuffer : IDisposable
{
    /// <summary>Words of the header holding the part count.</summary>
    private const int HeaderWords = 1;
    /// <summary>Words per packed part: position, shape and flags, first character word.</summary>
    private const int PartWords = 3;
    /// <summary>Words per four-bit planar character.</summary>
    private const int CharacterWords = SpriteArtwork.CharacterByteCount / sizeof(uint);
    /// <summary>Allocation for a frame with no or few draws; larger frames grow it.</summary>
    private const int InitialWords = 1024;
    /// <summary>Flags-byte bit mirroring a part horizontally (shape-word bit 16).</summary>
    private const byte FlipXFlag = 1;
    /// <summary>Flags-byte bit mirroring a part vertically (shape-word bit 17).</summary>
    private const byte FlipYFlag = 2;
    /// <summary>Flags-byte position of the three-bit OBJ palette (shape-word bits 18..20).</summary>
    private const int PaletteShift = 2;
    /// <summary>Flags-byte position of the two-bit priority (shape-word bits 21..22).</summary>
    private const int PriorityShift = 5;

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
    internal void Pack(ReadOnlySpan<SpriteDraw> draws)
    {
        int parts = 0;
        var artworkWords = new Dictionary<SpriteArtwork, int>(ReferenceEqualityComparer.Instance);
        foreach (SpriteDraw draw in draws)
            parts += draw.Frame.Parts.Length;
        int words = HeaderWords + parts * PartWords;
        foreach (SpriteDraw draw in draws)
            if (artworkWords.TryAdd(draw.Artwork, words))
                words += draw.Artwork.CharacterCount * CharacterWords;
        if (words > Upload.Length)
        {
            Dispose();
            Create(Math.Max(words, Upload.Length * 2));
        }

        Span<uint> upload = Upload;
        upload.Clear();
        upload[0] = (uint)parts;
        int record = HeaderWords;
        foreach (SpriteDraw draw in draws)
        {
            int artwork = artworkWords[draw.Artwork];
            for (int index = 0; index < draw.Frame.Parts.Length; index++)
            {
                SpriteScreenPart part = draw.Place(index);
                upload[record] = D3D11ConstantPacking.Halves(unchecked((ushort)part.Position.X), unchecked((ushort)part.Position.Y));
                byte flags = (byte)((part.FlipX ? FlipXFlag : 0) | (part.FlipY ? FlipYFlag : 0)
                    | part.Palette << PaletteShift | part.Priority << PriorityShift);
                upload[record + 1] = D3D11ConstantPacking.Bytes(checked((byte)part.Size.X), checked((byte)part.Size.Y), flags, 0);
                upload[record + 2] = (uint)(artwork + part.Character * CharacterWords);
                record += PartWords;
            }
        }
        foreach ((SpriteArtwork artwork, int offset) in artworkWords)
            System.Runtime.InteropServices.MemoryMarshal.Cast<byte, uint>(artwork.Characters)
                .CopyTo(upload[offset..]);
    }

    /// <summary>Creates the GPU buffer, its view and the CPU words for <paramref name="words"/> words.</summary>
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
