using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.AssetExtraction.Areas.Ceres;

namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>
/// The installed opening cinematic (#1463, #1616, #1636, #1638): what the game binds of it (<see cref="Cinematic"/>). Its
/// characters, layouts, Mode 7 maps (#1693) and sprite compositions are import data the installation never holds, and its
/// colours are palette documents only the import reads (<see cref="ImportIntroCinematicArtwork"/>); this read reads none of
/// them and keeps the backdrops the backdrop document names (#1544). The sheets' frames are named from the families' poses
/// (<see cref="CinematicSpritePoses"/>).
/// </summary>
public class IntroCinematicSourceArtwork
{
    /// <summary>Validates and retains the selected opening artwork.</summary>
    /// <param name="parts">The opening's installed scene artwork.</param>
    /// <exception cref="ArgumentNullException">A required presentation is null.</exception>
    public IntroCinematicSourceArtwork(IntroCinematicSceneParts parts)
    {
        Ensure.NotNull(parts);
        Backdrop = parts.Backdrop;
        CeresFlight = Ensure.NotNull(parts.CeresFlight);
        CeresDestruction = Ensure.NotNull(parts.CeresDestruction);
        Cinematic = new IntroCinematicArtwork(Backdrop, CeresFlight.Flight, CeresDestruction);
    }

    /// <summary>What the game binds of the opening: its backdrop and the Ceres scenes' backdrop and actor placements.</summary>
    public IntroCinematicArtwork Cinematic { get; }

    /// <summary>The colour the opening shows where nothing is drawn, at full brightness: its first background colour (#1512).</summary>
    public RgbColor Backdrop { get; }

    /// <summary>The Ceres approach's backdrop and placements.</summary>
    public CeresFlightSourceArtwork CeresFlight { get; }

    /// <summary>The destruction and Zebes-reveal actors' placements.</summary>
    public CeresDestructionArtwork CeresDestruction { get; }

    /// <summary>
    /// Identity of the entire selected opening bundle's installed documents; its characters, layouts, Mode 7 maps and
    /// compositions are import data the installation never holds (#1636, #1638, #1693), which the sheets composed from them
    /// carry the identity of.
    /// </summary>
    public virtual string ContentIdentity => SelectedPresentationHash.Create("IntroCinematicArtworkCatalog-v5", content =>
    {
        content.AppendColors("backdrop", [Backdrop]);
        content.Append("ceres-flight", Convert.FromHexString(CeresFlight.ContentIdentity));
        content.Append("ceres-destruction", Convert.FromHexString(CeresDestructionIdentity));
    });

    /// <summary>Identity of the selected destruction and Zebes-reveal actor placements.</summary>
    private string CeresDestructionIdentity => SelectedPresentationHash.Create("CeresDestructionArtworkCatalog-v4", content =>
    {
        content.Append("reveal-actors", Convert.FromHexString(CeresDestruction.RevealActors.ContentIdentity));
        content.Append("destruction-actors", Convert.FromHexString(CeresDestruction.DestructionActors.ContentIdentity));
    });
}
