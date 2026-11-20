namespace SuperMetroid.Core.Enemies.Dragon;

/// <summary>
/// A Dragon's cosmetic wing (#2175), which shares the Dragon's definition: its placement names it the wing of the body its
/// room lists right before it, which moves it and starts its flapping. It has no AI of its own, and keeps the Dragon's
/// reactions where its definition leaves them to it; its intangibility normally keeps Samus and her shots from reaching it.
/// </summary>
/// <param name="species">The Dragon species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the wing occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class DragonWingEntity(DragonBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyInitialization, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <inheritdoc/>
    public void Initialize() => species.InitializeDragonWing(Place);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => DragonBehavior.AdvanceDragonWingTypedAnimation(Place);

    /// <summary>Whether the Dragon's own touch, where its definition leaves Samus's touch to it, or a shared one applies.</summary>
    public override bool ReactsToTouch => LeavesTouchToSpecies || base.ReactsToTouch;

    /// <summary>Runs the Dragon's own touch where its definition leaves Samus's touch to it, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (LeavesTouchToSpecies)
            species.ReactToDragonTouch(context);
        else
            base.ReactToTouch(context);
    }

    /// <summary>The Dragon's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction? ShotReaction => LeavesShotToSpecies ? species.DragonShotReaction : base.ShotReaction;

    /// <summary>The Dragon's own stages around common bomb damage for a bomb that selects its own shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override EnemyBombReaction? CommonBombDamageReaction(EnemyBombContext context) =>
        context.SelectedShot is null ? species.DragonBombReaction : base.CommonBombDamageReaction(context);

    /// <summary>The Dragon's own Power Bomb reaction where its definition leaves the blast to it, or the shared one.</summary>
    public override EnemyPowerBombReaction? PowerBombReaction => LeavesPowerBombToSpecies ? species.DragonPowerBombReaction : base.PowerBombReaction;

    /// <summary>The Dragon's own shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects; none where the definition leaves shots to the species.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) => reaction is null && LeavesShotToSpecies;
}
