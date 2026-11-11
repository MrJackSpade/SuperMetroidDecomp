using System.Collections.Immutable;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Verification.Frontend.Ending;

/// <summary>
/// A live turning floor paired with the floor a memory image composes (#1568), so a frame check tells from the floors'
/// descriptions whether they show alike. A backend lays a floor's pictures, each over those before it, and colours each
/// pixel from its colour key alone (#1633): whether it is clear, and the colour its key shows under the floor's shading
/// (<see cref="PictureShading.KeyColours(SpriteImage)"/>). Two floors that are clear at the same floor points and show the
/// same colour at every other floor point therefore draw the same pixels in any frame that shows either of them the same
/// way. The memory floor's pixels are fixed by the image's palette indices; each shows the palette's colour at the palette
/// index it was composed as. So the floors' pixels are paired once, and each frame compares only the colours of the paired
/// pixels.
/// </summary>
internal sealed class ShownFloorPairing
{
    /// <summary>The live floor whose pictures were paired.</summary>
    private readonly AffineFloor live;

    /// <summary>The memory image the paired floor was composed from.</summary>
    private readonly byte[] memoryImage;

    /// <summary>Each distinct pair of pixels the two floors lay at one floor point.</summary>
    private readonly ShownFloorPixelPair[] pairs;

    /// <summary>Whether some floor point is clear on one floor and not the other, which no colours make show alike.</summary>
    private readonly bool kindsDiffer;

    /// <summary>The memory floor last composed, in <see cref="composedPalette"/>.</summary>
    private AffineFloor composed;

    /// <summary>The palette <see cref="composed"/> was composed in.</summary>
    private RgbColor[] composedPalette;

    /// <summary>Pairs <paramref name="liveFloor"/> with the floor <paramref name="image"/> composes in <paramref name="palette"/>.</summary>
    /// <exception cref="InvalidOperationException">The composed floor does not show its colours at the palette indices its pixels were composed as.</exception>
    internal ShownFloorPairing(AffineFloor liveFloor, ReadOnlySpan<byte> image, ReadOnlySpan<RgbColor> palette)
    {
        live = Ensure.NotNull(liveFloor);
        memoryImage = image.ToArray();
        composedPalette = palette.ToArray();
        composed = AffineFloorFixtures.Floor(image, palette);
        PictureShading memoryShading = ShadingOf(composed);
        for (int picture = 0; picture < composed.Pictures.Length; picture++)
        {
            SpriteImage sheet = composed.Pictures[picture].Sheet;
            RgbColor[] shown = memoryShading.KeyColours(sheet);
            ImmutableArray<ColourKey> keys = sheet.ColourKeys;
            for (int key = 0; key < shown.Length; key++)
            {
                foreach (int index in InstalledPictureColoursConstants.ComposedIndicesOf(sheet, keys[key]))
                    AssertEqual(MemoryColour(palette, index), shown[key],
                        $"memory floor picture {picture} shows its colour key {keys[key]} as the palette's colour at its composed index {index}");
            }
        }
        int length = AffineFloor.SidePixels * AffineFloor.SidePixels;
        var livePixels = new LaidPixel[length];
        var memoryPixels = new LaidPixel[length];
        Lay(live, livePixels, composedIndices: false);
        Lay(composed, memoryPixels, composedIndices: true);
        var distinct = new HashSet<ShownFloorPixelPair>();
        for (int point = 0; point < length; point++)
        {
            distinct.Add(new ShownFloorPixelPair(livePixels[point].Picture, livePixels[point].Value, livePixels[point].Clear,
                memoryPixels[point].Picture, memoryPixels[point].Value, memoryPixels[point].Clear));
        }
        pairs = [.. distinct];
        kindsDiffer = pairs.Any(pair => pair.LiveClear != pair.MemoryClear);
    }

    /// <summary>Whether this pairing is of <paramref name="liveFloor"/>'s pictures and the floor <paramref name="image"/> composes.</summary>
    internal bool Pairs(AffineFloor liveFloor, ReadOnlySpan<byte> image) => live.SamePictures(liveFloor) && image.SequenceEqual(memoryImage);

