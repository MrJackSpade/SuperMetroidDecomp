namespace SuperMetroid.Core.Enemies.Boyon;

/// <summary>
/// One Boyon occupant: its initialization, bounce AI and typed animation, with the shared reactions. It owns the
/// occupant's state; the setup writes every value and the animation before any is read, so a re-initialized
/// occupant of the same place never sees the previous one's values.
/// </summary>
/// <param name="species">The Boyon species owner, which publishes the bounce sound.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the Boyon occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="settings">What the enemy's population entry tells its species.</param>
/// <param name="state">What the enemy's setup built (#2554).</param>
internal sealed partial class BoyonEntity(BoyonBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
    BoyonSpawnSettings settings, BoyonEnemyState state)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation
{
    /// <summary>
    /// Builds the enemy, running its setup as the game builds it (#2554). A restore of the game's state builds it through the
    /// primary constructor from what it holds, so the setup never runs again.
    /// </summary>
    /// <param name="species">The Boyon species owner, which publishes the bounce sound.</param>
    /// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
    /// <param name="place">The place the Boyon occupies.</param>
    /// <param name="common">The shared participation.</param>
    /// <param name="settings">What the enemy's population entry tells its species.</param>
    internal BoyonEntity(BoyonBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
        BoyonSpawnSettings settings)
        : this(species, occupant, place, common, settings, BoyonBehavior.InitializeBoyon(place))
    {
    }

    /// <summary>What the enemy's population entry tells its species, which its factory checked as it built the enemy.</summary>
    internal BoyonSpawnSettings Settings { get; } = settings;

    /// <summary>The enemy's state, which its setup builds as the enemy is built (#2554).</summary>
    private readonly BoyonEnemyState state = state;

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) =>
        BoyonBehavior.RunBoyonMain(Settings, Place, state, frame.Samus);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => species.AdvanceBoyonAnimation(Place, state);
}
