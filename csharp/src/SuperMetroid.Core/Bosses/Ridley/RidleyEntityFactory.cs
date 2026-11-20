using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// The factory for Lower Norfair Ridley, Ceres Ridley and the breakup explosion actor. Each Ridley body runs its own
/// encounter over the parts of his body both share (#2229).
/// </summary>
/// <param name="norfair">Lower Norfair Ridley's encounter behaviour.</param>
/// <param name="ceres">Ceres Ridley's encounter behaviour.</param>
/// <param name="escape">The station's escape progress, which ends Ceres Ridley's touch and drawing.</param>
internal sealed class RidleyEntityFactory(NorfairRidleyBehavior norfair, CeresRidleyBehavior ceres, ICeresEscapeStatus escape) : IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } =
        [EnemyDefinitionId.Ridley, EnemyDefinitionId.RidleyCeres, EnemyDefinitionId.RidleyBreakupPiece];

    /// <summary>Lower Norfair Ridley, Ceres Ridley, or a piece of the breakup explosion (#2146).</summary>
    /// <exception cref="InvalidOperationException">The place holds no Ridley definition.</exception>
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common)
    {
        EnemyDefinitionId definition = Ensure.NotNull(place).RequireOccupant().DefinitionId;
        if (definition == EnemyDefinitionId.Ridley)
            return new NorfairRidleyEntity(norfair, place, common);
        if (definition == EnemyDefinitionId.RidleyCeres)
            return new CeresRidleyEntity(ceres, escape, place, common);
        if (definition == EnemyDefinitionId.RidleyBreakupPiece)
            return new RidleyBreakupPieceEntity(norfair, place, common);
        throw new InvalidOperationException($"The Ridley factory creates no {place.Occupant?.DefinitionId}.");
    }
}
