using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// Places a room's steam vents (#2480), each drawn as the motions the room's scenery gives its vents place it: in the
/// turning elevator shaft, which alone builds <see cref="CeresShaftSteamVentMotions"/>, a rotating-elevator vent rides the
/// shaft's turn; everywhere else every vent holds its place (<see cref="CeresSteamVentsHoldingPlace"/>).
/// </summary>
/// <param name="vents">The room's owner of the steam vents.</param>
/// <param name="motions">The motions the room gives the vents it places.</param>
[RoomLifetime]
internal sealed class CeresSteamFactory(CeresSteamBehavior vents, CeresSteamVentMotions motions) : IEnemyEntityFactory
{

    /// <summary>The room's owner of the steam vents.</summary>
    private readonly CeresSteamBehavior _vents = Ensure.NotNull(vents);

    /// <summary>The motions the room gives the Vents it places.</summary>
    private readonly CeresSteamVentMotions _motions = Ensure.NotNull(motions);

    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.SteamVent];

    /// <inheritdoc/>
    public EnemyOccupant Create(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common) => EnemyOccupant.Of(new CeresSteamEntity(_vents, occupant, place, common, _motions));
}
