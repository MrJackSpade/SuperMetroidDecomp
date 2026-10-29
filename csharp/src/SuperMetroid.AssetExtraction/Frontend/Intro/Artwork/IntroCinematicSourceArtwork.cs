using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;
using SuperMetroid.AssetExtraction.Video;

namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>
/// The installed opening cinematic as the import pipeline reads it (#1463): beside the scene artwork the game binds, the
/// character atlases and sprite compositions the intro, Ceres destruction and Zebes reveal sheets are composed from.
/// </summary>
public sealed class IntroCinematicSourceArtwork : IntroCinematicArtwork
{
    /// <summary>Validates and retains the selected opening artwork.</summary>
    /// <param name="parts">The opening's scene artwork, character atlases and sprite compositions.</param>
    /// <exception cref="ArgumentNullException">A required atlas, presentation, palette, or page collection is null.</exception>
    /// <exception cref="InvalidDataException">The scene pages, portrait or narration tilemaps are malformed.</exception>
    public IntroCinematicSourceArtwork(IntroCinematicArtworkParts parts) : base(Ensure.NotNull(parts))
    {
        BackgroundCharacters = Ensure.NotNull(parts.BackgroundCharacters);
        IntroObjectCharacters = Ensure.NotNull(parts.IntroObjectCharacters);
        CinematicObjectCharacters = Ensure.NotNull(parts.CinematicObjectCharacters);
        CaretSprites = Ensure.NotNull(parts.CaretSprites);
        MotherBrainSprites = Ensure.NotNull(parts.MotherBrainSprites);
        MotherBrainExplosionSprites = Ensure.NotNull(parts.MotherBrainExplosionSprites);
        RinkaSprites = Ensure.NotNull(parts.RinkaSprites);
        EggEffectSprites = Ensure.NotNull(parts.EggEffectSprites);
        DiscoveryActorSprites = Ensure.NotNull(parts.DiscoveryActorSprites);
        ScientistSprites = Ensure.NotNull(parts.ScientistSprites);
        CeresDestructionSprites = Ensure.NotNull(parts.CeresDestructionSprites);
    }

    /// <summary>BG characters of the illustrated scenes.</summary>
    public RoomCharacterAtlas BackgroundCharacters { get; }
    /// <summary>The intro's fixed-bank OBJ characters.</summary>
    public RoomCharacterAtlas IntroObjectCharacters { get; }
    /// <summary>The intro's cinematic OBJ characters.</summary>
    public RoomCharacterAtlas CinematicObjectCharacters { get; }
    /// <summary>The one visible caret composition, without its blink timing or position.</summary>
    public IntroCaretSpritePresentation CaretSprites { get; }
    /// <summary>Three editable intro Mother Brain compositions, independent of its program.</summary>
    public IntroMotherBrainSpritePresentation MotherBrainSprites { get; }
    /// <summary>Twelve editable fourth-hit explosion compositions, independent of actor timing.</summary>
    public IntroMotherBrainExplosionSpritePresentation MotherBrainExplosionSprites { get; }
    /// <summary>Three editable Rinka compositions used by the intro Mother Brain scene.</summary>
    public IntroRinkaSpritePresentation RinkaSprites { get; }
    /// <summary>Eleven editable SR388 egg-shell and slime compositions.</summary>
    public IntroEggEffectSpritePresentation EggEffectSprites { get; }
    /// <summary>Twenty editable SR388 egg and confused-baby actor compositions.</summary>
    public IntroDiscoveryActorSpritePresentation DiscoveryActorSprites { get; }
    /// <summary>Ten editable delivery/examination baby-Metroid compositions.</summary>
    public IntroScientistSpritePresentation ScientistSprites { get; }
    /// <summary>Station-blast and Zebes-reveal compositions; the approach's are the Ceres flight's.</summary>
    public CeresDestructionSpritePresentation CeresDestructionSprites { get; }

    /// <inheritdoc/>
    public override string ContentIdentity => SelectedPresentationHash.Create("IntroCinematicArtworkCatalog", content =>
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
        content.Append("palette", ScreenColourImage.Words(Palette.Colours));
        content.Append("ceres-flight", Convert.FromHexString(CeresFlight.ContentIdentity));
        content.Append("ceres-destruction", Convert.FromHexString(CeresDestruction.ContentIdentity));
        content.Append("ceres-destruction-sprites", Convert.FromHexString(CeresDestructionSprites.ContentIdentity));
    });

    /// <summary>The import-side artwork <paramref name="intro"/> was read with.</summary>
    /// <exception cref="InvalidDataException">The opening artwork was not read from installed artwork.</exception>
    public static IntroCinematicSourceArtwork Of(IntroCinematicArtwork intro) => intro as IntroCinematicSourceArtwork
        ?? throw new InvalidDataException("The opening cinematic artwork was not read from installed artwork.");
}
