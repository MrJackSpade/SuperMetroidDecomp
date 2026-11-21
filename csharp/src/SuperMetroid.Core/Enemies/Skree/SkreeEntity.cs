namespace SuperMetroid.Core.Enemies.Skree;

/// <summary>One Skree occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Skree species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed partial class SkreeEntity(SkreeBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyInitialization, IEnemyAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <summary>The Skree's own shot reaction, built for this occupant.</summary>
    private readonly EnemyShotReaction ownShot = species.SkreeShotReaction(occupant);

    /// <summary>The Skree's own stages around common bomb damage, built for this occupant.</summary>
    private readonly EnemyBombReaction ownBomb = species.SkreeBombReaction(occupant);

    /// <inheritdoc/>
    public void Initialize() => SkreeBehavior.InitializeSkree(Place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunSkreeMainAi(Occupant, Place, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => SkreeBehavior.AdvanceSkreeTypedAnimation(Place);

    /// <summary>The Skree's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction ShotReaction => Occupant.Definition.Shot == SkreeReactions.Shot ? ownShot : base.ShotReaction;

    /// <summary>The Skree's own stages around common bomb damage for a bomb that selects its own shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override EnemyBombReaction CommonBombDamageReaction(EnemyBombContext context) =>
        context.SelectedShot == SkreeReactions.Shot ? ownBomb : base.CommonBombDamageReaction(context);

    /// <summary>The Skree's own shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects: the species' own where its definition names it.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction reaction) => reaction == SkreeReactions.Shot;
}
