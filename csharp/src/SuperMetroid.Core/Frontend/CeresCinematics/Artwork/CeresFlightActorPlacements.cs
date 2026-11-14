
namespace SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

/// <summary>
/// The installed starting screen positions of the five rear-view Ceres approach actors, as the player left them, keyed
/// by role (#1768); the installation is their only source (#1596). The front star field, actor animations, and physical
/// motion stay compiled.
/// </summary>
public sealed class CeresFlightActorPlacements
{
    /// <summary>The installed placements, by role.</summary>
    internal readonly Dictionary<CeresRearViewActorRole, CeresFlightActorPlacement> placements;

    /// <summary>Keeps the complete installed layout.</summary>
    /// <param name="placements">One placement for each rear-view role, in any order.</param>
    /// <exception cref="InvalidDataException">The layout does not place each of the five actors exactly once.</exception>
    internal CeresFlightActorPlacements(IEnumerable<CeresFlightActorPlacement> placements) =>
        this.placements = CeresPlacementsByRole.Index(placements, placement => placement.Role, "The Ceres approach");

    /// <summary>The installed starting screen placement of the rear-view actor that shows <paramref name="role"/>.</summary>
    /// <returns>The actor's role and whole-pixel X/Y starting coordinates.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="role"/> is not a rear-view role.</exception>
    public CeresFlightActorPlacement this[CeresRearViewActorRole role] => CeresPlacementsByRole.Of(placements, role);
}
