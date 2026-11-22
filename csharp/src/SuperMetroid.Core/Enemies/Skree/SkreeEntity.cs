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
    private EnemyShotReaction OwnShot => field ??= SkreeBehavior.SkreeShotReaction(this);

    /// <summary>The Skree's own stages around common bomb damage, built for this occupant.</summary>
    private EnemyBombReaction OwnBomb => field ??= SkreeBehavior.SkreeBombReaction(this);

    /// <summary>Spawns this Skree's debris before a lethal shot's common death.</summary>
    internal void SpawnDebris(EnemyShotHitContext context) => species.SpawnSkreeDebris(Occupant, Ensure.NotNull(context).Enemy);

    /// <summary>Spawns this Skree's debris and starts its death after a bomb's common damage kills it.</summary>
    internal void ResolveBombAfterCommon(EnemyBombHitContext context) => species.ResolveSkreeBombAfterCommon(Occupant, context);

    /// <inheritdoc/>
    public void Initialize() => SkreeBehavior.InitializeSkree(Place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunSkreeMainAi(Occupant, Place, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => SkreeBehavior.AdvanceSkreeTypedAnimation(Place);

    /// <summary>The Skree's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction ShotReaction => Occupant.Definition.Shot == SkreeReactions.Shot ? OwnShot : base.ShotReaction;

    /// <summary>The Skree's own stages around common bomb damage for a bomb that selects its own shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override EnemyBombReaction CommonBombDamageReaction(EnemyBombContext context) =>
        context.SelectedShot == SkreeReactions.Shot ? OwnBomb : base.CommonBombDamageReaction(context);

    /// <summary>The Skree's own shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects: the species' own where its definition names it.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction reaction) => reaction == SkreeReactions.Shot;
}
