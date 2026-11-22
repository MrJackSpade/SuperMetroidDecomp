namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// One of a Dachora's speed-boost echoes (#2283): its own flicker and pose, placed and timed by the body it trails, with the
/// shared reactions.
/// </summary>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the echo occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="settings">What the enemy's population entry tells its species.</param>
/// <param name="state">What the enemy's setup built (#2554).</param>
internal sealed partial class DachoraEchoEntity(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
    DachoraSpawnSettings settings, DachoraEchoState state)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation
{
    /// <summary>
    /// Builds the enemy, running its setup as the game builds it (#2554). A restore of the game's state builds it through the
    /// primary constructor from what it holds, so the setup never runs again.
    /// </summary>
    /// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
    /// <param name="place">The place the echo occupies.</param>
    /// <param name="common">The shared participation.</param>
    /// <param name="settings">What the enemy's population entry tells its species.</param>
    internal DachoraEchoEntity(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, DachoraSpawnSettings settings)
        : this(occupant, place, common, settings, DachoraBehavior.InitializeDachoraEcho(settings, place))
    {
    }

    /// <summary>The enemy's state, which its setup builds as the enemy is built (#2554).</summary>
    internal DachoraEchoState State { get; } = state;

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => DachoraBehavior.RunDachoraEchoMainAi(Place, State, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => DachoraBehavior.AdvanceDachoraEchoTypedAnimation(Place, State);
}
