using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.AssetExtraction.Areas.Ceres;
using SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>
/// The installed opening cinematic (#1463, #1616, #1636, #1638): what the game binds of it (<see cref="Cinematic"/>) and the
/// destruction's maps the Ceres floor is composed from (#1467). Its characters, layouts and sprite compositions are import
/// data the installation never holds, and its colours are palette documents only the import reads
/// (<see cref="ImportIntroCinematicArtwork"/>); this read reads none of them and keeps the backdrops the backdrop document
/// names (#1544). The sheets' frames are named from the families' poses (<see cref="CinematicSpritePoses"/>).
/// </summary>
public class IntroCinematicSourceArtwork
{
    /// <summary>Validates and retains the selected opening artwork.</summary>
    /// <param name="parts">The opening's scene artwork and the destruction's maps.</param>
    /// <exception cref="ArgumentNullException">A required presentation is null.</exception>
    public IntroCinematicSourceArtwork(IntroCinematicArtworkParts parts)
    {
        Ensure.NotNull(parts);
        Backdrop = parts.Backdrop;
        CeresFlight = Ensure.NotNull(parts.CeresFlight);
        CeresDestruction = Ensure.NotNull(parts.CeresDestruction);
        Cinematic = new IntroCinematicArtwork(Backdrop, CeresFlight.Flight, CeresDestruction);
        CeresDestructionMaps = Ensure.NotNull(parts.CeresDestructionMaps);
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

    /// <summary>
    /// Identity of the entire selected opening bundle's installed documents; its characters, layouts and compositions are
    /// import data the installation never holds (#1636, #1638), which the sheets composed from them carry the identity of.
    /// </summary>
    public virtual string ContentIdentity => SelectedPresentationHash.Create("IntroCinematicArtworkCatalog-v4", content =>
    {
        content.AppendColors("backdrop", [Backdrop]);
        content.Append("ceres-flight", Convert.FromHexString(CeresFlight.ContentIdentity));
        content.Append("ceres-destruction", Convert.FromHexString(CeresDestructionIdentity));
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
