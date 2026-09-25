namespace SuperMetroid.Core.Assets;

/// <summary>
/// Installed palette-indexed characters and BG page tile references for the opening
/// cinematic. VRAM destinations and scene timing remain compiled game logic.
/// </summary>
public sealed class IntroCinematicArtworkCatalog
{
    public IntroCinematicArtworkCatalog(RoomCharacterAtlas backgroundCharacters,
        RoomCharacterAtlas introObjectCharacters, RoomCharacterAtlas cinematicObjectCharacters,
        IReadOnlyList<RoomBackgroundTilemapAtlas> backgroundPages,
        RoomBackgroundTilemapAtlas portraitTilemap,
        RoomBackgroundTilemapAtlas initialNarrationTilemap,
        IntroFinalLineTilemap finalLine,
        IntroEyeTilemapPresentation eyeFrames,
        IntroCaretSpritePresentation caretSprites,
        IntroCinematicPalette palette,
        CeresFlightArtworkCatalog ceresFlight,
        CeresDestructionArtworkCatalog ceresDestruction)
    {
        BackgroundCharacters = backgroundCharacters ?? throw new ArgumentNullException(nameof(backgroundCharacters));
        IntroObjectCharacters = introObjectCharacters ?? throw new ArgumentNullException(nameof(introObjectCharacters));
        CinematicObjectCharacters = cinematicObjectCharacters ?? throw new ArgumentNullException(nameof(cinematicObjectCharacters));
        ArgumentNullException.ThrowIfNull(backgroundPages);
        if (backgroundPages.Count != IntroCinematicArtworkFormat.BackgroundPageCount)
            throw new InvalidDataException("Opening cinematic requires four ordered BG tilemap pages.");
        var pages = new byte[IntroCinematicArtworkFormat.BackgroundPageByteCount * backgroundPages.Count];
        for (int index = 0; index < backgroundPages.Count; index++)
        {
            RoomBackgroundTilemapAtlas page = backgroundPages[index]
                ?? throw new InvalidDataException($"Opening BG page {index} is missing.");
            if (page.Transfer.Length != IntroCinematicArtworkFormat.BackgroundPageByteCount)
                throw new InvalidDataException($"Opening BG page {index} must contain one 32x32 tilemap.");
            page.Transfer.Span.CopyTo(pages.AsSpan(index * IntroCinematicArtworkFormat.BackgroundPageByteCount,
                IntroCinematicArtworkFormat.BackgroundPageByteCount));
        }
        BackgroundPages = pages;
        PortraitTilemap = RequirePage(portraitTilemap, "portrait");
        InitialNarrationTilemap = RequirePage(initialNarrationTilemap, "initial narration");
        FinalLine = finalLine ?? throw new ArgumentNullException(nameof(finalLine));
        EyeFrames = eyeFrames ?? throw new ArgumentNullException(nameof(eyeFrames));
        CaretSprites = caretSprites ?? throw new ArgumentNullException(nameof(caretSprites));
        Palette = palette ?? throw new ArgumentNullException(nameof(palette));
        CeresFlight = ceresFlight ?? throw new ArgumentNullException(nameof(ceresFlight));
        CeresDestruction = ceresDestruction ?? throw new ArgumentNullException(nameof(ceresDestruction));

        static ReadOnlyMemory<byte> RequirePage(RoomBackgroundTilemapAtlas? page, string name)
        {
            if (page is null || page.Transfer.Length != IntroCinematicArtworkFormat.BackgroundPageByteCount)
                throw new InvalidDataException($"Opening cinematic {name} tilemap must contain one 32x32 page.");
            return page.Transfer;
        }
    }

    /// <summary>BG characters uploaded to VRAM byte $0000.</summary>
    public RoomCharacterAtlas BackgroundCharacters { get; }
    /// <summary>Fixed-bank intro OBJ characters uploaded to VRAM byte $C000.</summary>
    public RoomCharacterAtlas IntroObjectCharacters { get; }
    /// <summary>Decompressed cinematic OBJ characters uploaded to VRAM byte $DC00.</summary>
    public RoomCharacterAtlas CinematicObjectCharacters { get; }
    /// <summary>Four ordered 32x32 BG tilemap pages uploaded at VRAM byte $A000.</summary>
    public ReadOnlyMemory<byte> BackgroundPages { get; }
    /// <summary>Samus-head portrait BG tilemap uploaded at VRAM byte $9000.</summary>
    public ReadOnlyMemory<byte> PortraitTilemap { get; }
    /// <summary>First, pre-typewriter narration BG3 tilemap uploaded at VRAM byte $9800.</summary>
    public ReadOnlyMemory<byte> InitialNarrationTilemap { get; }
    /// <summary>Four ornamental BG3 rows placed beneath illustrated-page text.</summary>
    public IntroFinalLineTilemap FinalLine { get; }
    /// <summary>Four editable Samus-portrait eye rectangles, without their blink timing.</summary>
    public IntroEyeTilemapPresentation EyeFrames { get; }
    /// <summary>The one visible caret OAM frame, without its blink timing or position.</summary>
    public IntroCaretSpritePresentation CaretSprites { get; }
    /// <summary>Native-precision colors loaded before the first narration card.</summary>
    public IntroCinematicPalette Palette { get; }
    /// <summary>Mode-7 and OBJ visual streams used after the narration fades to Ceres.</summary>
    public CeresFlightArtworkCatalog CeresFlight { get; }
    /// <summary>Destruction maps and subsequent Zebes reveal art.</summary>
    public CeresDestructionArtworkCatalog CeresDestruction { get; }
}

/// <summary>File identities and physical 4-bpp transfer lengths for the opening scene.</summary>
public static class IntroCinematicArtworkFormat
{
    public const string ManifestFileName = "intro-artwork.json";
    public const string BackgroundFileName = "intro-background-characters.png";
    public const string IntroObjectFileName = "intro-object-characters.png";
    public const string CinematicObjectFileName = "intro-cinematic-object-characters.png";
    public const string PortraitTilemapFileName = "intro-portrait-tilemap.json";
    public const string InitialNarrationTilemapFileName = "intro-initial-narration-tilemap.json";
    public const int BackgroundPageCount = 4;
    public const int BackgroundPageByteCount = 0x0800;
    public static string BackgroundPageFileName(int index) => $"intro-background-page-{index}.json";
    public const int BackgroundByteCount = 0x8000;
    public const int IntroObjectByteCount = 0x2000;
    public const int CinematicObjectByteCount = 0x2400;
    public const int TileColumns = 32;
}
