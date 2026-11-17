namespace SuperMetroid.Core.Enemies.Puyo;

/// <summary>One Puyo occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Puyo species owner.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class PuyoEntity(PuyoBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyInitialization, IEnemyAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize() => PuyoBehavior.InitializePuyo(Place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunPuyoMainAi(Place, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => PuyoBehavior.AdvancePuyoTypedAnimation(Place, context);
}
