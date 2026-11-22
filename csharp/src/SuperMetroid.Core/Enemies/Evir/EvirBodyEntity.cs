namespace SuperMetroid.Core.Enemies.Evir;

/// <summary>
/// An Evir's body (#2146): it bobs and turns toward Samus, and runs its own touch, shot, bomb and Power Bomb reactions
/// where its definition leaves them to it, each followed by the Evir's follow-up to its arms and spit.
/// </summary>
/// <param name="species">The Evir species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the body occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="surroundings">The room, Samus, controller and camera the population load sets the enemy up in.</param>
/// <param name="settings">What the enemy's population entry tells its species.</param>
internal sealed partial class EvirBodyEntity(EvirBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, EnemyPopulationSurroundings surroundings, EvirSpawnSettings settings)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <summary>The constructor's <c>surroundings</c>, kept so a debugger state rebuilds the object through it (#2602).</summary>
    private readonly EnemyPopulationSurroundings surroundings = surroundings;

    /// <summary>What the enemy's population entry tells its species, which its factory checked as it built the enemy.</summary>
    internal EvirSpawnSettings Settings { get; } = settings;

    /// <summary>The enemy's state, which its setup builds as the enemy is built (#2554).</summary>
    internal EvirBodyState State { get; } = species.InitializeEvirBody(settings, place, surroundings.Samus);

    /// <summary>The two enemies the part's follow-up to a hit reaches, the ones its room lists after it, found as it is built.</summary>
    private IReadOnlyList<RoomEnemyPlace> FollowUpRecipients { get; } = species.FollowUpRecipientsOf(place);

    /// <summary>The part's own bomb stages, which carry its hit to the enemies its follow-up reaches.</summary>
    private EnemyBombReaction OwnBomb => field ??= EvirBehavior.EvirBombReaction(PassHitOn);

    /// <summary>The part's own Power Bomb reaction, which carries its hit to the enemies its follow-up reaches.</summary>
    private EnemyPowerBombReaction OwnPowerBomb => field ??= EvirBehavior.EvirPowerBombReaction(PassHitOn);

    /// <summary>The body's own shot reaction, which carries its hit to the enemies its follow-up reaches.</summary>
    private EnemyShotReaction OwnShot => field ??= EvirBehavior.EvirShotReaction(PassHitOn);

    /// <summary>Carries the part's shot hit on to the enemies its follow-up reaches, after the common damage.</summary>
    /// <param name="context">The shot's hit.</param>
    internal void PassHitOn(EnemyShotHitContext context) =>
        EvirBehavior.ResolveEvirCombatAfterCommon(Ensure.NotNull(context).Enemy, FollowUpRecipients);

    /// <summary>Carries the part's bomb hit on to the enemies its follow-up reaches, after the common damage.</summary>
    /// <param name="context">The bomb's hit.</param>
    internal void PassHitOn(EnemyBombHitContext context) =>
        EvirBehavior.ResolveEvirCombatAfterCommon(Ensure.NotNull(context).Enemy, FollowUpRecipients);

    /// <summary>Carries the part's Power Bomb hit on to the enemies its follow-up reaches, after the common damage.</summary>
    /// <param name="context">The blast meeting the enemy.</param>
    internal void PassHitOn(EnemyPowerBombContext context) =>
        EvirBehavior.ResolveEvirCombatAfterCommon(context.Enemy, FollowUpRecipients);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => EvirBehavior.RunEvirBodyMainAi(Settings, Place, State, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => EvirBehavior.AdvanceEvirPartTypedAnimation<EvirBodyState>(Place, State);

    /// <summary>Whether the Evir's own touch, where its definition leaves Samus's touch to it, or a shared one applies.</summary>
    public override bool ReactsToTouch => Occupant.Definition.Touch == EvirReactions.Touch || base.ReactsToTouch;

    /// <summary>Runs the Evir's own touch where its definition leaves Samus's touch to it, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (Occupant.Definition.Touch == EvirReactions.Touch)
            species.ReactToEvirTouch(context, FollowUpRecipients);
        else
            base.ReactToTouch(context);
    }

    /// <summary>The Evir's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction ShotReaction => Occupant.Definition.Shot == EvirReactions.Shot ? OwnShot : base.ShotReaction;

    /// <summary>The Evir's own stages around common bomb damage for a bomb that selects its own shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override EnemyBombReaction CommonBombDamageReaction(EnemyBombContext context) =>
        context.SelectedShot == EvirReactions.Shot ? OwnBomb : base.CommonBombDamageReaction(context);

    /// <summary>The Evir's own Power Bomb reaction where its definition leaves the blast to it, or the shared one.</summary>
    public override EnemyPowerBombReaction PowerBombReaction => LeavesPowerBombToSpecies ? OwnPowerBomb : base.PowerBombReaction;

    /// <summary>The Evir's own shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects: the species' own where its definition names it.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction reaction) => reaction == EvirReactions.Shot;
}
