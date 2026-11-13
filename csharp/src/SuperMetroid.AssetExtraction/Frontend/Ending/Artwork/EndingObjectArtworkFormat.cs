using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>File identities and exact ending/credits artwork dimensions.</summary>
public static class EndingObjectArtworkFormat
{
    /// <summary>Required ending-artwork manifest schema revision, including all selected character, map, and sprite-composition resources; 14 is import data (#1693).</summary>
    public const int ManifestVersion = 14;
    /// <summary>Installation manifest containing the supported source identity and stock-resource hashes.</summary>
    public const string ManifestFileName = "ending-object-artwork.json";
    /// <summary>Indexed PNG filename for the atmospheric-cloud OBJ characters.</summary>
    public const string CloudFileName = "ending-cloud-characters.png";
    /// <summary>Indexed PNG filename for the Zebes-explosion OBJ characters, separate from its four tilemap fragments.</summary>
    public const string ExplosionFileName = "ending-explosion-objects.png";
    /// <summary>Indexed PNG filename for the waiting-Samus characters reused by suited reward variants.</summary>
    public const string WaitingSamusFileName = "credits-waiting-samus.png";
    /// <summary>Indexed PNG filename for the post-credits shooting OBJ sheet.</summary>
    public const string ShootingScreenFileName = "post-credits-shooting.png";
    /// <summary>Indexed PNG filename for the under-three-hour suitless reward OBJ sheet.</summary>
    public const string SuitlessSamusFileName = "post-credits-suitless-samus.png";
    /// <summary>JSON filename for the waiting scene's ordered BG2 tile, palette, priority, and flip references.</summary>
    public const string WaitingTilemapFileName = "credits-waiting-tilemap.json";
    /// <summary>PNG filename preserving the small BG3 transformation-character transfer.</summary>
    public const string PostCreditsFragmentAFileName = "post-credits-tile-fragment-a.png";
    /// <summary>JSON filename for the BG3 transformation tilemap's ordered tile, palette, priority, and flip references.</summary>
    public const string PostCreditsFragmentBFileName = "post-credits-tile-fragment-b.json";
    /// <summary>Indexed PNG filename for the final-logo characters.</summary>
    public const string PostShotLogoTileFileName = "post-credits-logo-tiles.png";
    /// <summary>JSON filename for the final-logo tilemap.</summary>
    public const string PostShotLogoMapFileName = "post-credits-logo-map.json";
    /// <summary>Exact compiled cloud-sheet length, 16384 bytes or 512 4-bpp 8-by-8 characters.</summary>
    public const int CloudByteCount = 512 * 32;
    /// <summary>Exact compiled explosion-sheet length, 24576 bytes or 768 4-bpp 8-by-8 characters.</summary>
    public const int ExplosionByteCount = 768 * 32;
    /// <summary>Exact compiled length of each waiting, shooting, or suitless sheet, 16384 bytes or 512 4-bpp characters.</summary>
    public const int RewardByteCount = 512 * 32;
    /// <summary>Cells in each installed ending tilemap: one ordered 32-by-32 page.</summary>
    public const int MapCellCount = 1024;
    /// <summary>Exact waiting-map length, 2048 bytes containing 1024 sixteen-bit cells in one ordered 32-by-32 page.</summary>
    public const int WaitingTilemapByteCount = MapCellCount * sizeof(ushort);
    /// <summary>Exact BG3 transformation-character length, 256 bytes, retained without format conversion when loaded.</summary>
    public const int PostCreditsFragmentAByteCount = 256;
    /// <summary>Exact BG3 transformation tilemap length, 2048 bytes containing 1024 tilemap words.</summary>
    public const int PostCreditsFragmentBByteCount = MapCellCount * sizeof(ushort);
    /// <summary>Exact final-logo character length, 8192 bytes or 256 4-bpp characters, divided among four 2048-byte loads.</summary>
    public const int PostShotLogoTileByteCount = 256 * 32;
    /// <summary>Exact final-logo map length, 2048 bytes containing one 32-by-32 page of sixteen-bit tilemap cells.</summary>
    public const int PostShotLogoMapByteCount = MapCellCount * sizeof(ushort);
    /// <summary>Exact explosion-fragment length, 2048 bytes containing one 32-by-32 page of sixteen-bit tilemap cells.</summary>
    public const int FragmentByteCount = MapCellCount * sizeof(ushort);
    /// <summary>Exact byte length of <paramref name="transfer"/>.</summary>
    /// <param name="transfer">Transfer whose length is required.</param>
    /// <returns>The byte count the game loads for that artwork.</returns>
    public static int ByteCount(EndingObjectTransfer transfer) => transfer switch
    {
        EndingObjectTransfer.Clouds => CloudByteCount,
        EndingObjectTransfer.Explosion => ExplosionByteCount,
        EndingObjectTransfer.WaitingSamus or EndingObjectTransfer.ShootingScreen or
            EndingObjectTransfer.SuitlessSamus => RewardByteCount,
        EndingObjectTransfer.WaitingTilemap => WaitingTilemapByteCount,
        EndingObjectTransfer.PostCreditsFragmentA => PostCreditsFragmentAByteCount,
        EndingObjectTransfer.PostCreditsFragmentB => PostCreditsFragmentBByteCount,
        EndingObjectTransfer.PostShotLogoTiles => PostShotLogoTileByteCount,
        EndingObjectTransfer.PostShotLogoMap => PostShotLogoMapByteCount,
        EndingObjectTransfer.ExplosionFragment => FragmentByteCount,
        _ => throw new InvalidOperationException($"Undefined {nameof(EndingObjectTransfer)} {transfer}."),
    };
    /// <summary>Number of required explosion fragments, ordered by the four map pages they fill.</summary>
    public const int FragmentCount = 4;
    /// <summary>Published fragment keys use the page sequence70,74,78,7c.</summary>
    /// <param name="index">Zero-based fragment index, zero through three, matching <see cref="EndingObjectFragmentId"/> order.</param>
    /// <returns>The lowercase page-keyed JSON filename of the fragment's tile references.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is outside zero through three.</exception>
    public static string FragmentFileName(int index)
    {
        if ((uint)index >= FragmentCount) throw new ArgumentOutOfRangeException(nameof(index));
        return "ending-explosion-fragment-" + (112 + 4 * index).ToString("x2", System.Globalization.CultureInfo.InvariantCulture) + ".json";
    }
}
