
namespace SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

/// <summary>
/// The installed initial screen positions of the six Ceres-to-Zebes reveal actors and of the Japanese PLANET ZEBES
/// subtitle, as the player left them; the installation is their only source (#1596, #1733). Their animations, slide
/// acceleration, deletion, and phase handoffs remain in compiled game mechanics.
/// </summary>
public sealed class CeresRevealActorPlacements
{
    /// <summary>The installed placements, in spawn order.</summary>
    internal readonly CeresRevealActorPlacement[] placements;

    /// <summary>Keeps the complete installed layout.</summary>
    /// <param name="placements">The six actors' placements, in spawn order, then the subtitle's.</param>
    /// <exception cref="InvalidDataException">The layout does not hold exactly the reveal's placements.</exception>
    internal CeresRevealActorPlacements(CeresRevealActorPlacement[] placements)
    {
        Ensure.NotNull(placements);
        if (placements.Length != CeresDestructionActorDefinitions.RevealPlacementCount)
            throw new InvalidDataException(
                $"The Zebes reveal requires exactly {CeresDestructionActorDefinitions.RevealPlacementCount} ordered actor placements.");
        this.placements = [.. placements];
    }

    /// <summary>The installed initial placement of one actor in the Zebes-reveal spawn order.</summary>
    /// <param name="actorIndex">
    /// Index 0..6: planet, upper-left stars, upper-right stars, lower-left stars, lower-right stars, Planet Zebes title,
    /// then the Japanese subtitle.
    /// </param>
    /// <returns>Named screen-pixel placement used when creating the actor, without changing its animation or motion policy.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The actor index is outside 0..6.</exception>
    public CeresRevealActorPlacement this[int actorIndex] =>
        (uint)actorIndex < (uint)placements.Length ? placements[actorIndex] : throw new ArgumentOutOfRangeException(nameof(actorIndex));
}
