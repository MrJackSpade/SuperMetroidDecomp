namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>
/// The growing shutter (#2146): it grows and shrinks by its own AI and animation, and takes the destroyable shutter's shot
/// reaction where its definition leaves shots to it.
/// </summary>
/// <param name="species">The Shutters species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the shutter occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="settings">What the shutter's population entry tells its species.</param>
internal sealed class GrowingShutterEntity(ShuttersBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
    GrowingShutterSpawnSettings settings)
    : ShuttersEntity(species, occupant, place, common)
{
    /// <summary>What the shutter's population entry tells its species, which its factory checked as it built the enemy.</summary>
    internal GrowingShutterSpawnSettings Settings { get; } = settings;

    /// <summary>The shutter's state, which its setup builds as the enemy is built (#2554).</summary>
    internal GrowingShutterEnemyState State { get; } = ShuttersBehavior.InitializeGrowingShutter(settings, place);


    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => Species.RunShutterGrowingMainAi(Occupant, Settings, Place, State, frame);

    /// <inheritdoc/>
    public override void AdvanceTypedAnimation(EnemyAnimationCueContext context) => ShuttersBehavior.AdvanceShutterGrowingTypedAnimation(Place, State);


}
