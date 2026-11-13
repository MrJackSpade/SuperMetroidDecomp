using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.AssetExtraction.Areas.Ceres;
using SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>
/// The installed opening cinematic (#1463, #1616, #1636, #1638): the sprite compositions the intro, Ceres destruction and
/// Zebes reveal sheets are composed from (#1467) and named by, beside what the game binds of it (<see cref="Cinematic"/>).
/// Its characters and layouts are import data the installation never holds, and its colours are palette documents only the
/// import reads (<see cref="ImportIntroCinematicArtwork"/>); this read reads none of them and keeps the backdrops the
/// backdrop document names (#1544).
/// </summary>
public class IntroCinematicSourceArtwork
{
    /// <summary>Validates and retains the selected opening artwork.</summary>
    /// <param name="parts">The opening's scene artwork and sprite compositions.</param>
    /// <exception cref="ArgumentNullException">A required presentation is null.</exception>
    public IntroCinematicSourceArtwork(IntroCinematicArtworkParts parts)
    {
        Ensure.NotNull(parts);
        Backdrop = parts.Backdrop;
        CeresFlight = Ensure.NotNull(parts.CeresFlight);
        CeresDestruction = Ensure.NotNull(parts.CeresDestruction);
        Cinematic = new IntroCinematicArtwork(Backdrop, CeresFlight.Flight, CeresDestruction);
        CeresDestructionMaps = Ensure.NotNull(parts.CeresDestructionMaps);
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

    /// <summary>What the game binds of the opening: its backdrop and the Ceres scenes' backdrop and actor placements.</summary>
    public IntroCinematicArtwork Cinematic { get; }

    /// <summary>The colour the opening shows where nothing is drawn, at full brightness: its first background colour (#1512).</summary>
    public RgbColor Backdrop { get; }

    /// <summary>The Ceres approach's maps, compositions and placements.</summary>
    public CeresFlightSourceArtwork CeresFlight { get; }

    /// <summary>The destruction and Zebes-reveal actors' placements.</summary>
    public CeresDestructionArtwork CeresDestruction { get; }

    /// <summary>The destruction's Mode 7 maps, which the Ceres floor's shots are composed from.</summary>
    public CeresDestructionMaps CeresDestructionMaps { get; }
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
    /// Identity of the entire selected opening bundle's installed documents; its characters and layouts are import data the
    /// installation never holds (#1636, #1638), which the sheets composed from them carry the identity of.
    /// </summary>
    public string ContentIdentity => SelectedPresentationHash.Create("IntroCinematicArtworkCatalog-v3", content =>
    {
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
    /// Identity of the selected destruction maps and actor placements: the maps are the import's (#1467), and the
    /// destruction's identity still covers them as one catalog.
    /// </summary>
    private string CeresDestructionIdentity => SelectedPresentationHash.Create("CeresDestructionArtworkCatalog-v3", content =>
    {
        content.Append("ceres-maps", CeresDestructionMaps.Views.Span);
        content.Append("reveal-actors", Convert.FromHexString(CeresDestruction.RevealActors.ContentIdentity));
        content.Append("destruction-actors", Convert.FromHexString(CeresDestruction.DestructionActors.ContentIdentity));
    });
}
