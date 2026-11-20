namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>One YappingMaw occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The YappingMaw species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class YappingMawEntity(YappingMawBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyInitialization, IEnemyAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <inheritdoc/>
    public void Initialize() => species.InitializeYappingMaw(Occupant, Place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunYappingMawMainAi(Occupant, Place, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => species.AdvanceYappingMawTypedAnimation(Place);

    /// <summary>Whether the Yapping Maw's own touch, where its definition leaves Samus's touch to it, or a shared one applies.</summary>
    public override bool ReactsToTouch => Place.Definition.Touch == YappingMawReactions.Touch || base.ReactsToTouch;

    /// <summary>Runs the Yapping Maw's own touch where its definition leaves Samus's touch to it, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (Place.Definition.Touch == YappingMawReactions.Touch)
            YappingMawBehavior.ReactToYappingMawTouch(context);
        else
            base.ReactToTouch(context);
    }

    /// <summary>The Yapping Maw's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction ShotReaction => Place.Definition.Shot == YappingMawReactions.Shot ? YappingMawBehavior.YappingMawShotReaction : base.ShotReaction;

    /// <summary>The Yapping Maw's own stages around common bomb damage for a bomb that selects its own shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override EnemyBombReaction CommonBombDamageReaction(EnemyBombContext context) =>
        context.SelectedShot == YappingMawReactions.Shot ? YappingMawBehavior.YappingMawBombReaction : base.CommonBombDamageReaction(context);

    /// <summary>Runs the Yapping Maw's frozen behaviour after the common thaw.</summary>
    public override void RunFrozen() => YappingMawBehavior.RunYappingMawFrozen(Place, Place.RequireSpeciesState<YappingMawEnemyState>());

    /// <summary>The Yapping Maw's own shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects: the species' own where its definition names it.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction reaction) => reaction == YappingMawReactions.Shot;
}
