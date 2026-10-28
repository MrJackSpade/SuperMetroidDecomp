using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>
/// The installed opening cinematic the game binds: its scene, portrait and narration tilemaps, palette and Ceres
/// artwork. Its sprites are drawn from the installed sheets; the characters and compositions those sheets are composed
/// from belong to the import pipeline that reads the installed artwork, which also names the content's identity (#1463).
/// Scene timing remains compiled game logic.
/// </summary>
[ArchitectureReview("context-bag", "The opening cinematic's installed artwork; every member is immutable artwork data the cinematic draws, not a service.")]
public abstract class IntroCinematicArtwork
{
    /// <summary>Assembles the selected opening-art bundle, validating and concatenating its four background pages and retaining edited portrait and narration tilemaps.</summary>
    /// <param name="parts">The opening cinematic's tilemaps, palette and Ceres artwork (#633).</param>
    /// <exception cref="ArgumentNullException">A required tilemap, presentation, palette, or background-page collection is null.</exception>
    /// <exception cref="InvalidDataException">The background-page count is not four, a page is missing, or a background, portrait, or narration page is not $0800 bytes.</exception>
    internal IntroCinematicArtwork(IntroCinematicSceneParts parts)
    {
        Ensure.NotNull(parts);
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
        if (!pages.SequenceEqual(IntroBackgroundPages.Compile()))
            suppliedBackgroundPages = pages;
        ReadOnlyMemory<BackgroundTile> portrait = RequirePage(parts.PortraitTilemap, "portrait");
        if (!portrait.Span.SequenceEqual(IntroPortraitPage.Compile()))
            suppliedPortrait = portrait.ToArray();
        ReadOnlyMemory<BackgroundTile> narration = RequirePage(parts.InitialNarrationTilemap, "initial narration");
        if (!narration.Span.SequenceEqual(IntroInitialNarrationPage.Compile()))
            suppliedInitialNarration = narration.ToArray();
        FinalLine = Ensure.NotNull(parts.FinalLine);
        EyeFrames = Ensure.NotNull(parts.EyeFrames);
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

    /// <summary>The four ordered 32x32 scene pages' tiles.</summary>
    public ReadOnlyMemory<BackgroundTile> BackgroundPages => suppliedBackgroundPages ?? IntroBackgroundPages.Compile();
    /// <summary>Only selected scene pages differing from the compiled intro layout.</summary>
    internal readonly BackgroundTile[]? suppliedBackgroundPages;
    /// <summary>The Samus-head portrait's tiles.</summary>
    public ReadOnlyMemory<BackgroundTile> PortraitTilemap => suppliedPortrait ?? IntroPortraitPage.Compile();
    /// <summary>Copied portrait page when authoring differs from the compiled stock map.</summary>
    internal readonly BackgroundTile[]? suppliedPortrait;
    /// <summary>The first, pre-typewriter narration card's tiles.</summary>
    public ReadOnlyMemory<BackgroundTile> InitialNarrationTilemap => suppliedInitialNarration ?? IntroInitialNarrationPage.Compile();
    /// <summary>Copied pre-typewriter narration page when authoring differs from stock.</summary>
    internal readonly BackgroundTile[]? suppliedInitialNarration;
    /// <summary>Four BG3 rows mapping Japanese subtitle glyph staging beneath illustrated-page text.</summary>
    public IntroFinalLineTilemap FinalLine { get; }
    /// <summary>Four editable Samus-portrait eye rectangles, without their blink timing.</summary>
    public IntroEyeTilemapPresentation EyeFrames { get; }
    /// <summary>Native-precision colors loaded before the first narration card.</summary>
    public IntroCinematicPalette Palette { get; }
    /// <summary>Mode-7 and OBJ visual streams used after the narration fades to Ceres.</summary>
    public CeresFlightArtwork CeresFlight { get; }
    /// <summary>Destruction maps and subsequent Zebes reveal art.</summary>
    public CeresDestructionArtwork CeresDestruction { get; }

    /// <summary>Identity of the entire selected opening bundle, including the artwork its sheets are composed from.</summary>
    public abstract string ContentIdentity { get; }
}
