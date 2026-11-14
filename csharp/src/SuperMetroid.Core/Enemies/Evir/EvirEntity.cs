namespace SuperMetroid.Core.Enemies.Evir;

/// <summary>One Evir occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Evir species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class EvirEntity(EvirBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemySurroundingsInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <inheritdoc/>
    public void Initialize(EnemySpawnSurroundings surroundings)
    {
        switch (EvirPartDefinitions.Parts[Slot.DefinitionId])
        {
            case EvirPart.Evir:
                species.RunEvirInitializationAi(Slot, surroundings);
                break;
            case EvirPart.Projectile:
                species.InitializeEvirProjectile(Slot);
                break;
            default:
                throw new InvalidOperationException($"Evir entity bound to {Slot.DefinitionId}.");
        }
    }

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame)
    {
        switch (EvirPartDefinitions.Parts[Slot.DefinitionId])
        {
            case EvirPart.Evir:
                species.RunEvirMainAi(Slot, frame);
                break;
            case EvirPart.Projectile:
                species.RunEvirProjectileMainAi(Slot, frame);
                break;
            default:
                throw new InvalidOperationException($"Evir entity bound to {Slot.DefinitionId}.");
        }
    }

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => species.AdvanceEvirTypedAnimation(Slot, context);

    /// <summary>
    /// Whether the occupant is an Evir's arms, which share the Evir's definition but take neither its touch nor its shot
    /// reaction; their bombs and Power Bombs still take the Evir's.
    /// </summary>
    private bool IsArms => Slot.SpawnSettings<EvirSpawnSettings>().Part == EvirBodyPart.Arms;

    /// <summary>Whether the Evir's own touch, where its definition leaves Samus's touch to it, or a shared one applies.</summary>
    public override bool ReactsToTouch => (LeavesTouchToSpecies && !IsArms) || base.ReactsToTouch;

    /// <summary>Runs the Evir's own touch where its definition leaves Samus's touch to it, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (LeavesTouchToSpecies && !IsArms)
            species.ReactToEvirTouch(context);
        else
            base.ReactToTouch(context);
    }

    /// <summary>The Evir's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction? ShotReaction => (LeavesShotToSpecies && !IsArms) ? species.EvirShotReaction : base.ShotReaction;

    /// <summary>The Evir's own stages around common bomb damage for a bomb that selects its own shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override EnemyBombReaction? CommonBombDamageReaction(EnemyBombContext context) =>
        context.SelectedShot is null ? species.EvirBombReaction : base.CommonBombDamageReaction(context);

    /// <summary>The Evir's own Power Bomb reaction where its definition leaves the blast to it, or the shared one.</summary>
    public override EnemyPowerBombReaction? PowerBombReaction => LeavesPowerBombToSpecies ? species.EvirPowerBombReaction : base.PowerBombReaction;

    /// <summary>The Evir's own shot reaction, which normal bombs dispatch like the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects; none where the definition leaves shots to the species.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) => Slot.DefinitionId == EnemyDefinitionId.Evir && reaction is null && LeavesShotToSpecies;
}
