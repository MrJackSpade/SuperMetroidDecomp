using SuperMetroid.Core.Assets;

namespace SuperMetroid.Rendering.Direct3D11.Frames;

/// <summary>
/// The places of colour keys already found while one image is copied in (#1633), by the colour and recolour region that
/// make each key: a visible pixel's key is looked up among its sheet's keys (<see cref="SpriteImage.PlaceOf"/>) the first
/// time its colour shows, then read here. A turning floor is a million pixels whose colour changes from one pixel to the
/// next, so a lookup among the sheet's keys for every pixel was most of the time its image took to lay out. Each holder of
/// an image texture keeps one and empties it (<see cref="Clear"/>) for each image it copies in. A colour whose slot another
/// colour took is looked up again; what it holds is always the place its own key has.
/// </summary>
internal sealed class D3D11KeyPlaces
{
    /// <summary>The colours held at once: more than a sheet's pictures usually show, so few colours share a slot.</summary>
    private const int Slots = 1024;

    /// <summary>The bits of a slot's number, taken from the top of the colour's hash.</summary>
    private const int SlotBits = 10;

    /// <summary>Spreads a colour's channels over the hash's top bits (the 32-bit golden-ratio multiplier).</summary>
    private const uint Spread = 2654435761;

    /// <summary>Spreads a recolour region before it joins the colour (the 32-bit FNV prime).</summary>
    private const int RegionSpread = 16777619;

    /// <summary>Each slot's colour, its channels red lowest, plus one, or zero while the slot holds none.</summary>
    private readonly int[] colours = new int[Slots];

    /// <summary>Each slot's recolour region.</summary>
    private readonly int[] regions = new int[Slots];

    /// <summary>Each slot's key place among the sheet's keys.</summary>
    private readonly uint[] places = new uint[Slots];

    /// <summary>Forgets every place, for the next image's keys.</summary>
    internal void Clear() => Array.Clear(colours);

    /// <summary>The place of the key of <paramref name="pixel"/>'s colour in recolour region <paramref name="region"/>, when held.</summary>
    internal bool TryGet(RgbaPixel pixel, int region, out uint place)
    {
        int slot = SlotOf(pixel, region);
        if (colours[slot] == Colour(pixel) + 1 && regions[slot] == region)
        {
            place = places[slot];
            return true;
        }
        place = 0;
        return false;
    }

    /// <summary>Holds <paramref name="place"/> as the place of the key of <paramref name="pixel"/>'s colour in recolour region <paramref name="region"/>.</summary>
    internal void Set(RgbaPixel pixel, int region, uint place)
    {
        int slot = SlotOf(pixel, region);
        colours[slot] = Colour(pixel) + 1;
        regions[slot] = region;
        places[slot] = place;
    }

    /// <summary><paramref name="pixel"/>'s colour as one number, red lowest; its alpha is no part of its key.</summary>
    private static int Colour(RgbaPixel pixel) => pixel.R | pixel.G << 8 | pixel.B << 16;

    /// <summary>The slot <paramref name="pixel"/>'s colour in <paramref name="region"/> is held in.</summary>
    private static int SlotOf(RgbaPixel pixel, int region) =>
        (int)(unchecked((uint)(Colour(pixel) ^ region * RegionSpread) * Spread) >> (32 - SlotBits));
}
