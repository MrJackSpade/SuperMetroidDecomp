
namespace SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

/// <summary>
/// The installed spawn positions of the three persistent actors behind Ceres's explosion, as the player left them, keyed
/// by role (#1768); the installation is their only source (#1596). Their animations, wrap behavior, and movement remain
/// compiled mechanics.
/// </summary>
public sealed class CeresDestructionActorPlacements
{
    /// <summary>The installed placements, by role.</summary>
    internal readonly Dictionary<CeresDestructionActorRole, CeresDestructionActorPlacement> placements;

    /// <summary>Keeps the complete installed layout.</summary>
    /// <param name="placements">One placement for each destruction role, in any order.</param>
    /// <exception cref="InvalidDataException">The layout does not place each of the three actors exactly once.</exception>
    internal CeresDestructionActorPlacements(IEnumerable<CeresDestructionActorPlacement> placements) =>
        this.placements = CeresPlacementsByRole.Index(placements, placement => placement.Role, "The Ceres destruction");

    /// <summary>The installed starting placement of the persistent destruction-scene actor that shows <paramref name="role"/>.</summary>
    /// <returns>The actor's role and whole-pixel starting screen coordinates.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="role"/> is not a destruction role.</exception>
    public CeresDestructionActorPlacement this[CeresDestructionActorRole role] => CeresPlacementsByRole.Of(placements, role);
}
