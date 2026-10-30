namespace SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

/// <summary>
/// Editable actor placements unique to the destruction of Ceres and the following Zebes reveal. The Ceres character
/// sheets and the first two map slices are shared with <see cref="CeresFlightArtwork"/>; the destruction's maps and the
/// Zebes reveal's tilemap and characters are the import's, which pictures the floor and the planet from them (#1467).
/// </summary>
public sealed class CeresDestructionArtwork
{
    /// <summary>Retains validated actor placements.</summary>
    private CeresDestructionArtwork(CeresRevealActorPlacements revealActors, CeresDestructionActorPlacements destructionActors)
    {
        RevealActors = revealActors;
        DestructionActors = destructionActors;
    }

    /// <summary>Initial screen-pixel placements of the six Zebes-reveal actors; instruction lists, slide acceleration, deletion, and phase transitions remain compiled cinematic behavior.</summary>
    public CeresRevealActorPlacements RevealActors { get; }
    /// <summary>Initial screen-pixel placements of the three persistent destruction-background actors; movement and wrap behavior are not editable through this layout.</summary>
    public CeresDestructionActorPlacements DestructionActors { get; }

    /// <summary>Loads the destruction's actor layouts, validating each without reading cartridge data.</summary>
    /// <param name="revealActorsJson">Six ordered initial Zebes-reveal actor placements.</param>
    /// <param name="destructionActorsJson">Three ordered initial destruction-background actor placements.</param>
    /// <returns>The compiled placements; both input streams are consumed from their current positions and remain caller-owned.</returns>
    public static CeresDestructionArtwork Load(Stream revealActorsJson, Stream destructionActorsJson)
    {
        Ensure.NotNull(revealActorsJson);
        Ensure.NotNull(destructionActorsJson);
        return new CeresDestructionArtwork(CeresRevealActorPlacements.Load(revealActorsJson),
            CeresDestructionActorPlacements.Load(destructionActorsJson));
    }
}
