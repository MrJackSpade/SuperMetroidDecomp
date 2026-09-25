namespace SuperMetroid.Core.Assets;

/// <summary>Four mutually exclusive native explosion OBJ fragment uploads.</summary>
public enum EndingObjectFragmentId
{
    Segment70,
    Segment74,
    Segment78,
    Segment7C,
}

/// <summary>
/// Editable character sheets and the waiting-scene BG2 map for the ending and credits.
/// Actor instructions, palette selection and timing remain code; atmospheric
/// cloud OAM compositions are a separate editable presentation resource.
/// </summary>
public sealed class EndingObjectArtworkCatalog
{
    private readonly RoomCharacterAtlas[] fragments;

    public EndingObjectArtworkCatalog(RoomCharacterAtlas clouds,
        RoomCharacterAtlas explosion, IReadOnlyList<RoomCharacterAtlas> fragments,
        RoomCharacterAtlas waitingSamus, RoomCharacterAtlas shootingScreen,
        RoomCharacterAtlas suitlessSamus, RoomBackgroundTilemapAtlas waitingTilemap,
        RoomCharacterAtlas postCreditsFragmentA, RoomCharacterAtlas postCreditsFragmentB,
        RoomCharacterAtlas postShotLogoTiles, RoomBackgroundTilemapAtlas postShotLogoMap,
        EndingCloudSpritePresentation cloudSprites,
        EndingExplosionSpritePresentation explosionSprites,
        EndingCompletionTextSpritePresentation completionTextSprites)
    {
        Clouds = clouds ?? throw new ArgumentNullException(nameof(clouds));
        Explosion = explosion ?? throw new ArgumentNullException(nameof(explosion));
        WaitingSamus = waitingSamus ?? throw new ArgumentNullException(nameof(waitingSamus));
        ShootingScreen = shootingScreen ?? throw new ArgumentNullException(nameof(shootingScreen));
        SuitlessSamus = suitlessSamus ?? throw new ArgumentNullException(nameof(suitlessSamus));
        WaitingTilemap = waitingTilemap ?? throw new ArgumentNullException(nameof(waitingTilemap));
        PostCreditsFragmentA = postCreditsFragmentA ?? throw new ArgumentNullException(nameof(postCreditsFragmentA));
        PostCreditsFragmentB = postCreditsFragmentB ?? throw new ArgumentNullException(nameof(postCreditsFragmentB));
        PostShotLogoTiles = postShotLogoTiles ?? throw new ArgumentNullException(nameof(postShotLogoTiles));
        PostShotLogoMap = postShotLogoMap ?? throw new ArgumentNullException(nameof(postShotLogoMap));
        CloudSprites = cloudSprites ?? throw new ArgumentNullException(nameof(cloudSprites));
        ExplosionSprites = explosionSprites ?? throw new ArgumentNullException(nameof(explosionSprites));
        CompletionTextSprites = completionTextSprites ??
            throw new ArgumentNullException(nameof(completionTextSprites));
        ArgumentNullException.ThrowIfNull(fragments);
        if (Clouds.Transfer.Length != EndingObjectArtworkFormat.CloudByteCount ||
            Explosion.Transfer.Length != EndingObjectArtworkFormat.ExplosionByteCount ||
            WaitingSamus.Transfer.Length != EndingObjectArtworkFormat.RewardByteCount ||
            ShootingScreen.Transfer.Length != EndingObjectArtworkFormat.RewardByteCount ||
            SuitlessSamus.Transfer.Length != EndingObjectArtworkFormat.RewardByteCount ||
            WaitingTilemap.Transfer.Length != EndingObjectArtworkFormat.WaitingTilemapByteCount ||
            PostCreditsFragmentA.Transfer.Length != EndingObjectArtworkFormat.PostCreditsFragmentAByteCount ||
            PostCreditsFragmentB.Transfer.Length != EndingObjectArtworkFormat.PostCreditsFragmentBByteCount ||
            PostShotLogoTiles.Transfer.Length != EndingObjectArtworkFormat.PostShotLogoTileByteCount ||
            PostShotLogoMap.Transfer.Length != EndingObjectArtworkFormat.PostShotLogoMapByteCount ||
            fragments.Count != EndingObjectArtworkFormat.FragmentCount ||
            fragments.Any(fragment => fragment is null ||
                fragment.Transfer.Length != EndingObjectArtworkFormat.FragmentByteCount))
            throw new InvalidDataException("Ending OBJ catalog has an incorrect native transfer length.");
        this.fragments = fragments.ToArray();
    }

