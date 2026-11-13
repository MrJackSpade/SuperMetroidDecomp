
namespace SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

/// <summary>
/// The installed spawn positions of the three persistent actors behind Ceres's explosion, as the player left them; the
/// installation is their only source (#1596). Their animations, wrap behavior, and movement remain compiled mechanics.
/// </summary>
public sealed class CeresDestructionActorPlacements
{
    /// <summary>The installed placements, in spawn order.</summary>
    internal readonly CeresDestructionActorPlacement[] placements;

    /// <summary>Keeps the complete installed layout.</summary>
    /// <param name="placements">The three placements, in spawn order.</param>
    /// <exception cref="InvalidDataException">The layout does not hold exactly the three actors.</exception>
    internal CeresDestructionActorPlacements(CeresDestructionActorPlacement[] placements)
    {
        Ensure.NotNull(placements);
        if (placements.Length != CeresDestructionActorDefinitions.InitialActorCount)
            throw new InvalidDataException("Ceres destruction requires exactly three ordered actor placements.");
        this.placements = [.. placements];
    }

    /// <summary>The installed starting placement of one persistent destruction-scene actor.</summary>
    /// <param name="actorIndex">Zero-based spawn-order index: large asteroids, small asteroids, then the stationary vortex.</param>
    /// <returns>The fixed actor identity and encoded whole-pixel starting screen coordinates.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside 0 through 2.</exception>
    public CeresDestructionActorPlacement this[int actorIndex] =>
        (uint)actorIndex < (uint)placements.Length ? placements[actorIndex] : throw new ArgumentOutOfRangeException(nameof(actorIndex));
}
