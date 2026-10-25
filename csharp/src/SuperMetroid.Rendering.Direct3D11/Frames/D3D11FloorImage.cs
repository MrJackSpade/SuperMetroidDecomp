using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms.Backgrounds;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace SuperMetroid.Rendering.Direct3D11.Frames;

/// <summary>
/// The perspective floor's image (#1408): the whole floor, 1024 pixels square, laid out from its tiles' pictures once
/// whenever the floor changes, so the floor shader samples one image. Beside it, the first tile's picture, which the
/// floor shows outside itself when it fills with it. Each texel holds the pixel's pre-coloured colour and its ink word,
/// with the index of the floor sheet it comes from above the ink's flags.
/// </summary>
internal sealed class D3D11FloorImage : IDisposable
{
    /// <summary>The floor's side in pixels.</summary>
    internal const int Side = AffineFloor.Side * TilePixels;
    /// <summary>The image's width: the floor, then the first tile's picture.</summary>
    private const int Width = Side + TilePixels;
    /// <summary>A tile's side in pixels.</summary>
    private const int TilePixels = 8;
    /// <summary>Where a texel's ink word keeps the index of the floor sheet it comes from.</summary>
    internal const int SheetShift = 20;
    /// <summary>An ink word's bit marking a pixel of a tile nothing pictures, which shows nothing.</summary>
    internal const uint UnpicturedInk = 1u << 19;
    /// <summary>Words per texel: the colour, then the ink.</summary>
    private const int TexelWords = 2;

    /// <summary>The device context that copies the image in.</summary>
    private readonly ID3D11DeviceContext context;
    /// <summary>The floor the image shows.</summary>
    private AffineFloor? shown;
    /// <summary>The image's texels, reused while each new floor is laid out.</summary>
    private readonly uint[] texels = new uint[TexelWords * Width * Side];

    /// <summary>Creates the empty image.</summary>
    internal D3D11FloorImage(ID3D11Device device, ID3D11DeviceContext context)
    {
        Ensure.NotNull(device);
        this.context = Ensure.NotNull(context);
        Texture = device.CreateTexture2D(new Texture2DDescription(Format.R32G32_UInt, Width, Side, 1, 1, BindFlags.ShaderResource));
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
        if (floor.Equals(shown))
            return;
        FloorTilePictures tiles = floor.Tiles;
        for (int cell = 0; cell < AffineFloor.CellCount; cell++)
            LayTile(tiles, floor.Cells[cell].Tile, cell % AffineFloor.Side * TilePixels, cell / AffineFloor.Side * TilePixels);
        LayTile(tiles, 0, Side, 0);
        fixed (uint* source = texels)
            context.UpdateSubresource(Texture, 0, null, (nint)source, TexelWords * sizeof(uint) * Width, 0);
        shown = floor;
    }

    /// <summary>Lays tile <paramref name="tile"/>'s picture at (<paramref name="left"/>, <paramref name="top"/>).</summary>
    private void LayTile(FloorTilePictures tiles, int tile, int left, int top)
    {
        if (tiles.Pictures[tile] is not { } picture)
        {
            for (int y = 0; y < TilePixels; y++)
            {
                for (int x = 0; x < TilePixels; x++)
                {
                    int target = TexelWords * ((top + y) * Width + left + x);
                    texels[target] = 0;
                    texels[target + 1] = UnpicturedInk;
                }
            }
            return;
        }
        SpriteImage sheet = picture.Picture.Sheet;
        uint sheetIndex = (uint)tiles.Sheets.IndexOf(picture.Picture) << SheetShift;
        int cornerX = picture.Picture.Corner.X + picture.Corner.X, cornerY = picture.Picture.Corner.Y + picture.Corner.Y;
        ReadOnlySpan<Rgba32> pixels = sheet.Pixels;
        ReadOnlySpan<SpriteImageInk> inks = sheet.Inks;
        for (int y = 0; y < TilePixels; y++)
        {
            int source = (cornerY + y) * sheet.Size.X + cornerX;
            for (int x = 0; x < TilePixels; x++)
            {
                Rgba32 pixel = pixels[source + x];
                SpriteImageInk ink = inks[source + x];
                bool colourZero = sheet.IsColourZero(ink);
                int target = TexelWords * ((top + y) * Width + left + x);
                texels[target] = pixel.R | (uint)pixel.G << 8 | (uint)pixel.B << 16 | (uint)pixel.A << 24;
                texels[target + 1] = (uint)ink.PaletteIndex | (uint)ink.Priority << 8 | sheetIndex
                    | (colourZero ? D3D11ImageAtlas.ColourZeroInk : 0)
                    | (colourZero && sheet.ColourZeroKeepsGroup ? D3D11ImageAtlas.KeepsGroupInk : 0);
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
