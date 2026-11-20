using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One Zebetite occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Zebetite species owner.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="setup">How the half is set up: a linked half joins its primary.</param>
/// <param name="settings">What the enemy's population entry tells its species.</param>
internal sealed class ZebetiteEntity(ZebetiteBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, EnemySetup setup, ZebetiteSpawnSettings settings)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <summary>The enemy's own bomb reaction, built over its state (#2554).</summary>
    private EnemyBombReaction OwnZebetiteBombReaction => field ??= species.ZebetiteBombReactionOf(State);

    /// <summary>The enemy's own shot reaction, built over its state (#2554).</summary>
    private EnemyShotReaction OwnZebetiteShotReaction => field ??= species.ZebetiteShotReactionOf(State);

    /// <summary>The enemy's state, which its initialization builds as the enemy is built (#2554).</summary>
    internal ZebetiteEnemyState State { get; } = species.InitializeZebetite(place, setup, settings);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunZebetiteMain(Place, State);

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
    /// <param name="reaction">The reaction the radius path or a hitbox selects; none where the definition leaves shots to the species.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction reaction) => reaction == ZebetiteReactions.Shot;
}
