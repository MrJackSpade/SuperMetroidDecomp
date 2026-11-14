namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>One Shutters occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Shutters species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class ShuttersEntity(ShuttersBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <inheritdoc/>
    public void Initialize()
    {
        switch (ShuttersPartDefinitions.Parts[Slot.DefinitionId])
        {
            case ShuttersPart.ShutterGrowing:
                ShuttersBehavior.InitializeGrowingShutter(Slot);
                break;
            case ShuttersPart.ShutterShootable or ShuttersPart.ShutterDestroyable or ShuttersPart.Kamer:
                ShuttersBehavior.InitializeVerticalShutter(Slot);
                break;
            default:
                throw new InvalidOperationException($"Shutters entity bound to {Slot.DefinitionId}.");
        }
    }

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame)
    {
        switch (ShuttersPartDefinitions.Parts[Slot.DefinitionId])
        {
            case ShuttersPart.ShutterGrowing:
                species.RunShutterGrowingMainAi(Slot, frame);
                break;
            case ShuttersPart.ShutterShootable or ShuttersPart.ShutterDestroyable or ShuttersPart.Kamer:
                species.RunShutterShootableMainAi(Slot, frame);
                break;
            default:
                throw new InvalidOperationException($"Shutters entity bound to {Slot.DefinitionId}.");
        }
    }

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context)
    {
        switch (ShuttersPartDefinitions.Parts[Slot.DefinitionId])
        {
            case ShuttersPart.ShutterGrowing:
                ShuttersBehavior.AdvanceShutterGrowingTypedAnimation(Slot, context);
                break;
            case ShuttersPart.ShutterShootable or ShuttersPart.ShutterDestroyable or ShuttersPart.Kamer:
                ShuttersBehavior.AdvanceShutterShootableTypedAnimation(Slot, context);
                break;
            default:
                throw new InvalidOperationException($"Shutters entity bound to {Slot.DefinitionId}.");
        }
    }

    /// <summary>Whether the occupant is a shootable vertical shutter (or the Kamer that reuses it) rather than the destroyable one.</summary>
    private bool IsShootable => ShuttersPartDefinitions.Parts[Slot.DefinitionId] is ShuttersPart.ShutterShootable or ShuttersPart.Kamer;

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
        Slot.DefinitionId == EnemyDefinitionId.ShutterDestroyable && context.SelectedShot is null
            ? species.DestroyableShutterBombReaction
            : base.CommonBombDamageReaction(context);

    /// <summary>The vertical shutters' own Power Bomb reaction where the definition leaves the blast to them, or the shared one.</summary>
    public override EnemyPowerBombReaction? PowerBombReaction =>
        LeavesPowerBombToSpecies ? species.VerticalShutterPowerBombReaction : base.PowerBombReaction;

    /// <summary>
    /// The shootable and destroyable vertical shutters' shot reactions, which normal bombs dispatch like the common one.
    /// </summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects; none where the definition leaves shots to the species.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) => reaction is null && LeavesShotToSpecies;
}
