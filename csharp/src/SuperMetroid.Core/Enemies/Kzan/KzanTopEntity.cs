namespace SuperMetroid.Core.Enemies.Kzan;

/// <summary>A Kzan's top (#2146), which waits, falls and rises again: its own AI and animation, with the shared reactions.</summary>
/// <param name="species">The Kzan species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the top occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="settings">What the enemy's population entry tells its species.</param>
internal sealed class KzanTopEntity(KzanBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, KzanSpawnSettings settings)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation
{
    /// <summary>The enemy's state, which its initialization builds as the enemy is built (#2554).</summary>
    internal KzanEnemyState State { get; } = KzanBehavior.InitializeKzanTop(Occupant, place, settings);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunKzanTopMainAi(Occupant, Place, State, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => KzanBehavior.AdvanceKzanTypedAnimation(Place, State, context);
}
