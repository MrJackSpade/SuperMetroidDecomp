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
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } =
        [EnemyDefinitionId.Ridley, EnemyDefinitionId.RidleyCeres, EnemyDefinitionId.RidleyBreakupPiece];

    /// <summary>Lower Norfair Ridley, Ceres Ridley, or a piece of the breakup explosion (#2146).</summary>
    /// <exception cref="InvalidOperationException">The place holds no Ridley definition.</exception>
    public EnemyOccupant Create(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    {
        EnemyDefinitionId definition = Ensure.NotNull(occupant).DefinitionId;
        return PartFactories.TryGetValue(definition, out EnemyPartFactory? factory) ? factory(occupant, place, common)
            : throw new InvalidOperationException($"The Ridley factory creates no {occupant.DefinitionId}.");
    }

    /// <summary>The factory of each part the species creates, by the definition that places it.</summary>
    private IReadOnlyDictionary<EnemyDefinitionId, EnemyPartFactory> PartFactories => field ??=
        new Dictionary<EnemyDefinitionId, EnemyPartFactory>
        {
            [EnemyDefinitionId.Ridley] = (occupant, place, common) => EnemyOccupant.Of(new NorfairRidleyEntity(Norfair, occupant, place, common)),
            [EnemyDefinitionId.RidleyCeres] = (occupant, place, common) => EnemyOccupant.Of(new CeresRidleyEntity(Ceres, escape, occupant, place, common)),
            [EnemyDefinitionId.RidleyBreakupPiece] = (occupant, place, common) => EnemyOccupant.Of(new RidleyBreakupPieceEntity(Norfair, occupant, place, common)),
        };
}
