namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>
/// The mama turtle (<see cref="MamaTurtleEntity"/>) or one of her babies (<see cref="BabyTurtleEntity"/>): each runs its
/// own touch where its definition leaves Samus's touch to it.
/// </summary>
/// <param name="species">The mama turtle species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the turtle occupies.</param>
/// <param name="common">The shared participation.</param>
internal abstract partial class TurtleEntity(MamaTurtleBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation
{
    /// <summary>The mama turtle species owner.</summary>
    protected MamaTurtleBehavior Species => species;

    /// <inheritdoc/>
    public abstract void RunMainAi(IEnemyAiFrame frame);

    /// <inheritdoc/>
    public abstract void AdvanceTypedAnimation(EnemyAnimationCueContext context);

    /// <summary>Runs the turtle's own touch.</summary>
    /// <param name="context">The touch.</param>
    protected abstract void ReactToOwnTouch(EnemyTouchContext context);

    /// <summary>Whether the turtle's own touch, where its definition leaves Samus's touch to it, or a shared one applies.</summary>
    public override bool ReactsToTouch => Occupant.Definition.Touch == TurtleReactions.Touch || base.ReactsToTouch;

    /// <summary>Runs the turtle's own touch where the definition leaves Samus's touch to it, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (Occupant.Definition.Touch != TurtleReactions.Touch)
            base.ReactToTouch(context);
        else
            ReactToOwnTouch(context);
    }
}
