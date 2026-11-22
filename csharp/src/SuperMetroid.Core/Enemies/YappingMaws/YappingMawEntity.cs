namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>One YappingMaw occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The YappingMaw species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="settings">What the enemy's population entry tells its species.</param>
internal sealed partial class YappingMawEntity(YappingMawBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, YappingMawSpawnSettings settings)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <summary>The species' YappingMawShotReaction, built once for this enemy with its state (#2554).</summary>
    private EnemyShotReaction OwnYappingMawShotReaction => field ??= YappingMawBehavior.YappingMawShotReaction(FinishHit);

    /// <summary>The species' YappingMawBombReaction, built once for this enemy with its state (#2554).</summary>
    private EnemyBombReaction OwnYappingMawBombReaction => field ??= YappingMawBehavior.YappingMawBombReaction(FinishHit);

    /// <summary>What the enemy's population entry tells its species, which its factory checked as it built the enemy.</summary>
    internal YappingMawSpawnSettings Settings { get; } = settings;

    /// <summary>The enemy's state, which its setup builds as the enemy is built (#2554).</summary>
    private readonly YappingMawEnemyState state = species.InitializeYappingMaw(occupant, settings, place);

    /// <summary>Releases the Maw's captive when a shot froze it, or cleans up its death, after the common damage.</summary>
    /// <param name="context">The shot's hit.</param>
    internal void FinishHit(EnemyShotHitContext context) =>
        YappingMawBehavior.ResolveYappingMawShotAfterCommon(Ensure.NotNull(context).Enemy, state, context.Samus);

    /// <summary>Releases the Maw's captive when a bomb froze it, or cleans up its death, after the common damage.</summary>
    /// <param name="context">The bomb's hit.</param>
    internal void FinishHit(EnemyBombHitContext context) =>
        YappingMawBehavior.ResolveYappingMawShotAfterCommon(Ensure.NotNull(context).Enemy, state, context.Samus);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunYappingMawMainAi(Settings, Place, state, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => species.AdvanceYappingMawTypedAnimation(Place, state);

    /// <summary>Whether the Yapping Maw's own touch, where its definition leaves Samus's touch to it, or a shared one applies.</summary>
    public override bool ReactsToTouch => Occupant.Definition.Touch == YappingMawReactions.Touch || base.ReactsToTouch;

    /// <summary>Runs the Yapping Maw's own touch where its definition leaves Samus's touch to it, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (Occupant.Definition.Touch == YappingMawReactions.Touch)
            YappingMawBehavior.ReactToYappingMawTouch(context, state);
        else
            base.ReactToTouch(context);
    }

    /// <summary>The Yapping Maw's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction ShotReaction => Occupant.Definition.Shot == YappingMawReactions.Shot ? OwnYappingMawShotReaction : base.ShotReaction;

    /// <summary>The Yapping Maw's own stages around common bomb damage for a bomb that selects its own shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override EnemyBombReaction CommonBombDamageReaction(EnemyBombContext context) =>
        context.SelectedShot == YappingMawReactions.Shot ? OwnYappingMawBombReaction : base.CommonBombDamageReaction(context);

    /// <summary>Runs the Yapping Maw's frozen behaviour after the common thaw.</summary>
    public override void RunFrozen() => YappingMawBehavior.RunYappingMawFrozen(Place, state);

    /// <summary>The Yapping Maw's own shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects: the species' own where its definition names it.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction reaction) => reaction == YappingMawReactions.Shot;
}
