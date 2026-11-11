using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Spatial;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;

namespace SuperMetroid.Rendering.Direct3D11.Frames;

/// <summary>
/// The installed images a scene draws, as one texture (#1408): each sprite frame, block picture, background image and
/// tile sheet a scene shows is copied in once, at a place it keeps while later scenes show it. Each texel holds the
/// image's pixel and, beside it, the pixel's key word (<see cref="KeyWord"/>): for a visible pixel the place of its colour
/// key among the image's (<see cref="SpriteImage.ColourKeys"/>), so a shading recolours the image as it shows it (#1512,
/// #1633), with whether it blends where it is drawn as a sprite; for a clear pixel only that it is clear.
/// </summary>
internal sealed class D3D11ImageAtlas : IDisposable
{
    /// <summary>The texture's width and height in texels.</summary>
    internal const int Edge = 2048;
    /// <summary>The most colour keys an image may have: a key word holds the place of a pixel's key below its marks.</summary>
    internal const int MostColourKeys = 1 << 16;
    /// <summary>
    /// A key word's mark on a clear pixel, one whose alpha is zero (#1544, #1633): a see-through layer shows what lies behind
    /// it, and a layer drawn opaque shows its stored colour, unshaded. A clear pixel has no colour key.
    /// </summary>
    internal const uint ClearMark = 1u << 16;
    /// <summary>
    /// A key word's mark on a pixel that takes part in colour blends where it is drawn as a sprite (#1544, #1633): every pixel
    /// of a frame that blends (<see cref="SpriteSheetFrame.Blends"/>), or a pixel its sheet's blend map marks
    /// (<see cref="SpriteImage.Blending"/>).
    /// </summary>
    internal const uint BlendsMark = 1u << 17;
    /// <summary>Words per texel: the colour, then the key word.</summary>
    private const int TexelWords = 2;

    /// <summary>The device context that copies images in.</summary>
    private readonly ID3D11DeviceContext context;
    /// <summary>
    /// Where each image region the texture holds starts, by sheet and region: a capture that names the same region of a
    /// sheet through a new frame finds the place the region already has.
    /// </summary>
    private readonly Dictionary<(SpriteImage Sheet, Position16<ImagePixels> Corner, Extent16<ImagePixels> Size, bool Blends), Position16<ImagePixels>> places = [];
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

    /// <summary>Copies <paramref name="image"/>'s pixels and key words into the texture at (<paramref name="left"/>, <paramref name="top"/>).</summary>
    /// <exception cref="InvalidDataException">The image's sheet has more colour keys than a key word can place.</exception>
    private unsafe void CopyIn(SpriteSheetFrame image, int left, int top)
    {
        SpriteImage sheet = image.Sheet;
        int width = image.Size.X, height = image.Size.Y;
        if (texels.Length < TexelWords * width * height)
            texels = new uint[TexelWords * width * height];
        ReadOnlySpan<RgbaPixel> pixels = sheet.Pixels;
        int target = 0;
        for (int row = 0; row < height; row++)
        {
            int source = (image.Corner.Y + row) * sheet.Size.X + image.Corner.X;
            for (int column = 0; column < width; column++, target += TexelWords)
            {
                RgbaPixel pixel = pixels[source + column];
                texels[target] = pixel.R | (uint)pixel.G << 8 | (uint)pixel.B << 16 | (uint)pixel.A << 24;
                texels[target + 1] = KeyWord(image, source + column);
            }
        }
        fixed (uint* source = texels)
            context.UpdateSubresource(Texture, 0, new Box(left, top, 0, left + width, top + height, 1), (nint)source,
                (uint)(TexelWords * sizeof(uint) * width), 0);
        places[Region(image)] = new Position16<ImagePixels>((ushort)left, (ushort)top);
    }

    /// <summary>
    /// The word the shaders read beside sheet pixel <paramref name="pixel"/> of <paramref name="image"/> (#1633): for a clear
    /// pixel, one whose alpha is zero, <see cref="ClearMark"/> alone, since a clear pixel is never recoloured and an opaque
    /// layer shows its stored colour as it is; for a visible pixel, the place of its colour key among its sheet's
    /// (<see cref="SpriteImage.ColourKeys"/>) in the low sixteen bits, with <see cref="BlendsMark"/> where the frame blends or
    /// the sheet's blend map marks the pixel.
    /// </summary>
    /// <param name="image">The frame the pixel is copied in as part of.</param>
    /// <param name="pixel">The pixel, counted row by row from the top of the whole sheet.</param>
    /// <exception cref="InvalidDataException">
    /// The sheet has more colour keys than a key word can place, or its keys do not hold a visible pixel's.
    /// </exception>
    internal static uint KeyWord(SpriteSheetFrame image, int pixel)
    {
        Ensure.NotNull(image);
        SpriteImage sheet = image.Sheet;
        if (sheet.Pixels[pixel].A == 0)
            return ClearMark;
        if (sheet.ColourKeys.Length > MostColourKeys)
            throw new InvalidDataException(
                $"Sheet {sheet.Name} has {sheet.ColourKeys.Length} colour keys, more than the {MostColourKeys} a key word can place.");
        ColourKey key = sheet.KeyAt(pixel);
        int place = sheet.PlaceOf(key)
            ?? throw new InvalidDataException($"Sheet {sheet.Name}'s colour keys do not hold {key}, the key of its visible pixel {pixel}.");
        ReadOnlySpan<bool> blending = sheet.Blending;
        bool blends = image.Blends || (!blending.IsEmpty && blending[pixel]);
        return (uint)place | (blends ? BlendsMark : 0);
    }

    /// <summary>
    /// The sheet and region <paramref name="image"/> shows, with whether the frame blends, which its key words carry: a
    /// region copied in for a frame that blends is not the one a frame that does not blend shows.
    /// </summary>
    private static (SpriteImage Sheet, Position16<ImagePixels> Corner, Extent16<ImagePixels> Size, bool Blends) Region(SpriteSheetFrame image) =>
        (image.Sheet, image.Corner, image.Size, image.Blends);

    /// <summary>Releases the texture and its view.</summary>
    public void Dispose()
    {
        View.Dispose();
        Texture.Dispose();
    }
}
