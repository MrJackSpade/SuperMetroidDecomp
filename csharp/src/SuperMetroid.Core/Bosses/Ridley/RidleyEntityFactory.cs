using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// The factory for Lower Norfair Ridley, Ceres Ridley and the breakup explosion actor. The two Ridleys share the body
/// animation and drawing, while Ceres Ridley's initialization and main AI are the Ceres encounter's.
/// </summary>
/// <param name="ridley">The Lower Norfair Ridley owner.</param>
/// <param name="ceres">The Ceres Ridley owner.</param>
/// <param name="encounter">The shared Ridley encounter.</param>
internal sealed class RidleyEntityFactory(RidleyBehavior ridley, CeresRidleyBehavior ceres, RidleyEncounter encounter) : IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } =
        [EnemyDefinitionId.Ridley, EnemyDefinitionId.RidleyCeres, EnemyDefinitionId.RidleyBreakupPiece];

    /// <summary>Lower Norfair Ridley, Ceres Ridley, or a piece of the breakup explosion (#2146).</summary>
    /// <exception cref="InvalidOperationException">The place holds no Ridley definition.</exception>
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common)
    {
        EnemyDefinitionId definition = Ensure.NotNull(place).DefinitionId;
        if (definition == EnemyDefinitionId.Ridley)
            return new NorfairRidleyEntity(ridley, ceres, encounter, place, common);
        if (definition == EnemyDefinitionId.RidleyCeres)
            return new CeresRidleyEntity(ridley, ceres, encounter, place, common);
        if (definition == EnemyDefinitionId.RidleyBreakupPiece)
            return new RidleyBreakupPieceEntity(ridley, place, common);
        throw new InvalidOperationException($"The Ridley factory creates no {place.DefinitionId}.");
    }
}
