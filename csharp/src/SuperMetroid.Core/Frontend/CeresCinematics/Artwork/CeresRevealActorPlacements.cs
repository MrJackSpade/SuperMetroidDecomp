
namespace SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

/// <summary>
/// The installed initial screen positions of the six Ceres-to-Zebes reveal actors, as the player left them; the
/// installation is their only source (#1596). Their animations, slide acceleration, deletion, and phase handoffs
/// remain in compiled game mechanics.
/// </summary>
public sealed class CeresRevealActorPlacements
{
    /// <summary>The installed placements, in spawn order.</summary>
    internal readonly CeresRevealActorPlacement[] placements;

    /// <summary>Keeps the complete installed layout.</summary>
    /// <param name="placements">The six placements, in spawn order.</param>
    /// <exception cref="InvalidDataException">The layout does not hold exactly the six actors.</exception>
    internal CeresRevealActorPlacements(CeresRevealActorPlacement[] placements)
    {
        Ensure.NotNull(placements);
        if (placements.Length != CeresDestructionActorDefinitions.ZebesActorCount)
            throw new InvalidDataException("The Zebes reveal requires exactly six ordered actor placements.");
        this.placements = [.. placements];
    }

    /// <summary>The installed initial placement of one actor in the Zebes-reveal spawn order.</summary>
    /// <param name="actorIndex">Index 0..5: planet, upper-left stars, upper-right stars, lower-left stars, lower-right stars, then Planet Zebes title.</param>
    /// <returns>Named screen-pixel placement used when creating the actor, without changing its animation or motion policy.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The actor index is outside 0..5.</exception>
    public CeresRevealActorPlacement this[int actorIndex] =>
        (uint)actorIndex < (uint)placements.Length ? placements[actorIndex] : throw new ArgumentOutOfRangeException(nameof(actorIndex));
}
