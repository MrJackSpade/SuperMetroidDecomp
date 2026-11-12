using System.Text.Json.Serialization;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// Where the installation's picture store holds one frame's picture (#1635), the maps the frame shows it with (#1644), and
/// where it lies in the frame's rectangle (#1646): its tag, the page, the top-left pixel and the size of its rectangle there,
/// its place in the frame's rectangle, and the tags of its recolour-region and blend maps where it has them. A stored picture
/// is the box of what its frame shows, and the frame's rectangle is clear outside it, so the frame keeps where the box lies
/// and every pixel lands where it lies in the whole rectangle. Every frame showing the same pixels names the same tag and
/// rectangle, so an edit to the picture changes it wherever it shows; frames showing it with different maps name different
/// maps, and frames showing it at different places in their rectangles name different places.
/// </summary>
public sealed record InstalledPictureReferenceDocument
{
    /// <summary>
    /// The picture's tag, unique in the installation (#1639): <c>P</c>, its page, a hyphen and its place in the page's write
    /// order, such as <c>P3-0412</c>. The store's provenance lists every tag with the picture's place, and a developer names a
    /// picture by it.
    /// </summary>
    public required string Tag { get; init; }

    /// <summary>The page holding the picture, counted from zero (<see cref="Installation.PictureStoreFiles.PageFile"/>).</summary>
    public required int Page { get; init; }

    /// <summary>The top-left pixel of the picture's rectangle on its page.</summary>
    public required Position16<ImagePixels> Corner { get; init; }

    /// <summary>
    /// The width and height of the picture's rectangle (#1646): the box of what the frame shows, inside the frame's rectangle,
    /// or the frame's whole rectangle where the game shows every pixel of it, as a turning floor repeated beyond its edges is.
    /// </summary>
    public required Extent16<ImagePixels> Size { get; init; }

    /// <summary>
    /// Where the picture's top-left pixel lies in the frame's rectangle, counted from the rectangle's top-left pixel as the
    /// frame's origin is (#1646); the rectangle is clear outside the picture. Absent where the picture starts at the
    /// rectangle's top-left pixel.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Offset16<ImagePixels> Offset { get; init; }

    /// <summary>
    /// The tag of the recolour-region map the frame shows the picture with (<see cref="RecolourRegionMapFile"/>), such as
    /// <c>R0042</c>; absent where every pixel lies in region zero.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? RecolourRegionMap { get; init; }

    /// <summary>
    /// The tag of the blend map the frame shows the picture with (<see cref="BlendMapFile"/>), such as <c>B0007</c>; absent
    /// where no pixel blends by a map.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? BlendMap { get; init; }

    /// <summary>
    /// Whether the picture is one period of what the frame shows (#1651): it starts at the rectangle's top-left pixel, its size
    /// divides the rectangle's, and the frame shows it repeated across and down over the whole rectangle, as a turning floor
    /// the game repeats beyond its edges shows a plane that repeats within it. Absent where the frame shows the picture once.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool Repeats { get; init; }

    /// <summary>
    /// The rectangles the picture fills on the sheet image of a frame whose rectangle is <paramref name="frame"/>: the one
    /// place it lies (<see cref="PlacedIn"/>), or each period of the rectangle where it <see cref="Repeats"/>.
    /// </summary>
    internal IEnumerable<FrameRectangle> PlacesIn(FrameRectangle frame)
    {
        if (!Repeats)
        {
            yield return PlacedIn(frame.Corner);
            yield break;
        }
        for (int y = 0; y < frame.Size.Y; y += Size.Y)
        {
            for (int x = 0; x < frame.Size.X; x += Size.X)
                yield return new FrameRectangle(Position16<ImagePixels>.Of(frame.Corner.X + x, frame.Corner.Y + y), Size);
        }
    }

    /// <summary>Whether a repeating picture tiles a frame rectangle of <paramref name="frameSize"/>: it starts at its corner and its size divides it.</summary>
    internal bool TilesExactly(Extent16<ImagePixels> frameSize) =>
        Offset == default && Size.X > 0 && Size.Y > 0 && frameSize.X % Size.X == 0 && frameSize.Y % Size.Y == 0;

    /// <summary>The picture's rectangle on its page.</summary>
    internal FrameRectangle OnPage() => new(Corner, Size);

    /// <summary>
    /// The rectangle the picture fills on the sheet image of a frame whose rectangle's top-left pixel is
    /// <paramref name="frameCorner"/>.
    /// </summary>
    internal FrameRectangle PlacedIn(Position16<ImagePixels> frameCorner) =>
        new(Position16<ImagePixels>.Of(frameCorner.X + Offset.X, frameCorner.Y + Offset.Y), Size);

    /// <summary>Whether the picture lies wholly inside a frame rectangle of <paramref name="frameSize"/>.</summary>
    internal bool LiesInside(Extent16<ImagePixels> frameSize) =>
        Offset.X >= 0 && Offset.Y >= 0 && new FrameRectangle(Position16<ImagePixels>.Of(Offset.X, Offset.Y), Size).LiesInside(frameSize);
}
