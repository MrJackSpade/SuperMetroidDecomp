namespace SuperMetroid.Core.Assets;

/// <summary>Identities of the four native explosion tilemap fragments uploaded to successive VRAM word pages.</summary>
public enum EndingObjectFragmentId
{
    /// <summary>Wide explosion map from <c>$98:B5C1</c>, <c>Wide_Part_of_Zebes_Explosion_Tilemap</c>, uploaded at VRAM word $7000 (byte $E000).</summary>
    Segment70,
    /// <summary>Concentric wide explosion map from <c>$98:B857</c>, <c>Concentric_Wide_Part_of_Zebes_Explosion_Tilemap</c>, uploaded at VRAM word $7400 (byte $E800).</summary>
    Segment74,
    /// <summary>Eclipse map from <c>$98:BAED</c>, <c>Eclipse_of_Zebes_during_Explosion_Tilemap</c>, uploaded at VRAM word $7800 (byte $F000).</summary>
    Segment78,
    /// <summary>Blank map from <c>$98:BCCD</c>, <c>Blank_BG2_Tilemap</c>, uploaded at VRAM word $7C00 (byte $F800).</summary>
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

    /// <summary>Combines already compiled ending artwork and sprite compositions, enforcing the native transfer lengths.</summary>
    /// <param name="clouds">$4000-byte atmospheric-cloud OBJ character sheet.</param>
    /// <param name="explosion">$6000-byte Zebes-explosion OBJ character sheet.</param>
    /// <param name="fragments">Four $0800-byte raw tilemap transfers, ordered by <see cref="EndingObjectFragmentId"/>; their bytes are carried in character-atlas PNG transport.</param>
    /// <param name="waitingSamus">$4000-byte waiting-Samus character sheet, also used for suited rewards.</param>
    /// <param name="shootingScreen">$4000-byte Samus-shooting OBJ sheet.</param>
    /// <param name="suitlessSamus">$4000-byte suitless reward OBJ sheet.</param>
    /// <param name="waitingTilemap">$0800-byte, 32-by-32-cell BG2 waiting-scene map.</param>
    /// <param name="postCreditsFragmentA">$0100-byte BG3 transformation-character transfer.</param>
    /// <param name="postCreditsFragmentB">$0800-byte BG3 transformation tilemap, carried in character-atlas PNG transport.</param>
    /// <param name="postShotLogoTiles">$2000-byte logo character stream divided into four staged uploads.</param>
    /// <param name="postShotLogoMap">$0800-byte, 32-by-32-cell final-logo map.</param>
    /// <param name="cloudSprites">Selected atmospheric-cloud OAM compositions.</param>
    /// <param name="explosionSprites">Selected Zebes-explosion OAM compositions.</param>
    /// <param name="completionTextSprites">Selected completion-message and clear-time OAM compositions.</param>
    /// <param name="rewardSprites">Selected post-credits Samus reward OAM compositions.</param>
    /// <param name="logoSprites">Selected assembling-logo OAM compositions.</param>
    /// <remarks>Copies the fragment sequence but retains the supplied atlas and presentation objects. This constructor validates transfer sizes, not stock hashes or manifest provenance; installation loading performs those checks.</remarks>
    /// <exception cref="ArgumentNullException">An atlas, composition resource, or fragment sequence is null.</exception>
    /// <exception cref="InvalidDataException">A transfer has an incorrect byte length, or the fragment sequence does not contain exactly four non-null, correctly sized entries.</exception>
    public EndingObjectArtworkCatalog(RoomCharacterAtlas clouds,
        RoomCharacterAtlas explosion, IReadOnlyList<RoomCharacterAtlas> fragments,
        RoomCharacterAtlas waitingSamus, RoomCharacterAtlas shootingScreen,
        RoomCharacterAtlas suitlessSamus, RoomBackgroundTilemapAtlas waitingTilemap,
        RoomCharacterAtlas postCreditsFragmentA, RoomCharacterAtlas postCreditsFragmentB,
        RoomCharacterAtlas postShotLogoTiles, RoomBackgroundTilemapAtlas postShotLogoMap,
        EndingCloudSpritePresentation cloudSprites,
        EndingExplosionSpritePresentation explosionSprites,
        EndingCompletionTextSpritePresentation completionTextSprites,
        EndingRewardSpritePresentation rewardSprites,
        EndingLogoSpritePresentation logoSprites)
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
        RewardSprites = rewardSprites ?? throw new ArgumentNullException(nameof(rewardSprites));
        LogoSprites = logoSprites ?? throw new ArgumentNullException(nameof(logoSprites));
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

