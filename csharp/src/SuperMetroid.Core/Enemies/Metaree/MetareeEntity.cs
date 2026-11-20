namespace SuperMetroid.Core.Enemies.Metaree;

/// <summary>One Metaree occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Metaree species owner.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class MetareeEntity(MetareeBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <summary>The enemy's state, which its initialization builds as the enemy is built (#2554).</summary>
    internal MetareeEnemyState State { get; } = MetareeBehavior.InitializeMetaree();

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunMetareeMainAi(Place, State, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => MetareeBehavior.AdvanceMetareeTypedAnimation(Place, State, context);

    /// <summary>The Metaree's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction ShotReaction => Occupant.Definition.Shot == MetareeReactions.Shot ? species.MetareeShotReaction : base.ShotReaction;

    /// <summary>The Metaree's own stages around common bomb damage for a bomb that selects its own shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override EnemyBombReaction CommonBombDamageReaction(EnemyBombContext context) =>
        context.SelectedShot == MetareeReactions.Shot ? species.MetareeBombReaction : base.CommonBombDamageReaction(context);

    /// <summary>The Metaree's own shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects; none where the definition leaves shots to the species.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction reaction) => reaction == MetareeReactions.Shot;
}
