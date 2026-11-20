using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One Rinka occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Rinka species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="settings">What the enemy's population entry tells its species.</param>
internal sealed class RinkaEntity(RinkaBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, RinkaSpawnSettings settings)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <summary>The enemy's own bomb reaction, built over its state (#2554).</summary>
    private EnemyBombReaction OwnRinkaBombReaction => field ??= species.RinkaBombReactionOf(State);

    /// <summary>The enemy's own Power Bomb reaction, built over its state (#2554).</summary>
    private EnemyPowerBombReaction OwnRinkaPowerBombReaction => field ??= species.RinkaPowerBombReactionOf(State);

    /// <summary>The enemy's own shot reaction, built over its state (#2554).</summary>
    private EnemyShotReaction OwnRinkaShotReaction => field ??= species.RinkaShotReactionOf(State);

    /// <summary>The enemy's state, which its initialization builds as the enemy is built (#2554).</summary>
    internal RinkaEnemyState State { get; } = species.InitializeRinka(place, settings);

    /// <inheritdoc/>
    public void Initialize() => species.InitializeRinka(Occupant, State, Place);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => RinkaBehavior.AdvanceRinkaTypedAnimation(Place, State, context);

    /// <summary>Whether Rinka's own touch, where its definition leaves Samus's touch to it, or a shared one applies.</summary>
    public override bool ReactsToTouch => Occupant.Definition.Touch == RinkaReactions.Touch || base.ReactsToTouch;

    /// <summary>Runs Rinka's own touch where its definition leaves Samus's touch to it, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (Occupant.Definition.Touch == RinkaReactions.Touch)
            species.ReactToRinkaTouch(Occupant, context, State);
        else
            base.ReactToTouch(context);
    }

    /// <summary>Rinka's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction? ShotReaction => LeavesShotToSpecies ? OwnRinkaShotReaction : base.ShotReaction;

    /// <summary>Rinka's own stages around common bomb damage for a bomb that selects its own shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override EnemyBombReaction? CommonBombDamageReaction(EnemyBombContext context) =>
        context.SelectedShot is null ? OwnRinkaBombReaction : base.CommonBombDamageReaction(context);

    /// <summary>Rinka's own Power Bomb reaction where its definition leaves the blast to it, or the shared one.</summary>
    public override EnemyPowerBombReaction? PowerBombReaction => LeavesPowerBombToSpecies ? OwnRinkaPowerBombReaction : base.PowerBombReaction;

    /// <summary>Runs a frozen Rinka's own step before the common thaw; true when it ended the turn.</summary>
    public override bool PreemptFrozenTurn() => species.ThawOrDieWhileFrozen(Occupant, State);

    /// <summary>Rinka's own shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects: the species' own where its definition names it.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction reaction) => reaction == RinkaReactions.Shot;
}
