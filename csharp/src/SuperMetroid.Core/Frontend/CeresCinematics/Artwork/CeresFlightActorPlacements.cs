using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

/// <summary>
/// Editable starting screen positions for the five rear-view Ceres approach actors.
/// The front star field, actor instruction programs, and physical motion stay compiled.
/// </summary>
public sealed class CeresFlightActorPlacements
{
    /// <summary>Edited actor positions, or null when all five match their stock placements.</summary>
    internal readonly CeresFlightActorPlacement[]? placements;

    /// <summary>Retains supplied placements only when at least one differs from the stock layout.</summary>
    internal CeresFlightActorPlacements(CeresFlightActorPlacement[] placements)
    {
        bool stock = true;
        for (int index = 0; index < placements.Length; index++)
            stock &= placements[index] == DefaultPlacement(index);
        this.placements = stock ? null : placements;
    }

    /// <summary>Resolves one selected rear-view actor's starting screen placement, using its stock default when the complete layout is unchanged.</summary>
    /// <param name="actorIndex">Zero-based spawn-order index: large asteroids, station under attack, small asteroids, vortex, then rear stars.</param>
    /// <returns>The fixed actor identity and encoded whole-pixel X/Y coordinates for that slot.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside 0 through 4.</exception>
    public CeresFlightActorPlacement this[int actorIndex]
    {
        get
        {
            if ((uint)actorIndex >= CeresFlightActorDefinitions.RearViewActorCount)
                throw new ArgumentOutOfRangeException(nameof(actorIndex));
            return placements is null ? DefaultPlacement(actorIndex) : placements[actorIndex];
        }
    }

    /// <summary>Resolves the named actor's initializer placement without a second stock lookup.</summary>
    private static CeresFlightActorPlacement DefaultPlacement(int index)
    {
        var actor = CeresFlightActors.RearViewActor(index);
        return new() { Id = CeresFlightActors.RearViewPlacementId(index), X = actor.Position.X, Y = actor.Position.Y };
    }
    /// <summary>Identity of ordered selected actor IDs and both initial visual coordinates.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("CeresFlightActorLayout", content =>
    {
        content.Append("actors", CeresFlightActorDefinitions.RearViewActorCount);
        for (int index = 0; index < CeresFlightActorDefinitions.RearViewActorCount; index++)
        {
            CeresFlightActorPlacement placement = this[index];
            content.Append("id", System.Text.Encoding.UTF8.GetBytes(placement.Id));
            content.Append("x", placement.X);
            content.Append("y", placement.Y);
        }
    });
}
