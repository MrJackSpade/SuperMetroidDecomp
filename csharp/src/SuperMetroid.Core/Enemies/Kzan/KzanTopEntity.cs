namespace SuperMetroid.Core.Enemies.Kzan;

/// <summary>A Kzan's top (#2146), which waits, falls and rises again: its own AI and animation, with the shared reactions.</summary>
/// <param name="species">The Kzan species owner.</param>
/// <param name="place">The place the top occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class KzanTopEntity(KzanBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyAi, IEnemyTypedAnimation
{
    /// <summary>The enemy's state, which its initialization builds as the enemy is built (#2554).</summary>
    internal KzanEnemyState State { get; } = KzanBehavior.InitializeKzanTop(place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunKzanTopMainAi(Place, State, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => KzanBehavior.AdvanceKzanTypedAnimation(Place, State, context);
}
