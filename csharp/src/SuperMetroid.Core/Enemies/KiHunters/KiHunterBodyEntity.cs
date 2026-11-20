namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>
/// One Ki-Hunter's body, green, yellow or red (#2146): its own AI, with the shared reactions and its own reaction to the
/// shots and bombs that may strip its wings.
/// </summary>
/// <param name="species">The Ki-Hunter species owner.</param>
/// <param name="place">The place the body occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class KiHunterBodyEntity(KiHunterBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <summary>The enemy's own bomb reaction, built over its state (#2554).</summary>
    private EnemyBombReaction OwnKiHunterBombReaction => field ??= species.KiHunterBombReactionOf(State);

    /// <summary>The enemy's own shot reaction, built over its state (#2554).</summary>
    private EnemyShotReaction OwnKiHunterShotReaction => field ??= species.KiHunterShotReactionOf(State);

    /// <summary>The enemy's state, which its initialization builds as the enemy is built (#2554).</summary>
    internal KiHunterBodyState State { get; } = species.InitializeKiHunter(place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunKiHunterBodyMainAi(Place, State, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => species.AdvanceKiHunterBodyTypedAnimation(Place, State);

    /// <summary>The Ki-Hunter's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction? ShotReaction => LeavesShotToSpecies ? OwnKiHunterShotReaction : base.ShotReaction;

    /// <summary>The Ki-Hunter's own stages around common bomb damage for a bomb that selects its own shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override EnemyBombReaction? CommonBombDamageReaction(EnemyBombContext context) =>
        context.SelectedShot is null ? OwnKiHunterBombReaction : base.CommonBombDamageReaction(context);

    /// <summary>The Ki-Hunter's own shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects; none where the definition leaves shots to the species.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) => reaction is null && LeavesShotToSpecies;
}
