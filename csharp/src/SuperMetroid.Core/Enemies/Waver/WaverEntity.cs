namespace SuperMetroid.Core.Enemies.Waver;

/// <summary>One Waver occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Waver species owner.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class WaverEntity(WaverBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyAi, IEnemyTypedAnimation
{
    /// <summary>The enemy's state, which its initialization builds as the enemy is built (#2554).</summary>
    internal WaverEnemyState State { get; } = WaverBehavior.InitializeWaver(place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunWaverMainAi(Place, State, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => WaverBehavior.AdvanceWaverTypedAnimation(Place, State, context);
}
