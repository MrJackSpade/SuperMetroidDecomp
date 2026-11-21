namespace SuperMetroid.Core.Enemies.Puyo;

/// <summary>One Puyo occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Puyo species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="settings">What the enemy's population entry tells its species.</param>
internal sealed class PuyoEntity(PuyoBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, PuyoSpawnSettings settings)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation
{
    /// <summary>What the enemy's population entry tells its species, which its factory checked as it built the enemy.</summary>
    internal PuyoSpawnSettings Settings { get; } = settings;

    /// <summary>The enemy's state, which its setup builds as the enemy is built (#2554).</summary>
    internal PuyoEnemyState State { get; } = PuyoBehavior.InitializePuyo(settings, place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunPuyoMainAi(Settings, Place, State, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => PuyoBehavior.AdvancePuyoTypedAnimation(Place, State);
}
