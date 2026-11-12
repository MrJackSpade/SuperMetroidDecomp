using SuperMetroid.Core;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>
/// The installed opening cinematic (#1463, #1636): beside the scene artwork the game binds, the tilemaps and sprite
/// compositions the intro, Ceres destruction and Zebes reveal sheets are composed from (#1467) and named by. Its characters
/// are import data the installation never holds, and its colours are palette documents only the import reads
/// (<see cref="ImportIntroCinematicArtwork"/>); this read reads neither and keeps the backdrops the backdrop document names
/// (#1544).
/// </summary>
public class IntroCinematicSourceArtwork : IntroCinematicArtwork
{
    /// <summary>Validates and retains the selected opening artwork.</summary>
    /// <param name="parts">The opening's scene artwork, character atlases and sprite compositions.</param>
    /// <exception cref="ArgumentNullException">A required atlas, presentation, or page collection is null.</exception>
    /// <exception cref="InvalidDataException">The scene pages, portrait or narration tilemaps are malformed.</exception>
    public IntroCinematicSourceArtwork(IntroCinematicArtworkParts parts)
        : base(Ensure.NotNull(parts).Backdrop, parts.CeresFlight, parts.CeresDestruction)
    {
        Ensure.NotNull(parts.BackgroundPages);
        if (parts.BackgroundPages.Count != IntroCinematicArtworkFormat.BackgroundPageCount)
            throw new InvalidDataException("Opening cinematic requires four ordered BG tilemap pages.");
        var pages = new BackgroundTile[PageCells * parts.BackgroundPages.Count];
        for (int index = 0; index < parts.BackgroundPages.Count; index++)
        {
            RoomBackgroundTilemapAtlas page = parts.BackgroundPages[index]
                ?? throw new InvalidDataException($"Opening BG page {index} is missing.");
            if (page.Tiles.Length != PageCells)
                throw new InvalidDataException($"Opening BG page {index} must contain one 32x32 tilemap.");
            page.Tiles.Span.CopyTo(pages.AsSpan(index * PageCells, PageCells));
        }
        backgroundPages = pages;
        portrait = RequirePage(parts.PortraitTilemap, "portrait").ToArray();
        initialNarration = RequirePage(parts.InitialNarrationTilemap, "initial narration").ToArray();
        FinalLine = Ensure.NotNull(parts.FinalLine);
        EyeFrames = Ensure.NotNull(parts.EyeFrames);
        CeresDestructionMaps = Ensure.NotNull(parts.CeresDestructionMaps);
        ZebesRevealMap = Ensure.NotNull(parts.ZebesRevealMap);
        CaretSprites = Ensure.NotNull(parts.CaretSprites);
        NextPageArrowSprites = Ensure.NotNull(parts.NextPageArrowSprites);
        MotherBrainSprites = Ensure.NotNull(parts.MotherBrainSprites);
        MotherBrainExplosionSprites = Ensure.NotNull(parts.MotherBrainExplosionSprites);
        RinkaSprites = Ensure.NotNull(parts.RinkaSprites);
        EggEffectSprites = Ensure.NotNull(parts.EggEffectSprites);
        DiscoveryActorSprites = Ensure.NotNull(parts.DiscoveryActorSprites);
        ScientistSprites = Ensure.NotNull(parts.ScientistSprites);
        CeresDestructionSprites = Ensure.NotNull(parts.CeresDestructionSprites);
    }

    /// <summary>Requires a tilemap page of one 32x32 page's cells.</summary>
    /// <exception cref="InvalidDataException">The page is missing or is not one 32x32 page.</exception>
    private static ReadOnlyMemory<BackgroundTile> RequirePage(RoomBackgroundTilemapAtlas? page, string name)
    {
        if (page is null || page.Tiles.Length != PageCells)
            throw new InvalidDataException($"Opening cinematic {name} tilemap must contain one 32x32 page.");
        return page.Tiles;
    }

    /// <summary>Cells in one 32-by-32 opening tilemap page.</summary>
    private const int PageCells = 32 * 32;

