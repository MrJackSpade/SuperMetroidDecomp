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
        var pages = new byte[IntroCinematicArtworkFormat.BackgroundPageByteCount * parts.BackgroundPages.Count];
        for (int index = 0; index < parts.BackgroundPages.Count; index++)
        {
            RoomBackgroundTilemapAtlas page = parts.BackgroundPages[index]
                ?? throw new InvalidDataException($"Opening BG page {index} is missing.");
            if (page.Bytes.Length != IntroCinematicArtworkFormat.BackgroundPageByteCount)
                throw new InvalidDataException($"Opening BG page {index} must contain one 32x32 tilemap.");
            page.Bytes.Span.CopyTo(pages.AsSpan(index * IntroCinematicArtworkFormat.BackgroundPageByteCount,
                IntroCinematicArtworkFormat.BackgroundPageByteCount));
        }
        if (!pages.SequenceEqual(IntroBackgroundTilemapDefinitions.Compile()))
            suppliedBackgroundPages = pages;
        ReadOnlyMemory<byte> portrait = RequirePage(parts.PortraitTilemap, "portrait");
        if (!portrait.Span.SequenceEqual(IntroPortraitTilemapDefinitions.Compile()))
            suppliedPortrait = portrait.ToArray();
        ReadOnlyMemory<byte> narration = RequirePage(parts.InitialNarrationTilemap, "initial narration");
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

        static ReadOnlyMemory<byte> RequirePage(RoomBackgroundTilemapAtlas? page, string name)
        {
            if (page is null || page.Bytes.Length != IntroCinematicArtworkFormat.BackgroundPageByteCount)
                throw new InvalidDataException($"Opening cinematic {name} tilemap must contain one 32x32 page.");
            return page.Bytes;
        }
    }

    /// <summary>BG characters uploaded to VRAM byte $0000.</summary>
    public RoomCharacterAtlas BackgroundCharacters { get; }
    /// <summary>Fixed-bank intro OBJ characters uploaded to VRAM byte $C000.</summary>
    public RoomCharacterAtlas IntroObjectCharacters { get; }
    /// <summary>Decompressed cinematic OBJ characters uploaded to VRAM byte $DC00.</summary>
    public RoomCharacterAtlas CinematicObjectCharacters { get; }
    /// <summary>Four ordered 32x32 BG tilemap pages uploaded at VRAM byte $A000.</summary>
    public ReadOnlyMemory<byte> BackgroundPages => suppliedBackgroundPages ?? IntroBackgroundTilemapDefinitions.Compile();
    /// <summary>Only a selected four-page BG transfer differing from the compiled intro layout.</summary>
    internal readonly byte[]? suppliedBackgroundPages;
    /// <summary>Samus-head portrait BG tilemap uploaded at VRAM byte $9000.</summary>
    public ReadOnlyMemory<byte> PortraitTilemap => suppliedPortrait ?? IntroPortraitTilemapDefinitions.Compile();
    /// <summary>Copied portrait page when authoring differs from the compiled stock map.</summary>
    internal readonly byte[]? suppliedPortrait;
    /// <summary>First, pre-typewriter narration BG3 tilemap uploaded at VRAM byte $9800.</summary>
    public ReadOnlyMemory<byte> InitialNarrationTilemap => suppliedInitialNarration ?? IntroInitialNarrationTilemapDefinitions.Compile();
    /// <summary>Copied pre-typewriter narration page when authoring differs from stock.</summary>
    internal readonly byte[]? suppliedInitialNarration;
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
    public CeresFlightArtworkCatalog CeresFlight { get; }
    /// <summary>Destruction maps and subsequent Zebes reveal art.</summary>
    public CeresDestructionArtworkCatalog CeresDestruction { get; }

    /// <summary>The intro OBJ page: the intro sheet with the cinematic sheet uploaded over its tail.</summary>
    public SpriteArtwork ObjectPage
    {
        get
        {
            if (field is null)
            {
                var id = new SpriteArtworkId(SpriteArtworkKind.IntroObjects);
                field = ObjCharacterPage.With(id, ObjCharacterPage.Create(id, IntroObjectCharacters.Characters.Span),
                    CinematicObjectCharacters.Characters.Span, SuperMetroid.Core.Samus.Artwork.ProjectileObjectPageDefinitions.IntroCinematicObjectsFirstCharacter);
            }
            return field;
        }
    }

    /// <summary>Identity of the entire selected opening bundle, including its owned Ceres flight/reveal art.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(IntroCinematicArtworkCatalog), content =>
    {
        content.Append("background-characters", BackgroundCharacters.Characters.Span);
        content.Append("intro-characters", IntroObjectCharacters.Characters.Span);
        content.Append("cinematic-characters", CinematicObjectCharacters.Characters.Span);
        content.Append("background-pages", BackgroundPages.Span);
        content.Append("portrait-map", PortraitTilemap.Span);
        content.Append("narration-map", InitialNarrationTilemap.Span);
        content.AppendWords("final-line", FinalLine.Words.Span);
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
