namespace SuperMetroid.Core.Enemies.Beetom;

/// <summary>One Beetom occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Beetom species owner.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class BeetomEntity(BeetomBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemySurroundingsInitialization, IEnemyAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <inheritdoc/>
    public void Initialize(EnemySpawnSurroundings surroundings) => species.RunBeetomInitializationAi(Place, surroundings);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunBeetomMainAi(Place, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => BeetomBehavior.AdvanceBeetomTypedAnimation(Place, context);

    /// <summary>Whether the Beetom's own touch, where its definition leaves Samus's touch to it, or a shared one applies.</summary>
    public override bool ReactsToTouch => LeavesTouchToSpecies || base.ReactsToTouch;

    /// <summary>Runs the Beetom's own touch where its definition leaves Samus's touch to it, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (LeavesTouchToSpecies)
            species.ReactToBeetomTouch(context);
        else
            base.ReactToTouch(context);
    }

    /// <summary>The Beetom's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction? ShotReaction => LeavesShotToSpecies ? BeetomBehavior.BeetomShotReaction : base.ShotReaction;

    /// <summary>The Beetom's own stages around common bomb damage for a bomb that selects its own shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override EnemyBombReaction? CommonBombDamageReaction(EnemyBombContext context) =>
        LeavesShotToSpecies && context.SelectedShot is null ? BeetomBehavior.BeetomBombReaction : base.CommonBombDamageReaction(context);

    /// <summary>The Beetom's own shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects; none where the definition leaves shots to the species.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) => reaction is null && LeavesShotToSpecies;
}
