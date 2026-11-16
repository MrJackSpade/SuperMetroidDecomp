namespace SuperMetroid.Core.Enemies.Evir;

/// <summary>One Evir occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Evir species owner.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class EvirEntity(EvirBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemySurroundingsInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <inheritdoc/>
    public void Initialize(EnemySpawnSurroundings surroundings)
    {
        switch (EvirPartDefinitions.Parts[Place.DefinitionId])
        {
            case EvirPart.Evir:
                species.RunEvirInitializationAi(Place, surroundings);
                break;
            case EvirPart.Projectile:
                species.InitializeEvirProjectile(Place);
                break;
            default:
                throw new InvalidOperationException($"Evir entity bound to {Place.DefinitionId}.");
        }
    }

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame)
    {
        switch (EvirPartDefinitions.Parts[Place.DefinitionId])
        {
            case EvirPart.Evir:
                EvirBehavior.RunEvirMainAi(Place, frame);
                break;
            case EvirPart.Projectile:
                EvirBehavior.RunEvirProjectileMainAi(Place, frame);
                break;
            default:
                throw new InvalidOperationException($"Evir entity bound to {Place.DefinitionId}.");
        }
    }

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context)
    {
        switch (EvirPartDefinitions.Parts[Place.DefinitionId])
        {
            case EvirPart.Evir:
                EvirBehavior.AdvanceEvirBodyOrArmsTypedAnimation(Place);
                break;
            case EvirPart.Projectile:
                species.AdvanceEvirSpitTypedAnimation(Place);
                break;
            default:
                throw new InvalidOperationException($"Evir entity bound to {Place.DefinitionId}.");
        }
    }

    /// <summary>
    /// Whether the occupant is an Evir's arms, which share the Evir's definition but take neither its touch nor its shot
    /// reaction; their bombs and Power Bombs still take the Evir's.
    /// </summary>
    private bool IsArms => Place.SpawnSettings<EvirSpawnSettings>().Part == EvirBodyPart.Arms;

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

    /// <summary>The Evir's own shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects; none where the definition leaves shots to the species.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) => Place.DefinitionId == EnemyDefinitionId.Evir && reaction is null && LeavesShotToSpecies;
}
