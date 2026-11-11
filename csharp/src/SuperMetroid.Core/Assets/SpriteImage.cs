using System.Collections.Immutable;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// A finished, pre-coloured image the installation provides: a sheet of an actor's frames or one part's image, or a
/// sheet of a room tileset's blocks (#1633). A pixel is only its colour: alpha zero is clear, and a clear pixel's RGB is
/// the colour it keeps of its own. A shade recolours the pixels by the colour they hold (<see cref="ColourKey"/>); where
/// pixels of one colour recolour differently, the sheet's recolour-region map tells them apart. Where a picture blends in
/// some pixels and not others when drawn as a sprite, the sheet's blend map says which.
/// </summary>
public sealed class SpriteImage
{
    /// <summary>
    /// The pixels and the maps, decoded when first read: an installation's stock images are verified when it opens, so
    /// decoding every sheet up front only cost the time and memory of images nothing draws (#1430).
    /// </summary>
    private readonly Lazy<DecodedImage> content;

    /// <summary>
    /// Names an image whose pixels <paramref name="decode"/> produces when they are first read. The pixels and maps are
    /// checked then.
    /// </summary>
    /// <param name="name">The image's installed name, which names it in diagnostics.</param>
    /// <param name="size">The image's width and height in pixels; both are positive.</param>
    /// <param name="colours">The shades the image is pre-coloured in and its scene's backdrop, if it names one.</param>
    /// <param name="decode">Produces the image's pixels and maps.</param>
    /// <exception cref="ArgumentException">The name is empty or a dimension is zero.</exception>
    public SpriteImage(string name, Extent16<ImagePixels> size, PictureColours colours, Func<SpriteImagePixels> decode)
    {
        Ensure.NotNull(colours);
        ArgumentException.ThrowIfNullOrEmpty(name);
        Ensure.NotNull(decode);
        if (size.X == 0 || size.Y == 0)
            throw new ArgumentException($"Sprite image {name} has no pixels.", nameof(size));
        Name = name;
        Size = size;
        Shades = Ensure.NotNull(colours.Shades);
        Backdrop = colours.Backdrop;
        content = new(() => Decoded(decode()));
    }

    /// <summary>
    /// Checks one image's decoded pixels and maps against its size and finds its distinct colour keys, in the order its
    /// pixels first show them, the backdrop's last where no pixel shows it.
    /// </summary>
    /// <exception cref="ArgumentException">A length does not match the image's size, or a recolour region is negative.</exception>
    private DecodedImage Decoded(SpriteImagePixels decoded)
    {
        Ensure.NotNull(decoded);
        int pixels = Size.X * Size.Y;
        Ensure.LengthEqual(decoded.Pixels, pixels);
        if (decoded.RecolourRegions is { } regions)
        {
            Ensure.LengthEqual(regions, pixels);
            if (regions.Any(region => region < 0))
                throw new ArgumentException($"Sprite image {Name} has a pixel in a negative recolour region.", nameof(decoded));
        }
        if (decoded.Blending is { } blending)
            Ensure.LengthEqual(blending, pixels);
        var places = new Dictionary<ColourKey, int>();
        var keys = ImmutableArray.CreateBuilder<ColourKey>();
        for (int pixel = 0; pixel < pixels; pixel++)
            Add(KeyAt(decoded, pixel), places, keys);
        if (Backdrop is { } backdrop)
            Add(backdrop, places, keys);
        return new DecodedImage(decoded, keys.ToImmutable(), places);
    }

    /// <summary>The colour key of pixel <paramref name="pixel"/> of <paramref name="decoded"/>.</summary>
    private static ColourKey KeyAt(SpriteImagePixels decoded, int pixel)
    {
        RgbaPixel shown = decoded.Pixels[pixel];
        return new ColourKey(new RgbColor(shown.R, shown.G, shown.B), decoded.RecolourRegions?[pixel] ?? 0);
    }

