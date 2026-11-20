namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>
/// The growing shutter (#2146): it grows and shrinks by its own AI and animation. Its definition names the ordinary shot and
/// no touch or Power Bomb reaction, so it takes the shared reactions and has none of its own (#2554).
/// </summary>
/// <param name="species">The Shutters species owner.</param>
/// <param name="place">The place the shutter occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class GrowingShutterEntity(ShuttersBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : ShuttersEntity(species, place, common)
{
    /// <summary>The enemy's state, which its initialization builds as the enemy is built (#2554).</summary>
    internal GrowingShutterEnemyState State { get; } = ShuttersBehavior.InitializeGrowingShutter(place);

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => Species.RunShutterGrowingMainAi(Place, State, frame);

    /// <inheritdoc/>
    public override void AdvanceTypedAnimation(EnemyAnimationCueContext context) => ShuttersBehavior.AdvanceShutterGrowingTypedAnimation(Place, State, context);

}
