namespace SuperMetroid.Core.Enemies.Fireflea;

/// <summary>One Fireflea occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Fireflea species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="settings">What the enemy's population entry tells its species.</param>
internal sealed class FirefleaEntity(FirefleaBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, FirefleaSpawnSettings settings)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <summary>The enemy's state, which its initialization builds as the enemy is built (#2554).</summary>
    internal FirefleaEnemyState State { get; } = FirefleaBehavior.InitializeFireflea(Occupant, place, settings);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => FirefleaBehavior.RunFirefleaMain(Place, State);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => FirefleaBehavior.AdvanceFirefleaTypedAnimation(Place, State, context);

    /// <summary>Whether the Fireflea's own touch, where its definition leaves Samus's touch to it, or a shared one applies.</summary>
    public override bool ReactsToTouch => Occupant.Definition.Touch == FirefleaReactions.Touch || base.ReactsToTouch;

    /// <summary>Runs the Fireflea's own touch where its definition leaves Samus's touch to it, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (Occupant.Definition.Touch == FirefleaReactions.Touch)
            species.ReactToFirefleaTouch(context);
        else
            base.ReactToTouch(context);
    }

    /// <summary>The Fireflea's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction ShotReaction => Occupant.Definition.Shot == FirefleaReactions.Shot ? species.FirefleaShotReaction : base.ShotReaction;

    /// <summary>The Fireflea's own stages around common bomb damage for a bomb that selects its own shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override EnemyBombReaction CommonBombDamageReaction(EnemyBombContext context) =>
        context.SelectedShot == FirefleaReactions.Shot ? species.FirefleaBombReaction : base.CommonBombDamageReaction(context);

    /// <summary>The Fireflea's own Power Bomb reaction where its definition leaves the blast to it, or the shared one.</summary>
    public override EnemyPowerBombReaction PowerBombReaction => LeavesPowerBombToSpecies ? species.FirefleaPowerBombReaction : base.PowerBombReaction;

    /// <summary>The Fireflea's own shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects: the species' own where its definition names it.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction reaction) => reaction == FirefleaReactions.Shot;
}
