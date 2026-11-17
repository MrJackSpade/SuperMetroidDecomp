namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>
/// The mama turtle (<see cref="MamaTurtleEntity"/>) or one of her babies (<see cref="BabyTurtleEntity"/>): each runs its
/// own touch where its definition leaves Samus's touch to it, and both take the baby's shot and bomb stages where their
/// definitions leave shots to them.
/// </summary>
/// <param name="species">The mama turtle species owner.</param>
/// <param name="place">The place the turtle occupies.</param>
/// <param name="common">The shared participation.</param>
internal abstract class TurtleEntity(MamaTurtleBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyInitialization, IEnemyAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <summary>The mama turtle species owner.</summary>
    protected MamaTurtleBehavior Species => species;

    /// <inheritdoc/>
    public abstract void Initialize();

    /// <inheritdoc/>
    public abstract void RunMainAi(IEnemyAiFrame frame);

    /// <inheritdoc/>
    public abstract void AdvanceTypedAnimation(EnemyAnimationCueContext context);

    /// <summary>Runs the turtle's own touch.</summary>
    /// <param name="context">The touch.</param>
    protected abstract void ReactToOwnTouch(EnemyTouchContext context);

    /// <summary>Whether the turtle's own touch, where its definition leaves Samus's touch to it, or a shared one applies.</summary>
    public override bool ReactsToTouch => LeavesTouchToSpecies || base.ReactsToTouch;

    /// <summary>Runs the turtle's own touch where the definition leaves Samus's touch to it, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (!LeavesTouchToSpecies)
            base.ReactToTouch(context);
        else
            ReactToOwnTouch(context);
    }

    /// <summary>The baby turtle's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction? ShotReaction => LeavesShotToSpecies ? species.BabyTurtleShotReaction : base.ShotReaction;

    /// <summary>The baby turtle's own stages around common bomb damage, for a bomb that selects its own shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override EnemyBombReaction? CommonBombDamageReaction(EnemyBombContext context) =>
        context.SelectedShot is null ? species.BabyTurtleBombReaction : base.CommonBombDamageReaction(context);

    /// <summary>The baby turtle's own shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects; none where the definition leaves shots to the species.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) => reaction is null && LeavesShotToSpecies;
}
