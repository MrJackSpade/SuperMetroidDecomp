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
internal sealed class RidleyEntityFactory(NorfairRidleyBehavior norfair, CeresRidleyBehavior ceres, ICeresEscapeStatus escape) : IEnemyEntityFactory, IEnemySettingsFactory<RidleyExplosionSpawnSettings>
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } =
        [EnemyDefinitionId.Ridley, EnemyDefinitionId.RidleyCeres, EnemyDefinitionId.RidleyBreakupPiece];

    /// <summary>Lower Norfair Ridley, Ceres Ridley, or a piece of the breakup explosion (#2146); a defeated room's Lower Norfair Ridley is deleted before any setup (#2554).</summary>
    /// <exception cref="InvalidOperationException">The place holds no Ridley definition.</exception>
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common, EnemySetup setup)
    {
        EnemyDefinitionId definition = Ensure.NotNull(place).DefinitionId;
        if (definition == EnemyDefinitionId.Ridley)
        {
            if (!norfair.Combat.IsDeletedAtOnce)
                return new NorfairRidleyEntity(norfair, place, common);
            NorfairRidleyCombat.DeleteDefeatedRidley(place);
            return new DefeatedNorfairRidleyEntity(place, common);
        }
        if (definition == EnemyDefinitionId.RidleyCeres)
            return new CeresRidleyEntity(ceres, escape, place, common);
        throw new InvalidOperationException($"The Ridley factory creates no {place.DefinitionId} told nothing.");
    }

    /// <inheritdoc/>
    void IEnemySpeciesFactory.EnrollIn(EnemySpeciesRegistry registry)
    {
        IEnemyEntityFactory untold = this;
        Ensure.NotNull(registry).Add(untold);
        registry.Add<RidleyExplosionSpawnSettings>(this);
    }

    /// <summary>A piece of Lower Norfair Ridley's breakup explosion, the part its spawn names (#2146).</summary>
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common, EnemySetup setup, RidleyExplosionSpawnSettings settings) =>
        new RidleyBreakupPieceEntity(norfair, place, common, setup, settings);
}
