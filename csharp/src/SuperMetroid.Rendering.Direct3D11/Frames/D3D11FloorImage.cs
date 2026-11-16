using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Spatial;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace SuperMetroid.Rendering.Direct3D11.Frames;

/// <summary>
/// The perspective floor's image (#1408, #1467, #1649, #2104): the whole floor, as large as the floor's installed image, with
/// that image and the pictures of its states laid over it at their places, once whenever the floor changes, so the floor
/// shader samples one image. Each texel holds the pixel's colour and its key word (<see cref="D3D11ImageAtlas.KeyWord"/>),
/// with the index of the floor picture it comes from above the key word's marks.
/// </summary>
internal sealed class D3D11FloorImage : IDisposable
{
    /// <summary>Where a texel's key word keeps the index of the floor picture it comes from, above the key's place and marks.</summary>
    internal const int SheetShift = 20;
    /// <summary>Words per texel: the colour, then the key word.</summary>
    private const int TexelWords = 2;

    /// <summary>The device that creates the image for each floor of another extent.</summary>
    private readonly ID3D11Device device;
    /// <summary>The device context that copies the image in.</summary>
    private readonly ID3D11DeviceContext context;
    /// <summary>The floor the image shows.</summary>
    private AffineFloor? shown;
    /// <summary>The image's texels, reused while each new floor of the same extent is laid out.</summary>
    private uint[] texels = [];

    /// <summary>Creates the image of no floor yet: one clear texel, which no floor pass reads.</summary>
    internal D3D11FloorImage(ID3D11Device device, ID3D11DeviceContext context)
    {
        this.device = Ensure.NotNull(device);
        this.context = Ensure.NotNull(context);
        Create(new Extent16<ImagePixels>(1, 1));
    }

    /// <summary>The image.</summary>
    internal ID3D11Texture2D Texture { get; private set; }
    /// <summary>The shader view of the image.</summary>
    internal ID3D11ShaderResourceView View { get; private set; }
    /// <summary>The image's width and height: the extent of the floor it shows.</summary>
    internal Extent16<ImagePixels> Extent { get; private set; }

    /// <summary>
    /// Lays out <paramref name="floor"/> and copies it in, unless the image already shows it; a floor of another extent
    /// replaces the image and its view, so a pass binds <see cref="View"/> after this.
    /// </summary>
    internal unsafe void Show(AffineFloor floor)
    {
        Ensure.NotNull(floor);
        // The image holds the pictures' pixels and key words; what each key shows is the colour slots' (#1512, #1633).
        if (floor.SamePictures(shown))
            return;
        if (floor.Extent != Extent)
        {
            View.Dispose();
            Texture.Dispose();
            Create(floor.Extent);
        }
        Lay(floor, floor.Image, 0, 0);
        foreach (SpriteSheetFrame picture in floor.DrawnOver)
            Lay(floor, picture, -picture.Origin.X, -picture.Origin.Y);
        fixed (uint* source = texels)
            context.UpdateSubresource(Texture, 0, null, (nint)source, (uint)(TexelWords * sizeof(uint) * Extent.X), 0);
        shown = floor;
    }

    /// <summary>Creates a clear image of <paramref name="extent"/> and its view.</summary>
    [System.Diagnostics.CodeAnalysis.MemberNotNull(nameof(Texture), nameof(View))]
    private void Create(Extent16<ImagePixels> extent)
    {
        Extent = extent;
        texels = new uint[TexelWords * extent.X * extent.Y];
        for (int texel = 0; texel < extent.X * extent.Y; texel++)
            texels[TexelWords * texel + 1] = D3D11ImageAtlas.ClearMark;
        Texture = device.CreateTexture2D(new Texture2DDescription(Format.R32G32_UInt, extent.X, extent.Y, 1, 1, BindFlags.ShaderResource));
        View = device.CreateShaderResourceView(Texture);
    }

    /// <summary>Lays <paramref name="picture"/>'s pixels with its top-left at floor pixel (<paramref name="left"/>, <paramref name="top"/>).</summary>
    private void Lay(AffineFloor floor, SpriteSheetFrame picture, int left, int top)
    {
        SpriteImage sheet = picture.Sheet;
        uint sheetIndex = (uint)floor.Pictures.IndexOf(picture) << SheetShift;
        ReadOnlySpan<RgbaPixel> pixels = sheet.Pixels;
        for (int y = 0; y < picture.Size.Y; y++)
        {
            int source = (picture.Corner.Y + y) * sheet.Size.X + picture.Corner.X;
            for (int x = 0; x < picture.Size.X; x++)
            {
                RgbaPixel pixel = pixels[source + x];
                int target = TexelWords * ((top + y) * Extent.X + left + x);
                texels[target] = pixel.R | (uint)pixel.G << 8 | (uint)pixel.B << 16 | (uint)pixel.A << 24;
                texels[target + 1] = D3D11ImageAtlas.KeyWord(picture, source + x) | sheetIndex;
            }
        }
    }

    /// <summary>Releases the image and its view.</summary>
    public void Dispose()
    {
        View.Dispose();
        Texture.Dispose();
    }
}
