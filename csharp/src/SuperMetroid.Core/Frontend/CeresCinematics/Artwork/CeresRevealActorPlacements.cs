
namespace SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

/// <summary>
/// Editable initial screen positions for the six Ceres-to-Zebes reveal actors.
/// Their animation scripts, slide acceleration, deletion, and phase handoffs
/// remain in compiled game mechanics.
/// </summary>
public sealed class CeresRevealActorPlacements
{
    /// <summary>Explicit authored positions, or null when all six actors use their compiled stock placements.</summary>
    internal readonly CeresRevealActorPlacement[]? placements;

    /// <summary>Collapses an all-stock layout to the stock placement lookup while retaining edited actor positions.</summary>
    internal CeresRevealActorPlacements(CeresRevealActorPlacement[] placements)
    {
        bool stock = true;
        for (int index = 0; index < placements.Length; index++)
            stock &= placements[index] == DefaultPlacement(index);
        this.placements = stock ? null : placements;
    }

    /// <summary>Returns the selected initial placement for one actor in the Zebes-reveal spawn order.</summary>
    /// <param name="actorIndex">Index 0..5: planet, upper-left stars, upper-right stars, lower-left stars, lower-right stars, then Planet Zebes title.</param>
    /// <returns>Named screen-pixel placement used when creating the actor, without changing its instruction list or motion policy.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The actor index is outside 0..5.</exception>
    public CeresRevealActorPlacement this[int actorIndex]
    {
        get
        {
            if ((uint)actorIndex >= CeresDestructionActorDefinitions.ZebesActorCount)
                throw new ArgumentOutOfRangeException(nameof(actorIndex));
            return placements is null ? DefaultPlacement(actorIndex) : placements[actorIndex];
        }
    }

    /// <summary>Resolves the named actor's initializer placement without a second stock lookup.</summary>
    private static CeresRevealActorPlacement DefaultPlacement(int index)
    {
        var actor = CeresDestructionActors.ZebesActor(index);
        return new() { Id = CeresDestructionActors.ZebesPlacementId(index), X = actor.Position.X, Y = actor.Position.Y };
    }
}
