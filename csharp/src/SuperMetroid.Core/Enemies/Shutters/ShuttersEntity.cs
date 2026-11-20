namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>
/// One shutter: the growing shutter (<see cref="GrowingShutterEntity"/>), or a vertical shutter
/// (<see cref="VerticalShutterEntity"/>), each running its own AI and animation.
/// </summary>
/// <param name="species">The Shutters species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the shutter occupies.</param>
/// <param name="common">The shared participation.</param>
internal abstract class ShuttersEntity(ShuttersBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation
{
    /// <summary>The shutter's own Power Bomb reaction, built for this occupant.</summary>
    private readonly EnemyPowerBombReaction ownPowerBomb = species.VerticalShutterPowerBombReaction(occupant);

    /// <summary>The Shutters species owner.</summary>
    protected ShuttersBehavior Species => species;

    /// <inheritdoc/>
    public abstract void RunMainAi(IEnemyAiFrame frame);

    /// <inheritdoc/>
    public abstract void AdvanceTypedAnimation(EnemyAnimationCueContext context);

}
