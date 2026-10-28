using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>
/// Installed palette-indexed characters and BG page tile references for the opening
/// cinematic. VRAM destinations and scene timing remain compiled game logic.
/// </summary>
[ArchitectureReview("context-bag", "The opening cinematic's installed artwork; every member is immutable artwork data the cinematic draws, not a service.")]
public sealed class IntroCinematicArtworkCatalog
{
    /// <summary>Assembles the selected opening-art bundle, validating and concatenating its four background pages and retaining edited portrait and narration tilemaps.</summary>
    /// <param name="parts">The opening cinematic's character atlases, tilemaps, sprite presentations and palette (#633).</param>
    /// <exception cref="ArgumentNullException">A required character atlas, presentation catalog, palette, or background-page collection is null.</exception>
    /// <exception cref="InvalidDataException">The background-page count is not four, a page is missing, or a background, portrait, or narration page is not $0800 bytes.</exception>
    public IntroCinematicArtworkCatalog(IntroCinematicArtworkParts parts)
    {
        BackgroundCharacters = Ensure.NotNull(parts.BackgroundCharacters);
        IntroObjectCharacters = Ensure.NotNull(parts.IntroObjectCharacters);
        CinematicObjectCharacters = Ensure.NotNull(parts.CinematicObjectCharacters);
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
        if (!pages.SequenceEqual(IntroBackgroundTilemapDefinitions.Compile()))
            suppliedBackgroundPages = pages;
        ReadOnlyMemory<BackgroundTile> portrait = RequirePage(parts.PortraitTilemap, "portrait");
        if (!portrait.Span.SequenceEqual(IntroPortraitTilemapDefinitions.Compile()))
            suppliedPortrait = portrait.ToArray();
        ReadOnlyMemory<BackgroundTile> narration = RequirePage(parts.InitialNarrationTilemap, "initial narration");
        if (!narration.Span.SequenceEqual(IntroInitialNarrationTilemapDefinitions.Compile()))
            suppliedInitialNarration = narration.ToArray();
        FinalLine = Ensure.NotNull(parts.FinalLine);
        EyeFrames = Ensure.NotNull(parts.EyeFrames);
        CaretSprites = Ensure.NotNull(parts.CaretSprites);
        MotherBrainSprites = Ensure.NotNull(parts.MotherBrainSprites);
        MotherBrainExplosionSprites = Ensure.NotNull(parts.MotherBrainExplosionSprites);
        RinkaSprites = Ensure.NotNull(parts.RinkaSprites);
        EggEffectSprites = Ensure.NotNull(parts.EggEffectSprites);
        DiscoveryActorSprites = Ensure.NotNull(parts.DiscoveryActorSprites);
        ScientistSprites = Ensure.NotNull(parts.ScientistSprites);
        Palette = Ensure.NotNull(parts.Palette);
        CeresFlight = Ensure.NotNull(parts.CeresFlight);
        CeresDestruction = Ensure.NotNull(parts.CeresDestruction);

        static ReadOnlyMemory<BackgroundTile> RequirePage(RoomBackgroundTilemapAtlas? page, string name)
        {
            if (page is null || page.Tiles.Length != PageCells)
                throw new InvalidDataException($"Opening cinematic {name} tilemap must contain one 32x32 page.");
            return page.Tiles;
        }
    }

    /// <summary>Cells in one 32-by-32 opening tilemap page.</summary>
    private const int PageCells = 32 * 32;