    /// <summary>
    /// Whether <paramref name="liveFloor"/>, which lays the paired live pictures, shows at every floor point what the memory
    /// floor composed in <paramref name="palette"/> shows there.
    /// </summary>
    /// <exception cref="InvalidOperationException">The floor lays other pictures than the paired live floor's.</exception>
    internal bool ShowAlike(AffineFloor liveFloor, ReadOnlySpan<RgbColor> palette)
    {
        AssertTrue(live.SamePictures(liveFloor), "the compared live floor lays the paired floor's pictures");
        if (kindsDiffer)
            return false;
        PictureShading shading = ShadingOf(liveFloor);
        RgbColor[][] liveColours = [.. liveFloor.Pictures.Select(picture => shading.KeyColours(picture.Sheet))];
        foreach (ShownFloorPixelPair pair in pairs)
        {
            if (!pair.LiveClear && liveColours[pair.LivePicture][pair.LiveKey] != MemoryColour(palette, pair.MemoryIndex))
                return false;
        }
        return true;
    }

    /// <summary>The floor the paired memory image composes in <paramref name="palette"/>, composed again only when the palette differs from the last one's.</summary>
    internal AffineFloor MemoryFloor(ReadOnlySpan<RgbColor> palette)
    {
        if (!palette.SequenceEqual(composedPalette))
        {
            composed = AffineFloorFixtures.Floor(memoryImage, palette);
            composedPalette = palette.ToArray();
        }
        return composed;
    }

    /// <summary>The colour a memory floor pixel composed as palette index <paramref name="index"/> shows when composed in <paramref name="palette"/>.</summary>
    private static RgbColor MemoryColour(ReadOnlySpan<RgbColor> palette, int index) => RgbColor.FromWord(palette[index].ToWord());

    /// <summary>The shading a frontend floor names.</summary>
    private static PictureShading ShadingOf(AffineFloor floor) =>
        floor.Shading ?? throw new InvalidOperationException("A frontend screen's turning floor names its shading.");

    /// <summary>
    /// Lays <paramref name="floor"/>'s pictures as a backend lays them, keeping each floor point's picture and pixel: the
    /// image over the whole floor, then each picture drawn over it at its origin's place, over those before it. A pixel is
    /// kept as its colour key's place among its sheet's keys, or, where <paramref name="composedIndices"/> is set, as the
    /// palette index it was composed as.
    /// </summary>
    private static void Lay(AffineFloor floor, LaidPixel[] laid, bool composedIndices)
    {
        Lay(floor, floor.Image, 0, 0, laid, composedIndices);
        foreach (SpriteSheetFrame picture in floor.DrawnOver)
            Lay(floor, picture, -picture.Origin.X, -picture.Origin.Y, laid, composedIndices);
    }

    /// <summary>Lays <paramref name="picture"/> of <paramref name="floor"/> with its top-left at floor pixel (<paramref name="left"/>, <paramref name="top"/>).</summary>
    private static void Lay(AffineFloor floor, SpriteSheetFrame picture, int left, int top, LaidPixel[] laid, bool composedIndices)
    {
        int place = floor.Pictures.IndexOf(picture);
        SpriteImage sheet = picture.Sheet;
        ReadOnlySpan<RgbaPixel> pixels = sheet.Pixels;
        for (int y = 0; y < picture.Size.Y; y++)
        {
            int source = (picture.Corner.Y + y) * sheet.Size.X + picture.Corner.X;
            int target = (top + y) * AffineFloor.SidePixels + left;
            for (int x = 0; x < picture.Size.X; x++)
            {
                int pixel = source + x;
                // A clear pixel holds no colour key and shows no colour of its key (#1633).
                bool clear = pixels[pixel].A == 0;
                int value = clear ? -1
                    : composedIndices ? InstalledPictureColoursConstants.ComposedIndexAt(sheet, pixel)
                    : sheet.PlaceOf(sheet.KeyAt(pixel)) ?? throw new InvalidOperationException($"{sheet} holds no key for its own pixel {pixel}.");
                laid[target + x] = new LaidPixel(place, value, clear);
            }
        }
    }

    /// <summary>One floor point as a floor lays it.</summary>
    /// <param name="Picture">The place among the floor's pictures of the picture laid there.</param>
    /// <param name="Value">The pixel's colour key place, or the palette index it was composed as; -1 for a clear pixel.</param>
    /// <param name="Clear">Whether the pixel is clear.</param>
    private readonly record struct LaidPixel(int Picture, int Value, bool Clear);
}
