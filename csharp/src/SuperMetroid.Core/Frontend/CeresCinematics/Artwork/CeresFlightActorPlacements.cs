
namespace SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

/// <summary>
/// The installed starting screen positions of the five rear-view Ceres approach actors, as the player left them; the
/// installation is their only source (#1596). The front star field, actor programs, and physical motion stay
/// compiled.
/// </summary>
public sealed class CeresFlightActorPlacements
{
    /// <summary>The installed placements, in spawn order.</summary>
    internal readonly CeresFlightActorPlacement[] placements;

    /// <summary>Keeps the complete installed layout.</summary>
    /// <param name="placements">The five placements, in spawn order.</param>
    /// <exception cref="InvalidDataException">The layout does not hold exactly the five actors.</exception>
    internal CeresFlightActorPlacements(CeresFlightActorPlacement[] placements)
    {
        Ensure.NotNull(placements);
        if (placements.Length != CeresFlightActorDefinitions.RearViewActorCount)
            throw new InvalidDataException("The Ceres approach requires exactly five ordered actor placements.");
        this.placements = [.. placements];
    }

    /// <summary>The installed starting screen placement of one rear-view actor.</summary>
    /// <param name="actorIndex">Zero-based spawn-order index: large asteroids, station under attack, small asteroids, vortex, then rear stars.</param>
    /// <returns>The fixed actor identity and encoded whole-pixel X/Y coordinates for that slot.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside 0 through 4.</exception>
    public CeresFlightActorPlacement this[int actorIndex] =>
        (uint)actorIndex < (uint)placements.Length ? placements[actorIndex] : throw new ArgumentOutOfRangeException(nameof(actorIndex));
}
