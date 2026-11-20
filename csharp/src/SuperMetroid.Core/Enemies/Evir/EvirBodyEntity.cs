namespace SuperMetroid.Core.Enemies.Evir;

/// <summary>
/// An Evir's body (#2146): it bobs and turns toward Samus, and runs its own touch, shot, bomb and Power Bomb reactions
/// where its definition leaves them to it, each followed by the Evir's follow-up to its arms and spit.
/// </summary>
/// <param name="species">The Evir species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the body occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class EvirBodyEntity(EvirBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemySurroundingsInitialization, IEnemyAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <inheritdoc/>
    public void Initialize(EnemyPopulationSurroundings surroundings) => species.InitializeEvirBody(Occupant, Place, surroundings.Samus);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => EvirBehavior.RunEvirBodyMainAi(Occupant, Place, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => EvirBehavior.AdvanceEvirPartTypedAnimation<EvirBodyState>(Place);

    /// <summary>Whether the Evir's own touch, where its definition leaves Samus's touch to it, or a shared one applies.</summary>
    public override bool ReactsToTouch => LeavesTouchToSpecies || base.ReactsToTouch;

    /// <summary>Runs the Evir's own touch where its definition leaves Samus's touch to it, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (LeavesTouchToSpecies)
            species.ReactToEvirTouch(context);
        else
            base.ReactToTouch(context);
    }

    /// <summary>The Evir's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction ShotReaction => LeavesShotToSpecies ? species.EvirShotReaction : base.ShotReaction;

    /// <summary>The Evir's own stages around common bomb damage for a bomb that selects its own shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override EnemyBombReaction CommonBombDamageReaction(EnemyBombContext context) =>
        context.SelectedShot is null ? species.EvirBombReaction : base.CommonBombDamageReaction(context);

    /// <summary>The Evir's own Power Bomb reaction where its definition leaves the blast to it, or the shared one.</summary>
    public override EnemyPowerBombReaction PowerBombReaction => LeavesPowerBombToSpecies ? species.EvirPowerBombReaction : base.PowerBombReaction;

    /// <summary>The Evir's own shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects; none where the definition leaves shots to the species.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) => reaction is null && LeavesShotToSpecies;
}
