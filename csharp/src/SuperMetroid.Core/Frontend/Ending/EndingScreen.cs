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

    /// <summary>The tiles each plane holds, until the uploads or the artwork change.</summary>
    [NonSerialized]
    private Dictionary<EndingPlane, BackgroundTile[]>? planes;

    /// <summary>The tiles each explosion map holds, until the uploads or the artwork change.</summary>
    [NonSerialized]
    private Dictionary<EndingObjectFragmentId, BackgroundTile[]>? explosionPlanes;

    /// <summary>The sheets <see cref="tiles"/> were read from.</summary>
    [NonSerialized]
    private InstalledSpriteSheets? tileSheets;

    /// <summary>The shot whose turning floor the scenes have loaded (#1467).</summary>
    internal EndingFloorShot FloorShot => EndingScreenFloor.Shot(uploads);

    /// <summary>What <paramref name="layer"/> shows, reading <paramref name="depth"/> characters (#1392).</summary>
    internal ScreenLayerPictures Tiles(InstalledSpriteSheets sheets, EndingTileLayer layer, TileInkDepth depth)
    {
        if (!ReferenceEquals(tileSheets, sheets))
        {
            tiles = null;
            tileSheets = sheets;
        }
        tiles ??= [];
        var key = new EndingLayerTiles(layer, depth);
        if (!tiles.TryGetValue(key, out ScreenLayerPictures? shown))
            tiles.Add(key, shown = EndingScreenTiles.Layer(sheets, layer, depth, uploads));
        return shown;
    }

    /// <summary>Records a transfer of the whole installed sheet <paramref name="sheet"/>.</summary>
    public void Record(EndingUploadKind sheet) => Record(new EndingSheetUpload(sheet));

    /// <summary>Records a transfer of chunk <paramref name="chunk"/> of installed artwork.</summary>
    public void Record(EndingUploadKind kind, int chunk) => Record(new EndingChunkUpload(kind, chunk));

    /// <summary>Records a tilemap transfer of <paramref name="tiles"/>, copying them as they are now.</summary>
    public void Record(EndingUploadKind kind, ReadOnlySpan<BackgroundTile> tiles) => Record(new EndingTilemapUpload(kind, tiles.ToArray()));

    /// <summary>Records the transfer of the installed floor scene <paramref name="scene"/>.</summary>
    public void Record(EndingFloorSceneId scene) => Record(new EndingFloorSceneUpload(scene));

    /// <summary>Records the transfer of the installed explosion map <paramref name="fragment"/>.</summary>
    public void Record(EndingObjectFragmentId fragment) => Record(new EndingObjectFragmentUpload(fragment));

    /// <summary>Records a transfer of <paramref name="tiles"/> to the text tilemap <paramref name="tilemap"/>, copying them as they are now.</summary>
    internal void Record(EndingTextTilemap tilemap, ReadOnlySpan<BackgroundTile> tiles) =>
        Record(new EndingTextTilemapUpload(tilemap, tiles.ToArray()));

    /// <summary>How many of the logo's character chunks the post-shot has shown.</summary>
    internal int LogoChunksShown => uploads.Count(upload => upload is EndingChunkUpload { Kind: EndingUploadKind.PostShotTransfer } transfer &&
        EndingPostShotUploads.Get(transfer.Chunk).Source == EndingPostShotUploadSource.LogoTiles);

    /// <summary>Forgets what the screen shows after the bound artwork changes.</summary>
    public void Invalidate()
    {
        ForgetShown();
    }

    /// <summary>The tiles <paramref name="plane"/> holds over <paramref name="objects"/>.</summary>
    internal ReadOnlySpan<BackgroundTile> Plane(EndingPlane plane, EndingObjectArtwork? objects)
    {
        planes ??= [];
        if (!planes.TryGetValue(plane, out BackgroundTile[]? tiles))
            planes.Add(plane, tiles = EndingScreenPlanes.Of(plane, uploads, objects));
        return tiles;
    }

    /// <summary>The tiles the explosion's <paramref name="fragment"/> map holds over <paramref name="objects"/>.</summary>
    internal ReadOnlySpan<BackgroundTile> ExplosionPlane(EndingObjectFragmentId fragment, EndingObjectArtwork? objects)
    {
        explosionPlanes ??= [];
        if (!explosionPlanes.TryGetValue(fragment, out BackgroundTile[]? tiles))
            explosionPlanes.Add(fragment, tiles = EndingScreenPlanes.Explosion(fragment, uploads, objects));
        return tiles;
    }

    /// <summary>Forgets every plane, cell and tile the screen showed.</summary>
    private void ForgetShown()
    {
        tiles = null;
        planes = null;
        explosionPlanes = null;
    }

    /// <summary>Appends <paramref name="upload"/>, dropping the earlier transfer whose cells it covers.</summary>
    private void Record(EndingUpload upload)
    {
        uploads.RemoveAll(earlier => earlier.IsReplacedBy(upload));
        uploads.Add(upload);
        ForgetShown();
    }
}
