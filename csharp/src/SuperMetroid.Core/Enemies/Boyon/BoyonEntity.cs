namespace SuperMetroid.Core.Enemies.Boyon;

/// <summary>
/// One Boyon occupant: its initialization, bounce AI and typed animation, with the shared reactions. It owns the
/// occupant's state; the setup writes every value and the animation before any is read, so a re-initialized
/// occupant of the same place never sees the previous one's values.
/// </summary>
/// <param name="species">The Boyon species owner, which publishes the bounce sound.</param>
/// <param name="place">The place the Boyon occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="settings">What the enemy's population entry tells its species.</param>
internal sealed class BoyonEntity(BoyonBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common, BoyonSpawnSettings settings)
    : CommonParticipationEnemyEntity(place, common), IEnemyAi, IEnemyTypedAnimation
{
    /// <summary>The enemy's state, which its initialization builds as the enemy is built (#2554).</summary>
    internal BoyonEnemyState State { get; } = BoyonBehavior.InitializeBoyon(place, settings);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) =>
        BoyonBehavior.RunBoyonMain(Place, State, frame.Samus);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => species.AdvanceBoyonAnimation(Place, State, context);
}
