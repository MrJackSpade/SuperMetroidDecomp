
namespace SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

/// <summary>
/// The installed initial screen positions of the six Ceres-to-Zebes reveal actors and of the Japanese PLANET ZEBES
/// subtitle, as the player left them, keyed by role (#1768); the installation is their only source (#1596, #1733). Their
/// animations, slide acceleration, deletion, and phase handoffs remain in compiled game mechanics.
/// </summary>
public sealed class CeresRevealActorPlacements
{
    /// <summary>The installed placements, by role.</summary>
    internal readonly Dictionary<ZebesRevealActorRole, CeresRevealActorPlacement> placements;

    /// <summary>Keeps the complete installed layout.</summary>
    /// <param name="placements">One placement for each reveal role, the Japanese subtitle's among them, in any order.</param>
    /// <exception cref="InvalidDataException">The layout does not place each of the reveal's actors exactly once.</exception>
    internal CeresRevealActorPlacements(IEnumerable<CeresRevealActorPlacement> placements) =>
        this.placements = CeresPlacementsByRole.Index(placements, placement => placement.Role, "The Zebes reveal");

    /// <summary>The installed initial placement of the reveal actor that shows <paramref name="role"/>.</summary>
    /// <returns>Named screen-pixel placement used when creating the actor, without changing its animation or motion policy.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="role"/> is not a reveal role.</exception>
    public CeresRevealActorPlacement this[ZebesRevealActorRole role] => CeresPlacementsByRole.Of(placements, role);
}
