using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using Vortice.Direct3D11;

namespace SuperMetroid.Rendering.Direct3D11.Frames;

/// <summary>
/// The shader's <c>Colours</c> buffer (#1408, #1512): the scene's backdrop, then the word of each colour slot's table, then the
/// tables. A table is the colour each of the slot's image's inks shows under the slot's shading. Packed every frame, as
/// colour effects change what the inks show.
/// </summary>
internal sealed class D3D11SceneColourBuffer : IDisposable
{
    /// <summary>Allocation for a scene with a few images; larger scenes grow it.</summary>
    private const int InitialWords = 1024;

    /// <summary>The device the buffer lives on.</summary>
    private readonly ID3D11Device device;

    /// <summary>The colours of the picture being packed, reused across pictures and frames.</summary>
    private RgbColor[] inkScratch = [];

    /// <summary>Creates the initial buffer.</summary>
    internal D3D11SceneColourBuffer(ID3D11Device device)
    {
        this.device = Ensure.NotNull(device);
        Create(InitialWords);
    }

    /// <summary>The packed words of the last <see cref="Pack"/>; its length is the GPU buffer's.</summary>
    internal uint[] Upload { get; private set; }
    /// <summary>The structured buffer.</summary>
    internal ID3D11Buffer Buffer { get; private set; }
    /// <summary>The shader view bound at t0.</summary>
    internal ID3D11ShaderResourceView View { get; private set; }

    /// <summary>
    /// Packs <paramref name="frame"/>'s backdrop and each slot's ink colours under its shading, growing the buffer to fit.
    /// </summary>
    /// <param name="frame">What the frame paints with: its backdrop and, for a gameplay frame, each picture group's shading.</param>
    /// <param name="slots">The images the scene's pictures were packed with, in slot order.</param>
    /// <exception cref="InvalidOperationException">
    /// A slot names no shading of its own and is no gameplay picture group's, or is a gameplay group's in a frame with no
    /// gameplay shading.
    /// </exception>
    internal void Pack(FramePaint frame, IReadOnlyList<D3D11ColourSlot> slots)
    {
        Ensure.NotNull(frame);
        // Every frame packs every picture's colours, so the packing reuses its scratch rather than allocating per picture.
        int words = 1 + slots.Count, mostInks = 0;
        for (int slot = 0; slot < slots.Count; slot++)
        {
            int inkCount = slots[slot].Sheet.StandardColours.Length;
            words += inkCount;
            mostInks = Math.Max(mostInks, inkCount);
        }
        if (words > Upload.Length)
        {
            Dispose();
            Create(Math.Max(words, Upload.Length * 2));
        }
        if (mostInks > inkScratch.Length)
            inkScratch = new RgbColor[mostInks];
        Span<uint> upload = Upload;
        upload[0] = Shown(frame.Backdrop);
        int table = 1 + slots.Count;
        for (int slot = 0; slot < slots.Count; slot++)
        {
            upload[1 + slot] = (uint)table;
            Span<RgbColor> inks = inkScratch.AsSpan(0, slots[slot].Sheet.StandardColours.Length);
            Shading(frame, slots[slot]).InkColours(slots[slot].Sheet, inks);
            for (int ink = 0; ink < inks.Length; ink++)
                upload[table + ink] = Shown(inks[ink]);
            table += inks.Length;
        }
    }

    /// <summary>The shading <paramref name="slot"/>'s pictures show: its own, or its group's in a gameplay frame's shading (#1512).</summary>
    /// <exception cref="InvalidOperationException">
    /// The slot names no shading of its own and is no gameplay picture group's, or is a gameplay group's in a frame with no
    /// gameplay shading.
    /// </exception>
    private static PictureShading Shading(FramePaint frame, D3D11ColourSlot slot)
    {
        if (slot.Shading is { } own)
            return own;
        if (D3D11ColourSlot.GameplayGroupOf(slot.Colouring) is not { } group)
            throw new InvalidOperationException($"Picture {slot.Sheet.Name} names no shading and belongs to no gameplay picture group.");
        return (frame.Gameplay ?? throw new InvalidOperationException(
            $"Picture {slot.Sheet.Name} is coloured as gameplay's {group} in a frame with no gameplay shading.")).Of(group);
    }

    /// <summary>A colour as the shaders write an opaque output pixel.</summary>
    private static uint Shown(RgbColor colour)
    {
        RgbaPixel shown = colour.ToRgbaPixel();
        return shown.R | (uint)shown.G << 8 | (uint)shown.B << 16 | (uint)byte.MaxValue << 24;
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
