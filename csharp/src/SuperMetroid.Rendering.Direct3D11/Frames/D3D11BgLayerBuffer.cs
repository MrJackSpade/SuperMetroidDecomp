using SuperMetroid.Core.Hardware;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using Vortice.Direct3D11;

namespace SuperMetroid.Rendering.Direct3D11.Frames;

/// <summary>
/// The scene's published background layers packed for Tiles.hlsl at register t2: a header of layer records, then each
/// layer's cells, then each distinct artwork's characters. Layers are addressed by their index in packing order.
/// </summary>
internal sealed class D3D11BgLayerBuffer : IDisposable
{
    /// <summary>Header words before the layer records: the layer count.</summary>
    private const int HeaderWords = 1;
    /// <summary>Words per layer record: origin, size, scanline band, bits per pixel, cell offset, artwork offset.</summary>
    private const int RecordWords = 6;
    /// <summary>Initial buffer size; it grows to fit larger scenes.</summary>
    private const int InitialWords = 1024;
    /// <summary>Cell word bit of the palette field.</summary>
    private const int PaletteShift = 16;
    /// <summary>Cell word bit of the priority flag.</summary>
    private const int PriorityShift = 19;
    /// <summary>Cell word bit of the horizontal flip.</summary>
    private const int FlipXShift = 20;
    /// <summary>Cell word bit of the vertical flip.</summary>
    private const int FlipYShift = 21;

    /// <summary>The device the buffer lives on.</summary>
    private readonly ID3D11Device device;

    /// <summary>Creates an empty buffer.</summary>
    internal D3D11BgLayerBuffer(ID3D11Device device)
    {
        this.device = Ensure.NotNull(device);
        Create(InitialWords);
    }

    /// <summary>The packed words of the last <see cref="Pack"/>.</summary>
    internal uint[] Upload { get; private set; }
    /// <summary>The structured buffer.</summary>
    internal ID3D11Buffer Buffer { get; private set; }
    /// <summary>The shader view bound at t2.</summary>
    internal ID3D11ShaderResourceView View { get; private set; }

    /// <summary>Packs <paramref name="layers"/> in order; a layer's index in the list is its shader index.</summary>
    /// <param name="layers">The scene's background layers.</param>
    internal void Pack(IReadOnlyList<BgLayer> layers)
    {
        Ensure.NotNull(layers);
        var artworkWords = new Dictionary<BgArtwork, int>(ReferenceEqualityComparer.Instance);
        int words = HeaderWords + layers.Count * RecordWords;
        var cellOffsets = new int[layers.Count];
        for (int index = 0; index < layers.Count; index++)
        {
            cellOffsets[index] = words;
            words += layers[index].Cells.Length;
        }
        foreach (BgLayer layer in layers)
            if (artworkWords.TryAdd(layer.Artwork, words))
                words += layer.Artwork.Characters.Length / sizeof(uint);
        if (words > Upload.Length)
        {
            Dispose();
            Create(Math.Max(words, Upload.Length * 2));
        }

        Span<uint> upload = Upload;
        upload.Clear();
        upload[0] = (uint)layers.Count;
        for (int index = 0; index < layers.Count; index++)
        {
            BgLayer layer = layers[index];
            int record = HeaderWords + index * RecordWords;
            upload[record] = D3D11ConstantPacking.Halves(layer.Origin.X, layer.Origin.Y);
            upload[record + 1] = D3D11ConstantPacking.Halves(layer.Size.X, layer.Size.Y);
            upload[record + 2] = D3D11ConstantPacking.Halves(checked((ushort)layer.FirstScanline), checked((ushort)layer.EndScanline));
            upload[record + 3] = (uint)layer.Artwork.BitsPerPixel;
            upload[record + 4] = (uint)cellOffsets[index];
            upload[record + 5] = (uint)artworkWords[layer.Artwork];
            for (int cell = 0; cell < layer.Cells.Length; cell++)
            {
                BgCell value = layer.Cells[cell];
                upload[cellOffsets[index] + cell] = (uint)value.Character | (uint)value.Palette << PaletteShift
                    | (value.Priority ? 1u << PriorityShift : 0)
                    | ((value.Flip & SnesTileFlipFlags.Horizontal) != 0 ? 1u << FlipXShift : 0)
                    | ((value.Flip & SnesTileFlipFlags.Vertical) != 0 ? 1u << FlipYShift : 0);
            }
        }
        foreach ((BgArtwork artwork, int offset) in artworkWords)
            System.Runtime.InteropServices.MemoryMarshal.Cast<byte, uint>(artwork.Characters).CopyTo(upload[offset..]);
    }

    /// <summary>Creates the buffer and its view at <paramref name="words"/> words.</summary>
    [System.Diagnostics.CodeAnalysis.MemberNotNull(nameof(Buffer), nameof(View), nameof(Upload))]
    private void Create(int words)
    {
        Buffer = device.CreateBuffer(new BufferDescription(checked((uint)(words * sizeof(uint))), BindFlags.ShaderResource,
            ResourceUsage.Default, CpuAccessFlags.None, ResourceOptionFlags.BufferStructured, sizeof(uint)));
        View = device.CreateShaderResourceView(Buffer);
        Upload = new uint[words];
    }

    /// <summary>Releases the buffer and its view.</summary>
    public void Dispose()
    {
        View.Dispose();
        Buffer.Dispose();
    }
}