    public RoomCharacterAtlas Clouds { get; }
    /// <summary>Six editable atmospheric-cloud OAM compositions.</summary>
    public EndingCloudSpritePresentation CloudSprites { get; }
    /// <summary>Sixteen editable Zebes-explosion OAM compositions.</summary>
    public EndingExplosionSpritePresentation ExplosionSprites { get; }
    /// <summary>Fifty-six editable completion-message and clear-time OAM compositions.</summary>
    public EndingCompletionTextSpritePresentation CompletionTextSprites { get; }
    public RoomCharacterAtlas Explosion { get; }
    /// <summary>BG2 waiting scene, also reused by both suited reward variants.</summary>
    public RoomCharacterAtlas WaitingSamus { get; }
    /// <summary>Post-credits shooting scene OBJ sheet.</summary>
    public RoomCharacterAtlas ShootingScreen { get; }
    /// <summary>Under-three-hour suitless reward sheet.</summary>
    public RoomCharacterAtlas SuitlessSamus { get; }
    /// <summary>BG2 waiting scene's ordered 32x32 tile and palette references.</summary>
    public RoomBackgroundTilemapAtlas WaitingTilemap { get; }
    /// <summary>Small BG/OBJ character uploads retained at VRAM bytes $4000 and $4800.</summary>
    public RoomCharacterAtlas PostCreditsFragmentA { get; }
    public RoomCharacterAtlas PostCreditsFragmentB { get; }
    /// <summary>Four post-shot logo tile chunks uploaded to BG/OBJ VRAM.</summary>
    public RoomCharacterAtlas PostShotLogoTiles { get; }
    /// <summary>Post-shot logo's ordered 32x32 tile and palette references.</summary>
    public RoomBackgroundTilemapAtlas PostShotLogoMap { get; }

    /// <summary>Four ordered $0800-byte OBJ fragments uploaded at VRAM $E000..$FFFF.</summary>
    public RoomCharacterAtlas Fragment(EndingObjectFragmentId id) =>
        (uint)id < fragments.Length ? fragments[(int)id] :
            throw new ArgumentOutOfRangeException(nameof(id));
}

/// <summary>File identities and exact native ending/credits transfer dimensions.</summary>
public static class EndingObjectArtworkFormat
{
    public const int ManifestVersion = 8;
    public const string ManifestFileName = "ending-object-artwork.json";
    public const string CloudFileName = "ending-cloud-characters.png";
    public const string ExplosionFileName = "ending-explosion-objects.png";
    public const string WaitingSamusFileName = "credits-waiting-samus.png";
    public const string ShootingScreenFileName = "post-credits-shooting.png";
    public const string SuitlessSamusFileName = "post-credits-suitless-samus.png";
    public const string WaitingTilemapFileName = "credits-waiting-tilemap.json";
    public const string PostCreditsFragmentAFileName = "post-credits-tile-fragment-a.png";
    public const string PostCreditsFragmentBFileName = "post-credits-tile-fragment-b.png";
    public const string PostShotLogoTileFileName = "post-credits-logo-tiles.png";
    public const string PostShotLogoMapFileName = "post-credits-logo-map.json";
    public const int CloudByteCount = 0x4000;
    public const int ExplosionByteCount = 0x6000;
    public const int RewardByteCount = 0x4000;
    public const int WaitingTilemapByteCount = 0x0800;
    public const int PostCreditsFragmentAByteCount = 0x0100;
    public const int PostCreditsFragmentBByteCount = 0x0800;
    public const int PostShotLogoTileByteCount = 0x2000;
    public const int PostShotLogoMapByteCount = 0x0800;
    public const int FragmentByteCount = 0x0800;
    public const int FragmentCount = 4;
    public static string FragmentFileName(int index) => index switch
    {
        0 => "ending-explosion-fragment-70.png",
        1 => "ending-explosion-fragment-74.png",
        2 => "ending-explosion-fragment-78.png",
        3 => "ending-explosion-fragment-7c.png",
        _ => throw new ArgumentOutOfRangeException(nameof(index)),
    };
}
