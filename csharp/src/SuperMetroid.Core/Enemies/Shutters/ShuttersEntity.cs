namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>
/// One shutter: the growing shutter (<see cref="GrowingShutterEntity"/>), or a vertical shutter, shootable
/// (<see cref="ShootableShutterEntity"/>, or the Kamer that reuses it, <see cref="KamerEntity"/>) or destroyable
/// (<see cref="DestroyableShutterEntity"/>). Each runs
/// the vertical shutters' own touch and Power Bomb reaction where its definition leaves them to it.
/// </summary>
/// <param name="species">The Shutters species owner.</param>
/// <param name="place">The place the shutter occupies.</param>
/// <param name="common">The shared participation.</param>
internal abstract class ShuttersEntity(ShuttersBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <summary>The Shutters species owner.</summary>
    protected ShuttersBehavior Species => species;

    /// <inheritdoc/>
    public abstract void Initialize();

    /// <inheritdoc/>
    public abstract void RunMainAi(IEnemyAiFrame frame);

    /// <inheritdoc/>
    public abstract void AdvanceTypedAnimation(EnemyAnimationCueContext context);

    /// <summary>Whether the vertical shutter's own touch, where its definition leaves Samus's touch to it, or a shared one applies.</summary>
    public override bool ReactsToTouch => LeavesTouchToSpecies || base.ReactsToTouch;

    /// <summary>Runs the vertical shutter's own touch where its definition leaves Samus's touch to it, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (LeavesTouchToSpecies)
            species.ReactToVerticalShutterTouch(context);
        else
            base.ReactToTouch(context);
    }

    /// <summary>The vertical shutters' own Power Bomb reaction where the definition leaves the blast to them, or the shared one.</summary>
    public override EnemyPowerBombReaction? PowerBombReaction =>
        LeavesPowerBombToSpecies ? species.VerticalShutterPowerBombReaction : base.PowerBombReaction;

    /// <summary>
    /// The shootable and destroyable vertical shutters' shot reactions, which a normal bomb triggers as it does the common one.
    /// </summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects; none where the definition leaves shots to the species.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) => reaction is null && LeavesShotToSpecies;
}
