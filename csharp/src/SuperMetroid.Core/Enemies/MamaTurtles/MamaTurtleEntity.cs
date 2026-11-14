namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>One MamaTurtle occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The MamaTurtle species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class MamaTurtleEntity(MamaTurtleBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <inheritdoc/>
    public void Initialize()
    {
        switch (MamaTurtlePartDefinitions.Parts[Slot.DefinitionId])
        {
            case MamaTurtlePart.MamaTurtle:
                species.InitializeMamaTurtle(Slot);
                break;
            case MamaTurtlePart.BabyTurtle:
                MamaTurtleBehavior.InitializeBabyTurtle(Slot);
                break;
            default:
                throw new InvalidOperationException($"MamaTurtle entity bound to {Slot.DefinitionId}.");
        }
    }

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame)
    {
        switch (MamaTurtlePartDefinitions.Parts[Slot.DefinitionId])
        {
            case MamaTurtlePart.MamaTurtle:
                species.RunMamaTurtleMainAi(Slot, frame);
                break;
            case MamaTurtlePart.BabyTurtle:
                species.RunBabyTurtleMainAi(Slot, frame);
                break;
            default:
                throw new InvalidOperationException($"MamaTurtle entity bound to {Slot.DefinitionId}.");
        }
    }

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context)
    {
        switch (MamaTurtlePartDefinitions.Parts[Slot.DefinitionId])
        {
            case MamaTurtlePart.MamaTurtle:
                species.AdvanceMamaTurtleTypedAnimation(Slot, context);
                break;
            case MamaTurtlePart.BabyTurtle:
                species.AdvanceBabyTurtleTypedAnimation(Slot, context);
                break;
            default:
                throw new InvalidOperationException($"MamaTurtle entity bound to {Slot.DefinitionId}.");
        }
    }

    /// <summary>Whether the turtle's own touch, where its definition leaves Samus's touch to it, or a shared one applies.</summary>
    public override bool ReactsToTouch => LeavesTouchToSpecies || base.ReactsToTouch;

    /// <summary>Runs the mother's or the baby's own touch where the definition leaves Samus's touch to it, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (!LeavesTouchToSpecies)
            base.ReactToTouch(context);
        else if (MamaTurtlePartDefinitions.Parts[Slot.DefinitionId] == MamaTurtlePart.MamaTurtle)
            species.ReactToMamaTurtleTouch(context);
        else
            species.ReactToBabyTurtleTouch(context);
    }

    /// <summary>The baby turtle's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction? ShotReaction => LeavesShotToSpecies ? species.BabyTurtleShotReaction : base.ShotReaction;

    /// <summary>The baby turtle's own stages around common bomb damage, for a bomb that selects its own shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override EnemyBombReaction? CommonBombDamageReaction(EnemyBombContext context) =>
        context.SelectedShot is null ? species.BabyTurtleBombReaction : base.CommonBombDamageReaction(context);

    /// <summary>The baby turtle's own shot reaction, which normal bombs dispatch like the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects; none where the definition leaves shots to the species.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) => reaction is null && LeavesShotToSpecies;
}
