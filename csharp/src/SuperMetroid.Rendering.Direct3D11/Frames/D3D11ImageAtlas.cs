using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Spatial;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;

namespace SuperMetroid.Rendering.Direct3D11.Frames;

/// <summary>
/// The installed images a scene draws, as one texture (#1408): each sprite frame, block picture, background image and
/// tile sheet a scene shows is copied in once, at a place it keeps while later scenes show it. Each texel holds the
/// image's pre-coloured pixel and, beside it, the pixel's ink, priority and part row, so a colour effect recolours the
/// image as it shows it.
/// </summary>
internal sealed class D3D11ImageAtlas : IDisposable
{
    /// <summary>The texture's width and height in texels.</summary>
    internal const int Edge = 2048;
    /// <summary>An ink word's bit marking a pixel of colour zero.</summary>
    internal const uint ColourZeroInk = 1u << 16;
    /// <summary>An ink word's bit marking a colour-zero pixel that keeps its group, so an opaque layer shows its colour.</summary>
    internal const uint KeepsGroupInk = 1u << 17;
    /// <summary>Where an ink word keeps the pixel's priority.</summary>
    private const int PriorityShift = 8;
    /// <summary>Where an ink word keeps the pixel's part row and its fixed-row flag.</summary>
    private const int PartRowShift = 10;
    /// <summary>Words per texel: the colour, then the ink.</summary>
    private const int TexelWords = 2;

    /// <summary>The device context that copies images in.</summary>
    private readonly ID3D11DeviceContext context;
    /// <summary>
    /// Where each image region the texture holds starts, by sheet and region: a capture that names the same region of a
    /// sheet through a new frame finds the place the region already has.
    /// </summary>
    private readonly Dictionary<(SpriteImage Sheet, Position16<ImagePixels> Corner, Extent16<ImagePixels> Size), Position16<ImagePixels>> places = [];
    /// <summary>
    /// The texture's skyline: left to right, runs of columns each with the row below the lowest image placed in it, which
    /// together span the texture's width.
    /// </summary>
    private readonly List<(int Left, int Top, int Width)> skyline = [(0, 0, Edge)];
    /// <summary>The images being placed into an emptied texture, tallest first; reused across refills.</summary>
    private readonly List<SpriteSheetFrame> refill = [];
    /// <summary>Reused copy of one image's texels while it is copied in.</summary>
    private uint[] texels = new uint[TexelWords * 64 * 64];

    /// <summary>Creates the empty texture.</summary>
    internal D3D11ImageAtlas(ID3D11Device device, ID3D11DeviceContext context)
    {
        Ensure.NotNull(device);
        this.context = Ensure.NotNull(context);
        Texture = device.CreateTexture2D(new Texture2DDescription(Format.R32G32_UInt, Edge, Edge, 1, 1, BindFlags.ShaderResource));
        View = device.CreateShaderResourceView(Texture);
    }

    /// <summary>Counts the times the texture was emptied, so a holder of places knows when to ask again.</summary>
    internal int Generation { get; private set; }
    /// <summary>The images the scene being prepared shows; reused across scenes so a frame allocates nothing.</summary>
    internal HashSet<SpriteSheetFrame> Needed { get; } = new(ReferenceEqualityComparer.Instance);
    /// <summary>The texture.</summary>
    internal ID3D11Texture2D Texture { get; }
    /// <summary>The shader view of the texture.</summary>
    internal ID3D11ShaderResourceView View { get; }

    /// <summary>
    /// Makes sure the texture holds every image of <paramref name="images"/>, copying in those it lacks. When they do not
    /// fit beside the images earlier scenes left, the texture is emptied and holds only these.
    /// </summary>
    /// <exception cref="InvalidOperationException">The images do not fit the texture even alone.</exception>
    internal void Hold(IReadOnlyCollection<SpriteSheetFrame> images)
    {
        foreach (SpriteSheetFrame image in images)
        {
            if (!places.ContainsKey(Region(image)) && !TryPlace(image))
            {
                places.Clear();
                skyline.Clear();
                skyline.Add((0, 0, Edge));
                Generation++;
                // Tallest first, which keeps the skyline low.
                refill.Clear();
                refill.AddRange(images);
                refill.Sort((left, right) => right.Size.Y.CompareTo(left.Size.Y));
                foreach (SpriteSheetFrame shown in refill)
                {
                    if (!places.ContainsKey(Region(shown)) && !TryPlace(shown))
                        throw new InvalidOperationException($"A scene's {images.Count} images, {images.Sum(image => image.Size.X * image.Size.Y)} "
                            + $"pixels, the tallest {images.Max(image => image.Size.Y)} and widest {images.Max(image => image.Size.X)}, "
                            + $"do not fit a {Edge}x{Edge} texture.");
                }
                return;
            }
        }
    }

