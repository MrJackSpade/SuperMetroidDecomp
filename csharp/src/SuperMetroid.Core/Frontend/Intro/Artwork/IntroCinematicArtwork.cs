using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>
/// The installed opening cinematic the game binds: its backdrop and Ceres artwork. Its scenes, portrait, narration cards
/// and sprites are drawn from the installed sheets, which the import pre-colours in the opening's palette (#1512); the
/// palette, tilemaps, characters and compositions those sheets are composed from belong to the import pipeline that reads
/// the installed artwork, which also names the content's identity (#1463, #1467). Scene timing remains compiled game logic.
/// </summary>
public abstract class IntroCinematicArtwork
{
    /// <summary>Retains the selected opening backdrop and Ceres artwork.</summary>
    /// <param name="backdrop">The colour the opening shows where nothing is drawn, at full brightness: its first background colour.</param>
    /// <param name="ceresFlight">Turning-floor and sprite visual streams used after the narration fades to Ceres.</param>
    /// <param name="ceresDestruction">The destruction and Zebes-reveal actors' placements.</param>
    /// <exception cref="ArgumentNullException">The Ceres artwork is null.</exception>
    internal IntroCinematicArtwork(RgbColor backdrop, CeresFlightArtwork ceresFlight, CeresDestructionArtwork ceresDestruction)
    {
        Backdrop = backdrop;
        CeresFlight = Ensure.NotNull(ceresFlight);
        CeresDestruction = Ensure.NotNull(ceresDestruction);
    }

    /// <summary>The colour the opening shows where nothing is drawn, at full brightness: its first background colour (#1512).</summary>
    public RgbColor Backdrop { get; }
    /// <summary>Turning-floor and sprite visual streams used after the narration fades to Ceres.</summary>
    public CeresFlightArtwork CeresFlight { get; }
    /// <summary>The destruction and Zebes-reveal actors' placements.</summary>
    public CeresDestructionArtwork CeresDestruction { get; }
}
