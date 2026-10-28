using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// What the ending's background planes show (#1404): each plane's tiles are those of the newest upload that writes the
/// plane, a tilemap the ending's scenes wrote or the installed tilemap a scene loaded for it.
/// </summary>
internal static class EndingScreenPlanes
{
    /// <summary>Cells in one plane: one 32-cell screen.</summary>
    internal const int Cells = 32 * 32;

    /// <summary>The tiles <paramref name="plane"/> holds once <paramref name="uploads"/> have been made, in order.</summary>
    /// <exception cref="InvalidOperationException">An upload names installed artwork that is not bound.</exception>
    internal static BackgroundTile[] Of(EndingPlane plane, IReadOnlyList<EndingUpload> uploads, EndingObjectArtwork? objects)
    {
        var tiles = new BackgroundTile[Cells];
        foreach (EndingUpload upload in uploads)
        {
            switch (plane)
            {
                case EndingPlane.Credits:
                    if (upload.Kind == EndingUploadKind.CreditsTilemap || IsText(upload, EndingTextTilemap.ResultPanel))
                        Tiles(upload).CopyTo(tiles);
                    break;
                case EndingPlane.Text:
                    if (IsText(upload, EndingTextTilemap.FinalText))
                        Tiles(upload).CopyTo(tiles);
                    else if (upload.Kind == EndingUploadKind.CreditsCharacterArt)
                        Load(Objects(objects).WaitingTilemap.Tiles.Span, tiles);
                    break;
                case EndingPlane.TextLower:
                    if (upload.Kind == EndingUploadKind.PostShotLowerTilemap)
                        Tiles(upload).CopyTo(tiles);
                    break;
                case EndingPlane.Logo:
                    // The logo map's one transfer carries the whole installed map.
                    if (upload.Kind == EndingUploadKind.PostShotTransfer &&
                        EndingPostShotUploads.Get(upload.As<EndingChunkUpload>().Chunk).Source == EndingPostShotUploadSource.LogoMap)
                        Load(Objects(objects).PostShotLogoMap.Tiles.Span, tiles);
                    break;
                case EndingPlane.WavySamus:
                    if (upload.Kind == EndingUploadKind.CreditsCharacterArt)
                        Load(Objects(objects).PostCreditsFragmentB.Tiles.Span, tiles);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(plane), plane, "Undefined ending plane.");
            }
        }
        return tiles;
    }

    /// <summary>Whether <paramref name="upload"/> writes the text tilemap <paramref name="tilemap"/>.</summary>
    private static bool IsText(EndingUpload upload, EndingTextTilemap tilemap) =>
        upload is EndingTextTilemapUpload written && written.Tilemap == tilemap;

    /// <summary>The tiles the explosion's <paramref name="fragment"/> map holds once <paramref name="uploads"/> have been made.</summary>
    /// <exception cref="InvalidOperationException">An upload names installed artwork that is not bound.</exception>
    internal static BackgroundTile[] Explosion(EndingObjectFragmentId fragment, IReadOnlyList<EndingUpload> uploads, EndingObjectArtwork? objects)
    {
        var tiles = new BackgroundTile[Cells];
        foreach (EndingUpload upload in uploads)
        {
            if (upload is EndingObjectFragmentUpload loaded && loaded.Fragment == fragment)
                Load(Objects(objects).Fragment(fragment).Tiles.Span, tiles);
        }
        return tiles;
    }

    /// <summary>The tiles a tilemap upload recorded.</summary>
    private static ReadOnlySpan<BackgroundTile> Tiles(EndingUpload upload) => upload switch
    {
        EndingTilemapUpload tilemap => tilemap.Tiles,
        EndingTextTilemapUpload text => text.Tiles,
        _ => throw new InvalidOperationException($"The {upload.Kind} transfer records no tiles."),
    };

    /// <summary>Copies an installed map's tiles over the start of <paramref name="tiles"/>.</summary>
    private static void Load(ReadOnlySpan<BackgroundTile> installed, Span<BackgroundTile> tiles) =>
        installed[..Math.Min(installed.Length, tiles.Length)].CopyTo(tiles);

    /// <summary>The bound OBJ artwork, which a plane's artwork uploads require.</summary>
    private static EndingObjectArtwork Objects(EndingObjectArtwork? objects) =>
        objects ?? throw new InvalidOperationException("Ending objects require installed artwork.");
}