    /// <summary>Where <paramref name="image"/> starts in the texture, x below y.</summary>
    /// <exception cref="InvalidOperationException">The texture does not hold the image.</exception>
    internal uint Place(SpriteSheetFrame image) => places.TryGetValue(Region(image), out Position16<ImagePixels> place)
        ? D3D11ConstantPacking.Halves(place.X, place.Y)
        : throw new InvalidOperationException($"The image texture does not hold {image}.");

    /// <summary>
    /// Places <paramref name="image"/> where its top would be lowest on the skyline, leftmost among equals, and copies it
    /// in; false when it fits nowhere.
    /// </summary>
    private bool TryPlace(SpriteSheetFrame image)
    {
        int width = image.Size.X, height = image.Size.Y;
        if (width > Edge || height > Edge)
            throw new InvalidOperationException($"{image} is larger than the {Edge}x{Edge} image texture.");
        int best = -1, bestTop = Edge;
        for (int run = 0; run < skyline.Count; run++)
        {
            int left = skyline[run].Left;
            if (left + width > Edge)
                break;
            // The image's top is the lowest row below every run it spans.
            int top = 0;
            for (int spanned = run; spanned < skyline.Count && skyline[spanned].Left < left + width; spanned++)
                top = Math.Max(top, skyline[spanned].Top);
            if (top + height <= Edge && top < bestTop)
            {
                best = run;
                bestTop = top;
            }
        }
        if (best < 0)
            return false;
        int placedLeft = skyline[best].Left, right = placedLeft + width;
        // The runs the image covers give way to one run below it; a run it covers in part keeps the rest.
        int first = best;
        while (first < skyline.Count && skyline[first].Left < right)
        {
            (int runLeft, int runTop, int runWidth) = skyline[first];
            if (runLeft + runWidth > right)
            {
                skyline[first] = (right, runTop, runLeft + runWidth - right);
                break;
            }
            skyline.RemoveAt(first);
        }
        skyline.Insert(best, (placedLeft, bestTop + height, width));
        CopyIn(image, placedLeft, bestTop);
        return true;
    }

    /// <summary>Copies <paramref name="image"/>'s colours and inks into the texture at (<paramref name="left"/>, <paramref name="top"/>).</summary>
    private unsafe void CopyIn(SpriteSheetFrame image, int left, int top)
    {
        SpriteImage sheet = image.Sheet;
        int width = image.Size.X, height = image.Size.Y;
        if (texels.Length < TexelWords * width * height)
            texels = new uint[TexelWords * width * height];
        ReadOnlySpan<RgbaPixel> pixels = sheet.Pixels;
        ReadOnlySpan<SpriteImageInk> inks = sheet.Inks;
        int target = 0;
        for (int row = 0; row < height; row++)
        {
            int source = (image.Corner.Y + row) * sheet.Size.X + image.Corner.X;
            for (int column = 0; column < width; column++, target += TexelWords)
            {
                RgbaPixel pixel = pixels[source + column];
                SpriteImageInk ink = inks[source + column];
                bool colourZero = sheet.IsColourZero(ink);
                texels[target] = pixel.R | (uint)pixel.G << 8 | (uint)pixel.B << 16 | (uint)pixel.A << 24;
                texels[target + 1] = (uint)ink.PaletteIndex | (uint)ink.Priority << PriorityShift | (uint)ink.PartRow << PartRowShift
                    | (colourZero ? ColourZeroInk : 0) | (colourZero && sheet.ColourZeroKeepsGroup ? KeepsGroupInk : 0);
            }
        }
        fixed (uint* source = texels)
            context.UpdateSubresource(Texture, 0, new Box(left, top, 0, left + width, top + height, 1), (nint)source,
                (uint)(TexelWords * sizeof(uint) * width), 0);
        places[Region(image)] = new Position16<ImagePixels>((ushort)left, (ushort)top);
    }

    /// <summary>The sheet and region <paramref name="image"/> shows.</summary>
    private static (SpriteImage Sheet, Position16<ImagePixels> Corner, Extent16<ImagePixels> Size) Region(SpriteSheetFrame image) =>
        (image.Sheet, image.Corner, image.Size);

    /// <summary>Releases the texture and its view.</summary>
    public void Dispose()
    {
        View.Dispose();
        Texture.Dispose();
    }
}
