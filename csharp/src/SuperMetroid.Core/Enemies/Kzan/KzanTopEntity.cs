namespace SuperMetroid.Core.Enemies.Kzan;

/// <summary>A Kzan's top (#2146), which waits, falls and rises again: its own AI and animation, with the shared reactions.</summary>
/// <param name="species">The Kzan species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the top occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class KzanTopEntity(KzanBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyInitialization, IEnemyAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize() => KzanBehavior.InitializeKzanTop(Place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunKzanTopMainAi(Place, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => KzanBehavior.AdvanceKzanTypedAnimation(Place, context);
}
