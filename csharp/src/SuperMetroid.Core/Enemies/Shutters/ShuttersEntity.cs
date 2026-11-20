namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>
/// One shutter: the growing shutter (<see cref="GrowingShutterEntity"/>), or a vertical shutter
/// (<see cref="VerticalShutterEntity"/>), each running its own AI and animation.
/// </summary>
/// <param name="species">The Shutters species owner.</param>
/// <param name="place">The place the shutter occupies.</param>
/// <param name="common">The shared participation.</param>
internal abstract class ShuttersEntity(ShuttersBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyAi, IEnemyTypedAnimation
{
    /// <summary>The Shutters species owner.</summary>
    protected ShuttersBehavior Species => species;

    /// <inheritdoc/>
    public abstract void RunMainAi(IEnemyAiFrame frame);

    /// <inheritdoc/>
    public abstract void AdvanceTypedAnimation(EnemyAnimationCueContext context);

}
