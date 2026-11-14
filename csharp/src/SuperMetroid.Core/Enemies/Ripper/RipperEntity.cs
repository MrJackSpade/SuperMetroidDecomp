namespace SuperMetroid.Core.Enemies.Ripper;

/// <summary>One Ripper occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Ripper species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class RipperEntity(RipperBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <inheritdoc/>
    public void Initialize()
    {
        switch (RipperPartDefinitions.Parts[Slot.DefinitionId])
        {
            case RipperPart.GRipper:
                RipperBehavior.InitializeGRipper(Slot);
                break;
            case RipperPart.Ripper2:
                RipperBehavior.InitializeRipper2(Slot);
                break;
            case RipperPart.Ripper:
                RipperBehavior.InitializeRipper(Slot);
                break;
            default:
                throw new InvalidOperationException($"Ripper entity bound to {Slot.DefinitionId}.");
        }
    }

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame)
    {
        switch (RipperPartDefinitions.Parts[Slot.DefinitionId])
        {
            case RipperPart.GRipper:
                species.RunGRipperMainAi(Slot, frame);
                break;
            case RipperPart.Ripper2:
                species.RunRipper2MainAi(Slot, frame);
                break;
            case RipperPart.Ripper:
                species.RunRipperMainAi(Slot, frame);
                break;
            default:
                throw new InvalidOperationException($"Ripper entity bound to {Slot.DefinitionId}.");
        }
    }

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => RipperBehavior.AdvanceRipperTypedAnimation(Slot, context);

    /// <summary>The G-Ripper and Ripper II's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction? ShotReaction => LeavesShotToSpecies ? RipperBehavior.RipperShotReaction : base.ShotReaction;

    /// <summary>The G-Ripper and Ripper II's own stages around common bomb damage for a bomb that selects its own shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override EnemyBombReaction? CommonBombDamageReaction(EnemyBombContext context) =>
        context.SelectedShot is null ? RipperBehavior.RipperBombReaction : base.CommonBombDamageReaction(context);

    /// <summary>The G-Ripper and Ripper II's own shot reaction, which normal bombs dispatch like the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects; none where the definition leaves shots to the species.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) => reaction is null && LeavesShotToSpecies;
}
