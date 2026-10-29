using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>
/// The installed ending object artwork as the import pipeline reads it (#1463): beside the tilemaps the game binds, the
/// character atlases and sprite compositions the ending's sheets are composed from.
/// </summary>
public sealed class EndingObjectSourceArtwork : EndingObjectArtwork
{
    /// <summary>Validates and retains the selected ending artwork and compositions.</summary>
    /// <param name="parts">The ending's object character atlases, tilemaps and sprite presentations (#633).</param>
    /// <exception cref="ArgumentNullException">An atlas, composition resource, or fragment sequence is null.</exception>
    /// <exception cref="InvalidDataException">A transfer has an incorrect byte length, or the fragment sequence is malformed.</exception>
    public EndingObjectSourceArtwork(EndingObjectArtworkParts parts) : base(Ensure.NotNull(parts))
    {
        Clouds = Ensure.NotNull(parts.Clouds);
        Explosion = Ensure.NotNull(parts.Explosion);
        WaitingSamus = Ensure.NotNull(parts.WaitingSamus);
        ShootingScreen = Ensure.NotNull(parts.ShootingScreen);
        SuitlessSamus = Ensure.NotNull(parts.SuitlessSamus);
        PostCreditsFragmentA = Ensure.NotNull(parts.PostCreditsFragmentA);
        PostShotLogoTiles = Ensure.NotNull(parts.PostShotLogoTiles);
        if (!HasRequiredLength(Clouds, EndingObjectTransfer.Clouds) ||
            !HasRequiredLength(Explosion, EndingObjectTransfer.Explosion) ||
            !HasRequiredLength(WaitingSamus, EndingObjectTransfer.WaitingSamus) ||
            !HasRequiredLength(ShootingScreen, EndingObjectTransfer.ShootingScreen) ||
            !HasRequiredLength(SuitlessSamus, EndingObjectTransfer.SuitlessSamus) ||
            !HasRequiredLength(PostCreditsFragmentA, EndingObjectTransfer.PostCreditsFragmentA) ||
            !HasRequiredLength(PostShotLogoTiles, EndingObjectTransfer.PostShotLogoTiles))
            throw new InvalidDataException("Ending OBJ catalog has an incorrect native transfer length.");
        CloudSprites = Ensure.NotNull(parts.CloudSprites);
        ExplosionSprites = Ensure.NotNull(parts.ExplosionSprites);
        CompletionTextSprites = Ensure.NotNull(parts.CompletionTextSprites);
        RewardSprites = Ensure.NotNull(parts.RewardSprites);
        LogoSprites = Ensure.NotNull(parts.LogoSprites);
    }

    /// <summary>Checks an atlas transfer against the cartridge's required byte count.</summary>
    private static bool HasRequiredLength(RoomCharacterAtlas atlas, EndingObjectTransfer transfer) =>
        atlas.Characters.Length == EndingObjectArtworkFormat.ByteCount(transfer);

    /// <summary>Compiled 4-bpp atmospheric-cloud OBJ sheet imported from <c>$99:A56F</c> and uploaded at VRAM byte $C000.</summary>
    public RoomCharacterAtlas Clouds { get; }
    /// <summary>Compiled 4-bpp explosion OBJ sheet imported from <c>$98:8304</c> and uploaded at VRAM byte $8000; separate tilemap fragments occupy $E000-$FFFF.</summary>
    public RoomCharacterAtlas Explosion { get; }
    /// <summary>BG2 waiting scene, also reused by both suited reward variants.</summary>
    public RoomCharacterAtlas WaitingSamus { get; }
    /// <summary>Post-credits shooting scene OBJ sheet.</summary>
    public RoomCharacterAtlas ShootingScreen { get; }
    /// <summary>Under-three-hour suitless reward sheet.</summary>
    public RoomCharacterAtlas SuitlessSamus { get; }
    /// <summary>BG3 transformation characters imported from <c>$99:DA9F</c> and uploaded at VRAM byte $4000; separate from the transformation map.</summary>
    public RoomCharacterAtlas PostCreditsFragmentA { get; }
    /// <summary>Four post-shot logo tile chunks uploaded to BG/OBJ VRAM.</summary>
    public RoomCharacterAtlas PostShotLogoTiles { get; }

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

    /// <inheritdoc/>
    public override string ContentIdentity => SelectedPresentationHash.Create("EndingObjectArtworkCatalog", content =>
    {
        content.Append("clouds", Clouds.Characters.Span);
        content.Append("explosion", Explosion.Characters.Span);
        content.Append("waiting-samus", WaitingSamus.Characters.Span);
        content.Append("shooting-screen", ShootingScreen.Characters.Span);
        content.Append("suitless-samus", SuitlessSamus.Characters.Span);
        content.AppendTiles("waiting-map", WaitingTilemap.Tiles.Span);
        content.Append("post-credits-fragment-a", PostCreditsFragmentA.Characters.Span);
        content.AppendTiles("post-credits-fragment-b", PostCreditsFragmentB.Tiles.Span);
        content.Append("logo-characters", PostShotLogoTiles.Characters.Span);
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

    /// <summary>The import-side artwork <paramref name="objects"/> was read with.</summary>
    /// <exception cref="InvalidDataException">The ending artwork was not read from installed artwork.</exception>
    public static EndingObjectSourceArtwork Of(EndingObjectArtwork objects) => objects as EndingObjectSourceArtwork
        ?? throw new InvalidDataException("The ending object artwork was not read from installed artwork.");
}
