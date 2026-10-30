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
    /// <param name="parts">The opening cinematic's palette and Ceres artwork (#633).</param>
    /// <exception cref="ArgumentNullException">The parts, palette or Ceres artwork are null.</exception>
    internal IntroCinematicArtwork(IntroCinematicSceneParts parts)
    {
        Ensure.NotNull(parts);
        Palette = Ensure.NotNull(parts.Palette);
        CeresFlight = Ensure.NotNull(parts.CeresFlight);
        CeresDestruction = Ensure.NotNull(parts.CeresDestruction);
    }

    /// <summary>Native-precision colors loaded before the first narration card.</summary>
    public IntroCinematicPalette Palette { get; }
    /// <summary>Mode-7 and OBJ visual streams used after the narration fades to Ceres.</summary>
    public CeresFlightArtwork CeresFlight { get; }
    /// <summary>The destruction and Zebes-reveal actors' placements.</summary>
    public CeresDestructionArtwork CeresDestruction { get; }

    /// <summary>Identity of the entire selected opening bundle, including the artwork its sheets are composed from.</summary>
    public abstract string ContentIdentity { get; }
}
