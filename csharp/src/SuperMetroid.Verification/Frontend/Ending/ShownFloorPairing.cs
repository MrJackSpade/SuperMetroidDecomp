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
/// pixel from its ink alone: whether it is clear or has a colour, and the colour its picture's inks show under the
/// floor's shading (<see cref="PictureShading.InkColours"/>). Two floors that lay inks of the same kinds and show the same
/// colour at every floor point therefore draw the same pixels in any frame that shows either of them the same way. The
/// memory floor's inks are fixed by the image's palette indices; each of its colours shows the palette's colour at the
/// composed palette index its installation records. So the floors' inks are paired once, and each frame compares only
/// the colours of the paired inks.
/// </summary>
internal sealed class ShownFloorPairing
{
    /// <summary>The live floor whose pictures were paired.</summary>
    private readonly AffineFloor live;

    /// <summary>The memory image the paired floor was composed from.</summary>
    private readonly byte[] memoryImage;

    /// <summary>The composed palette index each colour of each of the memory floor's pictures came from, picture after picture.</summary>
    private readonly int[][] memoryIndices;

    /// <summary>Each distinct pair of inks the two floors lay at one floor point.</summary>
    private readonly ShownFloorInkPair[] pairs;

    /// <summary>Whether some floor point holds inks of different kinds on the two floors, which no colours make show alike.</summary>
    private readonly bool kindsDiffer;

    /// <summary>The memory floor last composed, in <see cref="composedPalette"/>.</summary>
    private AffineFloor composed;

    /// <summary>The palette <see cref="composed"/> was composed in.</summary>
    private RgbColor[] composedPalette;

    /// <summary>Pairs <paramref name="liveFloor"/> with the floor <paramref name="image"/> composes in <paramref name="palette"/>.</summary>
    /// <exception cref="InvalidOperationException">The composed floor does not show its colours at the palette indices its installation records.</exception>
    internal ShownFloorPairing(AffineFloor liveFloor, ReadOnlySpan<byte> image, ReadOnlySpan<RgbColor> palette)
    {
        live = Ensure.NotNull(liveFloor);
        memoryImage = image.ToArray();
        composedPalette = palette.ToArray();
        composed = AffineFloorFixtures.Floor(image, palette);
        memoryIndices = [.. composed.Pictures.Select(picture => Enumerable.Range(0, picture.Sheet.StandardColours.Length)
            .Select(colour => InstalledPictureColoursConstants.ComposedIndexOf(picture.Sheet, colour)).ToArray())];
        PictureShading memoryShading = ShadingOf(composed);
        for (int picture = 0; picture < composed.Pictures.Length; picture++)
        {
            RgbColor[] shown = memoryShading.InkColours(composed.Pictures[picture].Sheet);
            for (int colour = 0; colour < shown.Length; colour++)
                AssertEqual(MemoryColour(palette, picture, colour), shown[colour],
                    $"memory floor picture {picture} shows its colour {colour} as the palette's colour at its recorded composed index");
        }
        int[] livePictures = new int[AffineFloor.SidePixels * AffineFloor.SidePixels], memoryPictures = new int[livePictures.Length];
        var liveInks = new SpriteImageInk[livePictures.Length];
        var memoryInks = new SpriteImageInk[livePictures.Length];
        Lay(live, livePictures, liveInks);
        Lay(composed, memoryPictures, memoryInks);
        var distinct = new HashSet<ShownFloorInkPair>();
        for (int point = 0; point < liveInks.Length; point++)
            distinct.Add(new ShownFloorInkPair(livePictures[point], liveInks[point], memoryPictures[point], memoryInks[point]));
        pairs = [.. distinct];
        kindsDiffer = pairs.Any(pair => !SameKind(pair.LiveInk, pair.MemoryInk));
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
        RgbColor[][] liveColours = [.. liveFloor.Pictures.Select(picture => shading.InkColours(picture.Sheet))];
        foreach (ShownFloorInkPair pair in pairs)
        {
            if (pair.LiveInk.Colour is int liveColour && pair.MemoryInk.Colour is int memoryColour
                && liveColours[pair.LivePicture][liveColour] != MemoryColour(palette, pair.MemoryPicture, memoryColour))
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

    /// <summary>The colour the memory floor's picture <paramref name="picture"/> shows for its colour <paramref name="colour"/> when composed in <paramref name="palette"/>.</summary>
    private RgbColor MemoryColour(ReadOnlySpan<RgbColor> palette, int picture, int colour) =>
        RgbColor.FromWord(palette[memoryIndices[picture][colour]].ToWord());

    /// <summary>The shading a frontend floor names.</summary>
    private static PictureShading ShadingOf(AffineFloor floor) =>
        floor.Shading ?? throw new InvalidOperationException("A frontend screen's turning floor names its shading.");

    /// <summary>Whether <paramref name="first"/> and <paramref name="second"/> are drawn alike but for their colours.</summary>
    private static bool SameKind(SpriteImageInk first, SpriteImageInk second) =>
        first.IsTransparent == second.IsTransparent && first.Clear == second.Clear && first.Blends == second.Blends
        && first.FrontTile == second.FrontTile;

    /// <summary>
    /// Lays <paramref name="floor"/>'s pictures as a backend lays them, keeping each floor point's picture and ink: the
    /// image over the whole floor, then each picture drawn over it at its origin's place, over those before it.
    /// </summary>
    private static void Lay(AffineFloor floor, int[] pictures, SpriteImageInk[] inks)
    {
        Lay(floor, floor.Image, 0, 0, pictures, inks);
        foreach (SpriteSheetFrame picture in floor.DrawnOver)
            Lay(floor, picture, -picture.Origin.X, -picture.Origin.Y, pictures, inks);
    }

    /// <summary>Lays <paramref name="picture"/> of <paramref name="floor"/> with its top-left at floor pixel (<paramref name="left"/>, <paramref name="top"/>).</summary>
    private static void Lay(AffineFloor floor, SpriteSheetFrame picture, int left, int top, int[] pictures, SpriteImageInk[] inks)
    {
        int place = floor.Pictures.IndexOf(picture);
        SpriteImage sheet = picture.Sheet;
        ReadOnlySpan<SpriteImageInk> sheetInks = sheet.Inks;
        for (int y = 0; y < picture.Size.Y; y++)
        {
            int source = (picture.Corner.Y + y) * sheet.Size.X + picture.Corner.X;
            int target = (top + y) * AffineFloor.SidePixels + left;
            for (int x = 0; x < picture.Size.X; x++)
            {
                pictures[target + x] = place;
                inks[target + x] = sheetInks[source + x];
            }
        }
    }
}
