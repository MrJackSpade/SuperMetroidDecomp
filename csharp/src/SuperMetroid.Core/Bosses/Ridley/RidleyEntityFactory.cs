using SuperMetroid.Core.Rooms;
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
[RoomLifetime]
internal sealed class RidleyEntityFactory(NorfairRidleyBehavior norfair, CeresRidleyBehavior ceres, ICeresEscapeStatus escape) : IEnemyEntityFactory
{
    /// <summary>Lower Norfair Ridley's encounter behaviour.</summary>
    private NorfairRidleyBehavior Norfair { get; } = norfair;

    /// <summary>Ceres Ridley's encounter behaviour.</summary>
    private CeresRidleyBehavior Ceres { get; } = ceres;

    /// <inheritdoc/>
    public IReadOnlyDictionary<EnemyDefinitionId, EnemyEntityCreation> Creations => field ??= new Dictionary<EnemyDefinitionId, EnemyEntityCreation>
    {
        [EnemyDefinitionId.Ridley] = (place, common) => EnemyOccupant.Of(new NorfairRidleyEntity(Norfair, place, common)),
        [EnemyDefinitionId.RidleyCeres] = (place, common) => EnemyOccupant.Of(new CeresRidleyEntity(Ceres, escape, place, common)),
        [EnemyDefinitionId.RidleyBreakupPiece] = (place, common) => EnemyOccupant.Of(new RidleyBreakupPieceEntity(Norfair, place, common)),
    };

}