    /// <summary>BG characters uploaded to VRAM byte $0000.</summary>
    public RoomCharacterAtlas BackgroundCharacters { get; }
    /// <summary>Fixed-bank intro OBJ characters uploaded to VRAM byte $C000.</summary>
    public RoomCharacterAtlas IntroObjectCharacters { get; }
    /// <summary>Decompressed cinematic OBJ characters uploaded to VRAM byte $DC00.</summary>
    public RoomCharacterAtlas CinematicObjectCharacters { get; }
    /// <summary>The four ordered 32x32 scene pages' tiles.</summary>
    public ReadOnlyMemory<BackgroundTile> BackgroundPages => suppliedBackgroundPages ?? IntroBackgroundTilemapDefinitions.Compile();
    /// <summary>Only selected scene pages differing from the compiled intro layout.</summary>
    internal readonly BackgroundTile[]? suppliedBackgroundPages;
    /// <summary>The Samus-head portrait's tiles.</summary>
    public ReadOnlyMemory<BackgroundTile> PortraitTilemap => suppliedPortrait ?? IntroPortraitTilemapDefinitions.Compile();
    /// <summary>Copied portrait page when authoring differs from the compiled stock map.</summary>
    internal readonly BackgroundTile[]? suppliedPortrait;
    /// <summary>The first, pre-typewriter narration card's tiles.</summary>
    public ReadOnlyMemory<BackgroundTile> InitialNarrationTilemap => suppliedInitialNarration ?? IntroInitialNarrationTilemapDefinitions.Compile();
    /// <summary>Copied pre-typewriter narration page when authoring differs from stock.</summary>
    internal readonly BackgroundTile[]? suppliedInitialNarration;
    /// <summary>Four BG3 rows mapping Japanese subtitle glyph staging beneath illustrated-page text.</summary>
    public IntroFinalLineTilemap FinalLine { get; }
    /// <summary>Four editable Samus-portrait eye rectangles, without their blink timing.</summary>
    public IntroEyeTilemapPresentation EyeFrames { get; }
    /// <summary>The one visible caret OAM frame, without its blink timing or position.</summary>
    public IntroCaretSpritePresentation CaretSprites { get; }
    /// <summary>Three editable intro Mother Brain OAM frames, independent of its program.</summary>
    public IntroMotherBrainSpritePresentation MotherBrainSprites { get; }
    /// <summary>Twelve editable fourth-hit explosion frames, independent of actor timing.</summary>
    public IntroMotherBrainExplosionSpritePresentation MotherBrainExplosionSprites { get; }
    /// <summary>Three editable Rinka frames used by the intro Mother Brain scene.</summary>
    public IntroRinkaSpritePresentation RinkaSprites { get; }
    /// <summary>Eleven editable SR388 egg-shell and slime visual frames.</summary>
    public IntroEggEffectSpritePresentation EggEffectSprites { get; }
    /// <summary>Twenty editable SR388 egg and confused-baby actor frames.</summary>
    public IntroDiscoveryActorSpritePresentation DiscoveryActorSprites { get; }
    /// <summary>Ten editable delivery/examination baby-Metroid frames.</summary>
    public IntroScientistSpritePresentation ScientistSprites { get; }
    /// <summary>Native-precision colors loaded before the first narration card.</summary>
    public IntroCinematicPalette Palette { get; }
    /// <summary>Mode-7 and OBJ visual streams used after the narration fades to Ceres.</summary>
    public CeresFlightArtwork CeresFlight { get; }
    /// <summary>Destruction maps and subsequent Zebes reveal art.</summary>
    public CeresDestructionArtwork CeresDestruction { get; }

    /// <summary>Identity of the entire selected opening bundle, including its owned Ceres flight/reveal art.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(IntroCinematicArtworkCatalog), content =>
    {
        content.Append("background-characters", BackgroundCharacters.Characters.Span);
        content.Append("intro-characters", IntroObjectCharacters.Characters.Span);
        content.Append("cinematic-characters", CinematicObjectCharacters.Characters.Span);
        content.AppendTiles("background-pages", BackgroundPages.Span);
        content.AppendTiles("portrait-map", PortraitTilemap.Span);
        content.AppendTiles("narration-map", InitialNarrationTilemap.Span);
        content.AppendTiles("final-line", FinalLine.Tiles.Span);
        content.Append("eye-frames", Convert.FromHexString(EyeFrames.ContentIdentity));
        content.Append("caret-sprites", Convert.FromHexString(CaretSprites.ContentIdentity));
        content.Append("mother-brain-sprites", Convert.FromHexString(MotherBrainSprites.ContentIdentity));
        content.Append("mother-brain-explosion-sprites", Convert.FromHexString(MotherBrainExplosionSprites.ContentIdentity));
        content.Append("rinka-sprites", Convert.FromHexString(RinkaSprites.ContentIdentity));
        content.Append("egg-sprites", Convert.FromHexString(EggEffectSprites.ContentIdentity));
        content.Append("discovery-sprites", Convert.FromHexString(DiscoveryActorSprites.ContentIdentity));
        content.Append("scientist-sprites", Convert.FromHexString(ScientistSprites.ContentIdentity));
        content.Append("palette", Palette.ColorWords.Span);
        content.Append("ceres-flight", Convert.FromHexString(CeresFlight.ContentIdentity));
        content.Append("ceres-destruction", Convert.FromHexString(CeresDestruction.ContentIdentity));
    });
}
