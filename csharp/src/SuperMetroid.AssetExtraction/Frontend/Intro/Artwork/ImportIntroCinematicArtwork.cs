using SuperMetroid.AssetExtraction.Areas.Ceres;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.Rooms.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>
/// The opening cinematic as the import reads it (#1544, #1636, #1638): its installed documents with its characters,
/// layouts and sprite compositions, from the component's import data, and the colours only the import pre-colours the opening's, the Ceres scenes'
/// and the ending flyaway's sheets in. The game reads none of them; its own read of the opening
/// (<see cref="IntroCinematicArtworkFiles.Load"/>) reads no character page or layout and leaves the colour documents unread.
/// </summary>
public sealed class ImportIntroCinematicArtwork : IntroCinematicSourceArtwork
{
    /// <summary>Takes the opening's installed documents, its characters and layouts and the colours only the import reads.</summary>
    /// <param name="parts">The opening's scene artwork and the destruction's maps.</param>
    /// <param name="sprites">The opening's and the Ceres destruction scene's sprite compositions.</param>
    /// <param name="layouts">The opening's scene, portrait, narration, subtitle, eye and Zebes reveal layouts.</param>
    /// <param name="characters">The opening's characters; their Ceres approach takes the documents' place.</param>
    /// <param name="palette">The opening narration's colours.</param>
    /// <param name="flightPalette">The Ceres approach's colours.</param>
    /// <exception cref="ArgumentNullException">A part, a layout, the characters or a palette is null.</exception>
    /// <exception cref="InvalidDataException">The scene pages, portrait, narration or Zebes tilemaps, or the reveal's characters, are malformed.</exception>
    public ImportIntroCinematicArtwork(IntroCinematicArtworkParts parts, IntroCinematicSprites sprites, IntroCinematicLayouts layouts,
        IntroCinematicCharacters characters, IntroCinematicPalette palette, CeresFlightPalette flightPalette)
        : base(Ensure.NotNull(parts) with { CeresFlight = Ensure.NotNull(characters).CeresFlight })
    {
        Sprites = Ensure.NotNull(sprites);
        Ensure.NotNull(layouts);
        Ensure.NotNull(layouts.BackgroundPages);
        if (layouts.BackgroundPages.Count != IntroCinematicArtworkFormat.BackgroundPageCount)
            throw new InvalidDataException("Opening cinematic requires four ordered BG tilemap pages.");
        var pages = new BackgroundTile[PageCells * layouts.BackgroundPages.Count];
        for (int index = 0; index < layouts.BackgroundPages.Count; index++)
        {
            RoomBackgroundTilemapAtlas page = layouts.BackgroundPages[index]
                ?? throw new InvalidDataException($"Opening BG page {index} is missing.");
            if (page.Tiles.Length != PageCells)
                throw new InvalidDataException($"Opening BG page {index} must contain one 32x32 tilemap.");
            page.Tiles.Span.CopyTo(pages.AsSpan(index * PageCells, PageCells));
        }
        backgroundPages = pages;
        portrait = RequirePage(layouts.PortraitTilemap, "portrait").ToArray();
        initialNarration = RequirePage(layouts.InitialNarrationTilemap, "initial narration").ToArray();
        FinalLine = Ensure.NotNull(layouts.FinalLine);
        EyeFrames = Ensure.NotNull(layouts.EyeFrames);
        ZebesRevealMap = Ensure.NotNull(layouts.ZebesRevealMap);
        FlightCharacters = characters.CeresFlight;
        BackgroundCharacters = Ensure.NotNull(characters.BackgroundCharacters);
        IntroObjectCharacters = Ensure.NotNull(characters.IntroObjectCharacters);
        CinematicObjectCharacters = Ensure.NotNull(characters.CinematicObjectCharacters);
        ZebesReveal = ZebesRevealArtwork.Of(ZebesRevealMap, characters.ZebesRevealCharacters);
        Palette = Ensure.NotNull(palette);
        FlightPalette = Ensure.NotNull(flightPalette);
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

    /// <summary>The opening's and the Ceres destruction scene's sprite compositions, which their sheets are composed from.</summary>
    public IntroCinematicSprites Sprites { get; }

    /// <summary>The compositions of <paramref name="family"/>: the Ceres approach's are the flight's, the others the opening's.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The family is not an opening or Ceres family.</exception>
    internal ICinematicSpriteFrames SpritesOf(CinematicSpriteFamily family) => family switch
    {
        CinematicSpriteFamily.IntroDiscoveryActor => Sprites.DiscoveryActor,
        CinematicSpriteFamily.IntroEggEffect => Sprites.EggEffect,
        CinematicSpriteFamily.IntroRinka => Sprites.Rinka,
        CinematicSpriteFamily.IntroScientist => Sprites.Scientist,
        CinematicSpriteFamily.IntroMotherBrain => Sprites.MotherBrain,
        CinematicSpriteFamily.IntroMotherBrainExplosion => Sprites.MotherBrainExplosion,
        CinematicSpriteFamily.IntroCaret => Sprites.Caret,
        CinematicSpriteFamily.IntroNextPageArrow => Sprites.NextPageArrow,
        CinematicSpriteFamily.CeresFlight => FlightCharacters.Sprites,
        CinematicSpriteFamily.CeresDestruction => Sprites.CeresDestruction,
        CinematicSpriteFamily.EndingCloud or CinematicSpriteFamily.EndingExplosion or CinematicSpriteFamily.EndingCompletionText or
            CinematicSpriteFamily.EndingReward or CinematicSpriteFamily.EndingLogo or CinematicSpriteFamily.EndingAnimalEscape =>
            throw new ArgumentOutOfRangeException(nameof(family), family, "The family is not an opening or Ceres cinematic family."),
        _ => throw new ArgumentOutOfRangeException(nameof(family), family, "Undefined cinematic family."),
    };

    /// <summary>Identity of the installed documents and of the compositions the opening's sheets are composed from.</summary>
    public override string ContentIdentity => SelectedPresentationHash.Create("IntroCinematicImportArtwork-v1", content =>
    {
        content.Append("documents", Convert.FromHexString(base.ContentIdentity));
        content.Append("sprites", Convert.FromHexString(Sprites.ContentIdentity));
        content.Append("ceres-flight-sprites", Convert.FromHexString(FlightCharacters.Sprites.ContentIdentity));
    });

    /// <summary>The four ordered 32x32 scene pages' tiles.</summary>
    public ReadOnlyMemory<BackgroundTile> BackgroundPages => backgroundPages;
    /// <summary>The four scene pages, one after another.</summary>
    private readonly BackgroundTile[] backgroundPages;
    /// <summary>The Samus-head portrait's tiles.</summary>
    public ReadOnlyMemory<BackgroundTile> PortraitTilemap => portrait;
    /// <summary>The portrait page.</summary>
    private readonly BackgroundTile[] portrait;
    /// <summary>The first, pre-typewriter narration card's tiles.</summary>
    public ReadOnlyMemory<BackgroundTile> InitialNarrationTilemap => initialNarration;
    /// <summary>The pre-typewriter narration page.</summary>
    private readonly BackgroundTile[] initialNarration;
    /// <summary>Four BG3 rows mapping Japanese subtitle glyph staging beneath illustrated-page text.</summary>
    public IntroFinalLineTilemap FinalLine { get; }
    /// <summary>Four Samus-portrait eye rectangles, without their blink timing.</summary>
    public IntroEyeTilemapPresentation EyeFrames { get; }
    /// <summary>The Zebes reveal's tilemap, which the planet's picture is composed from with the reveal's characters.</summary>
    public RoomBackgroundTilemapAtlas ZebesRevealMap { get; }

    /// <summary>The Ceres approach's documents with its floor and OBJ characters, which the Ceres scenes' pictures are composed from.</summary>
    public CeresFlightCharacters FlightCharacters { get; }

    /// <summary>BG characters of the illustrated scenes.</summary>
    public RoomCharacterAtlas BackgroundCharacters { get; }

    /// <summary>The intro's fixed-bank OBJ characters.</summary>
    public RoomCharacterAtlas IntroObjectCharacters { get; }

    /// <summary>The intro's cinematic OBJ characters.</summary>
    public RoomCharacterAtlas CinematicObjectCharacters { get; }

    /// <summary>The Zebes reveal's tilemap and characters, which the planet's picture is composed from.</summary>
    public ZebesRevealArtwork ZebesReveal { get; }

    /// <summary>The opening narration's selected colours, which the import pre-colours the opening's sheets in (#1512).</summary>
    public IntroCinematicPalette Palette { get; }

    /// <summary>
    /// The Ceres approach's selected 256 colours, separate from the PNGs' transport palettes, which the import pre-colours
    /// the Ceres scenes' pictures and actors in (#1512).
    /// </summary>
    public CeresFlightPalette FlightPalette { get; }
}
