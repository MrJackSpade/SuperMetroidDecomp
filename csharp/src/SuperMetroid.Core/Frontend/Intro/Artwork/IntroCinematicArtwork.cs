using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>
/// The installed opening cinematic the game binds: its palette and Ceres artwork. Its scenes, portrait, narration cards
/// and sprites are drawn from the installed sheets; the tilemaps, characters and compositions those sheets are composed
/// from belong to the import pipeline that reads the installed artwork, which also names the content's identity (#1463,
/// #1467). Scene timing remains compiled game logic.
/// </summary>
public abstract class IntroCinematicArtwork
{
    /// <summary>Retains the selected opening palette and Ceres artwork.</summary>
    /// <param name="palette">Full-precision colors loaded before the first narration card.</param>
    /// <param name="ceresFlight">Mode-7 and OBJ visual streams used after the narration fades to Ceres.</param>
    /// <param name="ceresDestruction">The destruction and Zebes-reveal actors' placements.</param>
    /// <exception cref="ArgumentNullException">The palette or Ceres artwork are null.</exception>
    internal IntroCinematicArtwork(IntroCinematicPalette palette, CeresFlightArtwork ceresFlight, CeresDestructionArtwork ceresDestruction)
    {
        Palette = Ensure.NotNull(palette);
        CeresFlight = Ensure.NotNull(ceresFlight);
        CeresDestruction = Ensure.NotNull(ceresDestruction);
    }

    /// <summary>Full-precision colors loaded before the first narration card.</summary>
    public IntroCinematicPalette Palette { get; }
    /// <summary>Mode-7 and OBJ visual streams used after the narration fades to Ceres.</summary>
    public CeresFlightArtwork CeresFlight { get; }
    /// <summary>The destruction and Zebes-reveal actors' placements.</summary>
    public CeresDestructionArtwork CeresDestruction { get; }
}
