namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// One of a Dachora's speed-boost echoes (#2283): its own flicker and pose, placed and timed by the body it trails, with the
/// shared reactions.
/// </summary>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the echo occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="settings">What the enemy's population entry tells its species.</param>
internal sealed class DachoraEchoEntity(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, DachoraSpawnSettings settings)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation
{
    /// <summary>What the enemy's population entry tells its species, which its factory checked as it built the enemy.</summary>
    internal DachoraSpawnSettings Settings { get; } = settings;

    /// <summary>The enemy's state, which its setup builds as the enemy is built (#2554).</summary>
    internal DachoraEchoState State { get; } = DachoraBehavior.InitializeDachoraEcho(settings, place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => DachoraBehavior.RunDachoraEchoMainAi(Place, State, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => DachoraBehavior.AdvanceDachoraEchoTypedAnimation(Place, State);
}
