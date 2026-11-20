namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>
/// The growing shutter (#2146): it grows and shrinks by its own AI and animation. Its definition names the ordinary shot and
/// no touch or Power Bomb reaction, so it takes the shared reactions and has none of its own (#2554).
/// </summary>
/// <param name="species">The Shutters species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the shutter occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="settings">What the enemy's population entry tells its species.</param>
internal sealed class GrowingShutterEntity(ShuttersBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, GrowingShutterSpawnSettings settings)
    : ShuttersEntity(species, occupant, place, common)
{
    /// <summary>The enemy's state, which its initialization builds as the enemy is built (#2554).</summary>
    internal GrowingShutterEnemyState State { get; } = ShuttersBehavior.InitializeGrowingShutter(place, settings);

    /// <inheritdoc/>
    public override void Initialize() => ShuttersBehavior.InitializeGrowingShutter(Occupant, State, Place);

    /// <inheritdoc/>
    public override void AdvanceTypedAnimation(EnemyAnimationCueContext context) => ShuttersBehavior.AdvanceShutterGrowingTypedAnimation(Place, State, context);

}
