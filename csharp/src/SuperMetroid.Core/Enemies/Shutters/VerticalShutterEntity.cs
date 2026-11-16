namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>
/// A vertical shutter, shootable (<see cref="ShootableShutterEntity"/>) or destroyable
/// (<see cref="DestroyableShutterEntity"/>): both set up, move and animate as vertical shutters do.
/// </summary>
/// <param name="species">The Shutters species owner.</param>
/// <param name="place">The place the shutter occupies.</param>
/// <param name="common">The shared participation.</param>
internal abstract class VerticalShutterEntity(ShuttersBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : ShuttersEntity(species, place, common)
{
    /// <inheritdoc/>
    public override void Initialize() => ShuttersBehavior.InitializeVerticalShutter(Place);

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => Species.RunShutterShootableMainAi(Place, frame);

    /// <inheritdoc/>
    public override void AdvanceTypedAnimation(EnemyAnimationCueContext context) => ShuttersBehavior.AdvanceShutterShootableTypedAnimation(Place, context);
}
