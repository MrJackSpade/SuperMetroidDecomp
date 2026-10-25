using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The ending's screen (#1358): its palette and the graphics transfers its scenes have made since the last full clear,
/// which the capture replays over the bound artwork. A transfer covering exactly an earlier one's words replaces it, so the
/// record stays as small as the distinct transfers it holds.
/// </summary>
internal sealed class EndingScreen
{
    /// <summary>The transfers since the last full clear, oldest first.</summary>
    internal readonly List<EndingUpload> uploads = [];


    /// <summary>The ending's colors, including partial fades.</summary>
    public ScreenPalette Palette { get; } = new();

    /// <summary>Clears every transfer, as a full VRAM clear does.</summary>
    public void Clear()
    {
        uploads.Clear();
        ForgetShown();
    }

    /// <summary>What each tile layer shows (#1392), until the uploads change.</summary>
    [NonSerialized]
    private Dictionary<EndingLayerTiles, ScreenLayerPictures>? tiles;

    /// <summary>The words each plane holds, until the uploads or the artwork change.</summary>
    [NonSerialized]
    private Dictionary<EndingPlane, ushort[]>? planes;

    /// <summary>The words each explosion map holds, until the uploads or the artwork change.</summary>
    [NonSerialized]
    private Dictionary<EndingObjectFragmentId, ushort[]>? explosionPlanes;

    /// <summary>The floor's cells, until the uploads or the artwork change.</summary>
    [NonSerialized]
    private byte[]? floorCells;

    /// <summary>The sheets <see cref="tiles"/> were read from.</summary>
    [NonSerialized]
    private InstalledSpriteSheets? tileSheets;

    /// <summary>What the uploads' Mode 7 characters show (#1392), until the uploads change.</summary>
    [NonSerialized]
    private FloorTilePictures? floorTiles;

    /// <summary>What the uploads' Mode 7 characters show (#1392).</summary>
    internal FloorTilePictures FloorTiles(InstalledSpriteSheets sheets)
    {
        if (!ReferenceEquals(tileSheets, sheets))
        {
            tiles = null;
            floorTiles = null;
            tileSheets = sheets;
        }
        return floorTiles ??= EndingScreenTiles.Floor(sheets, uploads);
    }

    /// <summary>What <paramref name="layer"/> shows, reading <paramref name="depth"/> characters (#1392).</summary>
    internal ScreenLayerPictures Tiles(InstalledSpriteSheets sheets, EndingTileLayer layer, BgBitDepth depth)
    {
        if (!ReferenceEquals(tileSheets, sheets))
        {
            tiles = null;
            floorTiles = null;
            tileSheets = sheets;
        }
        tiles ??= [];
        var key = new EndingLayerTiles(layer, depth);
        if (!tiles.TryGetValue(key, out ScreenLayerPictures? shown))
            tiles.Add(key, shown = EndingScreenTiles.Layer(sheets, layer, depth, uploads));
        return shown;
    }

    /// <summary>Records a transfer of installed artwork.</summary>
    public void Record(EndingUploadKind kind, int key) => Record(new EndingUpload(kind, key, null));

    /// <summary>Records a tilemap transfer of <paramref name="words"/>, copying them as they are now.</summary>
    public void Record(EndingUploadKind kind, int key, ReadOnlySpan<ushort> words) => Record(new EndingUpload(kind, key, words.ToArray()));

    /// <summary>How many of the logo's character chunks the post-shot has shown.</summary>
    internal int LogoChunksShown => uploads.Count(upload => upload.Kind == EndingUploadKind.PostShotTransfer &&
        EndingPostShotUploadDefinitions.Get(upload.Key).Source == EndingPostShotUploadSource.LogoTiles);

    /// <summary>Forgets what the screen shows after the bound artwork changes.</summary>
    public void Invalidate()
    {
        ForgetShown();
    }

    /// <summary>The words <paramref name="plane"/> holds over <paramref name="objects"/>.</summary>
    internal ReadOnlySpan<ushort> Plane(EndingPlane plane, EndingObjectArtworkCatalog? objects)
    {
        planes ??= [];
        if (!planes.TryGetValue(plane, out ushort[]? words))
            planes.Add(plane, words = EndingScreenPlanes.Of(plane, uploads, objects));
        return words;
    }

    /// <summary>The words the explosion's <paramref name="fragment"/> map holds over <paramref name="objects"/>.</summary>
    internal ReadOnlySpan<ushort> ExplosionPlane(EndingObjectFragmentId fragment, EndingObjectArtworkCatalog? objects)
    {
        explosionPlanes ??= [];
        if (!explosionPlanes.TryGetValue(fragment, out ushort[]? words))
            explosionPlanes.Add(fragment, words = EndingScreenPlanes.Explosion(fragment, uploads, objects));
        return words;
    }

    /// <summary>The floor's cells over <paramref name="floor"/> and <paramref name="flight"/>.</summary>
    internal byte[] FloorCells(EndingFloorArtworkCatalog? floor, CeresFlightArtworkCatalog? flight) =>
        floorCells ??= EndingScreenFloor.Cells(uploads, floor, flight);

    /// <summary>Forgets every plane, cell and tile the screen showed.</summary>
    private void ForgetShown()
    {
        tiles = null;
        planes = null;
        explosionPlanes = null;
        floorCells = null;
        floorTiles = null;
    }

    /// <summary>Appends <paramref name="upload"/>, dropping the earlier transfer whose words it covers.</summary>
    private void Record(EndingUpload upload)
    {
        uploads.RemoveAll(earlier => earlier.IsReplacedBy(upload));
        uploads.Add(upload);
        ForgetShown();
    }
}
