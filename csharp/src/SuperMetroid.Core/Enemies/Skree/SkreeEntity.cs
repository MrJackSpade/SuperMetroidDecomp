namespace SuperMetroid.Core.Enemies.Skree;

/// <summary>One Skree occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Skree species owner.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class SkreeEntity(SkreeBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <summary>The enemy's state, which its initialization builds as the enemy is built (#2554).</summary>
    internal SkreeEnemyState State { get; } = SkreeBehavior.InitializeSkree(place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunSkreeMainAi(Place, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => SkreeBehavior.AdvanceSkreeTypedAnimation(Place, context);

    /// <summary>The Skree's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction? ShotReaction => LeavesShotToSpecies ? species.SkreeShotReaction : base.ShotReaction;

    /// <summary>The Skree's own stages around common bomb damage for a bomb that selects its own shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override EnemyBombReaction? CommonBombDamageReaction(EnemyBombContext context) =>
        context.SelectedShot is null ? species.SkreeBombReaction : base.CommonBombDamageReaction(context);

    /// <summary>The Skree's own shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects; none where the definition leaves shots to the species.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) => reaction is null && LeavesShotToSpecies;
}
