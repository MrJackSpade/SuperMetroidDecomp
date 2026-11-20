namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>
/// A vertical shutter, shootable (<see cref="ShootableShutterEntity"/>, or the Kamer that reuses it,
/// <see cref="KamerEntity"/>) or destroyable (<see cref="DestroyableShutterEntity"/>): each sets up, moves and animates as
/// vertical shutters do, showing its own pose.
/// </summary>
/// <param name="species">The Shutters species owner.</param>
/// <param name="place">The place the shutter occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="pose">The pose the shutter shows from its setup.</param>
internal abstract class VerticalShutterEntity(ShuttersBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common, ShutterSegment pose)
    : ShuttersEntity(species, place, common)
{
    /// <summary>The shutter's state, which its setup builds as it is built (#2554).</summary>
    internal VerticalShutterEnemyState State { get; } = ShuttersBehavior.InitializeVerticalShutter(place, pose);

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => Species.RunVerticalShutterMainAi(Place, frame);

    /// <inheritdoc/>
    public override void AdvanceTypedAnimation(EnemyAnimationCueContext context) => ShuttersBehavior.AdvanceVerticalShutterTypedAnimation(Place, context);
}
