namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>One Shutters occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Shutters species owner.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class ShuttersEntity(ShuttersBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <inheritdoc/>
    public void Initialize()
    {
        switch (ShuttersPartDefinitions.Parts[Place.DefinitionId])
        {
            case ShuttersPart.ShutterGrowing:
                ShuttersBehavior.InitializeGrowingShutter(Place);
                break;
            case ShuttersPart.ShutterShootable or ShuttersPart.ShutterDestroyable or ShuttersPart.Kamer:
                ShuttersBehavior.InitializeVerticalShutter(Place);
                break;
            default:
                throw new InvalidOperationException($"Shutters entity bound to {Place.DefinitionId}.");
        }
    }

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame)
    {
        switch (ShuttersPartDefinitions.Parts[Place.DefinitionId])
        {
            case ShuttersPart.ShutterGrowing:
                species.RunShutterGrowingMainAi(Place, frame);
                break;
            case ShuttersPart.ShutterShootable or ShuttersPart.ShutterDestroyable or ShuttersPart.Kamer:
                species.RunShutterShootableMainAi(Place, frame);
                break;
            default:
                throw new InvalidOperationException($"Shutters entity bound to {Place.DefinitionId}.");
        }
    }

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context)
    {
        switch (ShuttersPartDefinitions.Parts[Place.DefinitionId])
        {
            case ShuttersPart.ShutterGrowing:
                ShuttersBehavior.AdvanceShutterGrowingTypedAnimation(Place, context);
                break;
            case ShuttersPart.ShutterShootable or ShuttersPart.ShutterDestroyable or ShuttersPart.Kamer:
                ShuttersBehavior.AdvanceShutterShootableTypedAnimation(Place, context);
                break;
            default:
                throw new InvalidOperationException($"Shutters entity bound to {Place.DefinitionId}.");
        }
    }

    /// <summary>Whether the occupant is a shootable vertical shutter (or the Kamer that reuses it) rather than the destroyable one.</summary>
    private bool IsShootable => ShuttersPartDefinitions.Parts[Place.DefinitionId] is ShuttersPart.ShutterShootable or ShuttersPart.Kamer;

    /// <summary>Whether the vertical shutter's own touch, where its definition leaves Samus's touch to it, or a shared one applies.</summary>
    public override bool ReactsToTouch => LeavesTouchToSpecies || base.ReactsToTouch;

    /// <summary>Runs the vertical shutter's own touch where its definition leaves Samus's touch to it, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (LeavesTouchToSpecies)
            species.ReactToVerticalShutterTouch(context);
        else
            base.ReactToTouch(context);
    }

    /// <summary>The shootable or destroyable shutter's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction? ShotReaction =>
        !LeavesShotToSpecies ? base.ShotReaction
        : IsShootable ? species.ShootableShutterShotReaction
        : species.DestroyableShutterShotReaction;

    /// <summary>
    /// Runs the shootable shutter's own reaction to a bomb that selects its own shot reaction, before the ignoring check.
    /// </summary>
    /// <param name="context">The bomb contact.</param>
    public override bool TryReactToBomb(EnemyBombContext context)
    {
        if (!IsShootable || context.SelectedShot is not null)
            return base.TryReactToBomb(context);
        species.ReactToShootableShutterBomb(context);
        return true;
    }

    /// <summary>The destroyable shutter's own stages around common bomb damage, for a bomb that selects its own shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override EnemyBombReaction? CommonBombDamageReaction(EnemyBombContext context) =>
        Place.DefinitionId == EnemyDefinitionId.ShutterDestroyable && context.SelectedShot is null
            ? species.DestroyableShutterBombReaction
            : base.CommonBombDamageReaction(context);

    /// <summary>The vertical shutters' own Power Bomb reaction where the definition leaves the blast to them, or the shared one.</summary>
    public override EnemyPowerBombReaction? PowerBombReaction =>
        LeavesPowerBombToSpecies ? species.VerticalShutterPowerBombReaction : base.PowerBombReaction;

    /// <summary>
    /// The shootable and destroyable vertical shutters' shot reactions, which a normal bomb triggers as it does the common one.
    /// </summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects; none where the definition leaves shots to the species.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) => reaction is null && LeavesShotToSpecies;
}
