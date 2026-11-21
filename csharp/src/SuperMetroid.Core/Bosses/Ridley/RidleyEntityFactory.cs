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
internal sealed partial class RidleyEntityFactory(NorfairRidleyBehavior norfair, CeresRidleyBehavior ceres, ICeresEscapeStatus escape) : IEnemyEntityFactory
{
    /// <summary>Lower Norfair Ridley's encounter behaviour.</summary>
    internal NorfairRidleyBehavior Norfair { get; } = norfair;

    /// <summary>Ceres Ridley's encounter behaviour.</summary>
    internal CeresRidleyBehavior Ceres { get; } = ceres;

    /// <summary>The station's escape progress, which ends Ceres Ridley's touch and drawing.</summary>
    private ICeresEscapeStatus Escape { get; } = escape;

    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [.. PartFactories.Keys];

    /// <summary>Lower Norfair Ridley, Ceres Ridley, or a piece of the breakup explosion (#2146).</summary>
    /// <exception cref="KeyNotFoundException">The place holds no Ridley definition.</exception>
    public EnemyOccupant Create(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common) =>
        PartFactories[Ensure.NotNull(occupant).DefinitionId](this, occupant, place, common);

    /// <summary>
    /// The factory of each part the species creates, by the definition that places it: the species serves these
    /// definitions, and each builds its own part from the species it is given.
    /// </summary>
    private static readonly Dictionary<EnemyDefinitionId, EnemyPartFactory<RidleyEntityFactory>> PartFactories =
        new()
        {
            [EnemyDefinitionId.Ridley] = static (species, occupant, place, common) => EnemyOccupant.Of(new NorfairRidleyEntity(species.Norfair, occupant, place, common)),
            [EnemyDefinitionId.RidleyCeres] = static (species, occupant, place, common) => EnemyOccupant.Of(new CeresRidleyEntity(species.Ceres, species.Escape, occupant, place, common)),
            [EnemyDefinitionId.RidleyBreakupPiece] = static (species, occupant, place, common) => EnemyOccupant.Of(new RidleyBreakupPieceEntity(species.Norfair, occupant, place, common)),
        };
}
