using SuperMetroid.Core;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>
/// The installed ending object artwork, which only the import pipeline reads (#1463, #1616, #1636): the tilemaps and sprite
/// compositions the ending's sheets and pictures are composed from and named by. The game draws those sheets and binds none
/// of this artwork. Its characters are import data the installation never holds (<see cref="ImportEndingObjectArtwork"/>).
/// </summary>
public class EndingObjectSourceArtwork
{
    /// <summary>Validates and retains the selected ending maps and compositions.</summary>
    /// <param name="parts">The ending's object tilemaps and sprite presentations (#633).</param>
    /// <exception cref="ArgumentNullException">An atlas, composition resource, or fragment sequence is null.</exception>
    /// <exception cref="InvalidDataException">A transfer has an incorrect byte length, or the fragment sequence is malformed.</exception>
    public EndingObjectSourceArtwork(EndingObjectArtworkParts parts)
    {
        Ensure.NotNull(parts);
        WaitingTilemap = Ensure.NotNull(parts.WaitingTilemap);
        PostCreditsFragmentB = Ensure.NotNull(parts.PostCreditsFragmentB);
        PostShotLogoMap = Ensure.NotNull(parts.PostShotLogoMap);
        Ensure.NotNull(parts.Fragments);
        if (!IsWholeMap(WaitingTilemap) ||
            !IsWholeMap(PostCreditsFragmentB) ||
            !IsWholeMap(PostShotLogoMap) ||
            parts.Fragments.Count != EndingObjectArtworkFormat.FragmentCount ||
            parts.Fragments.Any(fragment => fragment is null || !IsWholeMap(fragment)))
            throw new InvalidDataException("Ending OBJ catalog has an incorrect native transfer length.");
        fragments = [.. parts.Fragments];
        fragmentsById = Enum.GetValues<EndingObjectFragmentId>().Zip(fragments, (id, fragment) => KeyValuePair.Create(id, fragment)).ToDictionary();
        CloudSprites = Ensure.NotNull(parts.CloudSprites);
        ExplosionSprites = Ensure.NotNull(parts.ExplosionSprites);
        CompletionTextSprites = Ensure.NotNull(parts.CompletionTextSprites);
        RewardSprites = Ensure.NotNull(parts.RewardSprites);
        LogoSprites = Ensure.NotNull(parts.LogoSprites);
    }

    /// <summary>The four explosion maps in <see cref="EndingObjectFragmentId"/> order.</summary>
    private readonly RoomBackgroundTilemapAtlas[] fragments;

    /// <summary>The same maps keyed by the explosion map each one holds.</summary>
    private readonly Dictionary<EndingObjectFragmentId, RoomBackgroundTilemapAtlas> fragmentsById;

    /// <summary>Whether <paramref name="map"/> holds exactly one 32-by-32 page of tiles.</summary>
    private static bool IsWholeMap(RoomBackgroundTilemapAtlas map) => map.Tiles.Length == EndingObjectArtworkFormat.MapCellCount;

    /// <summary>The waiting scene's 32x32 backdrop map.</summary>
    public RoomBackgroundTilemapAtlas WaitingTilemap { get; }
    /// <summary>The transformation's 32x32 map, imported from <c>$99:DAB1</c>.</summary>
    public RoomBackgroundTilemapAtlas PostCreditsFragmentB { get; }
    /// <summary>The post-shot logo's 32x32 map.</summary>
    public RoomBackgroundTilemapAtlas PostShotLogoMap { get; }

    /// <summary>The four explosion maps in <see cref="EndingObjectFragmentId"/> order.</summary>
    public IReadOnlyList<RoomBackgroundTilemapAtlas> Fragments => fragments;

    /// <summary>One 32-by-32-cell explosion map.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="id"/> is not one of the four maps.</exception>
    public RoomBackgroundTilemapAtlas Fragment(EndingObjectFragmentId id) => Ensure.Listed(fragmentsById, id);

    /// <summary>Six editable atmospheric-cloud compositions.</summary>
    public EndingCloudSpritePresentation CloudSprites { get; }
    /// <summary>Sixteen editable Zebes-explosion compositions.</summary>
    public EndingExplosionSpritePresentation ExplosionSprites { get; }
    /// <summary>Fifty-six editable completion-message and clear-time compositions.</summary>
    public EndingCompletionTextSpritePresentation CompletionTextSprites { get; }
    /// <summary>Thirty-seven editable post-credits Samus reward compositions.</summary>
    public EndingRewardSpritePresentation RewardSprites { get; }
    /// <summary>Eight editable compositions for the final assembling logo.</summary>
    public EndingLogoSpritePresentation LogoSprites { get; }

    /// <summary>The compositions of <paramref name="family"/>: the ending's own, or the escaping animals' compiled pods.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The family is not an ending family.</exception>
    internal ICinematicSpriteFrames SpritesOf(CinematicSpriteFamily family) => family switch
    {
        CinematicSpriteFamily.EndingCloud => CloudSprites,
        CinematicSpriteFamily.EndingExplosion => ExplosionSprites,
        CinematicSpriteFamily.EndingCompletionText => CompletionTextSprites,
        CinematicSpriteFamily.EndingReward => RewardSprites,
        CinematicSpriteFamily.EndingLogo => LogoSprites,
        CinematicSpriteFamily.EndingAnimalEscape => EndingAnimalEscapeParts.Sprites,
        CinematicSpriteFamily.IntroDiscoveryActor or CinematicSpriteFamily.IntroEggEffect or CinematicSpriteFamily.IntroRinka or
            CinematicSpriteFamily.IntroScientist or CinematicSpriteFamily.IntroMotherBrain or CinematicSpriteFamily.IntroMotherBrainExplosion or
            CinematicSpriteFamily.IntroCaret or CinematicSpriteFamily.IntroNextPageArrow or CinematicSpriteFamily.CeresFlight or
            CinematicSpriteFamily.CeresDestruction =>
            throw new ArgumentOutOfRangeException(nameof(family), family, "The family is not an ending family."),
        _ => throw new ArgumentOutOfRangeException(nameof(family), family, "Undefined cinematic family."),
    };

    /// <summary>
    /// Identity of every selected ending/credits map and the compositions its sheets are composed from; the characters are
    /// import data the installation never holds (#1636).
    /// </summary>
    public string ContentIdentity => SelectedPresentationHash.Create("EndingObjectArtworkCatalog-v2", content =>
    {
        content.AppendTiles("waiting-map", WaitingTilemap.Tiles.Span);
        content.AppendTiles("post-credits-fragment-b", PostCreditsFragmentB.Tiles.Span);
        content.AppendTiles("logo-map", PostShotLogoMap.Tiles.Span);
        content.Append("fragment-count", Fragments.Count);
        foreach (RoomBackgroundTilemapAtlas fragment in Fragments)
            content.AppendTiles("fragment", fragment.Tiles.Span);
        content.Append("cloud-sprites", Convert.FromHexString(CloudSprites.ContentIdentity));
        content.Append("explosion-sprites", Convert.FromHexString(ExplosionSprites.ContentIdentity));
        content.Append("completion-text-sprites", Convert.FromHexString(CompletionTextSprites.ContentIdentity));
        content.Append("reward-sprites", Convert.FromHexString(RewardSprites.ContentIdentity));
        content.Append("logo-sprites", Convert.FromHexString(LogoSprites.ContentIdentity));
    });
}
