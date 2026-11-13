namespace SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

/// <summary>
/// Editable actor placements unique to the destruction of Ceres and the following Zebes reveal. The Ceres characters,
/// the flight's map slices, the destruction's maps and the Zebes reveal's layout and characters are the import's, which
/// pictures the floor and the planet from them (#1467, #1616).
/// </summary>
public sealed class CeresDestructionArtwork
{
    /// <summary>Retains the actor placements the import validated.</summary>
    internal CeresDestructionArtwork(CeresRevealActorPlacements revealActors, CeresDestructionActorPlacements destructionActors)
    {
        RevealActors = revealActors;
        DestructionActors = destructionActors;
    }

    /// <summary>Initial screen-pixel placements of the six Zebes-reveal actors; animations, slide acceleration, deletion, and phase transitions remain compiled cinematic behavior.</summary>
    public CeresRevealActorPlacements RevealActors { get; }
    /// <summary>Initial screen-pixel placements of the three persistent destruction-background actors; movement and wrap behavior are not editable through this layout.</summary>
    public CeresDestructionActorPlacements DestructionActors { get; }
}
