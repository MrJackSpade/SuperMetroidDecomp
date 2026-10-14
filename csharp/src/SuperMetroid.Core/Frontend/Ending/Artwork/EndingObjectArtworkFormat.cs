namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>File identities and exact native ending/credits transfer dimensions.</summary>
public static class EndingObjectArtworkFormat
{
    /// <summary>Required ending-artwork manifest schema revision, including all selected character, map, and sprite-composition resources.</summary>
    public const int ManifestVersion = 10;
    /// <summary>Installation manifest containing the supported cartridge identity and stock-resource hashes.</summary>
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
    /// <summary>PNG transport filename preserving the raw BG3 transformation tilemap bytes; not an OBJ character-sheet identity.</summary>
    public const string PostCreditsFragmentBFileName = "post-credits-tile-fragment-b.png";
    /// <summary>Indexed PNG filename for the final-logo characters imported from <c>$99:E089</c>.</summary>
    public const string PostShotLogoTileFileName = "post-credits-logo-tiles.png";
    /// <summary>JSON filename for the final-logo tilemap imported from <c>$99:ECC4</c>.</summary>
    public const string PostShotLogoMapFileName = "post-credits-logo-map.json";
    /// <summary>Exact compiled cloud-sheet length, $4000 bytes or 512 4-bpp 8-by-8 characters.</summary>
    public const int CloudByteCount = 0x4000;
    /// <summary>Exact compiled explosion-sheet length, $6000 bytes or 768 4-bpp 8-by-8 characters.</summary>
    public const int ExplosionByteCount = 0x6000;
    /// <summary>Exact compiled length of each waiting, shooting, or suitless sheet, $4000 bytes or 512 4-bpp characters.</summary>
    public const int RewardByteCount = 0x4000;
    /// <summary>Exact waiting-map length, $0800 bytes containing 1024 sixteen-bit cells in one ordered 32-by-32 page.</summary>
    public const int WaitingTilemapByteCount = 0x0800;
    /// <summary>Exact BG3 transformation-character transfer length, $0100 bytes, retained without format conversion at upload.</summary>
    public const int PostCreditsFragmentAByteCount = 0x0100;
    /// <summary>Exact BG3 transformation tilemap transfer length, $0800 bytes containing 1024 native tilemap words.</summary>
    public const int PostCreditsFragmentBByteCount = 0x0800;
    /// <summary>Exact final-logo character length, $2000 bytes or 256 4-bpp characters, divided among four $0800-byte uploads.</summary>
    public const int PostShotLogoTileByteCount = 0x2000;
    /// <summary>Exact final-logo map length, $0800 bytes containing one 32-by-32 page of sixteen-bit tilemap cells.</summary>
    public const int PostShotLogoMapByteCount = 0x0800;
    /// <summary>Exact raw explosion-fragment length, $0800 bytes containing 1024 tilemap words; atlas transport preserves those bytes.</summary>
    public const int FragmentByteCount = 0x0800;
    /// <summary>Exact native byte length of <paramref name="transfer"/>.</summary>
    /// <param name="transfer">Transfer whose length is required.</param>
    /// <returns>The byte count the cartridge uploads for that transfer.</returns>
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
    /// <summary>Number of required explosion fragments, ordered for VRAM word destinations $7000, $7400, $7800, and $7C00.</summary>
    public const int FragmentCount = 4;
    /// <summary>Published fragment keys use the VRAM word-page sequence70,74,78,7c.</summary>
    /// <param name="index">Zero-based fragment index, zero through three, matching <see cref="EndingObjectFragmentId"/> order.</param>
    /// <returns>The lowercase page-keyed PNG filename for the fragment's byte-preserving atlas transport.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is outside zero through three.</exception>
    public static string FragmentFileName(int index)
    {
        if ((uint)index >= FragmentCount) throw new ArgumentOutOfRangeException(nameof(index));
        return "ending-explosion-fragment-" + (0x70 + 4 * index).ToString("x2", System.Globalization.CultureInfo.InvariantCulture) + ".png";
    }
}
