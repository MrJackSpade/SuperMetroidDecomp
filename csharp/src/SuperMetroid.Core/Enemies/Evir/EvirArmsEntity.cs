using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Evir;

/// <summary>
/// An Evir's arms (#2146), which share the Evir's definition and follow its body. They take neither its touch nor its
/// shot reaction, but their bombs and Power Bombs still take the Evir's, followed by the follow-up to the enemies after
/// them.
/// </summary>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the arms occupy.</param>
/// <param name="common">The shared participation.</param>
/// <param name="state">What the enemy's setup built (#2554).</param>
/// <param name="followUpRecipients">What the enemy's setup built (#2554).</param>
[RoomLifetime]
internal sealed partial class EvirArmsEntity(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, EvirArmsState state,
    IReadOnlyList<RoomEnemyPlace> followUpRecipients)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <summary>
    /// Builds the enemy, running its setup as the game builds it (#2554). A restore of the game's state builds it through the
    /// primary constructor from what it holds, so the setup never runs again.
    /// </summary>
    /// <param name="species">The Evir species owner.</param>
    /// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
    /// <param name="place">The place the arms occupy.</param>
    /// <param name="common">The shared participation.</param>
    /// <param name="body">The place of the body the room lists before the arms, which offered them their creation (#2554).</param>
    internal EvirArmsEntity(EvirBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
        RoomEnemyPlace body)
        : this(occupant, place, common, species.InitializeEvirArms(place, body), species.FollowUpRecipientsOf(place))
    {
    }

    /// <summary>The enemy's state, which its setup builds as the enemy is built (#2554).</summary>
    private readonly EvirArmsState state = state;

    /// <summary>The two enemies the part's follow-up to a hit reaches, the ones its room lists after it, found as it is built.</summary>
    private IReadOnlyList<RoomEnemyPlace> FollowUpRecipients { get; } = followUpRecipients;

    /// <summary>The part's own bomb stages, which carry its hit to the enemies its follow-up reaches.</summary>
    private EnemyBombReaction OwnBomb => field ??= EvirBehavior.EvirBombReaction(PassHitOn);

    /// <summary>The part's own Power Bomb reaction, which carries its hit to the enemies its follow-up reaches.</summary>
    private EnemyPowerBombReaction OwnPowerBomb => field ??= EvirBehavior.EvirPowerBombReaction(PassHitOn);

    /// <summary>Carries the part's bomb hit on to the enemies its follow-up reaches, after the common damage.</summary>
    /// <param name="context">The bomb's hit.</param>
    internal void PassHitOn(EnemyBombHitContext context) =>
        EvirBehavior.ResolveEvirCombatAfterCommon(Ensure.NotNull(context).Enemy, FollowUpRecipients);

    /// <summary>Carries the part's Power Bomb hit on to the enemies its follow-up reaches, after the common damage.</summary>
    /// <param name="context">The blast meeting the enemy.</param>
    internal void PassHitOn(EnemyPowerBombContext context) =>
        EvirBehavior.ResolveEvirCombatAfterCommon(context.Enemy, FollowUpRecipients);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => EvirBehavior.RunEvirArmsMainAi(Place, state);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => EvirBehavior.AdvanceEvirPartTypedAnimation<EvirArmsState>(Place, state);

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
