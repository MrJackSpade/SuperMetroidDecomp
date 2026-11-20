namespace SuperMetroid.Core.Enemies.Evir;

/// <summary>
/// An Evir's body (#2146): it bobs and turns toward Samus, and runs its own touch, shot, bomb and Power Bomb reactions
/// where its definition leaves them to it, each followed by the Evir's follow-up to its arms and spit.
/// </summary>
/// <param name="species">The Evir species owner.</param>
/// <param name="place">The place the body occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="setup">How the enemy comes to be set up, whose surroundings its initialization reads.</param>
/// <param name="settings">What the enemy's population entry tells its species.</param>
internal sealed class EvirBodyEntity(EvirBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common, EnemySetup setup, EvirSpawnSettings settings)
    : CommonParticipationEnemyEntity(place, common), IEnemyAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <summary>The enemy's state, which its initialization builds as the enemy is built (#2554).</summary>
    internal EvirBodyState State { get; } = species.InitializeEvirBody(place, setup.SurroundingsOf(place).Samus, settings);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => EvirBehavior.RunEvirBodyMainAi(Place, State, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => EvirBehavior.AdvanceEvirPartTypedAnimation(Place, State);

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
    public override EnemyShotReaction? ShotReaction => LeavesShotToSpecies ? species.EvirShotReaction : base.ShotReaction;

    /// <summary>The Evir's own stages around common bomb damage for a bomb that selects its own shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override EnemyBombReaction? CommonBombDamageReaction(EnemyBombContext context) =>
        context.SelectedShot is null ? species.EvirBombReaction : base.CommonBombDamageReaction(context);

    /// <summary>The Evir's own Power Bomb reaction where its definition leaves the blast to it, or the shared one.</summary>
    public override EnemyPowerBombReaction? PowerBombReaction => LeavesPowerBombToSpecies ? species.EvirPowerBombReaction : base.PowerBombReaction;

    /// <summary>The Evir's own shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects; none where the definition leaves shots to the species.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) => reaction is null && LeavesShotToSpecies;
}