    /// <summary>Adds <paramref name="key"/> to the keys found so far where it is new.</summary>
    private static void Add(ColourKey key, Dictionary<ColourKey, int> places, ImmutableArray<ColourKey>.Builder keys)
    {
        if (places.TryAdd(key, keys.Count))
            keys.Add(key);
    }

    /// <summary>The image's installed name.</summary>
    public string Name { get; }

    /// <summary>The image's width and height in pixels.</summary>
    public Extent16<ImagePixels> Size { get; }

    /// <summary>
    /// The pixels, row by row from the top. A pixel with zero alpha is clear; its RGB is the colour it keeps of its own, which
    /// a layer drawn opaque shows.
    /// </summary>
    public ReadOnlySpan<RgbaPixel> Pixels => content.Value.Source.Pixels;

    /// <summary>
    /// Each pixel's recolour region (<see cref="ColourKey.Region"/>), row by row from the top; empty for an image whose
    /// pixels of one colour all recolour alike, every pixel then lying in region zero.
    /// </summary>
    public ReadOnlySpan<int> RecolourRegions => content.Value.Source.RecolourRegions;

    /// <summary>
    /// Whether each pixel blends with what lies behind it when drawn as a sprite, row by row from the top; empty for an image
    /// whose pictures each blend wholly or not at all, as their frames say (<see cref="SpriteSheetFrame.Blends"/>).
    /// </summary>
    public ReadOnlySpan<bool> Blending => content.Value.Source.Blending;

    /// <summary>
    /// The image's distinct colour keys: each colour its pixels hold with its recolour region, in the order its pixels first
    /// show them, then the backdrop's where no pixel holds it. A shading shows one colour for each (<see cref="PictureShading.KeyColours(SpriteImage)"/>).
    /// </summary>
    public ImmutableArray<ColourKey> ColourKeys => content.Value.Keys;

    /// <summary>The colour key of pixel <paramref name="pixel"/>, counted row by row from the top.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The image has no such pixel.</exception>
    public ColourKey KeyAt(int pixel)
    {
        DecodedImage decoded = content.Value;
        Ensure.AtLeastZero(pixel);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(pixel, decoded.Source.Pixels.Length);
        return KeyAt(decoded.Source, pixel);
    }

    /// <summary>The place of <paramref name="key"/> among <see cref="ColourKeys"/>, or null where the image holds no pixel of it.</summary>
    public int? PlaceOf(ColourKey key) => content.Value.Places.TryGetValue(key, out int place) ? place : null;

    /// <summary>
    /// The colour the image's scene shows where nothing is drawn, as the key its shades recolour it by, for a room tileset's
    /// picture; null for every other image.
    /// </summary>
    public ColourKey? Backdrop { get; }

    /// <summary>The shades the import pre-coloured the image in besides its own colours (#1512).</summary>
    public ImmutableDictionary<PictureShade, PictureRecolouring> Shades { get; }

    /// <summary>What the image shows under <paramref name="shade"/>.</summary>
    /// <exception cref="InvalidDataException">The import did not pre-colour the image in that shade.</exception>
    public PictureRecolouring Shade(PictureShade shade) => Shades.TryGetValue(shade, out PictureRecolouring? recolouring) ? recolouring
        : throw new InvalidDataException($"Image {Name} was not pre-coloured in shade {shade.Key}.");

    /// <summary>Names the image with its size.</summary>
    public override string ToString() => $"{Name} ({Size.X}x{Size.Y})";

    /// <summary>An image's decoded pixels and maps with its colour keys.</summary>
    /// <param name="Source">The decoded pixels and maps.</param>
    /// <param name="Keys">The distinct colour keys in first-shown order.</param>
    /// <param name="Places">Each key's place among the keys.</param>
    private sealed record DecodedImage(SpriteImagePixels Source, ImmutableArray<ColourKey> Keys, Dictionary<ColourKey, int> Places);
}
