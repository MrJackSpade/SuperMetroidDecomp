namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>
/// A vertical shutter, shootable (<see cref="ShootableShutterEntity"/>, or the Kamer that reuses it,
/// <see cref="KamerEntity"/>) or destroyable (<see cref="DestroyableShutterEntity"/>): each sets up, moves and animates as
/// vertical shutters do, showing its own pose.
/// </summary>
/// <param name="species">The Shutters species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the shutter occupies.</param>
/// <param name="common">The shared participation.</param>
internal abstract class VerticalShutterEntity(ShuttersBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    : ShuttersEntity(species, occupant, place, common)
{
    /// <inheritdoc/>
    public override void Initialize() => ShuttersBehavior.InitializeVerticalShutter(Occupant, Place, Pose);

    /// <summary>The pose the shutter shows from its setup.</summary>
    protected abstract ShutterSegment Pose { get; }

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => Species.RunVerticalShutterMainAi(Occupant, Place, frame);

    /// <inheritdoc/>
    public override void AdvanceTypedAnimation(EnemyAnimationCueContext context) => ShuttersBehavior.AdvanceVerticalShutterTypedAnimation(Place, context);
}
