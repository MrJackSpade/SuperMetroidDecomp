using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One Zebetite occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Zebetite species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="linkedTo">The primary half a linked half is spawned linked to; null for a primary half.</param>
internal sealed partial class ZebetiteEntity(ZebetiteBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
    RoomEnemyPlace? linkedTo)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <summary>The constructor's <c>linkedTo</c>, kept so a debugger state rebuilds the object through it (#2602).</summary>
    private readonly RoomEnemyPlace? linkedTo = linkedTo;

    /// <summary>The species' ZebetiteShotReaction, built once for this enemy with its state (#2554).</summary>
    private EnemyShotReaction OwnZebetiteShotReaction => field ??= ZebetiteBehavior.ZebetiteShotReaction(ShareDamage);

    /// <summary>The species' ZebetiteBombReaction, built once for this enemy with its state (#2554).</summary>
    private EnemyBombReaction OwnZebetiteBombReaction => field ??= ZebetiteBehavior.ZebetiteBombReaction(ShareDamage);

    /// <summary>The enemy's state, which its setup builds as the enemy is built (#2554).</summary>
    internal ZebetiteEnemyState State { get; } = species.InitializeZebetite(occupant, place, linkedTo);

    /// <summary>Shares a shot's common damage across the linked Zebetites' health.</summary>
    /// <param name="context">The shot's hit.</param>
    internal void ShareDamage(EnemyShotHitContext context) =>
        species.ResolveZebetiteShotAfterCommon(Ensure.NotNull(context).Enemy, State);

    /// <summary>Shares a bomb's common damage across the linked Zebetites' health.</summary>
    /// <param name="context">The bomb's hit.</param>
    internal void ShareDamage(EnemyBombHitContext context) =>
        species.ResolveZebetiteShotAfterCommon(Ensure.NotNull(context).Enemy, State);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunZebetiteMainAi(Occupant, State);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => ZebetiteBehavior.AdvanceZebetiteAnimation(Place, State);

    /// <summary>Whether the Zebetite's own touch, where its definition leaves Samus's touch to it, or a shared one applies.</summary>
    public override bool ReactsToTouch => Occupant.Definition.Touch == ZebetiteReactions.Touch || base.ReactsToTouch;

    /// <summary>Runs the Zebetite's own touch where its definition leaves Samus's touch to it, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (Occupant.Definition.Touch == ZebetiteReactions.Touch)
            species.ReactToZebetiteTouch(context);
        else
            base.ReactToTouch(context);
    }

    /// <summary>The Zebetite's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction ShotReaction => Occupant.Definition.Shot == ZebetiteReactions.Shot ? OwnZebetiteShotReaction : base.ShotReaction;

    /// <summary>The Zebetite's own stages around common bomb damage for a bomb that selects its own shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override EnemyBombReaction CommonBombDamageReaction(EnemyBombContext context) =>
        context.SelectedShot == ZebetiteReactions.Shot ? OwnZebetiteBombReaction : base.CommonBombDamageReaction(context);

    /// <summary>The Zebetite's own shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects: the species' own where its definition names it.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction reaction) => reaction == ZebetiteReactions.Shot;
}
