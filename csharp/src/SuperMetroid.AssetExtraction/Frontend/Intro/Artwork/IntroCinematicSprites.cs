using SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>
/// The opening's and the Ceres destruction scene's sprite compositions (#1638), import data whose parts are the original's
/// packed sprite placements: only the import composes the intro, Ceres destruction and Zebes reveal sheets from them, and
/// the installation holds the pictures. The sheets' frames are named from the families' poses (<see cref="CinematicSpritePoses"/>).
/// The Ceres approach's compositions are the flight's import artwork's (<see cref="Areas.Ceres.CeresFlightCharacters.Sprites"/>).
/// </summary>
/// <param name="Caret">The one visible caret composition, without its blink timing or position.</param>
/// <param name="NextPageArrow">The Japanese text's three next-page arrow compositions, without their cadence or position (#1655).</param>
/// <param name="MotherBrain">Three intro Mother Brain compositions, independent of its program.</param>
/// <param name="MotherBrainExplosion">Twelve fourth-hit explosion compositions, independent of actor timing.</param>
/// <param name="Rinka">Three Rinka compositions used by the intro Mother Brain scene.</param>
/// <param name="EggEffect">Eleven SR388 egg-shell and slime compositions.</param>
/// <param name="DiscoveryActor">Nineteen SR388 egg and confused-baby actor compositions.</param>
/// <param name="Scientist">Six delivery/examination baby-Metroid compositions.</param>
/// <param name="CeresDestruction">Station-blast and Zebes-reveal compositions; the approach's are the Ceres flight's.</param>
[ArchitectureReview("context-bag", "The opening's decoded sprite composition documents the import composes its sheets from; every member is immutable artwork data, not a service.")]
public sealed record IntroCinematicSprites(IntroCaretSpritePresentation Caret, IntroNextPageArrowSpritePresentation NextPageArrow,
    IntroMotherBrainSpritePresentation MotherBrain, IntroMotherBrainExplosionSpritePresentation MotherBrainExplosion,
    IntroRinkaSpritePresentation Rinka, IntroEggEffectSpritePresentation EggEffect, IntroDiscoveryActorSpritePresentation DiscoveryActor,
    IntroScientistSpritePresentation Scientist, CeresDestructionSpritePresentation CeresDestruction)
{
    /// <summary>Identity of every composition, preserving each family's ordered parts.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("IntroCinematicSprites-v1", content =>
    {
        content.Append("caret-sprites", Convert.FromHexString(Caret.ContentIdentity));
        content.Append("next-page-arrow-sprites", Convert.FromHexString(NextPageArrow.ContentIdentity));
        content.Append("mother-brain-sprites", Convert.FromHexString(MotherBrain.ContentIdentity));
        content.Append("mother-brain-explosion-sprites", Convert.FromHexString(MotherBrainExplosion.ContentIdentity));
        content.Append("rinka-sprites", Convert.FromHexString(Rinka.ContentIdentity));
        content.Append("egg-sprites", Convert.FromHexString(EggEffect.ContentIdentity));
        content.Append("discovery-sprites", Convert.FromHexString(DiscoveryActor.ContentIdentity));
        content.Append("scientist-sprites", Convert.FromHexString(Scientist.ContentIdentity));
        content.Append("ceres-destruction-sprites", Convert.FromHexString(CeresDestruction.ContentIdentity));
    });
}
