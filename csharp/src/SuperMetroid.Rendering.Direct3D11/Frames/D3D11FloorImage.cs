using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Backgrounds;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace SuperMetroid.Rendering.Direct3D11.Frames;

/// <summary>
/// The perspective floor's image (#1408, #1467): the whole floor, 1024 pixels square, copied from the floor's installed
/// image with the pictures of its states laid over it at their places, once whenever the floor changes, so the floor
/// shader samples one image. Each texel holds the pixel's colour and its key word (<see cref="D3D11ImageAtlas.KeyWord"/>),
/// with the index of the floor picture it comes from above the key word's marks.
/// </summary>
internal sealed class D3D11FloorImage : IDisposable
{
    /// <summary>The floor's side in pixels.</summary>
    internal const int Side = AffineFloor.SidePixels;
    /// <summary>Where a texel's key word keeps the index of the floor picture it comes from, above the key's place and marks.</summary>
    internal const int SheetShift = 20;
    /// <summary>Words per texel: the colour, then the key word.</summary>
    private const int TexelWords = 2;

    /// <summary>The device context that copies the image in.</summary>
    private readonly ID3D11DeviceContext context;
    /// <summary>The floor the image shows.</summary>
    private AffineFloor? shown;
    /// <summary>The image's texels, reused while each new floor is laid out.</summary>
    private readonly uint[] texels = new uint[TexelWords * Side * Side];

    /// <summary>Creates the empty image.</summary>
    internal D3D11FloorImage(ID3D11Device device, ID3D11DeviceContext context)
    {
        Ensure.NotNull(device);
        this.context = Ensure.NotNull(context);
        Texture = device.CreateTexture2D(new Texture2DDescription(Format.R32G32_UInt, Side, Side, 1, 1, BindFlags.ShaderResource));
        View = device.CreateShaderResourceView(Texture);
    }

    /// <summary>The image.</summary>
    internal ID3D11Texture2D Texture { get; }
    /// <summary>The shader view of the image.</summary>
    internal ID3D11ShaderResourceView View { get; }

    /// <summary>Lays out <paramref name="floor"/> and copies it in, unless the image already shows it.</summary>
    internal unsafe void Show(AffineFloor floor)
    {
        Ensure.NotNull(floor);
        // The image holds the pictures' pixels and key words; what each key shows is the colour slots' (#1512, #1633).
        if (floor.SamePictures(shown))
            return;
        Lay(floor, floor.Image, 0, 0);
        foreach (SpriteSheetFrame picture in floor.DrawnOver)
            Lay(floor, picture, -picture.Origin.X, -picture.Origin.Y);
        fixed (uint* source = texels)
            context.UpdateSubresource(Texture, 0, null, (nint)source, TexelWords * sizeof(uint) * Side, 0);
        shown = floor;
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
                int target = TexelWords * ((top + y) * Side + left + x);
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