    /// <summary>Compiled 4-bpp atmospheric-cloud OBJ sheet imported from <c>$99:A56F</c> and uploaded at VRAM byte $C000; placement is supplied separately by <see cref="CloudSprites"/>.</summary>
    public RoomCharacterAtlas Clouds { get; }
    /// <summary>Six editable atmospheric-cloud OAM compositions.</summary>
    public EndingCloudSpritePresentation CloudSprites { get; }
    /// <summary>Sixteen editable Zebes-explosion OAM compositions.</summary>
    public EndingExplosionSpritePresentation ExplosionSprites { get; }
    /// <summary>Fifty-six editable completion-message and clear-time OAM compositions.</summary>
    public EndingCompletionTextSpritePresentation CompletionTextSprites { get; }
    /// <summary>Thirty-seven editable post-credits Samus reward OAM compositions.</summary>
    public EndingRewardSpritePresentation RewardSprites { get; }
    /// <summary>Eight editable OAM compositions for the final assembling logo.</summary>
    public EndingLogoSpritePresentation LogoSprites { get; }
    /// <summary>Compiled 4-bpp explosion OBJ sheet imported from <c>$98:8304</c> and uploaded at VRAM byte $8000; separate tilemap fragments occupy $E000-$FFFF.</summary>
    public RoomCharacterAtlas Explosion { get; }
    /// <summary>BG2 waiting scene, also reused by both suited reward variants.</summary>
    public RoomCharacterAtlas WaitingSamus { get; }
    /// <summary>Post-credits shooting scene OBJ sheet.</summary>
    public RoomCharacterAtlas ShootingScreen { get; }
    /// <summary>Under-three-hour suitless reward sheet.</summary>
    public RoomCharacterAtlas SuitlessSamus { get; }
    /// <summary>BG2 waiting scene's ordered 32x32 tile and palette references.</summary>
    public RoomBackgroundTilemapAtlas WaitingTilemap { get; }
    /// <summary>BG3 transformation characters imported from <c>$99:DA9F</c> and uploaded at VRAM byte $4000; separate from the transformation map.</summary>
    public RoomCharacterAtlas PostCreditsFragmentA { get; }
    /// <summary>BG3 transformation tilemap imported from <c>$99:DAB1</c> and uploaded at VRAM byte $4800; its raw words are preserved through PNG atlas transport rather than interpreted as OBJ characters.</summary>
    public RoomCharacterAtlas PostCreditsFragmentB { get; }
    /// <summary>Four post-shot logo tile chunks uploaded to BG/OBJ VRAM.</summary>
    public RoomCharacterAtlas PostShotLogoTiles { get; }
    /// <summary>Post-shot logo's ordered 32x32 tile and palette references.</summary>
    public RoomBackgroundTilemapAtlas PostShotLogoMap { get; }

    /// <summary>Identity of every selected ending/credits character, map and ordered OAM composition.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(EndingObjectArtworkCatalog), content =>
    {
        content.Append("clouds", Clouds.Transfer.Span);
        content.Append("explosion", Explosion.Transfer.Span);
        content.Append("waiting-samus", WaitingSamus.Transfer.Span);
        content.Append("shooting-screen", ShootingScreen.Transfer.Span);
        content.Append("suitless-samus", SuitlessSamus.Transfer.Span);
        content.Append("waiting-map", WaitingTilemap.Transfer.Span);
        content.Append("post-credits-fragment-a", PostCreditsFragmentA.Transfer.Span);
        content.Append("post-credits-fragment-b", PostCreditsFragmentB.Transfer.Span);
        content.Append("logo-characters", PostShotLogoTiles.Transfer.Span);
        content.Append("logo-map", PostShotLogoMap.Transfer.Span);
        content.Append("fragment-count", fragments.Length);
        foreach (RoomCharacterAtlas fragment in fragments)
            content.Append("fragment", fragment.Transfer.Span);
        content.Append("cloud-sprites", Convert.FromHexString(CloudSprites.ContentIdentity));
        content.Append("explosion-sprites", Convert.FromHexString(ExplosionSprites.ContentIdentity));
        content.Append("completion-text-sprites", Convert.FromHexString(CompletionTextSprites.ContentIdentity));
        content.Append("reward-sprites", Convert.FromHexString(RewardSprites.ContentIdentity));
        content.Append("logo-sprites", Convert.FromHexString(LogoSprites.ContentIdentity));
    });

    /// <summary>Returns one $0800-byte explosion tilemap fragment uploaded within VRAM bytes $E000-$FFFF.</summary>
    /// <param name="id">Native destination-page identity; the enum's numeric values index the copied fragment sequence.</param>
    /// <returns>The retained atlas carrying the selected raw tilemap bytes.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="id"/> is not one of the four defined fragment identities.</exception>
    public RoomCharacterAtlas Fragment(EndingObjectFragmentId id) =>
        (uint)id < fragments.Length ? fragments[(int)id] :
            throw new ArgumentOutOfRangeException(nameof(id));
}

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
