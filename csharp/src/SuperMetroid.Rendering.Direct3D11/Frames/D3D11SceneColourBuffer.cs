using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using Vortice.Direct3D11;

namespace SuperMetroid.Rendering.Direct3D11.Frames;

/// <summary>
/// The shader's <c>Colours</c> buffer (#1408): the scene's backdrop, then the word of each colour slot's table, then the
/// tables. A table is a word of one bit per sixteen-ink group still showing its image's own colours, then the colour each
/// of the image's inks shows now. Packed every frame, as colour effects change what the inks show.
/// </summary>
internal sealed class D3D11SceneColourBuffer : IDisposable
{
    /// <summary>Allocation for a scene with a few images; larger scenes grow it.</summary>
    private const int InitialWords = 1024;

    /// <summary>The device the buffer lives on.</summary>
    private readonly ID3D11Device device;

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

    /// <summary>Packs the backdrop and each slot's colours under <paramref name="frame"/>, growing the buffer to fit.</summary>
    /// <param name="frame">The colours the frame's background art shows.</param>
    /// <param name="slots">The images the scene's pictures were packed with, in slot order.</param>
    internal void Pack(BackgroundColours frame, IReadOnlyList<D3D11ColourSlot> slots)
    {
        Ensure.NotNull(frame);
        int words = 1 + slots.Count;
        foreach (D3D11ColourSlot slot in slots)
            words += 1 + BackgroundColours.Groups(slot.Sheet) * BackgroundColours.GroupColours;
        if (words > Upload.Length)
        {
            Dispose();
            Create(Math.Max(words, Upload.Length * 2));
        }
        Span<uint> upload = Upload;
        upload[0] = Shown(frame.Backdrop);
        BackgroundColours? messageText = null;
        int table = 1 + slots.Count;
        for (int slot = 0; slot < slots.Count; slot++)
        {
            SpriteImage sheet = slots[slot].Sheet;
            BackgroundColours colours = slots[slot].Colouring switch
            {
                D3D11PictureColouring.Frame => frame,
                D3D11PictureColouring.MessageText => messageText ??= MessageBoxRenderLayer.ShownIn(frame),
                _ => throw new InvalidDataException($"Unknown picture colouring {slots[slot].Colouring}."),
            };
            upload[1 + slot] = (uint)table;
            int groups = BackgroundColours.Groups(sheet);
            uint own = 0;
            for (int group = 0; group < groups; group++)
            {
                if (colours.ShowsOwnColours(sheet, group))
                    own |= 1u << group;
            }
            upload[table] = own;
            int inks = groups * BackgroundColours.GroupColours;
            for (int ink = 0; ink < inks; ink++)
                upload[table + 1 + ink] = Shown(colours.Colour(sheet, ink));
            table += 1 + inks;
        }
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
