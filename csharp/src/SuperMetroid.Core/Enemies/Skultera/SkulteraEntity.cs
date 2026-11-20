namespace SuperMetroid.Core.Enemies.Skultera;

/// <summary>One Skultera occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Skultera species owner.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class SkulteraEntity(SkulteraBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyAi, IEnemyTypedAnimation
{
    /// <summary>The enemy's state, which its initialization builds as the enemy is built (#2554).</summary>
    internal SkulteraEnemyState State { get; } = SkulteraBehavior.InitializeSkultera(place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunSkulteraMainAi(Place, State, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => SkulteraBehavior.AdvanceSkulteraTypedAnimation(Place, context);
}