    /// <summary>The four ordered 32x32 scene pages' installed tiles.</summary>
    public ReadOnlyMemory<BackgroundTile> BackgroundPages => backgroundPages;
    /// <summary>The four installed scene pages, one after another.</summary>
    private readonly BackgroundTile[] backgroundPages;
    /// <summary>The Samus-head portrait's installed tiles.</summary>
    public ReadOnlyMemory<BackgroundTile> PortraitTilemap => portrait;
    /// <summary>The installed portrait page.</summary>
    private readonly BackgroundTile[] portrait;
    /// <summary>The first, pre-typewriter narration card's installed tiles.</summary>
    public ReadOnlyMemory<BackgroundTile> InitialNarrationTilemap => initialNarration;
    /// <summary>The installed pre-typewriter narration page.</summary>
    private readonly BackgroundTile[] initialNarration;
    /// <summary>Four BG3 rows mapping Japanese subtitle glyph staging beneath illustrated-page text.</summary>
    public IntroFinalLineTilemap FinalLine { get; }
    /// <summary>Four editable Samus-portrait eye rectangles, without their blink timing.</summary>
    public IntroEyeTilemapPresentation EyeFrames { get; }
    /// <summary>The destruction's Mode 7 maps, which the Ceres floor's shots are composed from.</summary>
    public CeresDestructionMaps CeresDestructionMaps { get; }
    /// <summary>The Zebes reveal's tilemap, which the planet's picture is composed from with the reveal's characters.</summary>
    public RoomBackgroundTilemapAtlas ZebesRevealMap { get; }
    /// <summary>The one visible caret composition, without its blink timing or position.</summary>
    public IntroCaretSpritePresentation CaretSprites { get; }
    /// <summary>The Japanese text's three next-page arrow compositions, without their cadence or position (#1655).</summary>
    public IntroNextPageArrowSpritePresentation NextPageArrowSprites { get; }
    /// <summary>Three editable intro Mother Brain compositions, independent of its program.</summary>
    public IntroMotherBrainSpritePresentation MotherBrainSprites { get; }
    /// <summary>Twelve editable fourth-hit explosion compositions, independent of actor timing.</summary>
    public IntroMotherBrainExplosionSpritePresentation MotherBrainExplosionSprites { get; }
    /// <summary>Three editable Rinka compositions used by the intro Mother Brain scene.</summary>
    public IntroRinkaSpritePresentation RinkaSprites { get; }
    /// <summary>Eleven editable SR388 egg-shell and slime compositions.</summary>
    public IntroEggEffectSpritePresentation EggEffectSprites { get; }
    /// <summary>Nineteen editable SR388 egg and confused-baby actor compositions.</summary>
    public IntroDiscoveryActorSpritePresentation DiscoveryActorSprites { get; }
    /// <summary>Six editable delivery/examination baby-Metroid compositions.</summary>
    public IntroScientistSpritePresentation ScientistSprites { get; }
    /// <summary>Station-blast and Zebes-reveal compositions; the approach's are the Ceres flight's.</summary>
    public CeresDestructionSpritePresentation CeresDestructionSprites { get; }

    /// <summary>
    /// Identity of the entire selected opening bundle's installed documents; its characters are import data the
    /// installation never holds (#1636), which the sheets composed from them carry the identity of.
    /// </summary>
    public string ContentIdentity => SelectedPresentationHash.Create("IntroCinematicArtworkCatalog-v2", content =>
    {
        content.AppendTiles("background-pages", BackgroundPages.Span);
        content.AppendTiles("portrait-map", PortraitTilemap.Span);
        content.AppendTiles("narration-map", InitialNarrationTilemap.Span);
        content.AppendTiles("final-line", FinalLine.Tiles.Span);
        content.Append("eye-frames", Convert.FromHexString(EyeFrames.ContentIdentity));
        content.Append("caret-sprites", Convert.FromHexString(CaretSprites.ContentIdentity));
        content.Append("next-page-arrow-sprites", Convert.FromHexString(NextPageArrowSprites.ContentIdentity));
        content.Append("mother-brain-sprites", Convert.FromHexString(MotherBrainSprites.ContentIdentity));
        content.Append("mother-brain-explosion-sprites", Convert.FromHexString(MotherBrainExplosionSprites.ContentIdentity));
        content.Append("rinka-sprites", Convert.FromHexString(RinkaSprites.ContentIdentity));
        content.Append("egg-sprites", Convert.FromHexString(EggEffectSprites.ContentIdentity));
        content.Append("discovery-sprites", Convert.FromHexString(DiscoveryActorSprites.ContentIdentity));
        content.Append("scientist-sprites", Convert.FromHexString(ScientistSprites.ContentIdentity));
        content.AppendColors("backdrop", [Backdrop]);
        content.Append("ceres-flight", Convert.FromHexString(CeresFlight.ContentIdentity));
        content.Append("ceres-destruction", Convert.FromHexString(CeresDestructionIdentity));
        content.Append("ceres-destruction-sprites", Convert.FromHexString(CeresDestructionSprites.ContentIdentity));
    });

    /// <summary>
    /// Identity of the selected destruction maps, Zebes reveal tilemap and actor placements: the maps and the Zebes reveal's
    /// tilemap are the import's (#1467), and the destruction's identity still covers them as one catalog.
    /// </summary>
    private string CeresDestructionIdentity => SelectedPresentationHash.Create("CeresDestructionArtworkCatalog-v2", content =>
    {
        content.Append("ceres-maps", CeresDestructionMaps.Views.Span);
        content.AppendTiles("zebes-map", ZebesRevealMap.Tiles.Span);
        content.Append("reveal-actors", Convert.FromHexString(CeresDestruction.RevealActors.ContentIdentity));
        content.Append("destruction-actors", Convert.FromHexString(CeresDestruction.DestructionActors.ContentIdentity));
    });

    /// <summary>The import-side artwork <paramref name="intro"/> was read with.</summary>
    /// <exception cref="InvalidDataException">The opening artwork was not read from installed artwork.</exception>
    public static IntroCinematicSourceArtwork Of(IntroCinematicArtwork intro) => intro as IntroCinematicSourceArtwork
        ?? throw new InvalidDataException("The opening cinematic artwork was not read from installed artwork.");
}
