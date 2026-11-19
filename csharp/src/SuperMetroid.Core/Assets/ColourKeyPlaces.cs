namespace SuperMetroid.Core.Assets;

/// <summary>
/// The places of colour keys already found while one image's pixels are read (#1633), held by the colour and recolour
/// region that make each key, so code reading every pixel of an image finds a pixel's key place without building its key
/// and looking it up among the image's keys. A turning floor is a million pixels whose colour changes from one pixel to the
/// next, so that lookup for every pixel was most of the time reading such an image took. A reader keeps one, empties it
/// (<see cref="Clear"/>) for each image, and finds each key the first time its colour shows. A colour whose slot another
/// colour took is found again; what a slot holds is always the place of its own colour's key.
/// </summary>
public sealed class ColourKeyPlaces
{
    /// <summary>The colours held at once, a prime: more than an image's pictures usually show, so few colours share a slot.</summary>
    private const int Slots = 1021;

    /// <summary>The multiplier that spreads one channel from the next in a colour's slot.</summary>
    private const int ChannelSpread = 31;

    /// <summary>The multiplier that spreads a recolour region from the colour in a slot.</summary>
    private const int RegionSpread = 101;

    /// <summary>Each slot's colour, opaque.</summary>
    private readonly RgbaPixel[] colours = new RgbaPixel[Slots];

    /// <summary>Each slot's recolour region.</summary>
    private readonly int[] regions = new int[Slots];

    /// <summary>Each slot's key place, or -1 while the slot holds none.</summary>
    private readonly int[] places = new int[Slots];

    /// <summary>Holds no places.</summary>
    public ColourKeyPlaces() => Clear();

    /// <summary>Forgets every place, for the next image's keys.</summary>
    public void Clear() => Array.Fill(places, -1);

    /// <summary>
    /// The place of the key of <paramref name="pixel"/>'s colour in recolour region <paramref name="region"/> (never
    /// negative), when held; its alpha is no part of its key.
    /// </summary>
    public bool TryGet(RgbaPixel pixel, int region, out int place)
    {
        int slot = SlotOf(pixel, region);
        RgbaPixel held = colours[slot];
        place = places[slot];
        return place >= 0 && held.R == pixel.R && held.G == pixel.G && held.B == pixel.B && regions[slot] == region;
    }

    /// <summary>Holds <paramref name="place"/> as the place of the key of <paramref name="pixel"/>'s colour in recolour region <paramref name="region"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The place or the region is negative.</exception>
    public void Set(RgbaPixel pixel, int region, int place)
    {
        Ensure.AtLeastZero(place);
        int slot = SlotOf(pixel, region);
        colours[slot] = pixel with { A = byte.MaxValue };
        regions[slot] = region;
        places[slot] = place;
    }

    /// <summary>The slot <paramref name="pixel"/>'s colour in <paramref name="region"/> is held in.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The region is negative.</exception>
    private static int SlotOf(RgbaPixel pixel, int region) =>
        (int)((((long)pixel.R * ChannelSpread + pixel.G) * ChannelSpread + pixel.B + (long)Ensure.AtLeastZero(region) * RegionSpread) % Slots);
}
