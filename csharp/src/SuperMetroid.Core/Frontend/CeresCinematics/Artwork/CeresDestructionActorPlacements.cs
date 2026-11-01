using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

/// <summary>
/// Editable spawn positions for the three persistent actors behind Ceres's explosion.
/// Their instruction lists, wrap behavior, and movement remain compiled mechanics.
/// </summary>
public sealed class CeresDestructionActorPlacements
{
    /// <summary>Custom placements, or null when all three actors use their compiled spawn positions.</summary>
    internal readonly CeresDestructionActorPlacement[]? placements;

    /// <summary>Stores an owned layout only if at least one actor differs from the stock default.</summary>
    internal CeresDestructionActorPlacements(CeresDestructionActorPlacement[] placements)
    {
        bool stock = true;
        for (int index = 0; index < placements.Length; index++)
            stock &= placements[index] == DefaultPlacement(index);
        this.placements = stock ? null : placements;
    }

    /// <summary>Resolves the selected starting placement of one persistent destruction-scene actor, using stock defaults when the complete layout is unchanged.</summary>
    /// <param name="actorIndex">Zero-based spawn-order index: large asteroids, small asteroids, then the stationary vortex.</param>
    /// <returns>The fixed actor identity and encoded whole-pixel starting screen coordinates.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside 0 through 2.</exception>
    public CeresDestructionActorPlacement this[int actorIndex]
    {
        get
        {
            if ((uint)actorIndex >= CeresDestructionActorDefinitions.InitialActorCount)
                throw new ArgumentOutOfRangeException(nameof(actorIndex));
            return placements is null ? DefaultPlacement(actorIndex) : placements[actorIndex];
        }
    }

    /// <summary>Resolves the named actor's initializer placement without a second stock lookup.</summary>
    private static CeresDestructionActorPlacement DefaultPlacement(int index)
    {
        var actor = CeresDestructionActors.InitialActor(index);
        return new() { Id = CeresDestructionActors.InitialPlacementId(index), X = actor.Position.X, Y = actor.Position.Y };
    }
    /// <summary>Identity of ordered selected actor IDs and both initial visual coordinates.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("CeresDestructionActorLayout", content =>
    {
        content.Append("actors", CeresDestructionActorDefinitions.InitialActorCount);
        for (int index = 0; index < CeresDestructionActorDefinitions.InitialActorCount; index++)
        {
            CeresDestructionActorPlacement placement = this[index];
            content.Append("id", System.Text.Encoding.UTF8.GetBytes(placement.Id));
            content.Append("x", placement.X);
            content.Append("y", placement.Y);
        }
    });
}
