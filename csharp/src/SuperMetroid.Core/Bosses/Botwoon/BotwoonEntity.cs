using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>One Botwoon occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Botwoon species owner.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class BotwoonEntity(BotwoonBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <summary>The enemy's own bomb reaction, built over its state (#2554).</summary>
    private EnemyBombReaction OwnBotwoonBombReaction => field ??= BotwoonBehavior.BotwoonBombReactionOf(State);

    /// <summary>The enemy's own Power Bomb reaction, built over its state (#2554).</summary>
    private EnemyPowerBombReaction OwnBotwoonPowerBombReaction => field ??= BotwoonBehavior.BotwoonPowerBombReactionOf(State);

    /// <summary>The enemy's own shot reaction, built over its state (#2554).</summary>
    private EnemyShotReaction OwnBotwoonShotReaction => field ??= BotwoonBehavior.BotwoonShotReactionOf(State);

    /// <summary>The enemy's state, which its initialization builds as the enemy is built (#2554).</summary>
    internal BotwoonEnemyState State { get; } = species.InitializeBotwoon(place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunBotwoonMainAi(Place, State, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => species.AdvanceBotwoonHeadAnimation(Place, State);

    /// <summary>Whether Botwoon's own touch, where its definition leaves Samus's touch to it, or a shared one applies.</summary>
    public override bool ReactsToTouch => LeavesTouchToSpecies || base.ReactsToTouch;

    /// <summary>Runs Botwoon's own touch where its definition leaves Samus's touch to it, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (LeavesTouchToSpecies)
            species.ReactToBotwoonTouch(context, State);
        else
            base.ReactToTouch(context);
    }

    /// <summary>Botwoon's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction? ShotReaction => LeavesShotToSpecies ? OwnBotwoonShotReaction : base.ShotReaction;

    /// <summary>Botwoon's own stages around common bomb damage for a bomb that selects its own shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override EnemyBombReaction? CommonBombDamageReaction(EnemyBombContext context) =>
        context.SelectedShot is null ? OwnBotwoonBombReaction : base.CommonBombDamageReaction(context);

    /// <summary>Botwoon's own Power Bomb reaction where its definition leaves the blast to it, or the shared one.</summary>
    public override EnemyPowerBombReaction? PowerBombReaction => LeavesPowerBombToSpecies ? OwnBotwoonPowerBombReaction : base.PowerBombReaction;

    /// <summary>Botwoon's own shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects; none where the definition leaves shots to the species.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) => reaction is null && LeavesShotToSpecies;
}
